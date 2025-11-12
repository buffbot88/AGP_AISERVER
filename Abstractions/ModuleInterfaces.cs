namespace Abstractions
{
    /// <summary>
    /// Interface for Game Server modules that handle AI-driven game creation.
    /// </summary>
    public interface IGameServerModule
    {
        /// <summary>
        /// Create a game from a natural language description.
        /// </summary>
        Task<GameCreationResponse> CreateGameFromDescriptionAsync(GameCreationRequest request);

        /// <summary>
        /// Deploy a game server.
        /// </summary>
        Task<ServerDeploymentResponse> DeployGameServerAsync(string gameId, DeploymentOptions options);

        /// <summary>
        /// Get a preview of a game project.
        /// </summary>
        Task<GamePreview> GetGamePreviewAsync(string gameId);

        /// <summary>
        /// Update a game with new requirements.
        /// </summary>
        Task<GameUpdateResponse> UpdateGameAsync(string gameId, string updateDescription);

        /// <summary>
        /// List all games for a user.
        /// </summary>
        Task<List<GameProject>> ListUserGamesAsync(Guid userId);

        /// <summary>
        /// Get a specific game project.
        /// </summary>
        Task<GameProject?> GetGameProjectAsync(string gameId);

        /// <summary>
        /// Delete a game project.
        /// </summary>
        Task<GameServerResponse> DeleteGameProjectAsync(string gameId);

        /// <summary>
        /// Export a game project.
        /// </summary>
        Task<GameExportResponse> ExportGameProjectAsync(string gameId, ExportFormat format);

        /// <summary>
        /// Get server capabilities.
        /// </summary>
        Task<ServerCapabilities> GetCapabilitiesAsync();
    }

    /// <summary>
    /// Interface for Game Client modules that generate multi-platform clients.
    /// </summary>
    public interface IGameClientModule
    {
        /// <summary>
        /// Generate a game client for a specific platform.
        /// </summary>
        Task<GameClientPackage> GenerateClientAsync(Guid userId, string licenseKey, ClientPlatform platform, ClientConfiguration config);

        /// <summary>
        /// Update client configuration.
        /// </summary>
        Task<bool> UpdateClientConfigAsync(Guid packageId, ClientConfiguration config);
    }

    /// <summary>
    /// Interface for AI Content generation modules.
    /// </summary>
    public interface IAIContentModule
    {
        // Placeholder - methods would be defined based on implementation needs
    }

    /// <summary>
    /// Interface for Server Setup modules.
    /// </summary>
    public interface IServerSetupModule
    {
        /// <summary>
        /// Create admin folder structure for a project.
        /// </summary>
        Task<GameServerResponse> CreateAdminFolderStructureAsync(string licenseKey, string userId);
    }

    /// <summary>
    /// Status of a license.
    /// </summary>
    public enum LicenseStatus
    {
        Active,
        Expired,
        Suspended,
        Revoked
    }

    /// <summary>
    /// Represents a license.
    /// </summary>
    public class License
    {
        public Guid Id { get; set; }
        public string LicenseKey { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public LicenseStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// Interface for License modules.
    /// </summary>
    public interface ILicenseModule
    {
        /// <summary>
        /// Get all licenses.
        /// </summary>
        List<License> GetAllLicenses();
    }
}
