using Discord.WebSocket;

namespace DiscordBotAPI.Services
{
    public interface IMessageHandler
    {
        Task HandleMessageAsync(SocketMessage message);
    }
}
