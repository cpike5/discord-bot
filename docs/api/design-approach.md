# API Design Approach

## Current State
- Basic consent endpoints exist (`ConsentController`)
- Web API framework configured
- Swagger documentation enabled

## Planned API Structure

### Bot Management
```http
GET /api/bot/status          # Bot connection status
GET /api/bot/guilds          # Connected servers
POST /api/bot/restart        # Restart bot service
```

### Command Management
```http
GET /api/commands            # List registered commands
POST /api/commands/register  # Re-register commands
GET /api/commands/usage      # Command usage statistics
```

### User Management
```http
GET /api/users/{userId}      # User profile
GET /api/users/{userId}/consent  # User consent status
PUT /api/users/{userId}/settings # User preferences
```

### Consent (Extended)
```http
GET /api/consent/{userId}                    # All user consents
POST /api/consent/{userId}/{consentType}     # Add consent
DELETE /api/consent/{userId}/{consentType}   # Remove consent
GET /api/consent/types                       # Available consent types
```

### Analytics (Future)
```http
GET /api/analytics/messages     # Message statistics
GET /api/analytics/commands     # Command usage
GET /api/analytics/users        # User activity
```

## Design Principles

### REST Conventions
- Use HTTP verbs appropriately
- Consistent URL patterns
- Proper status codes (200, 201, 404, 400)

### Authentication/Authorization
- API keys for external access
- Discord OAuth for user operations
- Role-based permissions

### Response Format
```json
{
  "success": true,
  "data": { ... },
  "error": null,
  "timestamp": "2025-07-30T10:00:00Z"
}
```

### Error Handling
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "INVALID_USER",
    "message": "User not found",
    "details": { "userId": 123456789 }
  },
  "timestamp": "2025-07-30T10:00:00Z"
}
```

## Implementation Strategy

### Phase 1: Core APIs
- Extend consent endpoints
- Add bot status endpoints
- Basic user management

### Phase 2: Management APIs
- Command registration/statistics
- Guild/channel management
- User preferences

### Phase 3: Analytics
- Usage tracking
- Performance metrics
- Reporting endpoints

## Security Considerations

- API key authentication
- Rate limiting
- Input validation
- Audit logging for sensitive operations