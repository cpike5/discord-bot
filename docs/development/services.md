# Services Documentation

## Core Services

### BotService
**Type**: `BackgroundService`  
**Lifecycle**: Singleton (hosted service)

Manages Discord client lifecycle and event registration.

```csharp
public class BotService : BackgroundService, IBotService
{
    Task StartBotAsync();
    Task StopBotAsync();
}
```

**Responsibilities**:
- Discord client connection management
- Event handler registration
- Command registration on ready

### Command Services

#### CommandScanner
**Type**: Service  
**Lifecycle**: Singleton

Discovers command implementations via reflection.

```csharp
public Dictionary<string, Type> ScanForCommands(Assembly assembly = null);
public Dictionary<string, Type> ScanForCommandsViaAttributes(Assembly assembly = null);
```

#### CommandRegistrationService
**Type**: `ICommandRegistrationService`  
**Lifecycle**: Scoped

Registers discovered commands with Discord API.

```csharp
public interface ICommandRegistrationService
{
    Task RegisterCommandsAsync();
}
```

#### CommandHandlerService
**Type**: Service  
**Lifecycle**: Scoped

Routes slash commands to appropriate handlers.

```csharp
public Dictionary<string, Type> GetCommandHandlers(Assembly assembly = null);
public Task HandleSlashCommandExecutedAsync(SocketSlashCommand command);
```

### Consent Management

#### IConsentService
File-based user consent tracking.

```csharp
public interface IConsentService
{
    Task<bool> HasUserConsentedAsync(ulong userId, string consentType);
    Task AddUserConsentAsync(ulong userId, string consentType);
    Task RemoveUserConsentAsync(ulong userId, string consentType);
}
```

**Implementation**: `FileConsentService`  
**Storage**: `consent.json` file  
**Data**: `UserConsent(ulong userId, string consentType)` records

### Message Handling

#### IMessageHandler
Pluggable message processing system.

```csharp
public interface IMessageHandler
{
    Task HandleMessageAsync(SocketMessage message);
}
```

**Current Implementation**: `LogMessageHandler` - logs all received messages  
**Alternative**: `NullMessageHandler` - no-op implementation

### Logging Services

#### IDiscordBotLoggerService
Bridges Discord.NET logging to Serilog.

```csharp
public interface IDiscordBotLoggerService
{
    Task LogAsync(LogMessage logMessage);
}
```

Maps Discord log severity to appropriate Serilog levels.

## Service Registration

Services are registered in `ServiceCollectionExtensions`:

```csharp
public static IServiceCollection AddBotServices(this IServiceCollection services, IConfiguration configuration)
{
    // Core bot services
    services.RegisterCoreBotServices();
    // Consent services  
    services.RegisterConsentServices();
    return services;
}
```

### Registration Details

**Singletons**:
- `DiscordSocketClient`
- `InteractionService`
- `CommandScanner`

**Scoped**:
- `IDiscordBotLoggerService`
- `ICommandRegistrationService`
- `CommandHandlerService`

**Transient**:
- `IConsentService`
- `IMessageHandler`

## Creating Custom Services

### 1. Define Interface
```csharp
public interface IMyService
{
    Task DoSomethingAsync();
}
```

### 2. Implement Service
```csharp
public class MyService : IMyService
{
    private readonly ILogger<MyService> _logger;
    
    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }
    
    public async Task DoSomethingAsync()
    {
        // Implementation
    }
}
```

### 3. Register in DI
```csharp
// In ServiceCollectionExtensions
services.AddScoped<IMyService, MyService>();
```

### 4. Use in Commands
```csharp
public class MyCommand : CommandBase
{
    private readonly IMyService _myService;
    
    public MyCommand(ILogger<MyCommand> logger, IMyService myService) 
        : base(logger)
    {
        _myService = myService;
    }
}
```

## Service Dependencies

**Discord Components**:
- `DiscordSocketClient` → Core Discord functionality
- `InteractionService` → Slash command framework

**Configuration**:
- `BotOptions` → Bot settings from appsettings.json

**Cross-Service Dependencies**:
- Most services depend on `ILogger<T>`
- Command services require `IServiceProvider` for DI resolution
- Event handlers create scopes to resolve scoped services