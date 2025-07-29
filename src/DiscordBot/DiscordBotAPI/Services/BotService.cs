using Discord;
using Discord.WebSocket;
using DiscordBotAPI.Configuration;
using DiscordBotAPI.Exceptions;
using Microsoft.Extensions.Options;

namespace DiscordBotAPI.Services
{
    /// <summary>
    /// Service for managing the Discord bot.
    /// </summary>
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

                // Register event handlers
                RegisterEventHandlers();

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
        /// Registers event handlers for the DiscordSocketClient.
        /// </summary>
        /// <exception cref="MissingServiceException"></exception>
        private void RegisterEventHandlers()
        {
            _logger.LogTrace("Registering event handlers for DiscordSocketClient");

            // Register the logging event handler
            _discordClient.Log += async (message) =>
            {
                using var scope = _services.CreateScope();
                // Resolve the logger service from the scope
                var loggerService = scope.ServiceProvider.GetRequiredService<DiscordBotLoggerService>() ?? throw new MissingServiceException(nameof(DiscordBotLoggerService));
                await loggerService.LogAsync(message);
            };

            // Register the message received event handler
            _discordClient.MessageReceived += async (message) =>
            {
                using var scope = _services.CreateScope();
                // Resolve the message handler service from the scope
                var messageHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler>() ?? throw new MissingServiceException(nameof(IMessageHandler));
                await messageHandler.HandleMessageAsync(message);
            };

            // Register the ready event handler
            _discordClient.Ready += Client_Ready;
        }

        private async Task Client_Ready()
        {
            _logger.LogDebug("Client is ready and connected to Discord.");
            await Task.CompletedTask;
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
