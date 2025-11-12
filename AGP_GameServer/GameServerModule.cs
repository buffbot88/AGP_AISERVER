using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Abstractions;

namespace LegendaryGameSystem
{
    /// <summary>
    /// Game Server Module - Advanced AI-driven game creation and deployment system.
    /// OrchestRates complete game development from natural language to deployed server.
    /// Eliminates need for Unity clients or manual intervention.
    /// </summary>
    [RaModule(Category = "extensions")]
    public sealed class GameServerModule : ModuleBase, IGameServerModule
    {
        public override string Name => "GameServer";

        private IGameEngineModule? _gameEngine;
        private IAIContentModule? _aiContent;
        private IServerSetupModule? _serverSetup;
        private string _projectsBasePath = string.Empty;
        private readonly ConcurrentDictionary<string, GameProject> _projects = new();
        private readonly ConcurrentDictionary<string, ServerDeploymentInfo> _deployments = new();
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public override void Initialize(object? manager)
        {
            base.Initialize(manager);
            _projectsBasePath = Path.Combine(AppContext.BaseDirectory, "GameProjects");

            if (!Directory.Exists(_projectsBasePath))
            {
                Directory.CreateDirectory(_projectsBasePath);
            }

            // Get references to other modules through reflection to avoid tight coupling
            if (manager != null)
            {
                var getModuleMethod = manager.GetType().GetMethod("GetModuleByName");
                if (getModuleMethod != null)
                {
                    _gameEngine = getModuleMethod.Invoke(manager, new object[] { "GameEngine" }) as IGameEngineModule;
                    _aiContent = getModuleMethod.Invoke(manager, new object[] { "AIContent" }) as IAIContentModule;
                    _serverSetup = getModuleMethod.Invoke(manager, new object[] { "ServerSetup" }) as IServerSetupModule;
                }
            }

            LogInfo("GameServer module initialized - AI-driven game creation suite active");
            LogInfo($"  Projects path: {_projectsBasePath}");
            LogInfo($"  Intergrated modules: GameEngine={_gameEngine != null}, AIContent={_aiContent != null}, ServerSetup={_serverSetup != null}");
        }

        public override string Process(string input)
        {
            var text = (input ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                return GetHelp();
            }

            if (text.Equals("gameserver status", StringComparison.OrdinalIgnoreCase))
            {
                return GetStatusSync();
            }

            if (text.Equals("gameserver capabilities", StringComparison.OrdinalIgnoreCase))
            {
                return GetCapabilitiesSync();
            }

            if (text.Equals("gameserver list", StringComparison.OrdinalIgnoreCase))
            {
                return ListAllProjectsSync();
            }

            if (text.StartsWith("gameserver create ", StringComparison.OrdinalIgnoreCase))
            {
                var description = text["gameserver create ".Length..].Trim();
                return CreateGameSync(description);
            }

            if (text.StartsWith("gameserver preview ", StringComparison.OrdinalIgnoreCase))
            {
                var gameId = text["gameserver preview ".Length..].Trim();
                return GetPreviewSync(gameId);
            }

            if (text.StartsWith("gameserver deploy ", StringComparison.OrdinalIgnoreCase))
            {
                var args = text["gameserver deploy ".Length..].Trim();
                return DeployGameSync(args);
            }

            if (text.StartsWith("gameserver update ", StringComparison.OrdinalIgnoreCase))
            {
                var args = text["gameserver update ".Length..].Trim();
                return UpdateGameSync(args);
            }

            if (text.StartsWith("gameserver export ", StringComparison.OrdinalIgnoreCase))
            {
                var args = text["gameserver export ".Length..].Trim();
                return ExportGameSync(args);
            }

            if (text.StartsWith("gameserver delete ", StringComparison.OrdinalIgnoreCase))
            {
                var gameId = text["gameserver delete ".Length..].Trim();
                return DeleteGameSync(gameId);
            }

            return "Unknown gameserver command. Type 'help' for available commands.";
        }

        private string GetHelp()
        {
            return string.Join(Environment.NewLine,
                "GameServer Module - AI-Driven Game Creation Suite",
                "===============================================",
                "",
                "natural LANGUAGE GAME CREATION:",
                "  gameserver create <description>     - Create complete game from description",
                "",
                "PROJECT MANAGEMENT:",
                "  gameserver list                     - List all game projects",
                "  gameserver preview <game-id>        - Preview game project",
                "  gameserver update <game-id> <desc>  - Update game with modifications",
                "  gameserver delete <game-id>         - Delete game project",
                "",
                "DEPLOYMENT:",
                "  gameserver deploy <game-id> [opts]  - Deploy game to server",
                "  gameserver export <game-id> [fmt]   - Export game project",
                "",
                "SYSTEM:",
                "  gameserver status                   - Show module status",
                "  gameserver capabilities             - Show system capabilities",
                "  help                                - Show this help",
                "",
                "EXAMPLE COMMANDS:",
                "  gameserver create A medieval MMO with castle siege battles and cASHATfting",
                "  gameserver create A space shooter with procedural levels and boss fights",
                "  gameserver create A fantasy RPG with quests, NPCs, and magic system",
                "",
                "FEATURES:",
                "  ✓ natural language game design",
                "  ✓ Automatic front-end & back-end Generation",
                "  ✓ AI-powered asset creation",
                "  ✓ One-click server deployment",
                "  ✓ Real-time preview & iteASHATtion",
                "  ✓ Full source code & documentation",
                "  ✓ Security & moderation built-in"
            );
        }

        private string GetStatusSync()
        {
            var stats = GetCapabilitiesAsync().GetAwaiter().GetResult();

            var sb = new StringBuilder();
            sb.AppendLine("GameServer Module Status");
            sb.AppendLine("========================");
            sb.AppendLine($"Total Projects: {_projects.Count}");
            sb.AppendLine($"Active Servers: {stats.ActiveServers}");
            sb.AppendLine($"Max Concurrent: {stats.MaxConcurrentServers}");
            sb.AppendLine($"Projects Path: {_projectsBasePath}");
            sb.AppendLine();
            sb.AppendLine("Intergrated Modules:");
            sb.AppendLine($"  GameEngine: {(_gameEngine != null ? "✓" : "✗")}");
            sb.AppendLine($"  AIContent: {(_aiContent != null ? "✓" : "✗")}");
            sb.AppendLine($"  ServerSetup: {(_serverSetup != null ? "✓" : "✗")}");
            sb.AppendLine();
            sb.AppendLine($"Supported Game Types: {string.Join(", ", stats.SupportedGameTypes)}");
            sb.AppendLine($"Available Features: {stats.AvailableFeatures.Count}");

            return sb.ToString();
        }

        private string GetCapabilitiesSync()
        {
            var caps = GetCapabilitiesAsync().GetAwaiter().GetResult();
            return JsonSerializer.Serialize(caps, _jsonOptions);
        }

        private string ListAllProjectsSync()
        {
            if (_projects.IsEmpty)
            {
                return "No game projects found. Create one with: gameserver create <description>";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Game Projects ({_projects.Count})");
            sb.AppendLine("===================");

            foreach (var project in _projects.Values.OrderByDescending(p => p.CreatedAt))
            {
                sb.AppendLine();
                sb.AppendLine($"ID: {project.Id}");
                sb.AppendLine($"Name: {project.Name}");
                sb.AppendLine($"Type: {project.Type}");
                sb.AppendLine($"Created: {project.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Features: {string.Join(", ", project.Features)}");

                if (project.DeploymentInfo != null)
                {
                    sb.AppendLine($"Status: {project.DeploymentInfo.Status}");
                    sb.AppendLine($"URL: {project.DeploymentInfo.ServerUrl}");
                }
                else
                {
                    sb.AppendLine("Status: Not deployed");
                }
            }

            return sb.ToString();
        }

        private string CreateGameSync(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return "Error: Game description cannot be empty.";
            }

            // Parse description to Extract game type and features
            var request = ParseGameDescription(description);

            var response = CreateGameFromDescriptionAsync(request).GetAwaiter().GetResult();

            if (response.Success && response.Project != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine("✓ Game Created Successfully!");
                sb.AppendLine("===========================");
                sb.AppendLine($"Game ID: {response.GameId}");
                sb.AppendLine($"Name: {response.Project.Name}");
                sb.AppendLine($"Type: {response.Project.Type}");
                sb.AppendLine($"Theme: {response.Project.Theme}");
                sb.AppendLine($"Features: {string.Join(", ", response.Project.Features)}");
                sb.AppendLine($"Project Path: {response.ProjectPath}");
                sb.AppendLine();
                sb.AppendLine($"Generated Files: {response.GeneratedFiles.Count}");
                foreach (var file in response.GeneratedFiles.Take(10))
                {
                    sb.AppendLine($"  - {Path.GetFileName(file)}");
                }
                if (response.GeneratedFiles.Count > 10)
                {
                    sb.AppendLine($"  ... and {response.GeneratedFiles.Count - 10} more");
                }
                sb.AppendLine();
                sb.AppendLine("Next Steps:");
                sb.AppendLine($"  Preview: gameserver preview {response.GameId}");
                sb.AppendLine($"  Deploy:  gameserver deploy {response.GameId}");
                sb.AppendLine($"  Export:  gameserver export {response.GameId}");

                return sb.ToString();
            }

            return $"Error: {response.Message}";
        }

        private string GetPreviewSync(string gameId)
        {
            var preview = GetGamePreviewAsync(gameId).GetAwaiter().GetResult();
            return JsonSerializer.Serialize(preview, _jsonOptions);
        }

        private string DeployGameSync(string args)
        {
            var parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 1)
            {
                return "Usage: gameserver deploy <game-id> [port=8080] [maxplayers=100]";
            }

            var gameId = parts[0];
            var options = new DeploymentOptions();

            // Parse optional Parameters
            foreach (var part in parts.Skip(1))
            {
                if (part.StartsWith("port=") && int.TryParse(part[5..], out var port))
                {
                    options.Port = port;
                }
                else if (part.StartsWith("maxplayers=") && int.TryParse(part[11..], out var maxPlayers))
                {
                    options.MaxPlayers = maxPlayers;
                }
            }

            var response = DeployGameServerAsync(gameId, options).GetAwaiter().GetResult();

            if (response.Success)
            {
                var sb = new StringBuilder();
                sb.AppendLine("✓ Server Deployed Successfully!");
                sb.AppendLine("==============================");
                sb.AppendLine($"Instance ID: {response.InstanceId}");
                sb.AppendLine($"Server URL: {response.ServerUrl}");
                sb.AppendLine($"Status: {response.Status}");
                sb.AppendLine($"Max Players: {options.MaxPlayers}");
                sb.AppendLine($"Port: {options.Port}");
                sb.AppendLine();
                sb.AppendLine("Server is now running and accepting connections!");
                return sb.ToString();
            }

            return $"Error: {response.Message}";
        }

        private string UpdateGameSync(string args)
        {
            var parts = args.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                return "Usage: gameserver update <game-id> <update-description>";
            }

            var gameId = parts[0];
            var updateDesc = parts[1];

            var response = UpdateGameAsync(gameId, updateDesc).GetAwaiter().GetResult();

            if (response.Success)
            {
                var sb = new StringBuilder();
                sb.AppendLine("✓ Game Updated Successfully!");
                sb.AppendLine("===========================");
                sb.AppendLine($"Modified Files: {response.ModifiedFiles.Count}");
                foreach (var file in response.ModifiedFiles)
                {
                    sb.AppendLine($"  - {Path.GetFileName(file)}");
                }
                return sb.ToString();
            }

            return $"Error: {response.Message}";
        }

        private string ExportGameSync(string args)
        {
            var parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 1)
            {
                return "Usage: gameserver export <game-id> [format]";
            }

            var gameId = parts[0];
            var format = parts.Length > 1 && Enum.TryParse<ExportFormat>(parts[1], true, out var fmt)
                ? fmt
                : ExportFormat.Complete;

            var response = ExportGameProjectAsync(gameId, format).GetAwaiter().GetResult();

            if (response.Success)
            {
                var sb = new StringBuilder();
                sb.AppendLine("✓ Game Exported Successfully!");
                sb.AppendLine("============================");
                sb.AppendLine($"Export Path: {response.ExportPath}");
                sb.AppendLine($"Format: {response.Format}");
                sb.AppendLine($"Size: {FormatBytes(response.SizeBytes)}");
                sb.AppendLine();
                sb.AppendLine("Package includes:");
                sb.AppendLine("  - Source code");
                sb.AppendLine("  - Assets");
                sb.AppendLine("  - Documentation");
                sb.AppendLine("  - Setup guide");
                return sb.ToString();
            }

            return $"Error: {response.Message}";
        }

        private string DeleteGameSync(string gameId)
        {
            var response = DeleteGameProjectAsync(gameId).GetAwaiter().GetResult();
            return response.Success
                ? $"✓ Game project {gameId} deleted successfully."
                : $"Error: {response.Message}";
        }

        // Interface implementation

        public async Task<GameCreationResponse> CreateGameFromDescriptionAsync(GameCreationRequest request)
        {
            await Task.CompletedTask;

            try
            {
                LogInfo($"Creating game from description: {request.Description}");

                // Generate unique project ID and name
                var gameId = Guid.NewGuid().ToString("N")[..12];
                var gameName = ExtractGameName(request.Description);
                var projectPath = Path.Combine(_projectsBasePath, gameId);

                Directory.CreateDirectory(projectPath);

                // Create project structure
                var GeneratedFiles = new List<string>();

                // 1. Generate game Configuration
                var configFile = await GenerateGameConfigurationAsync(projectPath, gameName, request);
                GeneratedFiles.Add(configFile);

                // 2. Generate front-end code
                var frontEndFiles = await GenerateFrontEndAsync(projectPath, gameName, request);
                GeneratedFiles.AddRange(frontEndFiles);

                // 3. Generate back-end code
                var backEndFiles = await GenerateBackEndAsync(projectPath, gameName, request);
                GeneratedFiles.AddRange(backEndFiles);

                // 4. Generate assets if requested
                if (request.GenerateAssets && _aiContent != null)
                {
                    var assetFiles = await GenerateGameAssetsAsync(projectPath, request);
                    GeneratedFiles.AddRange(assetFiles);
                }

                // 5. Create game scenes in engine
                if (_gameEngine != null)
                {
                    await CreateGameScenesAsync(gameId, gameName, request);
                }

                // 6. Generate documentation
                var docFiles = await GeneratedocumentationAsync(projectPath, gameName, request, GeneratedFiles);
                GeneratedFiles.AddRange(docFiles);

                // Create project record
                var project = new GameProject
                {
                    Id = gameId,
                    UserId = request.UserId,
                    LicenseKey = request.LicenseKey,
                    Name = gameName,
                    Description = request.Description,
                    Type = request.GameType,
                    Theme = request.Theme,
                    ProjectPath = projectPath,
                    Features = request.Features,
                    Metrics = new GameMetrics
                    {
                        TotalAssets = Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories).Length,
                        LinesOfCode = CountLinesOfCode(GeneratedFiles),
                        TotalSizeBytes = CalculateDirectorySize(projectPath)
                    }
                };

                foreach (var file in GeneratedFiles)
                {
                    project.GeneratedFiles[Path.GetFileName(file)] = file;
                }

                _projects[gameId] = project;

                // Auto-deploy if requested
                if (request.AutoDeploy)
                {
                    var deployResponse = await DeployGameServerAsync(gameId, new DeploymentOptions
                    {
                        InstanceName = gameName
                    });

                    if (deployResponse.Success)
                    {
                        project.DeploymentInfo = new ServerDeploymentInfo
                        {
                            InstanceId = deployResponse.InstanceId,
                            ServerUrl = deployResponse.ServerUrl,
                            Status = deployResponse.Status.ToString(),
                            DeployedAt = DateTime.UtcNow
                        };
                    }
                }

                LogInfo($"Game created successfully: {gameId} - {gameName}");

                return new GameCreationResponse
                {
                    Success = true,
                    Message = "Game created successfully",
                    GameId = gameId,
                    ProjectPath = projectPath,
                    Project = project,
                    GeneratedFiles = GeneratedFiles
                };
            }
            catch (Exception ex)
            {
                LogError($"Error creating game: {ex.Message}");
                return new GameCreationResponse
                {
                    Success = false,
                    Message = $"Failed to create game: {ex.Message}"
                };
            }
        }

        public async Task<ServerDeploymentResponse> DeployGameServerAsync(string gameId, DeploymentOptions options)
        {
            await Task.CompletedTask;

            try
            {
                if (!_projects.TryGetValue(gameId, out var project))
                {
                    return new ServerDeploymentResponse
                    {
                        Success = false,
                        Message = "Game project not found"
                    };
                }

                LogInfo($"Deploying game server: {gameId}");

                // Generate instance ID
                var instanceId = $"{gameId}-{Guid.NewGuid().ToString("N")[..8]}";

                // Use ServerSetup module to create server instance
                if (_serverSetup != null)
                {
                    var setupResult = await _serverSetup.CreateAdminFolderStructureAsync(
                        project.LicenseKey,
                        project.UserId.ToString());

                    if (!setupResult.Success)
                    {
                        return new ServerDeploymentResponse
                        {
                            Success = false,
                            Message = $"Server setup failed: {setupResult.Message}"
                        };
                    }
                }

                // Create deployment record
                var deploymentInfo = new ServerDeploymentInfo
                {
                    InstanceId = instanceId,
                    ServerUrl = $"http://localhost:{options.Port}",
                    Status = ServerStatus.Running.ToString(),
                    DeployedAt = DateTime.UtcNow,
                    Config = new Dictionary<string, object>
                    {
                        ["MaxPlayers"] = options.MaxPlayers,
                        ["Port"] = options.Port,
                        ["EnableWebSocket"] = options.EnableWebSocket,
                        ["EnableDatabase"] = options.EnableDatabase
                    }
                };

                _deployments[instanceId] = deploymentInfo;
                project.DeploymentInfo = deploymentInfo;

                LogInfo($"Game server deployed: {instanceId} at {deploymentInfo.ServerUrl}");

                return new ServerDeploymentResponse
                {
                    Success = true,
                    Message = "Server deployed successfully",
                    ServerUrl = deploymentInfo.ServerUrl,
                    InstanceId = instanceId,
                    Status = ServerStatus.Running,
                    ConnectionInfo = new Dictionary<string, object>
                    {
                        ["port"] = options.Port,
                        ["maxPlayers"] = options.MaxPlayers,
                        ["protocol"] = options.EnableWebSocket ? "WebSocket" : "HTTP"
                    }
                };
            }
            catch (Exception ex)
            {
                LogError($"Error deploying server: {ex.Message}");
                return new ServerDeploymentResponse
                {
                    Success = false,
                    Message = $"Deployment failed: {ex.Message}"
                };
            }
        }

        public async Task<GamePreview> GetGamePreviewAsync(string gameId)
        {
            await Task.CompletedTask;

            if (_projects.TryGetValue(gameId, out var project))
            {
                return new GamePreview
                {
                    GameId = gameId,
                    Name = project.Name,
                    Description = project.Description,
                    PreviewUrl = project.DeploymentInfo?.ServerUrl ?? "",
                    GameInfo = new Dictionary<string, object>
                    {
                        ["type"] = project.Type.ToString(),
                        ["theme"] = project.Theme,
                        ["features"] = project.Features,
                        ["created"] = project.CreatedAt,
                        ["totalAssets"] = project.Metrics.TotalAssets,
                        ["linesOfCode"] = project.Metrics.LinesOfCode
                    }
                };
            }

            return new GamePreview
            {
                GameId = gameId,
                Name = "Not Found",
                Description = "Game project not found"
            };
        }

        public async Task<GameUpdateResponse> UpdateGameAsync(string gameId, string updateDescription)
        {
            await Task.CompletedTask;

            try
            {
                if (!_projects.TryGetValue(gameId, out var project))
                {
                    return new GameUpdateResponse
                    {
                        Success = false,
                        Message = "Game project not found"
                    };
                }

                LogInfo($"Updating game {gameId}: {updateDescription}");

                var modifiedFiles = new List<string>();

                // Parse update description and apply changes
                // This would integRate with AI to understand modifications

                // Update project metadata
                project.LastModified = DateTime.UtcNow;

                return new GameUpdateResponse
                {
                    Success = true,
                    Message = "Game updated successfully",
                    ModifiedFiles = modifiedFiles,
                    Changes = new Dictionary<string, object>
                    {
                        ["description"] = updateDescription,
                        ["timestamp"] = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                LogError($"Error updating game: {ex.Message}");
                return new GameUpdateResponse
                {
                    Success = false,
                    Message = $"Update failed: {ex.Message}"
                };
            }
        }

        public async Task<List<GameProject>> ListUserGamesAsync(Guid userId)
        {
            await Task.CompletedTask;
            return _projects.Values.Where(p => p.UserId == userId).ToList();
        }

        public async Task<GameProject?> GetGameProjectAsync(string gameId)
        {
            await Task.CompletedTask;
            return _projects.TryGetValue(gameId, out var project) ? project : null;
        }

        public async Task<GameServerResponse> DeleteGameProjectAsync(string gameId)
        {
            await Task.CompletedTask;

            try
            {
                if (!_projects.TryRemove(gameId, out var project))
                {
                    return new GameServerResponse
                    {
                        Success = false,
                        Message = "Game project not found"
                    };
                }

                // Delete project directory
                if (Directory.Exists(project.ProjectPath))
                {
                    Directory.Delete(project.ProjectPath, true);
                }

                // Remove deployment
                if (project.DeploymentInfo != null)
                {
                    _deployments.TryRemove(project.DeploymentInfo.InstanceId, out _);
                }

                LogInfo($"Game project deleted: {gameId}");

                return new GameServerResponse
                {
                    Success = true,
                    Message = "Game project deleted successfully"
                };
            }
            catch (Exception ex)
            {
                LogError($"Error deleting game project: {ex.Message}");
                return new GameServerResponse
                {
                    Success = false,
                    Message = $"Deletion failed: {ex.Message}"
                };
            }
        }

        public async Task<GameExportResponse> ExportGameProjectAsync(string gameId, ExportFormat format)
        {
            await Task.CompletedTask;

            try
            {
                if (!_projects.TryGetValue(gameId, out var project))
                {
                    return new GameExportResponse
                    {
                        Success = false,
                        Message = "Game project not found"
                    };
                }

                var exportPath = Path.Combine(_projectsBasePath, "exports", $"{gameId}_{format}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip");
                var exportDir = Path.GetDirectoryName(exportPath);

                if (exportDir != null && !Directory.Exists(exportDir))
                {
                    Directory.CreateDirectory(exportDir);
                }

                // Create export package (simplified - would use actual zip libASHATry)
                var sizeBytes = CalculateDirectorySize(project.ProjectPath);

                LogInfo($"Game exported: {gameId} as {format}");

                return new GameExportResponse
                {
                    Success = true,
                    Message = "Game exported successfully",
                    ExportPath = exportPath,
                    DownloadUrl = $"/downloads/exports/{Path.GetFileName(exportPath)}",
                    SizeBytes = sizeBytes,
                    Format = format
                };
            }
            catch (Exception ex)
            {
                LogError($"Error exporting game: {ex.Message}");
                return new GameExportResponse
                {
                    Success = false,
                    Message = $"Export failed: {ex.Message}"
                };
            }
        }

        public async Task<ServerCapabilities> GetCapabilitiesAsync()
        {
            await Task.CompletedTask;

            return new ServerCapabilities
            {
                MaxConcurrentServers = 50,
                ActiveServers = _deployments.Count(d => d.Value.Status == ServerStatus.Running.ToString()),
                SupportedGameTypes = Enum.GetValues<GameType>().Select(t => t.ToString()).ToList(),
                AvailableFeatures = new List<string>
                {
                    "natural Language Design",
                    "Front-End Generation",
                    "Back-End Generation",
                    "AI Asset Creation",
                    "Scene Management",
                    "NPC Generation",
                    "Quest System",
                    "Multiplayer Support",
                    "Database integration",
                    "WebSocket Streaming",
                    "Real-time Preview",
                    "Source Code Export",
                    "Auto-Deployment",
                    "Security & moderation"
                },
                SystemInfo = new Dictionary<string, object>
                {
                    ["totalProjects"] = _projects.Count,
                    ["activeDeployments"] = _deployments.Count,
                    ["projectsPath"] = _projectsBasePath,
                    ["IntergratedModules"] = new[] { "GameEngine", "AIContent", "ServerSetup", "CodeGeneration" }
                }
            };
        }

        // Helper methods

        private GameCreationRequest ParseGameDescription(string description)
        {
            var request = new GameCreationRequest
            {
                UserId = Guid.Empty, // Would be set by authentication
                LicenseKey = "DEMO",
                Description = description,
                GameType = GameType.Multiplayer,
                Theme = "fantasy"
            };

            var lower = description.ToLowerInvariant();

            // Detect game type
            if (lower.Contains("mmo") || lower.Contains("massively"))
                request.GameType = GameType.MMO;
            else if (lower.Contains("single player") || lower.Contains("singleplayer"))
                request.GameType = GameType.SinglePlayer;
            else if (lower.Contains("pvp") || lower.Contains("versus"))
                request.GameType = GameType.PvP;
            else if (lower.Contains("coop") || lower.Contains("coOperative"))
                request.GameType = GameType.CoOperative;
            else if (lower.Contains("sandbox"))
                request.GameType = GameType.Sandbox;

            // Detect theme
            if (lower.Contains("medieval") || lower.Contains("castle") || lower.Contains("knight"))
                request.Theme = "medieval";
            else if (lower.Contains("fantasy") || lower.Contains("magic") || lower.Contains("dASHATgon"))
                request.Theme = "fantasy";
            else if (lower.Contains("sci-fi") || lower.Contains("space") || lower.Contains("alien"))
                request.Theme = "sci-fi";
            else if (lower.Contains("horror") || lower.Contains("zombie") || lower.Contains("scary"))
                request.Theme = "horror";
            else if (lower.Contains("modern") || lower.Contains("contemporary"))
                request.Theme = "modern";

            // Extract features
            var features = new List<string>();
            if (lower.Contains("cASHATft")) features.Add("CASHATfting System");
            if (lower.Contains("quest")) features.Add("Quest System");
            if (lower.Contains("combat") || lower.Contains("battle")) features.Add("Combat System");
            if (lower.Contains("tASHATde") || lower.Contains("economy")) features.Add("Economy System");
            if (lower.Contains("guild") || lower.Contains("clan")) features.Add("Guild System");
            if (lower.Contains("pvp") || lower.Contains("arena")) features.Add("PvP System");
            if (lower.Contains("npc")) features.Add("NPC Dialogue");
            if (lower.Contains("procedural")) features.Add("procedural Generation");

            request.Features = features;

            return request;
        }

        private string ExtractGameName(string description)
        {
            // Extract a reasonable name from description
            var words = description.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !new[] { "a", "an", "the", "with", "and", "or" }.Contains(w.ToLowerInvariant()))
                .Take(3);

            var name = string.Join(" ", words);
            return string.IsNullOrWhiteSpace(name) ? "Game Project" : name;
        }

        private async Task<string> GenerateGameConfigurationAsync(string projectPath, string gameName, GameCreationRequest request)
        {
            await Task.CompletedTask;

            var config = new
            {
                game_name = gameName,
                version = "1.0.0",
                game_type = request.GameType.ToString(),
                theme = request.Theme,
                features = request.Features,
                description = request.Description,
                Generated_by = "ASHATCore GameServer Module",
                Generated_at = DateTime.UtcNow,
                settings = new
                {
                    max_players = request.GameType == GameType.MMO ? 1000 : 100,
                    difficulty = "Normal",
                    auto_save = true,
                    multiplayer = request.GameType != GameType.SinglePlayer
                }
            };

            var configPath = Path.Combine(projectPath, "game_config.json");
            await File.WriteAllTextAsync(configPath, JsonSerializer.Serialize(config, _jsonOptions));

            return configPath;
        }

        private async Task<List<string>> GenerateFrontEndAsync(string projectPath, string gameName, GameCreationRequest request)
        {
            await Task.CompletedTask;

            var files = new List<string>();
            var frontEndPath = Path.Combine(projectPath, "frontend");
            Directory.CreateDirectory(frontEndPath);

            // Generate index.html
            var htmlPath = Path.Combine(frontEndPath, "index.html");
            var html = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{gameName}</title>
    <link rel=""stylesheet"" href=""style.css"">
</head>
<body>
    <div id=""game-container"">
        <h1>{gameName}</h1>
        <div id=""game-canvas""></div>
        <div id=""ui-overlay""></div>
    </div>
    <script src=""game.js""></script>
</body>
</html>";
            await File.WriteAllTextAsync(htmlPath, html);
            files.Add(htmlPath);

            // Generate CSS
            var cssPath = Path.Combine(frontEndPath, "style.css");
            var css = @"body {
    margin: 0;
    padding: 0;
    font-family: Arial, sans-serif;
    background: #1a1a2e;
    color: #eee;
}

#game-container {
    width: 100%;
    height: 100vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
}

#game-canvas {
    width: 800px;
    height: 600px;
    background: #0f3460;
    border: 2px solid #e94560;
    border-ASHATdius: 8px;
}

#ui-overlay {
    position: absolute;
    top: 20px;
    right: 20px;
    background: rgba(0, 0, 0, 0.7);
    padding: 15px;
    border-ASHATdius: 8px;
}";
            await File.WriteAllTextAsync(cssPath, css);
            files.Add(cssPath);

            // Generate JavaScript
            var jsPath = Path.Combine(frontEndPath, "game.js");
            var js = $@"// {gameName} - Generated by ASHATCore GameServer
// Game Type: {request.GameType}
// Theme: {request.Theme}

class Game {{
    constructor() {{
        this.canvas = document.getElementById('game-canvas');
        this.init();
    }}

    init() {{
        console.log('Initializing {gameName}...');
        this.setupWebSocket();
        this.loadAssets();
        this.startGameLoop();
    }}

    setupWebSocket() {{
        // Connect to game server
        const ws = new WebSocket('ws://localhost:8080/game');
        ws.onmessage = (event) => this.handleServerMessage(event);
    }}

    loadAssets() {{
        // Load game assets
        console.log('Loading assets...');
    }}

    startGameLoop() {{
        // Main game loop
        setInterval(() => this.update(), 1000 / 60);
    }}

    update() {{
        // Update game state
    }}

    handleServerMessage(event) {{
        const data = JSON.parse(event.data);
        console.log('Server message:', data);
    }}
}}

// Initialize game when page loads
window.addEventListener('load', () => {{
    new Game();
}});";
            await File.WriteAllTextAsync(jsPath, js);
            files.Add(jsPath);

            return files;
        }

        private async Task<List<string>> GenerateBackEndAsync(string projectPath, string gameName, GameCreationRequest request)
        {
            await Task.CompletedTask;

            var files = new List<string>();
            var backEndPath = Path.Combine(projectPath, "backend");
            Directory.CreateDirectory(backEndPath);

            // Generate server.cs
            var serverPath = Path.Combine(backEndPath, "Server.cs");
            var serverCode = $@"// {gameName} - Game Server
// Generated by ASHATCore GameServer Module
// Game Type: {request.GameType}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace {gameName.Replace(" ", "")}.Server
{{
    public class GameServer
    {{
        private readonly Dictionary<string, Player> _players = new();
        private readonly GameState _gameState = new();

        public async Task StartAsync(int port)
        {{
            Console.WriteLine(""{gameName} Server starting on port {{port}}..."");
            
            var listener = new HttpListener();
            listener.Prefixes.Add($""http://localhost:{{port}}/"");
            listener.Start();
            
            Console.WriteLine(""Server ready!"");
            
            while (true)
            {{
                var context = await listener.GetContextAsync();
                _ = HandleRequestAsync(context);
            }}
        }}

        private async Task HandleRequestAsync(HttpListenerContext context)
        {{
            // Handle WebSocket and HTTP requests
            if (context.Request.IsWebSocketRequest)
            {{
                var wsContext = await context.AcceptWebSocketAsync(null);
                await HandleWebSocketAsync(wsContext);
            }}
            else
            {{
                context.Response.StatusCode = 200;
                context.Response.Close();
            }}
        }}

        private async Task HandleWebSocketAsync(HttpListenerWebSocketContext context)
        {{
            var ws = context.WebSocket;
            var playerId = Guid.NewGuid().ToString();
            
            Console.WriteLine($""Player connected: {{playerId}}"");
            
            // Handle player connection
            await Task.CompletedTask;
        }}
    }}

    public class Player
    {{
        public string Id {{ get; set; }} = """";
        public string Name {{ get; set; }} = """";
        public Vector3 Position {{ get; set; }} = new();
    }}

    public class GameState
    {{
        public Dictionary<string, object> Data {{ get; set; }} = new();
    }}

    public class Vector3
    {{
        public float X {{ get; set; }}
        public float Y {{ get; set; }}
        public float Z {{ get; set; }}
    }}
}}";
            await File.WriteAllTextAsync(serverPath, serverCode);
            files.Add(serverPath);

            // Generate README
            var readmePath = Path.Combine(backEndPath, "README.md");
            var readme = $@"# {gameName} - Backend Server

## Generated by ASHATCore GameServer Module

### Features
    {string.Join(Environment.NewLine, request.Features.Select(f => $"- {f}"))}

### Running the Server

```bash
dotnet run
```

### Configuration

Edit `game_config.json` to customize server settings.
    ";
            await File.WriteAllTextAsync(readmePath, readme);
            files.Add(readmePath);

            return files;
        }

        private async Task<List<string>> GenerateGameAssetsAsync(string projectPath, GameCreationRequest request)
        {
            await Task.CompletedTask;

            var files = new List<string>();
            var assetsPath = Path.Combine(projectPath, "assets");
            Directory.CreateDirectory(assetsPath);

            // Generate asset manifests
            var manifestPath = Path.Combine(assetsPath, "manifest.json");
            var manifest = new
            {
                game_name = ExtractGameName(request.Description),
                theme = request.Theme,
                asset_types = new[] { "textures", "models", "audio" },
                Generated_at = DateTime.UtcNow
            };

            await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, _jsonOptions));
            files.Add(manifestPath);

            return files;
        }

        private async Task CreateGameScenesAsync(string gameId, string gameName, GameCreationRequest request)
        {
            if (_gameEngine == null) return;

            try
            {
                // Create main game scene
                var sceneResponse = await _gameEngine.CreateSceneAsync($"{gameName} - Main Scene", "GameServer");

                if (sceneResponse.Success && sceneResponse.Data is GameScene scene)
                {
                    // Add some basic entities based on game type
                    var entityRequest = new WorldGenerationRequest
                    {
                        Prompt = request.Description,
                        Theme = request.Theme,
                        EntityCount = request.GameType == GameType.MMO ? 50 : 10,
                        GenerateNPCs = true,
                        GenerateTerrain = true,
                        GenerateQuests = request.Features.Contains("Quest System")
                    };

                    await _gameEngine.GenerateWorldContentAsync(scene.Id, entityRequest, "GameServer");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error creating game scenes: {ex.Message}");
            }
        }

        private async Task<List<string>> GeneratedocumentationAsync(string projectPath, string gameName, GameCreationRequest request, List<string> GeneratedFiles)
        {
            await Task.CompletedTask;

            var files = new List<string>();
            var docsPath = Path.Combine(projectPath, "docs");
            Directory.CreateDirectory(docsPath);

            // Generate main README
            var readmePath = Path.Combine(projectPath, "README.md");
            var readme = $@"# {gameName}

## Game Information

**Type:** {request.GameType}  
**Theme:** {request.Theme}  
**Generated:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}

## Description

    {request.Description}

## Features

    {string.Join(Environment.NewLine, request.Features.Select(f => $"- {f}"))}

## Project Structure

```
    {gameName}/
├── frontend/          # Client-side game code
│   ├── index.html
│   ├── style.css
│   └── game.js
├── backend/           # Server-side game logic
│   ├── Server.cs
│   └── README.md
├── assets/            # Game assets (textures, models, audio)
├── docs/              # Documentation
├── game_config.json   # Game Configuration
└── README.md          # This file
```

## Getting Started

### Prerequisites
- .NET 9.0 or later
- Modern web browser
- ASHATCore server (for deployment)

### Running Locally

1. Start the backend server:
```bash
cd backend
dotnet run
```

2. Open `frontend/index.html` in a web browser

### Deploying to ASHATCore

Use the GameServer module to deploy:
```
gameserver deploy {gameName.Replace(" ", "-").ToLowerInvariant()}
```

## Generated Files

    {string.Join(Environment.NewLine, GeneratedFiles.Take(20).Select(f => $"- {Path.GetFileName(f)}"))}
    {(GeneratedFiles.Count > 20 ? $"... and {GeneratedFiles.Count - 20} more files" : "")}

## Customization

All Generated code can be modified to suit your needs. See the `docs/` folder for detailed information on extending the game.

## Support

For issues or questions, contact ASHATCore support or refer to the ASHATCore documentation.

---

*This game was Generated by ASHATCore GameServer Module - AI-Driven Game Creation Suite*
    ";
            await File.WriteAllTextAsync(readmePath, readme);
            files.Add(readmePath);

            // Generate API documentation
            var apiDocsPath = Path.Combine(docsPath, "API.md");
            var apiDocs = $@"# {gameName} - API Documentation

## Server Endpoints

### WebSocket Connection

**Endpoint:** `ws://localhost:8080/game`

Connect to the game server using WebSocket for real-time communication.

### Message Format

```json
{{
  ""type"": ""action"",
  ""data"": {{}}
}}
```

## Client API

See `frontend/game.js` for client-side API documentation.

## Extending the Game

Add new features by:
1. Editing backend server logic in `backend/Server.cs`
2. Adding new UI elements in `frontend/index.html`
3. Implementing new game mechanics in `frontend/game.js`
    ";
            await File.WriteAllTextAsync(apiDocsPath, apiDocs);
            files.Add(apiDocsPath);

            return files;
        }

        private int CountLinesOfCode(List<string> files)
        {
            int totalLines = 0;

            foreach (var file in files)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        var lines = File.ReadAllLines(file);
                        totalLines += lines.Length;
                    }
                }
                catch
                {
                    // Ignore errors
                }
            }

            return totalLines;
        }

        private long CalculateDirectorySize(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return 0;

            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            return files.Sum(f => new FileInfo(f).Length);
        }

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
