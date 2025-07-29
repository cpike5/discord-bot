using System.Text.Json;

namespace DiscordBotAPI.Services
{
    public interface IConsentService
    {
        /// <summary>
        /// Adds a user's consent for a specific type.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="consentType"></param>
        /// <returns></returns>
        Task AddUserConsentAsync(ulong userId, string consentType);
        /// <summary>
        /// Checks if a user has given consent for a specific type.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="consentType"></param>
        /// <returns></returns>
        Task<bool> HasUserConsentedAsync(ulong userId, string consentType);
        /// <summary>
        /// Removes a user's consent for a specific type.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="consentType"></param>
        /// <returns></returns>
        Task RemoveUserConsentAsync(ulong userId, string consentType);
    }

    /// <summary>
    /// File-based implementation of the consent service.
    /// </summary>
    public class FileConsentService : IConsentService
    {
        private readonly ILogger<FileConsentService> _logger;
        private readonly string _consentFilePath = "consent.json";
        private IEnumerable<UserConsent> _consents;

        public FileConsentService(ILogger<FileConsentService> logger)
        {
            _logger = logger;
            _consents = LoadConsentList();
        }

        public async Task AddUserConsentAsync(ulong userId, string consentType)
        {
            if (string.IsNullOrWhiteSpace(consentType))
            {
                throw new ArgumentException("Consent type cannot be null or empty.", nameof(consentType));
            }
            if (_consents.Any(c => c.userId == userId && c.consentType == consentType))
            {
                _logger.LogInformation("User {UserId} already has consent for {ConsentType}.", userId, consentType);
            }
            else
            {
                _consents = _consents.Append(new UserConsent(userId, consentType));
                await SaveConsents();
            }
        }

        public Task<bool> HasUserConsentedAsync(ulong userId, string consentType)
        {
            if (string.IsNullOrWhiteSpace(consentType))
            {
                throw new ArgumentException("Consent type cannot be null or empty.", nameof(consentType));
            }
            var hasConsented = _consents.Any(c => c.userId == userId && c.consentType == consentType);
            _logger.LogInformation("User {UserId} has consented to {ConsentType}: {HasConsented}.", userId, consentType, hasConsented);
            return Task.FromResult(hasConsented);
        }

        public Task RemoveUserConsentAsync(ulong userId, string consentType)
        {
            if (string.IsNullOrWhiteSpace(consentType))
            {
                throw new ArgumentException("Consent type cannot be null or empty.", nameof(consentType));
            }
            if (_consents.Any(c => c.userId == userId && c.consentType == consentType))
            {
                _consents = _consents.Where(c => !(c.userId == userId && c.consentType == consentType));
                _logger.LogInformation("User {UserId} consent for {ConsentType} removed.", userId, consentType);
                return SaveConsents();
            }
            else
            {
                _logger.LogInformation("User {UserId} does not have consent for {ConsentType}.", userId, consentType);
                return Task.CompletedTask;
            }
        }


        /// <summary>
        /// Saves the current consent list to the file.
        /// </summary>
        /// <returns></returns>
        private async Task SaveConsents()
        {
            if (_consents == null || !_consents.Any())
            {
                _logger.LogTrace("No consents to save.");
            }
            var json = JsonSerializer.Serialize(_consents);
            try
            {
                await File.WriteAllTextAsync(_consentFilePath, json);
                _logger.LogInformation("Consent data saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save consent data.");
            }
        }

        /// <summary>
        /// Loads the consent list from the file.
        /// </summary>
        /// <returns></returns>
        private List<UserConsent> LoadConsentList()
        {
            if (!File.Exists(_consentFilePath))
            {
                return new List<UserConsent>();
            }
            var json = File.ReadAllText(_consentFilePath);
            return JsonSerializer.Deserialize<List<UserConsent>>(json) ?? new List<UserConsent>();
        }

    }

    public record UserConsent(ulong userId, string consentType);
}
