using dotnetwebsocket;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddWebSocketHandlers<Program>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseWebSockets();
app.MapWebSocket<ConnectionWebSocketHandler>("/connection/ws");
app.MapGet("/", () => "Hello World!");
app.MapGet("/socket/connection/connections", (ConnectionWebSocketHandler handler) => handler.ConnectionManager.Sockets());
app.Run();