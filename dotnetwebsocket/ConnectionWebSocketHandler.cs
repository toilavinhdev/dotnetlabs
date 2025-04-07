using System.Net.WebSockets;

namespace dotnetwebsocket;

public class ConnectionWebSocketHandler(WebSocketConnectionManager connectionManager, ILogger<ConnectionWebSocketHandler> logger)
    : WebSocketHandler(connectionManager)
{
    public override async Task OnConnectedAsync(string userId, WebSocket socket)
    {
        await base.OnConnectedAsync(userId, socket);
        logger.LogInformation("{MethodName}[userId={userId}]", nameof(OnConnectedAsync), userId);
    }

    public override async Task OnDisconnectedAsync(string userId, WebSocket socket)
    {
        await base.OnDisconnectedAsync(userId, socket);
        logger.LogInformation("{MethodName}[userId={userId}]", nameof(OnDisconnectedAsync), userId);
    }

    protected override async Task ReceiveAsync(string userId, WebSocket socket, string message)
    {
        logger.LogInformation("{MethodName}[userId={userId}]: {Message}", nameof(ReceiveAsync), userId, message);
        await Task.CompletedTask;
    }
}