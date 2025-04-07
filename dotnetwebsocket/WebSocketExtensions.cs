using System.Reflection;

namespace dotnetwebsocket;

public static class WebSocketExtensions
{
    /// <summary>
    /// Đăng ký websocket handlers
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TAssembly"></typeparam>
    public static void AddWebSocketHandlers<TAssembly>(this IServiceCollection services)
    {
        // Đăng ký connection manager lưu trữ các kết nối từng handler
        services.AddTransient<WebSocketConnectionManager>();
        
        // Đăng ký tất cả handler
        typeof(TAssembly).Assembly.ExportedTypes
            .Where(t => t.GetTypeInfo().BaseType == typeof(WebSocketHandler))
            .ToList()
            .ForEach(t => services.AddSingleton(t));
    }
    
    /// <summary>
    /// Map websocket endpoint với handler chỉ định
    /// </summary>
    /// <param name="app"></param>
    /// <param name="path"></param>
    /// <typeparam name="THandler"></typeparam>
    public static void MapWebSocket<THandler>(this WebApplication app, PathString path) where THandler : WebSocketHandler
    {
        var handlerService = app.Services.GetService<THandler>();
        app.Map(path, c => c.UseMiddleware<WebSocketMiddleware>(handlerService));
    }
}