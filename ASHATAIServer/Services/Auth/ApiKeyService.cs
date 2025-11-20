using Microsoft.Data.Sqlite;
using System.Security.Cryptography;

namespace ASHATAIServer.Services.Auth
{
    /// <summary>
    /// Service for managing API keys with secure storage and validation
    /// </summary>
    public class ApiKeyService
    {
        private readonly string _connectionString;
        private readonly ILogger<ApiKeyService> _logger;

        public ApiKeyService(IConfiguration configuration, ILogger<ApiKeyService> logger)
        {
            _logger = logger;
            var dbPath = configuration["Database:Path"] ?? "users.db";
            _connectionString = $"Data Source={dbPath}";
            
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS ApiKeys (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    KeyHash TEXT NOT NULL UNIQUE,
                    Name TEXT NOT NULL,
                    UserId INTEGER,
                    CreatedAt TEXT NOT NULL,
                    ExpiresAt TEXT,
                    LastUsedAt TEXT,
                    IsRevoked INTEGER NOT NULL DEFAULT 0,
                    RevokedAt TEXT,
                    Scopes TEXT,
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );

                CREATE INDEX IF NOT EXISTS idx_apikeys_hash ON ApiKeys(KeyHash);
                CREATE INDEX IF NOT EXISTS idx_apikeys_user ON ApiKeys(UserId);
            ";
            command.ExecuteNonQuery();

            _logger.LogInformation("API Keys table initialized successfully");
        }

        /// <summary>
        /// Generate a new API key
        /// </summary>
        public async Task<(bool Success, string? ApiKey, string? Message)> CreateApiKeyAsync(
            string name, 
            int? userId = null, 
            DateTime? expiresAt = null,
            string? scopes = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return (false, null, "API key name is required");
                }

                // Generate a cryptographically secure API key
                var apiKey = GenerateApiKey();
                var keyHash = HashApiKey(apiKey);

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO ApiKeys (KeyHash, Name, UserId, CreatedAt, ExpiresAt, Scopes)
                    VALUES ($keyHash, $name, $userId, $createdAt, $expiresAt, $scopes)
                    RETURNING Id;
                ";
                command.Parameters.AddWithValue("$keyHash", keyHash);
                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$userId", userId.HasValue ? (object)userId.Value : DBNull.Value);
                command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("o"));
                command.Parameters.AddWithValue("$expiresAt", expiresAt.HasValue ? expiresAt.Value.ToString("o") : DBNull.Value);
                command.Parameters.AddWithValue("$scopes", scopes ?? "");

                var keyId = Convert.ToInt32(await command.ExecuteScalarAsync());
                _logger.LogInformation("API key created: {Name} (ID: {KeyId})", name, keyId);
                
                return (true, apiKey, "API key created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API key: {Name}", name);
                return (false, null, "An error occurred while creating API key");
            }
        }

        /// <summary>
        /// Validate an API key and return associated information
        /// </summary>
        public async Task<(bool IsValid, int? UserId, string? Scopes, string? Message)> ValidateApiKeyAsync(string apiKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return (false, null, null, "API key is required");
                }

                var keyHash = HashApiKey(apiKey);

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Id, UserId, ExpiresAt, IsRevoked, Scopes
                    FROM ApiKeys
                    WHERE KeyHash = $keyHash;
                ";
                command.Parameters.AddWithValue("$keyHash", keyHash);

                using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return (false, null, null, "Invalid API key");
                }

                var keyId = reader.GetInt32(0);
                var userId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1);
                var expiresAt = reader.IsDBNull(2) ? (DateTime?)null : DateTime.Parse(reader.GetString(2));
                var isRevoked = reader.GetInt32(3) == 1;
                var scopes = reader.GetString(4);

                if (isRevoked)
                {
                    return (false, null, null, "API key has been revoked");
                }

                if (expiresAt.HasValue && expiresAt.Value < DateTime.UtcNow)
                {
                    return (false, null, null, "API key has expired");
                }

                // Update last used timestamp
                var updateCommand = connection.CreateCommand();
                updateCommand.CommandText = @"
                    UPDATE ApiKeys
                    SET LastUsedAt = $lastUsedAt
                    WHERE Id = $keyId;
                ";
                updateCommand.Parameters.AddWithValue("$lastUsedAt", DateTime.UtcNow.ToString("o"));
                updateCommand.Parameters.AddWithValue("$keyId", keyId);
                await updateCommand.ExecuteNonQueryAsync();

                return (true, userId, scopes, "API key is valid");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating API key");
                return (false, null, null, "An error occurred during API key validation");
            }
        }

        /// <summary>
        /// Revoke an API key by its hash
        /// </summary>
        public async Task<(bool Success, string Message)> RevokeApiKeyAsync(int keyId, int? adminUserId = null)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE ApiKeys
                    SET IsRevoked = 1, RevokedAt = $revokedAt
                    WHERE Id = $keyId AND IsRevoked = 0;
                ";
                command.Parameters.AddWithValue("$revokedAt", DateTime.UtcNow.ToString("o"));
                command.Parameters.AddWithValue("$keyId", keyId);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                
                if (rowsAffected == 0)
                {
                    return (false, "API key not found or already revoked");
                }

                _logger.LogInformation("API key revoked: ID {KeyId} by user {AdminUserId}", keyId, adminUserId);
                return (true, "API key revoked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking API key: {KeyId}", keyId);
                return (false, "An error occurred while revoking API key");
            }
        }

        /// <summary>
        /// List all API keys (for admin purposes)
        /// </summary>
        public async Task<List<ApiKeyInfo>> ListApiKeysAsync(int? userId = null, bool includeRevoked = false)
        {
            var keys = new List<ApiKeyInfo>();

            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                var whereClause = new List<string>();
                
                if (userId.HasValue)
                {
                    whereClause.Add("UserId = $userId");
                    command.Parameters.AddWithValue("$userId", userId.Value);
                }
                
                if (!includeRevoked)
                {
                    whereClause.Add("IsRevoked = 0");
                }

                command.CommandText = $@"
                    SELECT Id, Name, UserId, CreatedAt, ExpiresAt, LastUsedAt, IsRevoked, RevokedAt, Scopes
                    FROM ApiKeys
                    {(whereClause.Count > 0 ? "WHERE " + string.Join(" AND ", whereClause) : "")}
                    ORDER BY CreatedAt DESC;
                ";

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    keys.Add(new ApiKeyInfo
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        UserId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                        CreatedAt = DateTime.Parse(reader.GetString(3)),
                        ExpiresAt = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
                        LastUsedAt = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5)),
                        IsRevoked = reader.GetInt32(6) == 1,
                        RevokedAt = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7)),
                        Scopes = reader.GetString(8)
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing API keys");
            }

            return keys;
        }

        /// <summary>
        /// Generate a cryptographically secure API key
        /// Format: agp_live_[64 random hex characters]
        /// </summary>
        private static string GenerateApiKey()
        {
            byte[] randomBytes = new byte[32]; // 256 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            
            var hex = Convert.ToHexString(randomBytes).ToLowerInvariant();
            return $"agp_live_{hex}";
        }

        /// <summary>
        /// Hash an API key for secure storage using SHA256
        /// </summary>
        private static string HashApiKey(string apiKey)
        {
            using var sha256 = SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(apiKey);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    /// <summary>
    /// Information about an API key (without the actual key)
    /// </summary>
    public class ApiKeyInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string Scopes { get; set; } = string.Empty;
    }
}
