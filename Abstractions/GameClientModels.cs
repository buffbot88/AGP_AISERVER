namespace Abstractions
{
    /// <summary>
    /// Platforms supported for game client generation.
    /// </summary>
    public enum ClientPlatform
    {
        WebGL,
        Windows,
        Linux,
        MacOS,
        Android,
        iOS
    }

    /// <summary>
    /// Configuration for a game client.
    /// </summary>
    public class ClientConfiguration
    {
        public string ServerUrl { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string GameTitle { get; set; } = string.Empty;
        public int ServerPort { get; set; } = 5000;
        public Dictionary<string, object> Settings { get; set; } = new();
        public string Theme { get; set; } = "default";
    }

    /// <summary>
    /// Package containing a generated game client.
    /// </summary>
    public class GameClientPackage
    {
        public Guid Id { get; set; }
        public Guid PackageId { get; set; }
        public Guid UserId { get; set; }
        public string LicenseKey { get; set; } = string.Empty;
        public ClientPlatform Platform { get; set; }
        public ClientConfiguration Configuration { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string PackagePath { get; set; } = string.Empty;
        public string ClientUrl { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public Dictionary<string, string> GeneratedFiles { get; set; } = new();
    }
}
