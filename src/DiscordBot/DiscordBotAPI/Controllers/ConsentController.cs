using DiscordBotAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DiscordBotAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsentController : ControllerBase
    {
        private readonly ILogger<ConsentController> _logger;
        private readonly IConsentService _consentService;

        public ConsentController(ILogger<ConsentController> logger, IConsentService consentService)
        {
            _logger = logger;
            _consentService = consentService;
        }

        [HttpGet("check-consent")]
        public async Task<IActionResult> CheckUserConsent(ulong userId, string consentType)
        {
            _logger.LogInformation("Checking consent for user {UserId} and consent type {ConsentType}", userId, consentType);
            var hasConsent = await _consentService.HasUserConsentedAsync(userId, consentType);
            return Ok(new { UserId = userId, ConsentType = consentType, HasConsent = hasConsent });
        }

        [HttpPost("add-consent")]
        public async Task<IActionResult> AddUserConsent(ulong userId, string consentType)
        {
            _logger.LogInformation("Adding consent for user {UserId} and consent type {ConsentType}", userId, consentType);
            await _consentService.AddUserConsentAsync(userId, consentType);
            return Ok(new { UserId = userId, ConsentType = consentType, Status = "Consent added successfully" });
        }

        [HttpPost("remove-consent")]
        public async Task<IActionResult> RemoveUserConsent(ulong userId, string consentType)
        {
            _logger.LogInformation("Removing consent for user {UserId} and consent type {ConsentType}", userId, consentType);
            await _consentService.RemoveUserConsentAsync(userId, consentType);
            return Ok(new { UserId = userId, ConsentType = consentType, Status = "Consent removed successfully" });
        }
    }
}
