# Consent System

## Overview

The consent system manages user opt-in preferences for data processing features, particularly AI/LLM integrations that analyze user messages.

## Architecture

### Core Interface
```csharp
public interface IConsentService
{
    Task<bool> HasUserConsentedAsync(ulong userId, string consentType);
    Task AddUserConsentAsync(ulong userId, string consentType);
    Task RemoveUserConsentAsync(ulong userId, string consentType);
}
```

### Current Implementation
- **Service**: `FileConsentService`
- **Storage**: JSON file (`consent.json`)
- **Data Model**: `UserConsent(ulong userId, string consentType)`

## Consent Types

### Predefined Types
- `"ai_chat"` - LLM message analysis and responses
- `"ai_sentiment"` - Sentiment analysis of messages
- `"ai_moderation"` - AI-powered content moderation
- `"data_analytics"` - Message analytics and insights

### Custom Types
Any string can be a consent type. Use descriptive, kebab-case names.

## Usage in Commands

### Check Consent Before AI Processing
```csharp
public class ChatCommand : CommandBase
{
    private readonly IConsentService _consentService;
    private readonly IAIService _aiService;

    protected override async Task ExecuteCommandAsync(SocketSlashCommand command)
    {
        if (!await _consentService.HasUserConsentedAsync(command.User.Id, "ai_chat"))
        {
            await command.RespondAsync(
                "AI chat requires consent. Use `/consent add ai_chat` to opt-in.",
                ephemeral: true);
            return;
        }

        var response = await _aiService.ProcessMessageAsync(userMessage);
        await command.RespondAsync(response);
    }
}
```

### Consent Management Commands
```csharp
[SlashCommand("consent", "Manage AI feature consent")]
public class ConsentCommand : CommandBase
{
    private readonly IConsentService _consentService;

    protected override void ConfigureCommand(SlashCommandBuilder builder)
    {
        builder.AddOption("action", ApplicationCommandOptionType.String, 
            "Action to perform", isRequired: true)
            .AddChoice("Add", "add")
            .AddChoice("Remove", "remove")
            .AddChoice("Check", "check");
            
        builder.AddOption("feature", ApplicationCommandOptionType.String,
            "AI feature", isRequired: true)
            .AddChoice("AI Chat", "ai_chat")
            .AddChoice("Sentiment Analysis", "ai_sentiment");
    }

    protected override async Task ExecuteCommandAsync(SocketSlashCommand command)
    {
        var action = command.Data.Options.First(x => x.Name == "action").Value.ToString();
        var feature = command.Data.Options.First(x => x.Name == "feature").Value.ToString();
        var userId = command.User.Id;

        switch (action)
        {
            case "add":
                await _consentService.AddUserConsentAsync(userId, feature);
                await command.RespondAsync($"✅ Opted into {feature}", ephemeral: true);
                break;
            case "remove":
                await _consentService.RemoveUserConsentAsync(userId, feature);
                await command.RespondAsync($"❌ Opted out of {feature}", ephemeral: true);
                break;
            case "check":
                var hasConsent = await _consentService.HasUserConsentedAsync(userId, feature);
                await command.RespondAsync($"{feature}: {(hasConsent ? "✅ Opted in" : "❌ Opted out")}", ephemeral: true);
                break;
        }
    }
}
```

## REST API

Consent management via HTTP endpoints:

### Check Consent
```http
GET /api/consent/check-consent?userId=123456789&consentType=ai_chat
```

### Add Consent
```http
POST /api/consent/add-consent?userId=123456789&consentType=ai_chat
```

### Remove Consent
```http
POST /api/consent/remove-consent?userId=123456789&consentType=ai_chat
```

## Data Storage

### File Format (consent.json)
```json
[
  { "userId": 123456789, "consentType": "ai_chat" },
  { "userId": 123456789, "consentType": "ai_sentiment" },
  { "userId": 987654321, "consentType": "ai_chat" }
]
```

### Migration to Database
Replace `FileConsentService` with database implementation:

```csharp
public class DatabaseConsentService : IConsentService
{
    private readonly DbContext _context;
    
    public async Task<bool> HasUserConsentedAsync(ulong userId, string consentType)
    {
        return await _context.Consents
            .AnyAsync(c => c.UserId == userId && c.ConsentType == consentType);
    }
}
```

## Best Practices

### Privacy Compliance
- Always check consent before AI processing
- Provide clear consent descriptions
- Allow easy opt-out
- Log consent changes for audit

### User Experience
- Use ephemeral responses for consent commands
- Provide helpful error messages when consent missing
- Group related consent types

### Development
- Use descriptive consent type names
- Document all consent types
- Test consent flows thoroughly
- Consider consent expiration policies