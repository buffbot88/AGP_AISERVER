using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace LegendaryGameSystem.Networking
{
    /// <summary>
    /// WebSocket event broadcaster for real-time game engine events.
    /// Manages connections and broadcasts scene/entity changes to all connected clients.
    /// </summary>
    public class GameEngineWebSocketBroadcaster : IDisposable
    {
        private readonly ConcurrentDictionary<Guid, WebSocket> _clients = new();
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false };

        /// <summary>
        /// Register a WebSocket client for game engine events.
        /// </summary>
        public Guid RegisterClient(WebSocket webSocket)
        {
            var clientId = Guid.NewGuid();
            _clients[clientId] = webSocket;
            return clientId;
        }

        /// <summary>
        /// Unregister a WebSocket client.
        /// </summary>
        public void UnregisterClient(Guid clientId)
        {
            _clients.TryRemove(clientId, out _);
        }

        /// <summary>
        /// Get count of connected clients.
        /// </summary>
        public int ConnectedClients => _clients.Count;

        /// <summary>
        /// Broadcast a game engine event to all connected clients.
        /// </summary>
        public async Task BroadcastEventAsync(GameEngineEvent gameEvent, Guid? exceptClientId = null)
        {
            var json = JsonSerializer.Serialize(gameEvent, _jsonOptions);
            var buffer = Encoding.UTF8.GetBytes(json);

            var tasks = new List<Task>();

            foreach (var kvp in _clients.ToArray())
            {
                if (exceptClientId.HasValue && kvp.Key == exceptClientId.Value)
                    continue;

                if (kvp.Value.State == WebSocketState.Open)
                {
                    tasks.Add(SendToClientAsync(kvp.Key, kvp.Value, buffer));
                }
                else
                {
                    // Clean up dead connections
                    _clients.TryRemove(kvp.Key, out _);
                }
            }

            await Task.WhenAll(tasks);
        }

        private async Task SendToClientAsync(Guid clientId, WebSocket webSocket, byte[] buffer)
        {
            try
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            catch
            {
                // Remove failed client
                _clients.TryRemove(clientId, out _);
            }
        }

        public void Dispose()
        {
            foreach (var kvp in _clients.ToArray())
            {
                try
                {
                    if (kvp.Value.State == WebSocketState.Open)
                    {
                        kvp.Value.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Server shutting down",
                            CancellationToken.None).Wait(TimeSpan.FromSeconds(5));
                    }
                    kvp.Value.Dispose();
                }
                catch { /* ignore */ }
            }
            _clients.Clear();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Game engine event for WebSocket broadcasting.
    /// </summary>
    public class GameEngineEvent
    {
        public string EventType { get; set; } = string.Empty;
        public string SceneId { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public object? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Actor { get; set; } = string.Empty; // Who triggered the event
    }

    /// <summary>
    /// Event types for game engine Operations.
    /// </summary>
    public static class GameEngineEventTypes
    {
        public const string SceneCreated = "scene.created";
        public const string SceneUpdated = "scene.updated";
        public const string SceneDeleted = "scene.deleted";

        public const string EntityCreated = "entity.created";
        public const string EntityUpdated = "entity.updated";
        public const string EntityDeleted = "entity.deleted";

        public const string WorldGenerated = "world.Generated";
        public const string AssetStreamed = "asset.streamed";
    }
}
