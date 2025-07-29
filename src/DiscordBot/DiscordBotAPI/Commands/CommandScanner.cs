using System.Reflection;

namespace DiscordBotAPI.Commands
{
    /// <summary>
    /// Service that scans assemblies for command implementations
    /// </summary>
    public class CommandScanner
    {
        private readonly ILogger<CommandScanner> _logger;
        private readonly IServiceProvider _serviceProvider;

        public CommandScanner(ILogger<CommandScanner> logger, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Scans the specified assembly for classes implementing ISlashCommand
        /// </summary>
        /// <param name="assembly">The assembly to scan, or null to use the current assembly</param>
        /// <returns>Dictionary of command name to command type</returns>
        public Dictionary<string, Type> ScanForCommands(Assembly assembly = null)
        {
            try
            {
                _logger.LogInformation("Scanning for command implementations");
                assembly ??= Assembly.GetExecutingAssembly();

                // Find all non-abstract classes implementing ISlashCommand
                var commandTypes = assembly.GetTypes()
                    .Where(type => typeof(ISlashCommand).IsAssignableFrom(type) &&
                                 !type.IsAbstract &&
                                 !type.IsInterface)
                    .ToList();

                _logger.LogInformation("Found {Count} command implementations", commandTypes.Count);

                // Create dictionary of command name to type
                var commandMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
                foreach (var type in commandTypes)
                {
                    try
                    {
                        // Use the DI container to create the instance
                        var commandInstance = (ISlashCommand)ActivatorUtilities.CreateInstance(_serviceProvider, type);
                        var builder = commandInstance.GetCommandBuilder();
                        var commandName = builder.Name;

                        commandMap[commandName] = type;
                        _logger.LogDebug("Registered command: {CommandName} -> {CommandType}",
                            commandName, type.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not create instance of command type {CommandType}",
                            type.FullName);
                    }
                }

                return commandMap;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning for commands");
                return new Dictionary<string, Type>();
            }
        }

        /// <summary>
        /// Alternative method that extracts command information from attributes
        /// instead of instantiating commands
        /// </summary>
        public Dictionary<string, Type> ScanForCommandsViaAttributes(Assembly assembly = null)
        {
            try
            {
                _logger.LogInformation("Scanning for command implementations via attributes");
                assembly ??= Assembly.GetExecutingAssembly();

                var commandMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

                // Find all non-abstract classes implementing ISlashCommand
                var commandTypes = assembly.GetTypes()
                    .Where(type => typeof(ISlashCommand).IsAssignableFrom(type) &&
                                 !type.IsAbstract &&
                                 !type.IsInterface)
                    .ToList();

                foreach (var type in commandTypes)
                {
                    try
                    {
                        // Look for a SlashCommandAttribute to get the name
                        var attribute = type.GetCustomAttribute<SlashCommandAttribute>();
                        if (attribute != null && !string.IsNullOrEmpty(attribute.Name))
                        {
                            commandMap[attribute.Name] = type;
                            _logger.LogDebug("Registered command via attribute: {CommandName} -> {CommandType}",
                                attribute.Name, type.Name);
                        }
                        else if (TryGetCommandNameFromProperties(type, out string commandName))
                        {
                            // Try to get name from static properties or constants
                            commandMap[commandName] = type;
                            _logger.LogDebug("Registered command via property: {CommandName} -> {CommandType}",
                                commandName, type.Name);
                        }
                        else
                        {
                            _logger.LogWarning("Command type {CommandType} has no name attribute or property",
                                type.FullName);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing command type {CommandType}",
                            type.FullName);
                    }
                }

                _logger.LogInformation("Found {Count} command implementations via attributes", commandMap.Count);
                return commandMap;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning for commands via attributes");
                return new Dictionary<string, Type>();
            }
        }

        private bool TryGetCommandNameFromProperties(Type type, out string commandName)
        {
            try
            {
                // Try to get CommandName from a static property or constant
                var nameProperty = type.GetProperty("CommandName",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                if (nameProperty != null && nameProperty.PropertyType == typeof(string))
                {
                    commandName = (string)nameProperty.GetValue(null);
                    return !string.IsNullOrEmpty(commandName);
                }

                // Look for a const field
                var nameField = type.GetField("CommandName",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                if (nameField != null && nameField.FieldType == typeof(string))
                {
                    commandName = (string)nameField.GetValue(null);
                    return !string.IsNullOrEmpty(commandName);
                }

                commandName = null;
                return false;
            }
            catch
            {
                commandName = null;
                return false;
            }
        }

        /// <summary>
        /// Scans multiple assemblies for command implementations
        /// </summary>
        /// <param name="assemblies">The assemblies to scan</param>
        /// <returns>Dictionary of command name to command type</returns>
        public Dictionary<string, Type> ScanForCommands(IEnumerable<Assembly> assemblies)
        {
            var result = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            foreach (var assembly in assemblies)
            {
                var commands = ScanForCommands(assembly);
                foreach (var command in commands)
                {
                    result[command.Key] = command.Value;
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Attribute to specify the name and description of a slash command
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SlashCommandAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the command
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the command
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the SlashCommandAttribute class
        /// </summary>
        /// <param name="name">The name of the command</param>
        /// <param name="description">The description of the command</param>
        public SlashCommandAttribute(string name, string description = null)
        {
            Name = name;
            Description = description;
        }
    }
}