using Discord;
using Discord.WebSocket;

namespace DiscordBotAPI.Commands
{
    /// <summary>
    /// Base class for slash commands that provides common functionality
    /// </summary>
    public abstract class CommandBase : ISlashCommand
    {
        protected readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CommandBase class
        /// </summary>
        /// <param name="logger">The logger</param>
        protected CommandBase(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the command name
        /// </summary>
        protected abstract string CommandName { get; }

        /// <summary>
        /// Gets the command description
        /// </summary>
        protected abstract string CommandDescription { get; }

        /// <summary>
        /// Configures the command builder with options
        /// </summary>
        /// <param name="builder">The command builder to configure</param>
        protected virtual void ConfigureCommand(SlashCommandBuilder builder)
        {
            // Base implementation does nothing - override to add options
        }

        /// <summary>
        /// Gets the command builder for the command
        /// </summary>
        /// <returns>A configured SlashCommandBuilder</returns>
        public SlashCommandBuilder GetCommandBuilder()
        {
            var builder = new SlashCommandBuilder()
                .WithName(CommandName)
                .WithDescription(CommandDescription);

            ConfigureCommand(builder);

            return builder;
        }

        /// <summary>
        /// Handles the command execution
        /// </summary>
        /// <param name="command">The command interaction to handle</param>
        public async Task HandleCommandAsync(SocketSlashCommand command)
        {
            try
            {
                _logger.LogInformation("Handling {CommandName} command from user {UserId} in guild {GuildId}",
                    CommandName, command.User.Id, command.GuildId);

                var scopeProperties = new Dictionary<string, object>()
                {
                    { "TransactionId", Guid.NewGuid().ToString() }
                };
                using (_logger.BeginScope(scopeProperties))
                {
                    await ExecuteCommandAsync(command);
                }

                _logger.LogDebug("Successfully executed {CommandName} command for user {UserId}",
                    CommandName, command.User.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing {CommandName} command for user {UserId}",
                    CommandName, command.User.Id);

                try
                {
                    if (!command.HasResponded)
                    {
                        await command.RespondAsync($"An error occurred while executing the `{CommandName}` command.",
                            ephemeral: true);
                    }
                    else
                    {
                        await command.FollowupAsync($"An error occurred while executing the `{CommandName}` command.",
                            ephemeral: true);
                    }
                }
                catch (Exception followupEx)
                {
                    _logger.LogWarning(followupEx, "Failed to send error response for {CommandName} command",
                        CommandName);
                }
            }
        }

        /// <summary>
        /// Executes the command logic
        /// </summary>
        /// <param name="command">The command interaction to handle</param>
        protected abstract Task ExecuteCommandAsync(SocketSlashCommand command);

        /// <summary>
        /// Gets a color based on status value
        /// </summary>
        /// <param name="value">The value to convert to color</param>
        /// <param name="goodThreshold">Threshold for good status (green)</param>
        /// <param name="warningThreshold">Threshold for warning status (yellow)</param>
        /// <returns>Color representing the status</returns>
        protected Color GetStatusColor(double value, double goodThreshold = 0.7, double warningThreshold = 0.4)
        {
            return value switch
            {
                >= 0.7 => Color.Green,   // Good
                >= 0.4 => Color.Gold,    // Warning
                _ => Color.Red           // Bad
            };
        }

        /// <summary>
        /// Creates a basic embed with standard styling
        /// </summary>
        /// <param name="title">The embed title</param>
        /// <param name="description">The embed description</param>
        /// <param name="color">The embed color</param>
        /// <returns>A configured EmbedBuilder</returns>
        protected EmbedBuilder CreateEmbed(string title, string description = null, Color? color = null)
        {
            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithColor(color ?? Color.Blue)
                .WithCurrentTimestamp();

            if (!string.IsNullOrEmpty(description))
            {
                embed.WithDescription(description);
            }

            return embed;
        }

    }
}