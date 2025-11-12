namespace Abstractions
{
    /// <summary>
    /// Response from game engine operations.
    /// </summary>
    public class GameEngineResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }

    /// <summary>
    /// Request for AI-driven world generation.
    /// </summary>
    public class WorldGenerationRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public int EntityCount { get; set; } = 10;
        public bool GenerateNPCs { get; set; } = true;
        public bool GenerateTerrain { get; set; } = true;
        public bool GenerateQuests { get; set; } = false;
        public string Theme { get; set; } = "fantasy";
    }

    /// <summary>
    /// Request for asset streaming.
    /// </summary>
    public class AssetStreamRequest
    {
        public string AssetId { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Statistics for the game engine.
    /// </summary>
    public class EngineStats
    {
        public int TotalScenes { get; set; }
        public int TotalEntities { get; set; }
        public int ActiveScenes { get; set; }
        public TimeSpan Uptime { get; set; }
        public Dictionary<string, int> EntityTypeCount { get; set; } = new();
        public double MemoryUsageMB { get; set; }
        public DateTime StartTime { get; set; }
        public int ConnectedClients { get; set; }
    }
}
