using Discord;

namespace DiscordBotAPI.Services
{
    public interface IDiscordBotLoggerService
    {
        Task LogAsync(LogMessage logMessage);
    }
}
