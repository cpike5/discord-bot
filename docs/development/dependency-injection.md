# Dependency Injection Guide

## DI Container Setup

Services are registered in `ServiceCollectionExtensions.AddBotServices()`:

```csharp
public static IServiceCollection AddBotServices(this IServiceCollection services, IConfiguration configuration)
{
    services.Configure<BotOptions>(configuration.GetSection(BotOptions.SectionName));
    services.RegisterCoreBotServices();
    services.RegisterConsentServices();
    return services;
}
```

## Service Lifetimes

### Singleton
- `DiscordSocketClient` - Single instance for bot lifetime
- `InteractionService` - Discord command framework
- `CommandScanner` - Command discovery service

### Scoped
- `IDiscordBotLoggerService` - Per-request logging
- `ICommandRegistrationService` - Command registration
- `CommandHandlerService` - Command routing
- `IMessageHandler` - Message processing

### Transient
- `IConsentService` - User consent management
- Command instances - Created per command execution

## Configuration Binding

Bot settings use the Options pattern:

```csharp
services.Configure<BotOptions>(configuration.GetSection(BotOptions.SectionName));

// Usage in services:
public MyService(IOptionsMonitor<BotOptions> options)
{
    var token = options.CurrentValue.Token;
}
```

## Command DI Pattern

Commands receive dependencies via constructor injection:

```csharp
public class MyCommand : CommandBase
{
    private readonly IMyService _myService;
    private readonly DiscordSocketClient _client;

    public MyCommand(ILogger<MyCommand> logger, IMyService myService, DiscordSocketClient client)
        : base(logger)
    {
        _myService = myService;
        _client = client;
    }
}
```

Commands are created via `ActivatorUtilities.CreateInstance()` in `CommandHandlerService`.

## Event Handler Scoping

Discord events use manual scope creation due to singleton client:

```csharp
_discordClient.SlashCommandExecuted += async (command) =>
{
    var scope = _services.CreateScope();
    var handler = scope.ServiceProvider.GetRequiredService<CommandHandlerService>();
    await handler.HandleSlashCommandExecutedAsync(command);
};
```

## Adding New Services

### 1. Define Interface
```csharp
public interface IDataService
{
    Task<string> GetDataAsync();
}
```

### 2. Implement Service
```csharp
public class DataService : IDataService
{
    private readonly ILogger<DataService> _logger;
    
    public DataService(ILogger<DataService> logger)
    {
        _logger = logger;
    }
    
    public async Task<string> GetDataAsync() => "data";
}
```

### 3. Register Service
```csharp
// In ServiceCollectionExtensions
services.AddScoped<IDataService, DataService>();
```

### 4. Inject into Commands
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

## Service Provider Access

Direct service provider access is available in:
- `CommandHandlerService` - For creating command instances
- `BotService` - For creating scoped services in event handlers

Avoid `GetRequiredService()` in constructors - use proper DI instead.

## Configuration Services

### Options Pattern
```csharp
// Registration
services.Configure<MyOptions>(configuration.GetSection("MySection"));

// Usage
public MyService(IOptionsMonitor<MyOptions> options)
{
    var value = options.CurrentValue.SomeProperty;
}
```

### Direct Configuration Injection
```csharp
// For simple cases
public MyService(IConfiguration configuration)
{
    var value = configuration["SomeKey"];
}
```