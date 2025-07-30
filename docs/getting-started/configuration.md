# Configuration Guide

## Required Configuration

### Discord Bot Token

1. **Create Discord Application**:
   - Go to [Discord Developer Portal](https://discord.com/developers/applications)
   - Click "New Application" and name your bot
   - Navigate to "Bot" section and click "Add Bot"
   - Copy the bot token

2. **Store Bot Token Securely**:

   **Development (User Secrets)**:
   ```bash
   dotnet user-secrets set "DiscordBot:Token" "YOUR_BOT_TOKEN_HERE"
   dotnet user-secrets set "DiscordBot:ClientId" "YOUR_CLIENT_ID_HERE"
   ```

   **Production (Environment Variables)**:
   ```bash
   export DiscordBot__Token="YOUR_BOT_TOKEN_HERE"
   export DiscordBot__ClientId="YOUR_CLIENT_ID_HERE"
   ```

### Bot Permissions

Required Discord permissions for your bot:
- `applications.commands` (slash commands)
- `bot` (basic bot functionality)

Bot permissions integer: `2147483648`

### Invite Bot to Server

Use this URL template (replace `YOUR_CLIENT_ID`):
```
https://discord.com/api/oauth2/authorize?client_id=YOUR_CLIENT_ID&permissions=2147483648&scope=bot%20applications.commands
```

## Configuration Structure

### appsettings.json
```json
{
  "DiscordBot": {
    "Token": "",
    "ClientId": "",
    "DefaultGuildId": "",
    "DefaultChannelId": ""
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    }
  },
  "AllowedHosts": "*"
}
```

### BotOptions Class
Maps to `DiscordBot` configuration section:

```csharp
public class BotOptions
{
    public string Token { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string DefaultGuildId { get; set; } = string.Empty;
    public string DefaultChannelId { get; set; } = string.Empty;
}
```

## Environment-Specific Settings

### Development (appsettings.Development.json)
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    }
  }
}
```

### Production Considerations
- Use environment variables for sensitive values
- Set appropriate log levels (Information or Warning)
- Configure external log sinks (file, database, etc.)

## Logging Configuration

### Serilog Setup
Default configuration includes:
- Console output
- Structured logging with context enrichment
- Environment-specific log levels

### Custom Sinks (Optional)
Add to `Program.cs`:
```csharp
builder.Services.AddSerilog(logger =>
{
    logger.ReadFrom.Configuration(builder.Configuration)
          .Enrich.FromLogContext()
          .WriteTo.Console()
          .WriteTo.File("logs/bot-.txt", rollingInterval: RollingInterval.Day);
});
```

## Validation

### Required Values
- `DiscordBot:Token` - Discord bot token (required)
- `DiscordBot:ClientId` - Discord application client ID (required)

### Optional Values
- `DefaultGuildId` - For guild-specific commands (optional)
- `DefaultChannelId` - For bot announcements (optional)

## Troubleshooting

### Common Issues

**Invalid Token Error**:
- Verify token is correct and not expired
- Ensure no extra spaces or characters
- Check token has proper bot permissions

**Command Registration Fails**:
- Verify `ClientId` matches your Discord application
- Ensure bot has `applications.commands` scope
- Check bot is added to server with correct permissions

**Connection Issues**:
- Verify internet connectivity
- Check Discord API status
- Review firewall/proxy settings

### Debug Configuration
Enable debug logging to troubleshoot:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Discord": "Debug"
      }
    }
  }
}
```