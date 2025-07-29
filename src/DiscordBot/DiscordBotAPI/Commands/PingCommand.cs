using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace DiscordBotAPI.Commands
{
    /// <summary>
    /// Implementation of a simple ping command that responds with latency information
    /// </summary>
    [SlashCommand("ping", "Check if the bot is online and view latency")]
    public class PingCommand : CommandBase
    {
        private readonly DiscordSocketClient _client;

        public PingCommand(ILogger<PingCommand> logger, DiscordSocketClient client)
            : base(logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Gets the command name
        /// </summary>
        protected override string CommandName => "ping";

        /// <summary>
        /// Gets the command description
        /// </summary>
        protected override string CommandDescription => "Check if the bot is online and view latency";

        /// <summary>
        /// Executes the ping command
        /// </summary>
        /// <param name="command">The command interaction</param>
        protected override async Task ExecuteCommandAsync(SocketSlashCommand command)
        {
            // Get current latency
            int latency = _client.Latency;

            // Create response with latency information
            var embedBuilder = new EmbedBuilder()
                .WithTitle("🏓 Pong!")
                .WithDescription($"Bot is online and operational.")
                .AddField("Latency", $"{latency}ms", true)
                .WithColor(GetLatencyColor(latency))
                .WithFooter(footer => {
                    footer.Text = $"Requested by {command.User.Username}";
                })
                .WithCurrentTimestamp();

            // Respond to the command
            await command.RespondAsync(embed: embedBuilder.Build(), ephemeral: true);

            _logger.LogDebug("Ping command completed successfully with latency {Latency}ms", latency);
        }

        /// <summary>
        /// Gets appropriate color based on latency
        /// </summary>
        /// <param name="latency">Latency in milliseconds</param>
        /// <returns>Color representing latency quality</returns>
        private Color GetLatencyColor(int latency)
        {
            return latency switch
            {
                < 100 => Color.Green,  // Excellent latency
                < 200 => Color.Blue,   // Good latency
                < 400 => Color.Orange, // Fair latency
                _ => Color.Red         // Poor latency
            };
        }
    }
}