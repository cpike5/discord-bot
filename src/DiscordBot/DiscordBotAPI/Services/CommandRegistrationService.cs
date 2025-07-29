using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBotAPI.Commands;
using DiscordBotAPI.Exceptions;

namespace DiscordBotAPI.Services
{
    public interface ICommandRegistrationService
    {
        Task RegisterCommandsAsync();
    }

    public class CommandRegistrationService : ICommandRegistrationService
    {
        private readonly ILogger<CommandRegistrationService> _logger;
        private readonly DiscordSocketClient _discord;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _serviceProvider;

        public CommandRegistrationService(ILogger<CommandRegistrationService> logger, DiscordSocketClient discord, InteractionService interactionService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _discord = discord;
            _interactionService = interactionService;
            _serviceProvider = serviceProvider;
        }

        public async Task RegisterCommandsAsync()
        {
            _logger.LogInformation("Starting command registration process...");
            await RegisterSlashCommands();
        }

        public async Task RegisterSlashCommands()
        {
            _logger.LogInformation("Registering slash commands with Discord API");

            var commandHandlerService = _serviceProvider.GetRequiredService<CommandHandlerService>() ?? throw new MissingServiceException(nameof(CommandHandlerService));
            var commandHandlers = commandHandlerService.GetCommandHandlers();

            try
            {
                var builders = new List<SlashCommandBuilder>();

                // Create instances of each command to get builders
                foreach (var handler in commandHandlers)
                {
                    try
                    {
                        var commandInstance = (ISlashCommand)ActivatorUtilities.CreateInstance(
                            _serviceProvider, handler.Value);
                        builders.Add(commandInstance.GetCommandBuilder());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating builder for command {CommandName}", handler.Key);
                    }
                }

                // Register commands with Discord API
                foreach (var builder in builders)
                {
                    _logger.LogDebug("Registering command: {CommandName}", builder.Name);
                    await _discord.CreateGlobalApplicationCommandAsync(builder.Build());
                }

                _logger.LogInformation("Successfully registered {Count} slash commands", builders.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register slash commands");
                throw;
            }
        }
    }
}
