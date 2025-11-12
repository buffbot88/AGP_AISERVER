using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Abstractions;
using LegendaryGameSystem.Chat;
using LegendaryGameSystem.Database;
using LegendaryGameSystem.Networking;

namespace LegendaryGameSystem.Core
{
    /// <summary>
    /// Legendary Game Engine Module - Advanced game engine with Unreal Engine-inspired features.
    /// Provides scene management, entity creation, AI-driven world Generation, physics, and in-game chat.
    /// </summary>
    [RaModule(Category = "extensions")]
    public sealed class LegendaryGameEngineModule : ModuleBase, IGameEngineModule, ILegendaryGameEngineModule
    {
        public override string Name => "LegendaryGameEngine";

        private object? _manager;
        private readonly ConcurrentDictionary<string, GameScene> _scenes = new();
        private readonly ConcurrentDictionary<string, Asset> _assets = new();
        private readonly DateTime _startTime = DateTime.UtcNow;
        private readonly GameEngineDatabase _database;
        private readonly GameEngineWebSocketBroadcaster _broadcaster;
        private readonly InGameChatManager _inGameChat;
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public LegendaryGameEngineModule()
        {
            _database = new GameEngineDatabase();
            _broadcaster = new GameEngineWebSocketBroadcaster();
            _inGameChat = new InGameChatManager();
        }

        public override void Initialize(object? manager)
        {
            base.Initialize(manager);
            _manager = manager;

            // Load existing scenes from database
            var savedScenes = _database.LoadAllScenes();
            foreach (var scene in savedScenes)
            {
                _scenes.TryAdd(scene.Id, scene);
            }

            LogInfo($"Legendary Game Engine module initialized - Loaded {savedScenes.Count} scenes from database");
            LogInfo("Legendary Game Engine ready with advanced features, physics, and in-game chat");
        }

        public override void Dispose()
        {
            _inGameChat?.Dispose();
            _broadcaster?.Dispose();
            _database?.Dispose();
            base.Dispose();
        }

        public override string Process(string input)
        {
            var text = (input ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                return GetHelp();
            }

            if (text.Equals("engine status", StringComparison.OrdinalIgnoreCase))
            {
                return GetStatusSync();
            }

            if (text.Equals("engine stats", StringComparison.OrdinalIgnoreCase))
            {
                return GetStatsSync();
            }

            if (text.StartsWith("engine scene create ", StringComparison.OrdinalIgnoreCase))
            {
                var sceneName = text["engine scene create ".Length..].Trim();
                return CreateSceneSync(sceneName, "console");
            }

            if (text.Equals("engine scene list", StringComparison.OrdinalIgnoreCase))
            {
                return ListScenesSync();
            }

            if (text.StartsWith("engine scene delete ", StringComparison.OrdinalIgnoreCase))
            {
                var sceneId = text["engine scene delete ".Length..].Trim();
                return DeleteSceneSync(sceneId, "console");
            }

            if (text.StartsWith("engine entity create ", StringComparison.OrdinalIgnoreCase))
            {
                var args = text["engine entity create ".Length..].Trim();
                return CreateEntitySync(args, "console");
            }

            if (text.StartsWith("engine Generate ", StringComparison.OrdinalIgnoreCase))
            {
                var args = text["engine Generate ".Length..].Trim();
                return GenerateWorldContentSync(args, "console");
            }

            return "Unknown engine command. Type 'help' for available commands.";
        }

        private string GetHelp()
        {
            return string.Join(Environment.NewLine,
                "Game Engine commands:",
                "  engine status                      - Show engine status and Configuration",
                "  engine stats                       - Show engine statistics and metrics",
                "  engine scene create <name>         - Create a new game scene",
                "  engine scene list                  - List all scenes",
                "  engine scene delete <sceneId>      - Delete a scene",
                "  engine entity create <sceneId> <name> <type> - Create an entity in a scene",
                "  engine Generate <sceneId> <prompt> - AI-Generate world content",
                "  help                               - Show this help message",
                "",
                "Examples:",
                "  engine scene create Medieval Town",
                "  engine entity create scene123 Blacksmith NPC",
                "  engine Generate scene123 Generate a medieval town with 10 NPCs and market district"
            );
        }

        #region Async API Methods (IGameEngineModule)

        public async Task<GameEngineResponse> CreateSceneAsync(string sceneName, string createdBy)
        {
            var scene = new GameScene
            {
                Id = Guid.NewGuid().ToString(),
                Name = sceneName,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            if (_scenes.TryAdd(scene.Id, scene))
            {
                // Save to database
                _database.SaveScene(scene);

                // Broadcast event
                await _broadcaster.BroadcastEventAsync(new GameEngineEvent
                {
                    EventType = GameEngineEventTypes.SceneCreated,
                    SceneId = scene.Id,
                    Data = scene,
                    Actor = createdBy
                });

                LogInfo($"Scene created: {scene.Name} (ID: {scene.Id})");
                return new GameEngineResponse
                {
                    Success = true,
                    Message = $"Scene '{scene.Name}' created successfully",
                    Data = scene
                };
            }

            return new GameEngineResponse
            {
                Success = false,
                Message = "Failed to create scene"
            };
        }

        public async Task<List<GameScene>> ListScenesAsync()
        {
            return await Task.Run(() => _scenes.Values.ToList());
        }

        public async Task<GameScene?> GetSceneAsync(string sceneId)
        {
            return await Task.Run(() => _scenes.TryGetValue(sceneId, out var scene) ? scene : null);
        }

        public async Task<GameEngineResponse> DeleteSceneAsync(string sceneId, string deletedBy)
        {
            if (_scenes.TryRemove(sceneId, out var scene))
            {
                // Delete from database
                _database.DeleteScene(sceneId);

                // Broadcast event
                await _broadcaster.BroadcastEventAsync(new GameEngineEvent
                {
                    EventType = GameEngineEventTypes.SceneDeleted,
                    SceneId = sceneId,
                    Data = new { sceneName = scene.Name },
                    Actor = deletedBy
                });

                LogInfo($"Scene deleted: {scene.Name} (ID: {sceneId}) by {deletedBy}");
                return new GameEngineResponse
                {
                    Success = true,
                    Message = $"Scene '{scene.Name}' deleted successfully"
                };
            }

            return new GameEngineResponse
            {
                Success = false,
                Message = $"Scene '{sceneId}' not found"
            };
        }

        public async Task<GameEngineResponse> CreateEntityAsync(string sceneId, GameEntity entity, string createdBy)
        {
            if (!_scenes.TryGetValue(sceneId, out var scene))
            {
                return new GameEngineResponse
                {
                    Success = false,
                    Message = $"Scene '{sceneId}' not found"
                };
            }

            entity.CreatedBy = createdBy;
            entity.CreatedAt = DateTime.UtcNow;
            scene.Entities.Add(entity);

            // Save to database
            _database.SaveEntity(sceneId, entity);

            // Broadcast event
            await _broadcaster.BroadcastEventAsync(new GameEngineEvent
            {
                EventType = GameEngineEventTypes.EntityCreated,
                SceneId = sceneId,
                EntityId = entity.Id,
                Data = entity,
                Actor = createdBy
            });

            LogInfo($"Entity created: {entity.Name} ({entity.Type}) in scene {scene.Name}");
            return new GameEngineResponse
            {
                Success = true,
                Message = $"Entity '{entity.Name}' created successfully",
                Data = entity
            };
        }

        public async Task<GameEngineResponse> UpdateEntityAsync(string sceneId, string entityId, GameEntity entity, string updatedBy)
        {
            if (!_scenes.TryGetValue(sceneId, out var scene))
            {
                return new GameEngineResponse
                {
                    Success = false,
                    Message = $"Scene '{sceneId}' not found"
                };
            }

            var existingEntity = scene.Entities.FirstOrDefault(e => e.Id == entityId);
            if (existingEntity == null)
            {
                return new GameEngineResponse
                {
                    Success = false,
                    Message = $"Entity '{entityId}' not found in scene"
                };
            }

            // Update entity properties
            existingEntity.Name = entity.Name;
            existingEntity.Type = entity.Type;
            existingEntity.Position = entity.Position;
            existingEntity.Rotation = entity.Rotation;
            existingEntity.Scale = entity.Scale;
            existingEntity.Properties = entity.Properties;

            // Save to database
            _database.SaveEntity(sceneId, existingEntity);

            // Broadcast event
            await _broadcaster.BroadcastEventAsync(new GameEngineEvent
            {
                EventType = GameEngineEventTypes.EntityUpdated,
                SceneId = sceneId,
                EntityId = entityId,
                Data = existingEntity,
                Actor = updatedBy
            });

            LogInfo($"Entity updated: {entity.Name} in scene {scene.Name} by {updatedBy}");
            return new GameEngineResponse
            {
                Success = true,
                Message = $"Entity '{entity.Name}' updated successfully",
                Data = existingEntity
            };
        }

        public async Task<GameEngineResponse> DeleteEntityAsync(string sceneId, string entityId, string deletedBy)
        {
            if (!_scenes.TryGetValue(sceneId, out var scene))
            {
                return new GameEngineResponse
                {
                    Success = false,
                    Message = $"Scene '{sceneId}' not found"
                };
            }

            var entity = scene.Entities.FirstOrDefault(e => e.Id == entityId);
            if (entity == null)
            {
                return new GameEngineResponse
                {
                    Success = false,
                    Message = $"Entity '{entityId}' not found"
                };
            }

            scene.Entities.Remove(entity);

            // Delete from database
            _database.DeleteEntity(entityId);

            // Broadcast event
            await _broadcaster.BroadcastEventAsync(new GameEngineEvent
            {
                EventType = GameEngineEventTypes.EntityDeleted,
                SceneId = sceneId,
                EntityId = entityId,
                Data = new { entityName = entity.Name, entityType = entity.Type },
                Actor = deletedBy
            });

            LogInfo($"Entity deleted: {entity.Name} from scene {scene.Name} by {deletedBy}");

            return new GameEngineResponse
            {
                Success = true,
                Message = $"Entity '{entity.Name}' deleted successfully"
            };
        }

        public async Task<List<GameEntity>> ListEntitiesAsync(string sceneId)
        {
            return await Task.Run(() =>
            {
                if (_scenes.TryGetValue(sceneId, out var scene))
                {
                    return scene.Entities.ToList();
                }
                return new List<GameEntity>();
            });
        }

        public async Task<GameEngineResponse> GenerateWorldContentAsync(string sceneId, WorldGenerationRequest request, string requestedBy)
        {
            if (!_scenes.TryGetValue(sceneId, out var scene))
            {
                return new GameEngineResponse
                {
                    Success = false,
                    Message = $"Scene '{sceneId}' not found"
                };
            }

            // Generate entities based on the request
            var GeneratedEntities = new List<GameEntity>();

            if (request.GenerateNPCs)
            {
                for (int i = 0; i < request.EntityCount; i++)
                {
                    var npc = new GameEntity
                    {
                        Name = GenerateNPCName(request.Theme, i),
                        Type = "NPC",
                        Position = GenerateRandomPosition(),
                        CreatedBy = $"AI:{requestedBy}"
                    };

                    npc.Properties["dialogue"] = GenerateNPCDialogue(npc.Name, request.Theme);
                    npc.Properties["occupation"] = GenerateOccupation(request.Theme);

                    scene.Entities.Add(npc);
                    GeneratedEntities.Add(npc);

                    // Save to database
                    _database.SaveEntity(sceneId, npc);
                }
            }

            if (request.GenerateTerrain)
            {
                var terASHATin = new GameEntity
                {
                    Name = $"{request.Theme} TerASHATin",
                    Type = "TerASHATin",
                    Scale = new Vector3 { X = 100, Y = 1, Z = 100 },
                    CreatedBy = $"AI:{requestedBy}"
                };
                terASHATin.Properties["theme"] = request.Theme;
                scene.Entities.Add(terASHATin);
                GeneratedEntities.Add(terASHATin);

                // Save to database
                _database.SaveEntity(sceneId, terASHATin);
            }

            LogInfo($"Generated {GeneratedEntities.Count} entities for scene {scene.Name}");

            // Broadcast event
            await _broadcaster.BroadcastEventAsync(new GameEngineEvent
            {
                EventType = GameEngineEventTypes.WorldGenerated,
                SceneId = sceneId,
                Data = new { entityCount = GeneratedEntities.Count, entities = GeneratedEntities },
                Actor = requestedBy
            });

            return new GameEngineResponse
            {
                Success = true,
                Message = $"Generated {GeneratedEntities.Count} entities in scene '{scene.Name}'",
                Data = GeneratedEntities
            };
        }

        public async Task<GameEngineResponse> StreamAssetAsync(AssetStreamRequest request)
        {
            return await Task.Run(() =>
            {
                var asset = new Asset
                {
                    Id = request.AssetId,
                    Name = request.AssetName,
                    Type = request.AssetType,
                    Url = request.Url,
                    Metadata = request.Metadata,
                    LoadedAt = DateTime.UtcNow
                };

                if (_assets.TryAdd(asset.Id, asset))
                {
                    LogInfo($"Asset streamed: {asset.Name} ({asset.Type})");
                    return new GameEngineResponse
                    {
                        Success = true,
                        Message = $"Asset '{asset.Name}' streamed successfully",
                        Data = asset
                    };
                }

                return new GameEngineResponse
                {
                    Success = false,
                    Message = "Failed to stream asset"
                };
            });
        }

        public async Task<EngineStats> GetStatsAsync()
        {
            return await Task.Run(() => new EngineStats
            {
                TotalScenes = _scenes.Count,
                ActiveScenes = _scenes.Values.Count(s => s.IsActive),
                TotalEntities = _scenes.Values.Sum(s => s.Entities.Count),
                MemoryUsageMB = GC.GetTotalMemory(false) / (1024 * 1024),
                Uptime = DateTime.UtcNow - _startTime,
                StartTime = _startTime,
                ConnectedClients = _broadcaster.ConnectedClients
            });
        }

        public async Task BroadcastEventAsync(Networking.GameEngineEvent gameEvent)
        {
            await Task.Run(() =>
            {
                LogInfo($"Broadcasting event: {gameEvent.EventType} for scene {gameEvent.SceneId}");
                // In a full implementation, this would broadcast via WebSocket handler
            });
        }

        /// <summary>
        /// Get the WebSocket broadcaster for external Registration.
        /// </summary>
        public GameEngineWebSocketBroadcaster GetBroadcaster()
        {
            return _broadcaster;
        }

        #endregion

        #region Synchronous Helper Methods

        private string GetStatusSync()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Game Engine Status:");
            sb.AppendLine();
            sb.AppendLine($"Uptime: {DateTime.UtcNow - _startTime:hh\\:mm\\:ss}");
            sb.AppendLine($"Total Scenes: {_scenes.Count}");
            sb.AppendLine($"Active Scenes: {_scenes.Values.Count(s => s.IsActive)}");
            sb.AppendLine($"Total Entities: {_scenes.Values.Sum(s => s.Entities.Count)}");
            sb.AppendLine($"Loaded Assets: {_assets.Count}");
            sb.AppendLine();
            sb.AppendLine("Engine Features:");
            sb.AppendLine("  ✅ Scene Management");
            sb.AppendLine("  ✅ Entity Creation & Management");
            sb.AppendLine("  ✅ AI-Driven World Generation");
            sb.AppendLine("  ✅ Asset Streaming");
            sb.AppendLine("  ✅ Real-time Updates");
            sb.AppendLine("  ✅ Permission-based Access Control");

            return sb.ToString();
        }

        private string GetStatsSync()
        {
            var stats = GetStatsAsync().Result;
            var sb = new StringBuilder();
            sb.AppendLine("Game Engine Statistics:");
            sb.AppendLine();
            sb.AppendLine($"Total Scenes: {stats.TotalScenes}");
            sb.AppendLine($"Active Scenes: {stats.ActiveScenes}");
            sb.AppendLine($"Total Entities: {stats.TotalEntities}");
            sb.AppendLine($"Memory Usage: {stats.MemoryUsageMB} MB");
            sb.AppendLine($"Uptime: {stats.Uptime:dd\\:hh\\:mm\\:ss}");
            sb.AppendLine($"Started: {stats.StartTime:yyyy-MM-dd HH:mm:ss} UTC");

            return sb.ToString();
        }

        private string CreateSceneSync(string sceneName, string createdBy)
        {
            var result = CreateSceneAsync(sceneName, createdBy).Result;
            return result.Success ? result.Message : $"Error: {result.Message}";
        }

        private string ListScenesSync()
        {
            var scenes = ListScenesAsync().Result;
            if (scenes.Count == 0)
            {
                return "No scenes created yet. Use 'engine scene create <name>' to create one.";
            }

            var sb = new StringBuilder();
            sb.AppendLine("Game Scenes:");
            sb.AppendLine();

            foreach (var scene in scenes)
            {
                var status = scene.IsActive ? "🟢 Active" : "🔴 Inactive";
                sb.AppendLine($"{status} - {scene.Name} (ID: {scene.Id})");
                sb.AppendLine($"   Created: {scene.CreatedAt:yyyy-MM-dd HH:mm:ss} by {scene.CreatedBy}");
                sb.AppendLine($"   Entities: {scene.Entities.Count}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string DeleteSceneSync(string sceneId, string deletedBy)
        {
            var result = DeleteSceneAsync(sceneId, deletedBy).Result;
            return result.Success ? result.Message : $"Error: {result.Message}";
        }

        private string CreateEntitySync(string args, string createdBy)
        {
            var parts = args.Split(' ', 3);
            if (parts.Length < 3)
            {
                return "Error: Usage: engine entity create <sceneId> <name> <type>";
            }

            var sceneId = parts[0];
            var name = parts[1];
            var type = parts[2];

            var entity = new GameEntity
            {
                Name = name,
                Type = type
            };

            var result = CreateEntityAsync(sceneId, entity, createdBy).Result;
            return result.Success ? result.Message : $"Error: {result.Message}";
        }

        private string GenerateWorldContentSync(string args, string requestedBy)
        {
            var parts = args.Split(' ', 2);
            if (parts.Length < 2)
            {
                return "Error: Usage: engine Generate <sceneId> <prompt>";
            }

            var sceneId = parts[0];
            var prompt = parts[1];

            var request = new WorldGenerationRequest
            {
                Prompt = prompt,
                EntityCount = 10,
                GenerateNPCs = true,
                GenerateTerrain = true,
                Theme = DetermineTheme(prompt)
            };

            var result = GenerateWorldContentAsync(sceneId, request, requestedBy).Result;
            return result.Success ? result.Message : $"Error: {result.Message}";
        }

        #endregion

        #region Helper Methods

        private string DetermineTheme(string prompt)
        {
            var promptLower = prompt.ToLowerInvariant();
            if (promptLower.Contains("medieval")) return "medieval";
            if (promptLower.Contains("sci-fi") || promptLower.Contains("scifi")) return "scifi";
            if (promptLower.Contains("fantasy")) return "fantasy";
            if (promptLower.Contains("modern")) return "modern";
            return "fantasy";
        }

        private string GenerateNPCName(string theme, int index)
        {
            var namesByTheme = new Dictionary<string, string[]>
            {
                ["medieval"] = new[] { "Garret", "Thalia", "Edmund", "Isolde", "Roland" },
                ["fantasy"] = new[] { "ElaASHAT", "Theron", "LyASHAT", "Kael", "Aria" },
                ["scifi"] = new[] { "Nova", "Rex", "Luna", "Zeke", "Kai" },
                ["modern"] = new[] { "Alex", "Sam", "Jordan", "Casey", "Morgan" }
            };

            var names = namesByTheme.GetValueOrDefault(theme, namesByTheme["fantasy"]);
            return $"{names[index % names.Length]}_{index}";
        }

        private string GenerateNPCDialogue(string name, string theme)
        {
            return theme switch
            {
                "medieval" => $"Greetings, traveler! I am {name}. How may I assist you?",
                "fantasy" => $"Welcome, adventurer! My name is {name}. What brings you here?",
                "scifi" => $"Hello. I'm {name}. How can I help you today?",
                _ => $"Hi there! I'm {name}. Nice to meet you!"
            };
        }

        private string GenerateOccupation(string theme)
        {
            var occupationsByTheme = new Dictionary<string, string[]>
            {
                ["medieval"] = new[] { "Blacksmith", "Merchant", "Guard", "Innkeeper", "Farmer" },
                ["fantasy"] = new[] { "Wizard", "Healer", "Merchant", "Adventurer", "Alchemist" },
                ["scifi"] = new[] { "Engineer", "Scientist", "Pilot", "TASHATder", "Technician" },
                ["modern"] = new[] { "Shopkeeper", "Officer", "Teacher", "Doctor", "Manager" }
            };

            var occupations = occupationsByTheme.GetValueOrDefault(theme, occupationsByTheme["fantasy"]);
            return occupations[Random.Shared.Next(occupations.Length)];
        }

        private Vector3 GenerateRandomPosition()
        {
            return new Vector3
            {
                X = Random.Shared.Next(-50, 50),
                Y = 0,
                Z = Random.Shared.Next(-50, 50)
            };
        }

        #endregion

        private class Asset
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
            public Dictionary<string, object> Metadata { get; set; } = new();
            public DateTime LoadedAt { get; set; }
        }

        /// <summary>
        /// Get all scenes (Admin+).
        /// </summary>
        public List<GameScene> GetAllScenes()
        {
            return _scenes.Values.ToList();
        }

        #region In-Game Chat Methods

        /// <summary>
        /// Create an in-game chat room for a specific scene.
        /// </summary>
        public Task<(bool success, string message, string? roomId)> CreateInGameChatRoomAsync(string sceneId, string roomName, string createdBy)
        {
            return Task.FromResult(_inGameChat.CreateRoom(sceneId, roomName, createdBy));
        }

        /// <summary>
        /// Send a message to an in-game chat room.
        /// </summary>
        public Task<(bool success, string message, string? messageId)> SendInGameChatMessageAsync(string roomId, string userId, string username, string content)
        {
            return Task.FromResult(_inGameChat.SendMessage(roomId, userId, username, content));
        }

        /// <summary>
        /// Get messages from an in-game chat room.
        /// </summary>
        public Task<List<Chat.GameChatMessage>> GetInGameChatMessagesAsync(string roomId, int limit = 50)
        {
            return Task.FromResult(_inGameChat.GetMessages(roomId, limit));
        }

        /// <summary>
        /// Get all in-game chat rooms for a scene.
        /// </summary>
        public Task<List<Chat.GameChatRoom>> GetInGameChatRoomsForSceneAsync(string sceneId)
        {
            return Task.FromResult(_inGameChat.GetRoomsForScene(sceneId));
        }

        #endregion
    }
}
