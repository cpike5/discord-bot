using Discord;
using Discord.WebSocket;
using DiscordBotAPI.Configuration;
using DiscordBotAPI.Exceptions;
using Microsoft.Extensions.Options;

namespace DiscordBotAPI.Services
{
    public class BotService : BackgroundService, IBotService
    {
        private readonly ILogger<BotService> _logger;
        private readonly DiscordSocketClient _discordClient;
        private readonly IOptionsMonitor<BotOptions> _botOptions;
        private readonly IServiceProvider _services;

        public BotService(
            ILogger<BotService> logger,
            DiscordSocketClient discordClient,
            IOptionsMonitor<BotOptions> botOptions,
            IServiceProvider services)
        {
            _logger = logger;
            _discordClient = discordClient;
            _botOptions = botOptions;
            _services = services;

            try
            {
                _logger.LogDebug("Initializing Discord bot service");

                _logger.LogTrace("Registering event handlers for DiscordSocketClient");
                // Register event handlers
                _discordClient.Log += LogAsync;

                // Start the bot
                _logger.LogInformation("Starting Discord bot service");
                Task.Run(StartBotAsync);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the Discord bot service.");
            }
        }

        /// <summary>
        /// Logs messages using the DiscordBotLoggerService.
        /// </summary>
        /// <param name="logMessage"></param>
        /// <returns></returns>
        public async Task LogAsync(LogMessage logMessage)
        {
            using (var scope = _services.CreateScope())
            {
                // Resolve the logger service from the scope
                var loggerService = scope.ServiceProvider.GetRequiredService<DiscordBotLoggerService>();
                await loggerService.LogAsync(logMessage);
            }
        }

        /// <summary>
        /// Starts the Discord bot with the token specified in the BotOptions.
        /// </summary>
        /// <returns></returns>
        public async Task StartBotAsync()
        {
            await _discordClient.LoginAsync(TokenType.Bot, _botOptions.CurrentValue.Token);
            await _discordClient.StartAsync();
        }

        /// <summary>
        /// Stops the Discord bot if it is currently connected.
        /// </summary>
        /// <returns></returns>
        public async Task StopBotAsync()
        {
            if (_discordClient.ConnectionState == ConnectionState.Connected)
            {
                await _discordClient.StopAsync();
                _logger.LogInformation("Discord bot stopped successfully.");
            }
            else
            {
                _logger.LogWarning("Discord bot is not connected, no action taken.");
            }
        }

        /// <summary>
        /// Executes the background service.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        ~BotService()
        {
            // Ensure the bot is stopped when the service is disposed
            if (_discordClient.ConnectionState == ConnectionState.Connected)
            {
                _logger.LogInformation("Disposing BotService and stopping Discord client.");
                _discordClient.StopAsync().GetAwaiter().GetResult();
            }
        }
    }
}
