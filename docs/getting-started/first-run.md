# First Run Guide

## Prerequisites
- .NET 8 SDK installed
- Discord bot created and configured (see [configuration.md](configuration.md))
- Bot token stored in user secrets or environment variables

## Quick Start

### 1. Clone and Setup
```bash
git clone https://github.com/cpike5/discord-bot
cd DiscordBot
dotnet restore
```

### 2. Configure Bot Token
```bash
cd src/DiscordBot/DiscordBotAPI
dotnet user-secrets set "DiscordBot:Token" "YOUR_BOT_TOKEN"
dotnet user-secrets set "DiscordBot:ClientId" "YOUR_CLIENT_ID"
```

### 3. Run the Bot
```bash
dotnet run
```

## What Happens on First Run

### 1. Application Startup
- Web API starts on `https://localhost:7103` and `http://localhost:5210`
- Swagger UI available at `/swagger`
- Bot service initializes as background service

### 2. Discord Connection
- Bot connects to Discord using provided token
- Console shows connection status logs
- Ready event fires when connection established

### 3. Command Registration
- Bot automatically discovers slash commands
- Registers commands with Discord API globally
- Commands become available in Discord (may take up to 1 hour)

### 4. Bot Goes Online
- Bot status shows as online in Discord
- Commands are ready to use
- Message logging begins

## Verify Everything Works

### 1. Check Logs
Look for these messages:
```
[INF] Starting Discord bot service
[INF] Client is ready and connected to Discord
[INF] Successfully registered 1 slash commands
```

### 2. Test Ping Command
In Discord, type `/ping` and execute. Should respond with latency info.

### 3. Check Swagger API
Visit `https://localhost:7103/swagger` to see consent management endpoints.

## Expected File Structure After First Run
```
src/DiscordBot/DiscordBotAPI/
├── consent.json          # Created automatically for consent storage
└── logs/                 # If file logging configured
```

## Troubleshooting First Run

### Bot Won't Connect
- Verify token in user secrets: `dotnet user-secrets list`
- Check Discord Developer Portal for correct token
- Ensure bot has proper permissions

### Commands Don't Appear
- Global commands take up to 1 hour to register
- Check logs for registration errors
- Verify bot has `applications.commands` scope

### Permission Errors
- Ensure bot invited with correct permissions
- Check bot role position in server hierarchy
- Verify channel permissions if commands fail

### Web API Issues
- Check ports 5210/7103 aren't in use
- Verify .NET 8 SDK installed correctly
- Review startup logs for detailed errors

## Next Steps

After successful first run:
1. Create additional commands (see [creating-commands.md](../development/creating-commands.md))
2. Configure production deployment
3. Set up monitoring and logging
4. Review consent management for user data handling