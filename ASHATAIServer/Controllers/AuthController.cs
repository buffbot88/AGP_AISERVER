using ASHATAIServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASHATAIServer.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticationService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthenticationService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login attempt for user: {Username}", request.Username);

            var result = await _authService.LoginAsync(request.Username, request.Password);

            if (result.Success)
            {
                _logger.LogInformation("Login successful for user: {Username}", request.Username);
                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    sessionId = result.SessionId,
                    user = new
                    {
                        id = result.UserId,
                        username = result.Username,
                        email = result.Email,
                        role = result.Role
                    }
                });
            }

            _logger.LogWarning("Login failed for user: {Username}", request.Username);
            return Unauthorized(new
            {
                success = false,
                message = result.Message
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            _logger.LogInformation("Registration attempt for user: {Username}", request.Username);

            var result = await _authService.RegisterAsync(request.Username, request.Email, request.Password);

            if (result.Success)
            {
                _logger.LogInformation("Registration successful for user: {Username}", request.Username);
                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    sessionId = result.SessionId,
                    user = new
                    {
                        id = result.UserId,
                        username = result.Username,
                        email = result.Email,
                        role = result.Role
                    }
                });
            }

            _logger.LogWarning("Registration failed for user: {Username}", request.Username);
            return BadRequest(new
            {
                success = false,
                message = result.Message
            });
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateSession([FromBody] ValidateSessionRequest request)
        {
            _logger.LogInformation("Session validation request");

            var result = await _authService.ValidateSessionAsync(request.SessionId);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    user = new
                    {
                        id = result.UserId,
                        username = result.Username,
                        email = result.Email,
                        role = result.Role
                    }
                });
            }

            return Unauthorized(new
            {
                success = false,
                message = result.Message
            });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ValidateSessionRequest
    {
        public string SessionId { get; set; } = string.Empty;
    }
}
