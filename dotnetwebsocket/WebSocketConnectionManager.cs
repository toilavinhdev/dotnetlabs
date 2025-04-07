using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace dotnetwebsocket;

public class WebSocketConnectionManager
{
    /// <summary>
    /// Key: UserId
    /// Value: Sockets
    /// </summary>
    private readonly ConcurrentDictionary<string, List<WebSocket>> _sockets = new();
    
    private static string NormalizeId(string id) => id.ToLower();

    public Dictionary<string, List<WebSocket>> Sockets()
    {
        return _sockets.ToDictionary();
    }

    public IEnumerable<WebSocket>? GetSockets(string userId)
    {
        return _sockets.GetValueOrDefault(NormalizeId(userId));
    } 
    
    public string? GetUserId(WebSocket socket)
    {
        var id = _sockets.FirstOrDefault(x => x.Value.Any(s => s == socket)).Key;
        return string.IsNullOrEmpty(id) ? null : NormalizeId(id);
    }
    
    public void AddSocket(string userId, WebSocket socket)
    {
        var existed = _sockets.GetValueOrDefault(userId);
        if (existed is null)
        {
            _sockets[NormalizeId(userId)] = [socket];
            return;
        }
        existed.Add(socket);
    }
    
    public async Task RemoveSocketAsync(WebSocket socket)
    {
        foreach (var pair in _sockets)
        {
            var removed = pair.Value.Remove(socket);
            if (removed)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "The server closes this connection",
                    CancellationToken.None);
            }
            if (pair.Value.Count == 0) _sockets.TryRemove(pair);
        }
    }
}