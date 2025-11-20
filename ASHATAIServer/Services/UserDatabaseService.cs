using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Konscious.Security.Cryptography;

namespace ASHATAIServer.Services
{
    /// <summary>
    /// Database service for managing user accounts in SQLite
    /// </summary>
    public class UserDatabaseService
    {
        private readonly string _connectionString;
        private readonly ILogger<UserDatabaseService> _logger;

        public UserDatabaseService(IConfiguration configuration, ILogger<UserDatabaseService> logger)
        {
            _logger = logger;
            var dbPath = configuration["Database:Path"] ?? "users.db";
            _connectionString = $"Data Source={dbPath}";
            
            InitializeDatabase();
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

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    Email TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    Role TEXT NOT NULL DEFAULT 'User',
                    CreatedAt TEXT NOT NULL,
                    LastLoginAt TEXT
                );

                CREATE TABLE IF NOT EXISTS Sessions (
                    SessionId TEXT PRIMARY KEY,
                    UserId INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    ExpiresAt TEXT NOT NULL,
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );

                CREATE INDEX IF NOT EXISTS idx_username ON Users(Username);
                CREATE INDEX IF NOT EXISTS idx_email ON Users(Email);
                CREATE INDEX IF NOT EXISTS idx_sessions_user ON Sessions(UserId);
            ";
            command.ExecuteNonQuery();

            _logger.LogInformation("Database initialized successfully");
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        public async Task<(bool Success, string Message, int UserId)> RegisterUserAsync(string username, string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    return (false, "Username, email, and password are required", 0);
                }

                if (password.Length < 6)
                {
                    return (false, "Password must be at least 6 characters long", 0);
                }

                var passwordHash = HashPassword(password);

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Users (Username, Email, PasswordHash, Role, CreatedAt)
                    VALUES ($username, $email, $passwordHash, $role, $createdAt)
                    RETURNING Id;
                ";
                command.Parameters.AddWithValue("$username", username);
                command.Parameters.AddWithValue("$email", email);
                command.Parameters.AddWithValue("$passwordHash", passwordHash);
                command.Parameters.AddWithValue("$role", "User");
                command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("o"));

                var userId = Convert.ToInt32(await command.ExecuteScalarAsync());
                _logger.LogInformation("User registered successfully: {Username} (ID: {UserId})", SanitizeForLogging(username), userId);
                
                return (true, "Registration successful", userId);
            }
            catch (SqliteException ex) when (ex.Message.Contains("UNIQUE constraint failed"))
            {
                if (ex.Message.Contains("Username"))
                {
                    return (false, "Username already exists", 0);
                }
                else if (ex.Message.Contains("Email"))
                {
                    return (false, "Email already exists", 0);
                }
                return (false, "User already exists", 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user: {Username}", SanitizeForLogging(username));
                return (false, "An error occurred during registration", 0);
            }
        }

        /// <summary>
        /// Authenticate a user and create a session
        /// </summary>
        public async Task<(bool Success, string Message, string? SessionId, UserInfo? User)> LoginAsync(string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    return (false, "Username and password are required", null, null);
                }

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                // Get user
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Id, Username, Email, PasswordHash, Role
                    FROM Users
                    WHERE Username = $username;
                ";
                command.Parameters.AddWithValue("$username", username);

                using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return (false, "Invalid username or password", null, null);
                }

                var userId = reader.GetInt32(0);
                var storedUsername = reader.GetString(1);
                var email = reader.GetString(2);
                var passwordHash = reader.GetString(3);
                var role = reader.GetString(4);

                // Verify password
                if (!VerifyPassword(password, passwordHash))
                {
                    return (false, "Invalid username or password", null, null);
                }

                // Update last login
                var updateCommand = connection.CreateCommand();
                updateCommand.CommandText = @"
                    UPDATE Users
                    SET LastLoginAt = $lastLoginAt
                    WHERE Id = $userId;
                ";
                updateCommand.Parameters.AddWithValue("$lastLoginAt", DateTime.UtcNow.ToString("o"));
                updateCommand.Parameters.AddWithValue("$userId", userId);
                await updateCommand.ExecuteNonQueryAsync();

                // Create session
                var sessionId = GenerateSessionId();
                var sessionCommand = connection.CreateCommand();
                sessionCommand.CommandText = @"
                    INSERT INTO Sessions (SessionId, UserId, CreatedAt, ExpiresAt)
                    VALUES ($sessionId, $userId, $createdAt, $expiresAt);
                ";
                sessionCommand.Parameters.AddWithValue("$sessionId", sessionId);
                sessionCommand.Parameters.AddWithValue("$userId", userId);
                sessionCommand.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("o"));
                sessionCommand.Parameters.AddWithValue("$expiresAt", DateTime.UtcNow.AddDays(7).ToString("o"));
                await sessionCommand.ExecuteNonQueryAsync();

                var userInfo = new UserInfo
                {
                    Id = userId,
                    Username = storedUsername,
                    Email = email,
                    Role = role
                };

                _logger.LogInformation("User logged in successfully: {Username}", SanitizeForLogging(username));
                return (true, "Login successful", sessionId, userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login: {Username}", SanitizeForLogging(username));
                return (false, "An error occurred during login", null, null);
            }
        }

        /// <summary>
        /// Validate a session and return user info
        /// </summary>
        public async Task<(bool Success, string Message, UserInfo? User)> ValidateSessionAsync(string sessionId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return (false, "Session ID is required", null);
                }

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT u.Id, u.Username, u.Email, u.Role, s.ExpiresAt
                    FROM Sessions s
                    INNER JOIN Users u ON s.UserId = u.Id
                    WHERE s.SessionId = $sessionId;
                ";
                command.Parameters.AddWithValue("$sessionId", sessionId);

                using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return (false, "Invalid session", null);
                }

                var expiresAt = DateTime.Parse(reader.GetString(4));
                if (expiresAt < DateTime.UtcNow)
                {
                    // Clean up expired session
                    var deleteCommand = connection.CreateCommand();
                    deleteCommand.CommandText = "DELETE FROM Sessions WHERE SessionId = $sessionId;";
                    deleteCommand.Parameters.AddWithValue("$sessionId", sessionId);
                    await deleteCommand.ExecuteNonQueryAsync();

                    return (false, "Session expired", null);
                }

                var userInfo = new UserInfo
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Email = reader.GetString(2),
                    Role = reader.GetString(3)
                };

                return (true, "Session is valid", userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating session");
                return (false, "An error occurred during session validation", null);
            }
        }

        /// <summary>
        /// Clean up expired sessions
        /// </summary>
        public async Task CleanupExpiredSessionsAsync()
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Sessions WHERE ExpiresAt < $now;";
                command.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("o"));
                
                var deleted = await command.ExecuteNonQueryAsync();
                if (deleted > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} expired sessions", deleted);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired sessions");
            }
        }

        private static string HashPassword(string password)
        {
            // Generate a cryptographically secure salt
            byte[] salt = new byte[32]; // 256 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password with Argon2id
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8, // Number of threads
                MemorySize = 65536, // 64 MB
                Iterations = 4
            };

            byte[] hash = argon2.GetBytes(32); // 256-bit hash

            // Combine salt and hash for storage: salt(32 bytes) + hash(32 bytes) = 64 bytes
            byte[] combined = new byte[64];
            Buffer.BlockCopy(salt, 0, combined, 0, 32);
            Buffer.BlockCopy(hash, 0, combined, 32, 32);

            return Convert.ToBase64String(combined);
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                // Decode the stored hash
                byte[] combined = Convert.FromBase64String(storedHash);
                
                if (combined.Length != 64)
                {
                    // Invalid hash format - might be old SHA256 hash
                    return false;
                }

                // Extract salt and hash
                byte[] salt = new byte[32];
                byte[] hash = new byte[32];
                Buffer.BlockCopy(combined, 0, salt, 0, 32);
                Buffer.BlockCopy(combined, 32, hash, 0, 32);

                // Hash the provided password with the same salt
                using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
                {
                    Salt = salt,
                    DegreeOfParallelism = 8,
                    MemorySize = 65536,
                    Iterations = 4
                };

                byte[] testHash = argon2.GetBytes(32);

                // Compare hashes in constant time to prevent timing attacks
                return CryptographicOperations.FixedTimeEquals(hash, testHash);
            }
            catch
            {
                return false;
            }
        }

        private static string GenerateSessionId()
        {
            return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        }
    }

    /// <summary>
    /// User information model
    /// </summary>
    public class UserInfo
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
