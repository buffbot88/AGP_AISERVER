using ASHATAIServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASHATAIServer.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly AuthenticationService _authService;
        private readonly ILogger<UserController> _logger;

        public UserController(AuthenticationService authService, ILogger<UserController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Extract token from Authorization header
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("Missing or invalid Authorization header");
                return Unauthorized(new
                {
                    success = false,
                    message = "Authorization token required"
                });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            _logger.LogInformation("Validating session token");

            // Validate the session token
            var result = await _authService.ValidateSessionAsync(token);

            if (result.Success)
            {
                _logger.LogInformation("Session valid for user: {Username}", result.Username);
                return Ok(new
                {
                    userId = result.UserId,
                    username = result.Username,
                    email = result.Email,
                    isAdmin = result.Role == "Admin"
                });
            }

            _logger.LogWarning("Invalid or expired session token");
            return Unauthorized(new
            {
                success = false,
                message = result.Message
            });
        }
    }
}
