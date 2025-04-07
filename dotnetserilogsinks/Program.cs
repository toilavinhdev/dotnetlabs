using dotnetserilogsinks;
using Serilog;
using Serilog.Events;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

const string telegramBotToken = "8060362862:AAE0J8AK_h50rpIkJR26zkJICP_n7Hj0KF0";
const string telegramChannelId = "7121086973";

var builder = WebApplication.CreateBuilder(args);
var telegramBotClient = new TelegramBotClient(telegramBotToken);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(telegramBotClient);
builder.Services.AddHttpClient();
var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .Enrich.WithProperty("Assembly", typeof(Program).Assembly.FullName)
    .WriteTo.Console()
    .WriteTo.File(
        path: @"Logs\logs.txt",
        restrictedToMinimumLevel: LogEventLevel.Error,
        rollingInterval: RollingInterval.Day)
    .WriteTo.Sink(new TelegramSink(telegramBotClient, new ChatId(telegramChannelId)))
    .CreateLogger();
builder.Host.UseSerilog(loggerConfiguration);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => "Hello World!");
app.MapGet("/telegram/bot", async (HttpClient httpClient) =>
{
    const string url = $"https://api.telegram.org/bot{telegramBotToken}/getUpdates";
    var response = await httpClient.GetAsync(url);
    var data = await response.Content.ReadFromJsonAsync<object>();
    // var recentlyChat = data?.result[0].message.chat;
    return data;
});
app.MapPost("/telegram/bot/send-message", async (TelegramBotClient client, string message) =>
{
    await client.SendMessage(chatId: new ChatId(telegramChannelId), text: message, ParseMode.Html);
});
app.MapPost("/telegram/bot/logging", (ILogger<Program> logger) =>
{
    logger.LogInformation("Hello World! I'm toilavinhdev");
    logger.LogError("Oops! Something went wrong");
});
app.Run();