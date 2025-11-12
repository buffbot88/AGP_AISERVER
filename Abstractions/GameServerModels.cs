namespace Abstractions
{
    /// <summary>
    /// Represents a game project created by the game server.
    /// </summary>
    public class GameProject
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public string Genre { get; set; } = string.Empty;
        public GameType Type { get; set; }
        public string Theme { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public string LicenseKey { get; set; } = string.Empty;
        public string ProjectPath { get; set; } = string.Empty;
        public Dictionary<string, string> FrontEndFiles { get; set; } = new();
        public Dictionary<string, string> BackEndFiles { get; set; } = new();
        public List<string> GeneratedAssets { get; set; } = new();
        public Dictionary<string, string> GeneratedFiles { get; set; } = new();
        public GameProjectStatus Status { get; set; }
        public ServerDeploymentInfo? DeploymentInfo { get; set; }
        public GameProjectMetrics Metrics { get; set; } = new();
    }

    /// <summary>
    /// Metrics for a game project.
    /// </summary>
    public class GameProjectMetrics
    {
        public int TotalAssets { get; set; }
        public int LinesOfCode { get; set; }
        public int FileCount { get; set; }
        public long TotalSizeBytes { get; set; }
    }

    /// <summary>
    /// Alias for GameProjectMetrics for backward compatibility.
    /// </summary>
    public class GameMetrics : GameProjectMetrics
    {
    }

    /// <summary>
    /// Status of a game project.
    /// </summary>
    public enum GameProjectStatus
    {
        Draft,
        InDevelopment,
        ReadyForDeployment,
        Deployed,
        Archived
    }

    /// <summary>
    /// Status of a server.
    /// </summary>
    public enum ServerStatus
    {
        Starting,
        Running,
        Stopped,
        Error
    }

    /// <summary>
    /// Information about a deployed game server.
    /// </summary>
    public class ServerDeploymentInfo
    {
        public string GameId { get; set; } = string.Empty;
        public string InstanceId { get; set; } = string.Empty;
        public string ServerUrl { get; set; } = string.Empty;
        public DateTime DeployedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public Dictionary<string, object> Configuration { get; set; } = new();
        public Dictionary<string, object> Config { get; set; } = new();
    }

    /// <summary>
    /// Game types supported by the system.
    /// </summary>
    public enum GameType
    {
        RPG,
        MMO,
        FPS,
        Strategy,
        Puzzle,
        Adventure,
        Simulation,
        SinglePlayer,
        Multiplayer,
        PvP,
        CoOperative,
        Sandbox,
        Other
    }

    /// <summary>
    /// Request for creating a game from description.
    /// </summary>
    public class GameCreationRequest
    {
        public Guid UserId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Genre { get; set; }
        public GameType GameType { get; set; } = GameType.RPG;
        public string Theme { get; set; } = "fantasy";
        public List<string> Features { get; set; } = new();
        public string LicenseKey { get; set; } = string.Empty;
        public bool GenerateAssets { get; set; } = true;
        public bool AutoDeploy { get; set; } = false;
        public Dictionary<string, object>? Options { get; set; }
    }

    /// <summary>
    /// Response from game creation.
    /// </summary>
    public class GameCreationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public GameProject? Project { get; set; }
        public string GameId { get; set; } = string.Empty;
        public string ProjectPath { get; set; } = string.Empty;
        public List<string> GeneratedFiles { get; set; } = new();
    }

    /// <summary>
    /// Options for server deployment.
    /// </summary>
    public class DeploymentOptions
    {
        public string Environment { get; set; } = "production";
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
        public int Port { get; set; } = 5000;
        public int MaxPlayers { get; set; } = 100;
        public bool EnableWebSocket { get; set; } = true;
        public bool EnableDatabase { get; set; } = true;
        public string InstanceName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response from server deployment.
    /// </summary>
    public class ServerDeploymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ServerDeploymentInfo? Deployment { get; set; }
        public string ServerUrl { get; set; } = string.Empty;
        public string InstanceId { get; set; } = string.Empty;
        public ServerStatus Status { get; set; }
        public Dictionary<string, object> ConnectionInfo { get; set; } = new();
    }

    /// <summary>
    /// Preview of a game project.
    /// </summary>
    public class GamePreview
    {
        public string GameId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public int AssetCount { get; set; }
        public DateTime LastUpdated { get; set; }
        public string PreviewUrl { get; set; } = string.Empty;
        public Dictionary<string, object> GameInfo { get; set; } = new();
    }

    /// <summary>
    /// Response from game update.
    /// </summary>
    public class GameUpdateResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public GameProject? UpdatedProject { get; set; }
        public List<string> ModifiedFiles { get; set; } = new();
        public Dictionary<string, object> Changes { get; set; } = new();
    }

    /// <summary>
    /// Response from generic game server operations.
    /// </summary>
    public class GameServerResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }

    /// <summary>
    /// Format for game export.
    /// </summary>
    public enum ExportFormat
    {
        ZipArchive,
        SourceCode,
        Binary,
        Complete
    }

    /// <summary>
    /// Response from game export.
    /// </summary>
    public class GameExportResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FilePath { get; set; }
        public byte[]? Data { get; set; }
        public ExportFormat Format { get; set; }
        public string ExportPath { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Server capabilities for game creation.
    /// </summary>
    public class ServerCapabilities
    {
        public List<string> SupportedGenres { get; set; } = new();
        public List<string> SupportedPlatforms { get; set; } = new();
        public bool AIContentGeneration { get; set; }
        public bool AutomaticDeployment { get; set; }
        public int MaxProjectsPerUser { get; set; }
        public int MaxConcurrentServers { get; set; }
        public int ActiveServers { get; set; }
        public List<string> SupportedGameTypes { get; set; } = new();
        public List<string> AvailableFeatures { get; set; } = new();
        public Dictionary<string, object> SystemInfo { get; set; } = new();
    }

}
