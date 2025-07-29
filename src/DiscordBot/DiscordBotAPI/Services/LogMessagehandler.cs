using Discord.WebSocket;

namespace DiscordBotAPI.Services
{
    public class LogMessageHandler : IMessageHandler
    {
        private readonly ILogger<LogMessageHandler> _logger;
        public LogMessageHandler(ILogger<LogMessageHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleMessageAsync(SocketMessage message)
        {
            // Log the message content
            _logger.LogInformation("Received message from user {User}: {MessageContent}", message.Author.Username, message.Content);
            return Task.CompletedTask;
        }
    }
}
