using Discord;

namespace DiscordBotAPI.Services
{
    /// <summary>
    /// Service for logging messages from Discord.NET to the configured logger.
    /// </summary>
    public class DiscordBotLoggerService : IDiscordBotLoggerService
    {
        private readonly ILogger<DiscordBotLoggerService> _logger;
        public DiscordBotLoggerService(ILogger<DiscordBotLoggerService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Logs messages from Discord.NET to the configured logger.
        /// </summary>
        /// <param name="logMessage"></param>
        /// <returns></returns>
        public async Task LogAsync(LogMessage logMessage)
        {
            switch (logMessage.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(logMessage.Exception, logMessage.Message);
                    break;
                case LogSeverity.Error:
                    _logger.LogError(logMessage.Exception, logMessage.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(logMessage.Exception, logMessage.Message);
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(logMessage.Exception, logMessage.Message);
                    break;
                case LogSeverity.Verbose:
                    _logger.LogDebug(logMessage.Exception, logMessage.Message);
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(logMessage.Exception, logMessage.Message);
                    break;
            }
            await Task.CompletedTask;
        }
    }
}
