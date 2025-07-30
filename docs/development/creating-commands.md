# Creating Commands Guide

## Command Creation Steps

### 1. Create Command Class
Inherit from `CommandBase` or implement `ISlashCommand`:

```csharp
[SlashCommand("hello", "Say hello to someone")]
public class HelloCommand : CommandBase
{
    public HelloCommand(ILogger<HelloCommand> logger) : base(logger) { }

    protected override string CommandName => "hello";
    protected override string CommandDescription => "Say hello to someone";

    protected override async Task ExecuteCommandAsync(SocketSlashCommand command)
    {
        await command.RespondAsync("Hello!", ephemeral: true);
    }
}
```

### 2. Add Command Options
Override `ConfigureCommand()` to add parameters:

```csharp
protected override void ConfigureCommand(SlashCommandBuilder builder)
{
    builder.AddOption("user", ApplicationCommandOptionType.User, 
        "User to greet", isRequired: true);
    builder.AddOption("message", ApplicationCommandOptionType.String, 
        "Custom message", isRequired: false);
}
```

### 3. Handle Options in Command
```csharp
protected override async Task ExecuteCommandAsync(SocketSlashCommand command)
{
    var user = command.Data.Options.First(x => x.Name == "user").Value as IUser;
    var message = command.Data.Options.FirstOrDefault(x => x.Name == "message")?.Value as string ?? "Hello";
    
    await command.RespondAsync($"{message}, {user.Mention}!");
}
```

### 4. Register Dependencies
Commands automatically receive DI services:

```csharp
public class DataCommand : CommandBase
{
    private readonly IDataService _dataService;
    
    public DataCommand(ILogger<DataCommand> logger, IDataService dataService) 
        : base(logger)
    {
        _dataService = dataService;
    }
}
```

## Command Options

### Option Types
- `String` - Text input
- `Integer` - Whole numbers
- `Number` - Decimal numbers
- `Boolean` - True/false
- `User` - Discord user
- `Channel` - Discord channel
- `Role` - Discord role
- `Mentionable` - User or role
- `Attachment` - File uploads

### Option Configuration
```csharp
builder.AddOption(new SlashCommandOptionBuilder()
    .WithName("level")
    .WithDescription("Difficulty level")
    .WithType(ApplicationCommandOptionType.String)
    .WithRequired(true)
    .AddChoice("Easy", "easy")
    .AddChoice("Hard", "hard"));
```

## Response Types

### Basic Response
```csharp
await command.RespondAsync("Simple message", ephemeral: true);
```

### Embed Response
```csharp
var embed = CreateEmbed("Title", "Description", Color.Blue);
await command.RespondAsync(embed: embed.Build());
```

### Deferred Response
For long operations:
```csharp
await command.DeferAsync();
// ... do work ...
await command.FollowupAsync("Operation complete!");
```

## Error Handling

`CommandBase` provides automatic error handling. For custom error handling:

```csharp
protected override async Task ExecuteCommandAsync(SocketSlashCommand command)
{
    try
    {
        // Command logic
    }
    catch (SpecificException ex)
    {
        _logger.LogWarning(ex, "Specific error in command");
        await command.RespondAsync("Friendly error message", ephemeral: true);
    }
    // Base class handles general exceptions
}
```

## Testing Commands

### Unit Testing
```csharp
[Test]
public async Task HelloCommand_ShouldRespond()
{
    var logger = Mock.Of<ILogger<HelloCommand>>();
    var command = new HelloCommand(logger);
    
    var mockCommand = CreateMockSlashCommand("hello");
    await command.HandleCommandAsync(mockCommand);
    
    // Verify response
}
```

### Local Testing
1. Run bot locally
2. Commands register automatically
3. Test in Discord server (may take up to 1 hour for global commands)

## Command Examples

### Simple Command
```csharp
[SlashCommand("status", "Check bot status")]
public class StatusCommand : CommandBase
{
    protected override string CommandName => "status";
    protected override string CommandDescription => "Check bot status";

    protected override async Task ExecuteCommandAsync(SocketSlashCommand command)
    {
        var embed = CreateEmbed("Bot Status", "✅ Online and operational");
        await command.RespondAsync(embed: embed.Build());
    }
}
```

### Command with Service
```csharp
[SlashCommand("weather", "Get weather info")]
public class WeatherCommand : CommandBase
{
    private readonly IWeatherService _weatherService;
    
    public WeatherCommand(ILogger<WeatherCommand> logger, IWeatherService weatherService) 
        : base(logger)
    {
        _weatherService = weatherService;
    }

    protected override void ConfigureCommand(SlashCommandBuilder builder)
    {
        builder.AddOption("city", ApplicationCommandOptionType.String, 
            "City name", isRequired: true);
    }

    protected override async Task ExecuteCommandAsync(SocketSlashCommand command)
    {
        var city = command.Data.Options.First().Value.ToString();
        var weather = await _weatherService.GetWeatherAsync(city);
        
        var embed = CreateEmbed($"Weather for {city}", weather.Description);
        await command.RespondAsync(embed: embed.Build());
    }
}
```