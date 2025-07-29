using Discord.WebSocket;
using DiscordBotAPI.Commands;
using DiscordBotAPI.Exceptions;
using System.Reflection;

namespace DiscordBotAPI.Services
{
    public class CommandHandlerService
    {
        private readonly ILogger<CommandHandlerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly CommandScanner _commandScanner; 

        public CommandHandlerService(ILogger<CommandHandlerService> logger, IServiceProvider serviceProvider, CommandScanner commandScanner)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _commandScanner = commandScanner ?? throw new ArgumentNullException(nameof(commandScanner));
        }

        public Dictionary<string, Type> GetCommandHandlers(Assembly assembly = null)
        {
            _logger.LogInformation("Scanning for command handlers");
            assembly ??= Assembly.GetExecutingAssembly();
            // Use the command scanner to find commands
            var commandMap = _commandScanner.ScanForCommands(assembly);
            _logger.LogInformation("Found {Count} command handlers", commandMap.Count);
            return commandMap;
        }

        public async Task HandleSlashCommandExecutedAsync(SocketSlashCommand command)
        {
            try
            {
                _logger.LogInformation("Received slash command: {CommandName} from user {UserId}",
                command.CommandName, command.User.Id);
                var commandHandlers = GetCommandHandlers();
                // Check if we have a handler for this command
                if (commandHandlers.TryGetValue(command.CommandName, out Type handlerType))
                {
                    // Create instance of the handler
                    var handler = (ISlashCommand)ActivatorUtilities.CreateInstance(
                        _serviceProvider, handlerType);

                    // Execute the command
                    await handler.HandleCommandAsync(command);

                    _logger.LogDebug("Successfully handled command {CommandName}", command.CommandName);
                }
                else
                {
                    _logger.LogWarning("No handler registered for command {CommandName}", command.CommandName);
                    await command.RespondAsync("This command is not implemented.", ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling slash command {CommandName}", command.CommandName);

                try
                {
                    if (!command.HasResponded)
                    {
                        await command.RespondAsync("An error occurred while processing the command.", ephemeral: true);
                    }
                }
                catch
                {
                    _logger.LogWarning("Failed to send error response for command {CommandName}", command.CommandName);
                }
            }
        }
    }
}
