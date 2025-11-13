namespace ASHATAIServer.Services
{
    /// <summary>
    /// Authentication service that manages user authentication using local database.
    /// Handles login, registration, and session validation.
    /// </summary>
    public class AuthenticationService
    {
        private readonly UserDatabaseService _userDb;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(UserDatabaseService userDb, ILogger<AuthenticationService> logger)
        {
            _userDb = userDb;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate a user with username and password
        /// </summary>
        public async Task<AuthenticationResult> LoginAsync(string username, string password)
        {
            _logger.LogInformation("Login attempt for user: {Username}", username);

            var result = await _userDb.LoginAsync(username, password);

            if (result.Success && result.SessionId != null && result.User != null)
            {
                _logger.LogInformation("Login successful for user: {Username}", username);
                return new AuthenticationResult
                {
                    Success = true,
                    SessionId = result.SessionId,
                    UserId = result.User.Id,
                    Username = result.User.Username,
                    Email = result.User.Email,
                    Role = result.User.Role,
                    Message = result.Message
                };
            }

            _logger.LogWarning("Login failed for user: {Username}", username);
            return new AuthenticationResult
            {
                Success = false,
                Message = result.Message
            };
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        public async Task<AuthenticationResult> RegisterAsync(string username, string email, string password)
        {
            _logger.LogInformation("Registration attempt for user: {Username}", username);

            var registerResult = await _userDb.RegisterUserAsync(username, email, password);

            if (!registerResult.Success)
            {
                _logger.LogWarning("Registration failed for user: {Username}", username);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = registerResult.Message
                };
            }

            // Auto-login after successful registration
            var loginResult = await _userDb.LoginAsync(username, password);

            if (loginResult.Success && loginResult.SessionId != null && loginResult.User != null)
            {
                _logger.LogInformation("Registration and auto-login successful for user: {Username}", username);
                return new AuthenticationResult
                {
                    Success = true,
                    SessionId = loginResult.SessionId,
                    UserId = loginResult.User.Id,
                    Username = loginResult.User.Username,
                    Email = loginResult.User.Email,
                    Role = loginResult.User.Role,
                    Message = "Registration successful"
                };
            }

            return new AuthenticationResult
            {
                Success = false,
                Message = "Registration successful but auto-login failed. Please try logging in."
            };
        }

        /// <summary>
        /// Validate a session token
        /// </summary>
        public async Task<AuthenticationResult> ValidateSessionAsync(string sessionId)
        {
            _logger.LogDebug("Validating session");

            var result = await _userDb.ValidateSessionAsync(sessionId);

            if (result.Success && result.User != null)
            {
                _logger.LogDebug("Session validation successful");
                return new AuthenticationResult
                {
                    Success = true,
                    SessionId = sessionId,
                    UserId = result.User.Id,
                    Username = result.User.Username,
                    Email = result.User.Email,
                    Role = result.User.Role,
                    Message = result.Message
                };
            }

            return new AuthenticationResult
            {
                Success = false,
                Message = result.Message
            };
        }

        // Response models (kept for backward compatibility)
        private class LoginResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public string? SessionId { get; set; }
            public UserDto? User { get; set; }
        }

        private class ValidateResponse
        {
            public bool Success { get; set; }
            public UserDto? User { get; set; }
        }

        private class UserDto
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        private class ErrorResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
        }
    }

    /// <summary>
    /// Result of an authentication operation
    /// </summary>
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? SessionId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
