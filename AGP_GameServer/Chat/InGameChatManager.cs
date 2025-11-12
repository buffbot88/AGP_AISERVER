using System.Collections.Concurrent;
using Abstractions;

namespace LegendaryGameSystem.Chat
{
    /// <summary>
    /// In-Game Chat Manager - Manages chat rooms and messages within game instances.
    /// SepaRate from the main CMS website chat system.
    /// </summary>
    public class InGameChatManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, GameChatRoom> _rooms = new();
        private readonly ConcurrentDictionary<string, List<GameChatMessage>> _roomMessages = new();
        private readonly ConcurrentDictionary<string, HashSet<string>> _roomParticipants = new();

        /// <summary>
        /// Create a new in-game chat room for a specific scene/game instance.
        /// </summary>
        public (bool success, string message, string? roomId) CreateRoom(string sceneId, string roomName, string createdBy)
        {
            var roomId = $"game_{sceneId}_{Guid.NewGuid():N}";
            var room = new GameChatRoom
            {
                Id = roomId,
                SceneId = sceneId,
                Name = roomName,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            if (_rooms.TryAdd(roomId, room))
            {
                _roomMessages.TryAdd(roomId, new List<GameChatMessage>());
                _roomParticipants.TryAdd(roomId, new HashSet<string>());
                return (true, "In-game chat room created successfully", roomId);
            }

            return (false, "Failed to create in-game chat room", null);
        }

        /// <summary>
        /// Send a message to an in-game chat room.
        /// </summary>
        public (bool success, string message, string? messageId) SendMessage(string roomId, string userId, string username, string content)
        {
            if (!_rooms.ContainsKey(roomId))
                return (false, "Chat room not found", null);

            var messageId = Guid.NewGuid().ToString("N");
            var msg = new GameChatMessage
            {
                Id = messageId,
                RoomId = roomId,
                UserId = userId,
                Username = username,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            if (_roomMessages.TryGetValue(roomId, out var messages))
            {
                lock (messages)
                {
                    messages.Add(msg);
                    // Keep only last 200 messages per room
                    if (messages.Count > 200)
                    {
                        messages.RemoveAt(0);
                    }
                }
            }

            return (true, "Message sent", messageId);
        }

        /// <summary>
        /// Get recent messages from an in-game chat room.
        /// </summary>
        public List<GameChatMessage> GetMessages(string roomId, int limit = 50)
        {
            if (_roomMessages.TryGetValue(roomId, out var messages))
            {
                lock (messages)
                {
                    return messages.TakeLast(limit).ToList();
                }
            }
            return new List<GameChatMessage>();
        }

        /// <summary>
        /// Join a player to an in-game chat room.
        /// </summary>
        public bool JoinRoom(string roomId, string userId)
        {
            if (!_rooms.ContainsKey(roomId))
                return false;

            if (_roomParticipants.TryGetValue(roomId, out var participants))
            {
                lock (participants)
                {
                    participants.Add(userId);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove a player from an in-game chat room.
        /// </summary>
        public bool LeaveRoom(string roomId, string userId)
        {
            if (_roomParticipants.TryGetValue(roomId, out var participants))
            {
                lock (participants)
                {
                    return participants.Remove(userId);
                }
            }
            return false;
        }

        /// <summary>
        /// Get all in-game chat rooms for a specific scene.
        /// </summary>
        public List<GameChatRoom> GetRoomsForScene(string sceneId)
        {
            return _rooms.Values
                .Where(r => r.SceneId == sceneId && r.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        /// <summary>
        /// Get active participants in a room.
        /// </summary>
        public List<string> GetRoomParticipants(string roomId)
        {
            if (_roomParticipants.TryGetValue(roomId, out var participants))
            {
                lock (participants)
                {
                    return participants.ToList();
                }
            }
            return new List<string>();
        }

        /// <summary>
        /// Close an in-game chat room.
        /// </summary>
        public bool CloseRoom(string roomId)
        {
            if (_rooms.TryGetValue(roomId, out var room))
            {
                room.IsActive = false;
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            _rooms.Clear();
            _roomMessages.Clear();
            _roomParticipants.Clear();
        }
    }

    /// <summary>
    /// Represents an in-game chat room tied to a game scene.
    /// </summary>
    public class GameChatRoom
    {
        public string Id { get; set; } = string.Empty;
        public string SceneId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Represents a chat message in an in-game chat room.
    /// </summary>
    public class GameChatMessage
    {
        public string Id { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
