namespace ASHATAIServer.Services
{
    /// <summary>
    /// Authentication service that manages user authentication using local database.
    /// Handles login, registration, and session validation.
    /// </summary>
    public partial class AuthenticationService(UserDatabaseService userDb, ILogger<AuthenticationService> logger)
    {
        private readonly UserDatabaseService _userDb = userDb;
        private readonly ILogger<AuthenticationService> _logger = logger;

        /// <summary>
        /// Sanitize username for safe logging (remove newlines and control characters to prevent log injection)
        /// </summary>
        private static string SanitizeForLogging(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Normalize to a canonical form to avoid tricky Unicode sequences
            var normalized = input.Normalize(System.Text.NormalizationForm.FormKC);

            // Remove Unicode control and formatting characters (Cc = control, Cf = format)
            var cleaned = MyRegex().Replace(normalized, "");
           
            // Collapse any remaining whitespace (including newlines) to a single space and trim
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " ").Trim();

            // Bound length to avoid log flooding or extremely long entries
            const int MaxLength = 256;
            if (cleaned.Length > MaxLength)
                cleaned = cleaned[..MaxLength];

            return cleaned;
        }

        /// <summary>
        /// Authenticate a user with username and password
        /// </summary>
        public async Task<AuthenticationResult> LoginAsync(string username, string password)
        {
            _logger.LogInformation("Login attempt for user: {Username}", SanitizeForLogging(username));

            var (Success, Message, SessionId, User) = await _userDb.LoginAsync(username, password);

            if (Success && SessionId != null && User != null)
            {
                _logger.LogInformation("Login successful for user: {Username}", SanitizeForLogging(username));
                return new AuthenticationResult
                {
                    Success = true,
                    SessionId = SessionId,
                    UserId = User.Id,
                    Username = User.Username,
                    Email = User.Email,
                    Role = User.Role,
                    Message = Message
                };
            }

            _logger.LogWarning("Login failed for user: {Username}", SanitizeForLogging(username));
            return new AuthenticationResult
            {
                Success = false,
                Message = Message
            };
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        public async Task<AuthenticationResult> RegisterAsync(string username, string email, string password)
        {
            _logger.LogInformation("Registration attempt for user: {Username}", SanitizeForLogging(username));
            var (Success, Message, _) = await _userDb.RegisterUserAsync(username, email, password);

            if (!Success)
            {
                _logger.LogWarning("Registration failed for user: {Username}", SanitizeForLogging(username));
                return new AuthenticationResult
                {
                    Success = false,
                    Message = Message
                };
            }

            // Auto-login after successful registration
            var loginResult = await _userDb.LoginAsync(username, password);

            if (loginResult.Success && loginResult.SessionId != null && loginResult.User != null)
            {
                _logger.LogInformation("Registration and auto-login successful for user: {Username}", SanitizeForLogging(username));
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

            var (Success, Message, User) = await _userDb.ValidateSessionAsync(sessionId);

            if (Success && User != null)
            {
                _logger.LogDebug("Session validation successful");
                return new AuthenticationResult
                {
                    Success = true,
                    SessionId = sessionId,
                    UserId = User.Id,
                    Username = User.Username,
                    Email = User.Email,
                    Role = User.Role,
                    Message = Message
                };
            }

            return new AuthenticationResult
            {
                Success = false,
                Message = Message
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

        [System.Text.RegularExpressions.GeneratedRegex(@"[\p{Cc}\p{Cf}]+")]
        private static partial System.Text.RegularExpressions.Regex MyRegex();
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
