using ASHATAIServer.Services;
using ASHATAIServer.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ASHATAIServer.Controllers
{
    /// <summary>
    /// Admin endpoints for managing API keys and system settings
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApiKeyService _apiKeyService;
        private readonly UserDatabaseService _userDb;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApiKeyService apiKeyService,
            UserDatabaseService userDb,
            ILogger<AdminController> logger)
        {
            _apiKeyService = apiKeyService;
            _userDb = userDb;
            _logger = logger;
        }

        /// <summary>
        /// Verify admin session from Authorization header
        /// </summary>
        private async Task<(bool IsAdmin, int? UserId, string? Message)> VerifyAdminAsync()
        {
            // Get session token from Authorization header
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                return (false, null, "Authorization header required");
            }

            var token = authHeader.ToString();
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token["Bearer ".Length..].Trim();
            }

            // Validate session
            var (success, _, user) = await _userDb.ValidateSessionAsync(token);
            
            if (!success || user == null)
            {
                return (false, null, "Invalid or expired session");
            }

            // Check if user is admin
            if (user.Role != "Admin")
            {
                return (false, user.Id, "Admin privileges required");
            }

            return (true, user.Id, null);
        }

        /// <summary>
        /// Create a new API key
        /// POST /api/admin/keys/create
        /// </summary>
        [HttpPost("keys/create")]
        public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request)
        {
            var (isAdmin, userId, message) = await VerifyAdminAsync();
            
            if (!isAdmin)
            {
                return Unauthorized(new { success = false, message = message ?? "Unauthorized" });
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { success = false, message = "API key name is required" });
            }

            var (success, apiKey, errorMessage) = await _apiKeyService.CreateApiKeyAsync(
                request.Name,
                request.AssignToUserId,
                request.ExpiresAt,
                request.Scopes
            );

            if (!success)
            {
                return BadRequest(new { success = false, message = errorMessage });
            }

            _logger.LogInformation("API key created by admin user {AdminUserId}: {KeyName}", userId, request.Name);

            return Ok(new
            {
                success = true,
                message = "API key created successfully",
                apiKey = apiKey,
                warning = "Save this API key securely. It will not be shown again."
            });
        }

        /// <summary>
        /// List all API keys
        /// GET /api/admin/keys/list
        /// </summary>
        [HttpGet("keys/list")]
        public async Task<IActionResult> ListApiKeys([FromQuery] int? userId = null, [FromQuery] bool includeRevoked = false)
        {
            var (isAdmin, adminUserId, message) = await VerifyAdminAsync();
            
            if (!isAdmin)
            {
                return Unauthorized(new { success = false, message = message ?? "Unauthorized" });
            }

            var keys = await _apiKeyService.ListApiKeysAsync(userId, includeRevoked);

            return Ok(new
            {
                success = true,
                keys = keys
            });
        }

        /// <summary>
        /// Revoke an API key
        /// POST /api/admin/keys/revoke/{keyId}
        /// </summary>
        [HttpPost("keys/revoke/{keyId}")]
        public async Task<IActionResult> RevokeApiKey(int keyId)
        {
            var (isAdmin, adminUserId, message) = await VerifyAdminAsync();
            
            if (!isAdmin)
            {
                return Unauthorized(new { success = false, message = message ?? "Unauthorized" });
            }

            var (success, errorMessage) = await _apiKeyService.RevokeApiKeyAsync(keyId, adminUserId);

            if (!success)
            {
                return BadRequest(new { success = false, message = errorMessage });
            }

            _logger.LogInformation("API key {KeyId} revoked by admin user {AdminUserId}", keyId, adminUserId);

            return Ok(new
            {
                success = true,
                message = "API key revoked successfully"
            });
        }

        /// <summary>
        /// Get system health and statistics
        /// GET /api/admin/health
        /// </summary>
        [HttpGet("health")]
        public async Task<IActionResult> GetSystemHealth()
        {
            var (isAdmin, _, message) = await VerifyAdminAsync();
            
            if (!isAdmin)
            {
                return Unauthorized(new { success = false, message = message ?? "Unauthorized" });
            }

            var allKeys = await _apiKeyService.ListApiKeysAsync(includeRevoked: true);
            var activeKeys = allKeys.Where(k => !k.IsRevoked).ToList();

            return Ok(new
            {
                success = true,
                health = new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    apiKeys = new
                    {
                        total = allKeys.Count,
                        active = activeKeys.Count,
                        revoked = allKeys.Count - activeKeys.Count
                    }
                }
            });
        }
    }

    /// <summary>
    /// Request model for creating an API key
    /// </summary>
    public class CreateApiKeyRequest
    {
        public string Name { get; set; } = string.Empty;
        public int? AssignToUserId { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Scopes { get; set; }
    }
}
