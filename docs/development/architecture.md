# Architecture Overview

## System Architecture

The Discord Bot is built as an ASP.NET Core Web API application that hosts a Discord client as a background service. The architecture follows clean separation of concerns with dependency injection throughout.

## Core Components

### Bot Service (`BotService`)
- **Purpose**: Manages Discord client lifecycle and event registration
- **Type**: `BackgroundService` (hosted service)
- **Responsibilities**:
  - Discord client initialization and connection management
  - Event handler registration (logging, messages, commands)
  - Command registration on bot ready

### Command System

#### Command Interface (`ISlashCommand`)
Defines the contract for all slash commands:
```csharp
SlashCommandBuilder GetCommandBuilder();
Task HandleCommandAsync(SocketSlashCommand command);
```

#### Command Base (`CommandBase`)
Abstract base class providing:
- Common error handling and logging
- Scoped transaction IDs for request tracking
- Helper methods for embeds and status colors
- Standardized command execution flow

#### Command Discovery (`CommandScanner`)
- Scans assemblies for `ISlashCommand` implementations
- Supports both instantiation-based and attribute-based discovery
- Returns mapping of command names to types

#### Command Registration (`CommandRegistrationService`)
- Registers discovered commands with Discord API
- Creates global application commands
- Handles registration errors gracefully

#### Command Handling (`CommandHandlerService`)
- Routes incoming slash commands to appropriate handlers
- Creates command instances via DI container
- Provides error responses for unhandled commands

### Service Layer

#### Consent Management
- **Interface**: `IConsentService`
- **Implementation**: `FileConsentService`
- **Storage**: JSON file-based persistence
- **API**: REST endpoints for consent CRUD operations

#### Message Handling
- **Interface**: `IMessageHandler`
- **Current Implementation**: `LogMessageHandler` (logs all messages)
- **Extensible**: Supports pluggable message processing

#### Logging Integration
- **Service**: `DiscordBotLoggerService`
- **Purpose**: Bridges Discord.NET log messages to Serilog
- **Configuration**: Supports different log levels per environment

## Data Flow

### Command Execution Flow
```
Discord User → Slash Command → Discord API → 
CommandHandlerService → Command Instance → Response
```

1. User executes slash command in Discord
2. Discord API sends interaction to bot
3. `CommandHandlerService` receives the interaction
4. Service looks up command handler by name
5. Creates handler instance via DI container
6. Executes command with error handling and logging
7. Command responds to Discord interaction

### Startup Flow
```
Program.cs → Service Registration → BotService Start → 
Discord Connection → Command Registration
```

1. `Program.cs` configures services and DI container
2. `BotService` starts as hosted service
3. Discord client connects and fires Ready event
4. Commands are discovered and registered with Discord
5. Event handlers are attached for ongoing operations

## Technology Stack

- **.NET 8**: Core framework
- **ASP.NET Core**: Web API hosting
- **Discord.NET**: Discord API client library
- **Serilog**: Structured logging
- **System.Text.Json**: JSON serialization

## Design Patterns

### Dependency Injection
All services use constructor injection with proper interface abstractions.

### Command Pattern
Slash commands implement a common interface with standardized execution flow.

### Template Method
`CommandBase` provides template for command execution with hooks for customization.

### Service Locator (Limited)
Used only for scoped service resolution in event handlers due to Discord.NET's event model.

## Configuration

### Bot Options (`BotOptions`)
- Discord bot token
- Client ID
- Default guild/channel IDs
- Bound from `appsettings.json` "DiscordBot" section

### Serilog Configuration
- Configurable log levels per namespace
- Console output by default
- Environment-specific overrides

## Extension Points

### Adding New Commands
1. Implement `ISlashCommand` or inherit from `CommandBase`
2. Register in DI container (automatic via scanning)
3. Commands are automatically discovered and registered

### Custom Message Handlers
1. Implement `IMessageHandler`
2. Register in DI container
3. Replace `LogMessageHandler` in service registration

### Additional Services
- Add interface and implementation
- Register in `ServiceCollectionExtensions`
- Inject into commands or other services as needed

## Security Considerations

- Bot token stored in configuration (use User Secrets for development)
- Consent system for user data handling
- Error responses don't expose internal details
- Ephemeral responses for error conditions