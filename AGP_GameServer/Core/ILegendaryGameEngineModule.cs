using Abstractions;

namespace LegendaryGameSystem.Core
{
    /// <summary>
    /// Interface for Legendary Game Engine - extends base game engine with advanced features.
    /// </summary>
    public interface ILegendaryGameEngineModule : IGameEngineModule
    {
        /// <summary>
        /// Create an in-game chat room for a specific scene.
        /// </summary>
        Task<(bool success, string message, string? roomId)> CreateInGameChatRoomAsync(string sceneId, string roomName, string createdBy);

        /// <summary>
        /// Send a message to an in-game chat room.
        /// </summary>
        Task<(bool success, string message, string? messageId)> SendInGameChatMessageAsync(string roomId, string userId, string username, string content);

        /// <summary>
        /// Get messages from an in-game chat room.
        /// </summary>
        Task<List<Chat.GameChatMessage>> GetInGameChatMessagesAsync(string roomId, int limit = 50);

        /// <summary>
        /// Get all in-game chat rooms for a scene.
        /// </summary>
        Task<List<Chat.GameChatRoom>> GetInGameChatRoomsForSceneAsync(string sceneId);
    }
}
