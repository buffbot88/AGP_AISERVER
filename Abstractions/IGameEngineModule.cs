namespace Abstractions
{
    /// <summary>
    /// Interface for Game Engine modules.
    /// </summary>
    public interface IGameEngineModule
    {
        /// <summary>
        /// Create a new game scene/level/world.
        /// </summary>
        Task<GameEngineResponse> CreateSceneAsync(string sceneName, string createdBy);

        /// <summary>
        /// List all game scenes.
        /// </summary>
        Task<List<GameScene>> ListScenesAsync();

        /// <summary>
        /// Get a specific scene by ID.
        /// </summary>
        Task<GameScene?> GetSceneAsync(string sceneId);

        /// <summary>
        /// Delete a game scene.
        /// </summary>
        Task<GameEngineResponse> DeleteSceneAsync(string sceneId, string deletedBy);

        /// <summary>
        /// Create an entity in a scene.
        /// </summary>
        Task<GameEngineResponse> CreateEntityAsync(string sceneId, GameEntity entity, string createdBy);

        /// <summary>
        /// Update an entity in a scene.
        /// </summary>
        Task<GameEngineResponse> UpdateEntityAsync(string sceneId, string entityId, GameEntity entity, string updatedBy);

        /// <summary>
        /// Delete an entity from a scene.
        /// </summary>
        Task<GameEngineResponse> DeleteEntityAsync(string sceneId, string entityId, string deletedBy);

        /// <summary>
        /// List all entities in a scene.
        /// </summary>
        Task<List<GameEntity>> ListEntitiesAsync(string sceneId);

        /// <summary>
        /// Generate world content using AI.
        /// </summary>
        Task<GameEngineResponse> GenerateWorldContentAsync(string sceneId, WorldGenerationRequest request, string requestedBy);

        /// <summary>
        /// Stream an asset to clients.
        /// </summary>
        Task<GameEngineResponse> StreamAssetAsync(AssetStreamRequest request);

        /// <summary>
        /// Get engine statistics.
        /// </summary>
        Task<EngineStats> GetStatsAsync();

        /// <summary>
        /// Get all scenes.
        /// </summary>
        List<GameScene> GetAllScenes();
    }
}
