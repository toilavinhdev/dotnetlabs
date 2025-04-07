using Serilog.Core;
using Serilog.Events;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace dotnetserilogsinks;

public class TelegramSink(TelegramBotClient telegramBotClient, ChatId chatId) : ILogEventSink
{
    public void Emit(LogEvent @event)
    {
        try
        {
            EmitAsync(@event).Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TelegramSink Exception: {ex}");
            throw;
        }
    }

    private async Task EmitAsync(LogEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        var message =
            $"""
            <b>Timestamp:</b> {@event.Timestamp:yyyy-MM-dd HH:mm:ss}
            <b>Level:</b> {@event.Level}
            <b>Message:</b> {@event.RenderMessage()}
            """;
        await telegramBotClient.SendMessage(chatId, message, ParseMode.Html);
    }
}