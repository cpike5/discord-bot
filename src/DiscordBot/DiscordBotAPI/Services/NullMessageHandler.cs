using Discord.WebSocket;

namespace DiscordBotAPI.Services
{
    public class NullMessageHandler : IMessageHandler
    {
        public Task HandleMessageAsync(SocketMessage message)
        {
            // No operation for null handler
            return Task.CompletedTask;
        }
    }
}
