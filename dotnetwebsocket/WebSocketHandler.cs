using System.Net.WebSockets;
using System.Text;

namespace dotnetwebsocket;

public abstract class WebSocketHandler
{
    protected WebSocketHandler(WebSocketConnectionManager connectionManager)
    {
        ConnectionManager = connectionManager;
    }

    public WebSocketConnectionManager ConnectionManager { get; set; }

    /// <summary>
    /// Sự kiện WebSocket bắt đầu kết nối
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="socket"></param>
    public virtual Task OnConnectedAsync(string userId, WebSocket socket)
    {
        ConnectionManager.AddSocket(userId, socket);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sự kiện WebSocket ngắt kết nối
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="socket"></param>
    public virtual async Task OnDisconnectedAsync(string userId, WebSocket socket)
    {
        await ConnectionManager.RemoveSocketAsync(socket);
    }

    /// <summary>
    /// Sự kiện WebSocket nhận được message
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="result"></param>
    /// <param name="buffer"></param>
    public async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
    {
        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
        var userId = ConnectionManager.GetUserId(socket);
        if (string.IsNullOrEmpty(userId)) return;
        await ReceiveAsync(userId, socket, message);
    }

    /// <summary>
    /// Sự kiện WebSocket nhận được message
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="socket"></param>
    /// <param name="message"></param>
    protected abstract Task ReceiveAsync(string userId, WebSocket socket, string message);
    
    /// <summary>
    /// Gửi message user to users
    /// </summary>
    /// <param name="userIds"></param>
    /// <param name="message"></param>
    public async Task SendMessageAsync(IEnumerable<string> userIds, string message)
    {
        foreach (var userId in userIds)
        {
            await SendMessageAsync(userId, message);
        }
    }
    
    /// <summary>
    /// Gửi message user to user
    /// </summary>
    /// <param name="socketId"></param>
    /// <param name="message"></param>
    public async Task SendMessageAsync(string socketId, string message)
    {
        var sockets = ConnectionManager.GetSockets(socketId);
        if (sockets is null) return;
        foreach (var socket in sockets)
        {
            await SendMessageAsync(socket, message);
        }
    }
    
    /// <summary>
    /// Gửi message broadcast to users
    /// </summary>
    /// <param name="message"></param>
    public async Task SendMessageBroadcastAsync(string message)
    {
        foreach (var pair in ConnectionManager.Sockets())
        {
            foreach (var socket in pair.Value.Where(socket => socket.State == WebSocketState.Open))
            {
                await SendMessageAsync(socket, message);
            }
        }
    }
    
    /// <summary>
    /// Gửi message đến socket
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="message"></param>
    private static async Task SendMessageAsync(WebSocket socket, string message)
    {
        if (socket.State != WebSocketState.Open)
            return;

        await socket.SendAsync(
            buffer: new ArraySegment<byte>(
                array: Encoding.UTF8.GetBytes(message),
                offset: 0,
                count: message.Length),
            messageType: WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken: CancellationToken.None);
    }
}