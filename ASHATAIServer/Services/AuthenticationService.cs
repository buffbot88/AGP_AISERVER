using System.Net.Http.Json;
using System.Text.Json;

namespace ASHATAIServer.Services
{
    /// <summary>
    /// Authentication service that communicates with phpBB3 for user authentication.
    /// Handles login, registration, and session validation via the ASHATOS Authentication Bridge extension.
    /// </summary>
    public class AuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _phpbbBaseUrl;

        public AuthenticationService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _phpbbBaseUrl = configuration["Authentication:PhpBBBaseUrl"] ?? configuration["Authentication:CmsBaseUrl"] ?? "http://localhost/phpbb";
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(_phpbbBaseUrl);
        }

        /// <summary>
        /// Authenticate a user with username and password
        /// </summary>
        public async Task<AuthenticationResult> LoginAsync(string username, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new
                {
                    username,
                    password
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result?.Success == true && result.SessionId != null && result.User != null)
                    {
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
                }

                var errorResult = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return new AuthenticationResult
                {
                    Success = false,
                    Message = errorResult?.Message ?? "Login failed"
                };
            }
            catch (Exception ex)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = $"Authentication service error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        public async Task<AuthenticationResult> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/register", new
                {
                    username,
                    email,
                    password
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result?.Success == true && result.SessionId != null && result.User != null)
                    {
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
                }

                var errorResult = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return new AuthenticationResult
                {
                    Success = false,
                    Message = errorResult?.Message ?? "Registration failed"
                };
            }
            catch (Exception ex)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = $"Authentication service error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Validate a session token
        /// </summary>
        public async Task<AuthenticationResult> ValidateSessionAsync(string sessionId)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/auth/validate", new
                {
                    sessionId
                });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ValidateResponse>();
                    if (result?.Success == true && result.User != null)
                    {
                        return new AuthenticationResult
                        {
                            Success = true,
                            SessionId = sessionId,
                            UserId = result.User.Id,
                            Username = result.User.Username,
                            Email = result.User.Email,
                            Role = result.User.Role,
                            Message = "Session is valid"
                        };
                    }
                }

                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Invalid or expired session"
                };
            }
            catch (Exception ex)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = $"Session validation error: {ex.Message}"
                };
            }
        }

        // Response models
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
