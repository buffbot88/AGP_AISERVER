using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

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
        private readonly ILogger<AuthenticationService> _logger;
        private readonly string _phpbbBaseUrl;

        public AuthenticationService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<AuthenticationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _phpbbBaseUrl = configuration["Authentication:PhpBBBaseUrl"] ?? configuration["Authentication:CmsBaseUrl"] ?? "http://localhost/phpbb";
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(_phpbbBaseUrl);
        }

        /// <summary>
        /// Sanitize username for safe logging (remove newlines and control characters to prevent log injection)
        /// </summary>
        private static string SanitizeForLogging(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            
            // Remove newlines, carriage returns, and other control characters that could be used for log injection
            return Regex.Replace(input, @"[\r\n\t\x00-\x1F\x7F]", "");
        }

        /// <summary>
        /// Authenticate a user with username and password
        /// </summary>
        public async Task<AuthenticationResult> LoginAsync(string username, string password)
        {
            try
            {
                _logger.LogDebug("Attempting login for user: {Username} via phpBB at {BaseUrl}", SanitizeForLogging(username), _phpbbBaseUrl);
                
                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new
                {
                    username,
                    password
                });

                if (response.IsSuccessStatusCode)
                {
                    // Check if response is JSON before attempting to parse
                    var contentType = response.Content.Headers.ContentType?.MediaType;
                    if (contentType != "application/json")
                    {
                        _logger.LogWarning("phpBB returned non-JSON response (Content-Type: {ContentType})", contentType);
                        return new AuthenticationResult
                        {
                            Success = false,
                            Message = "Authentication service configuration error. Please contact administrator."
                        };
                    }

                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result?.Success == true && result.SessionId != null && result.User != null)
                    {
                        _logger.LogDebug("Login successful for user: {Username}", SanitizeForLogging(username));
                        return new AuthenticationResult
                        {
                            Success = true,
                            SessionId = result.SessionId,
                            UserId = result.User.Id,
                            Username = result.User.Username,
                            Email = result.User.Email,
                            Role = result.User.Role,
                            Message = result.Message ?? "Login successful"
                        };
                    }
                }

                // Handle HTTP error status codes
                if ((int)response.StatusCode >= 500)
                {
                    _logger.LogError("phpBB server error (Status: {StatusCode})", response.StatusCode);
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Authentication service is temporarily unavailable. Please try again later."
                    };
                }

                // Try to parse error response if it's JSON
                var contentTypeError = response.Content.Headers.ContentType?.MediaType;
                if (contentTypeError == "application/json")
                {
                    try
                    {
                        var errorResult = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                        if (errorResult?.Message != null)
                        {
                            return new AuthenticationResult
                            {
                                Success = false,
                                Message = errorResult.Message
                            };
                        }
                    }
                    catch (JsonException)
                    {
                        _logger.LogWarning("Failed to parse error response as JSON");
                    }
                }

                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Invalid username or password"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to phpBB at {BaseUrl}", _phpbbBaseUrl);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Unable to connect to authentication service. Please check that phpBB is running and accessible."
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse response from phpBB (likely received HTML instead of JSON)");
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Authentication service configuration error. The service may not be properly configured."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please try again later."
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
                _logger.LogDebug("Attempting registration for user: {Username} via phpBB at {BaseUrl}", SanitizeForLogging(username), _phpbbBaseUrl);
                
                var response = await _httpClient.PostAsJsonAsync("/api/auth/register", new
                {
                    username,
                    email,
                    password
                });

                if (response.IsSuccessStatusCode)
                {
                    // Check if response is JSON before attempting to parse
                    var contentType = response.Content.Headers.ContentType?.MediaType;
                    if (contentType != "application/json")
                    {
                        _logger.LogWarning("phpBB returned non-JSON response (Content-Type: {ContentType})", contentType);
                        return new AuthenticationResult
                        {
                            Success = false,
                            Message = "Authentication service configuration error. Please contact administrator."
                        };
                    }

                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result?.Success == true && result.SessionId != null && result.User != null)
                    {
                        _logger.LogDebug("Registration successful for user: {Username}", SanitizeForLogging(username));
                        return new AuthenticationResult
                        {
                            Success = true,
                            SessionId = result.SessionId,
                            UserId = result.User.Id,
                            Username = result.User.Username,
                            Email = result.User.Email,
                            Role = result.User.Role,
                            Message = result.Message ?? "Registration successful"
                        };
                    }
                }

                // Handle HTTP error status codes
                if ((int)response.StatusCode >= 500)
                {
                    _logger.LogError("phpBB server error (Status: {StatusCode})", response.StatusCode);
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Authentication service is temporarily unavailable. Please try again later."
                    };
                }

                // Try to parse error response if it's JSON
                var contentTypeError = response.Content.Headers.ContentType?.MediaType;
                if (contentTypeError == "application/json")
                {
                    try
                    {
                        var errorResult = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                        if (errorResult?.Message != null)
                        {
                            return new AuthenticationResult
                            {
                                Success = false,
                                Message = errorResult.Message
                            };
                        }
                    }
                    catch (JsonException)
                    {
                        _logger.LogWarning("Failed to parse error response as JSON");
                    }
                }

                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Registration failed"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to phpBB at {BaseUrl}", _phpbbBaseUrl);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Unable to connect to authentication service. Please check that phpBB is running and accessible."
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse response from phpBB (likely received HTML instead of JSON)");
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Authentication service configuration error. The service may not be properly configured."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration");
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please try again later."
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
                _logger.LogDebug("Validating session via phpBB at {BaseUrl}", _phpbbBaseUrl);
                
                var response = await _httpClient.PostAsJsonAsync("/api/auth/validate", new
                {
                    sessionId
                });

                if (response.IsSuccessStatusCode)
                {
                    // Check if response is JSON before attempting to parse
                    var contentType = response.Content.Headers.ContentType?.MediaType;
                    if (contentType != "application/json")
                    {
                        _logger.LogWarning("phpBB returned non-JSON response (Content-Type: {ContentType})", contentType);
                        return new AuthenticationResult
                        {
                            Success = false,
                            Message = "Authentication service configuration error"
                        };
                    }

                    var result = await response.Content.ReadFromJsonAsync<ValidateResponse>();
                    if (result?.Success == true && result.User != null)
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
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to phpBB at {BaseUrl}", _phpbbBaseUrl);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Unable to connect to authentication service"
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse response from phpBB (likely received HTML instead of JSON)");
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Authentication service configuration error"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during session validation");
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Invalid or expired session"
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
