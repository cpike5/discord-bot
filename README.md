# Discord Bot

A simple Discord bot that runs as a .NET Web API. It contains a hosted service running the bot, and a web API enabling remote administration. This is a re-creation of a project I worked on in the past, but hopefully better.

**Note: This is an early stage work in progress.**

## Tech Stack and Frameworks

- **.NET 8** - Web API and hosted services
- **Discord.NET 3.18** - Discord library for C#
- **ASP.NET Core** - Web API framework
- **Serilog** - Structured logging
- **Swashbuckle** - API documentation

## Getting Started

### Prerequisites
- .NET 8 SDK
- Discord application with bot token
- Visual Studio 2022 or VS Code

### Dev Environment Setup

1. Clone the repository
2. Open `src/DiscordBot/DiscordBot.sln` in Visual Studio
3. Configure your bot settings (see Configuration section)
4. Run the project - it will start both the Discord bot and web API

The bot runs as a hosted service alongside the web API on `https://localhost:7103` (or `http://localhost:5210`).

## Configuration

Configure your bot in `appsettings.json` or user secrets:

```json
{
  "DiscordBot": {
    "Token": "your-bot-token-here",
    "ClientId": "your-client-id",
    "DefaultGuildId": "optional-guild-id",
    "DefaultChannelId": "optional-channel-id"
  }
}
```

For development, use user secrets to store the token:
```bash
dotnet user-secrets set "DiscordBot:Token" "your-bot-token"
dotnet user-secrets set "DiscordBot:ClientId" "your-client-id"
```

## Commands

Currently implemented slash commands:

- **`/ping`** - Check if the bot is online and view latency

Commands are automatically discovered and registered using the `CommandScanner` service. New commands inherit from `CommandBase` and implement `ISlashCommand`.

## API

The web API provides endpoints for bot administration:

### Consent Management
- `GET /api/consent/check-consent` - Check if user has given consent
- `POST /api/consent/add-consent` - Add user consent
- `POST /api/consent/remove-consent` - Remove user consent

API documentation is available at `/swagger` when running in development mode.

## Observability

### Logging
Uses Serilog with console output. Log levels can be configured in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning"
      }
    }
  }
}
```

Bot events, command executions, and API calls are logged with structured data including user IDs, guild IDs, and transaction IDs for correlation.