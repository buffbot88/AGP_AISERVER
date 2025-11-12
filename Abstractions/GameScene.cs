namespace Abstractions
{
    /// <summary>
    /// Represents a game scene/level/world.
    /// </summary>
    public class GameScene
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
        public List<GameEntity> Entities { get; set; } = new();
    }
}
