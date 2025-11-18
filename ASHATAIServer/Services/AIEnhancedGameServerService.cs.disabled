using ASHATAIServer.Services;
using Abstractions;
using LegendaryGameSystem;

namespace ASHATAIServer.Services
{
    /// <summary>
    /// Service that enhances GameServerModule with AI capabilities
    /// </summary>
    public class AIEnhancedGameServerService
    {
        private readonly GameServerModule _gameServerModule;
        private readonly LanguageModelService _languageModelService;
        private readonly ILogger<AIEnhancedGameServerService> _logger;

        public AIEnhancedGameServerService(
            GameServerModule gameServerModule,
            LanguageModelService languageModelService,
            ILogger<AIEnhancedGameServerService> logger)
        {
            _gameServerModule = gameServerModule;
            _languageModelService = languageModelService;
            _logger = logger;
        }

        /// <summary>
        /// Sanitize input for logging to prevent log forging attacks
        /// </summary>
        private static string SanitizeForLog(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            
            // Remove newlines and carriage returns to prevent log forging
            return input.Replace("\r", "").Replace("\n", " ").Trim();
        }

        /// <summary>
        /// Create a game with AI-enhanced description parsing
        /// </summary>
        public async Task<GameCreationResponse> CreateGameWithAIAsync(string description, Guid userId = default, string licenseKey = "DEMO")
        {
            _logger.LogInformation("Creating game with AI-enhanced parsing: {Description}", SanitizeForLog(description));

            try
            {
                // Use AI to enhance the game description parsing
                var enhancedRequest = await ParseGameDescriptionWithAIAsync(description, userId, licenseKey);
                
                // Create the game using the enhanced request
                return await _gameServerModule.CreateGameFromDescriptionAsync(enhancedRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating game with AI enhancement");
                
                // Fall back to standard creation if AI enhancement fails
                var fallbackRequest = new GameCreationRequest
                {
                    UserId = userId,
                    LicenseKey = licenseKey,
                    Description = description,
                    GameType = GameType.Multiplayer,
                    Theme = "fantasy",
                    GenerateAssets = true
                };
                
                return await _gameServerModule.CreateGameFromDescriptionAsync(fallbackRequest);
            }
        }

        /// <summary>
        /// Parse game description using AI to extract game type, theme, and features
        /// </summary>
        private async Task<GameCreationRequest> ParseGameDescriptionWithAIAsync(string description, Guid userId, string licenseKey)
        {
            // Create a prompt for the AI to analyze the game description
            var prompt = $@"You are a game design expert. Analyze the following game description and extract:
1. Game type (choose from: RPG, MMO, FPS, Strategy, Puzzle, Adventure, Simulation, SinglePlayer, Multiplayer, PvP, CoOperative, Sandbox)
2. Theme (choose from: fantasy, medieval, sci-fi, horror, modern, cyberpunk, post-apocalyptic)
3. Key features (list up to 5 important game features mentioned)

Game description: ""{description}""

Respond in this exact format:
GameType: [type]
Theme: [theme]
Features: [feature1], [feature2], [feature3]

Be concise and only use the categories provided.";

            // Get AI response
            var aiResult = await _languageModelService.ProcessPromptAsync(prompt);

            // Parse AI response (with fallback to keyword-based parsing)
            if (aiResult.Success && !string.IsNullOrEmpty(aiResult.Response))
            {
                return ParseAIResponse(aiResult.Response, description, userId, licenseKey);
            }

            // Fallback to keyword-based parsing
            _logger.LogWarning("AI parsing failed or unavailable, using keyword-based parsing");
            return CreateFallbackRequest(description, userId, licenseKey);
        }

        /// <summary>
        /// Parse the AI response to create a game creation request
        /// </summary>
        private GameCreationRequest ParseAIResponse(string aiResponse, string description, Guid userId, string licenseKey)
        {
            var request = new GameCreationRequest
            {
                UserId = userId,
                LicenseKey = licenseKey,
                Description = description,
                GameType = GameType.Multiplayer,
                Theme = "fantasy",
                GenerateAssets = true
            };

            var lines = aiResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var cleanLine = line.Trim();

                // Parse GameType
                if (cleanLine.StartsWith("GameType:", StringComparison.OrdinalIgnoreCase))
                {
                    var typeStr = cleanLine.Substring("GameType:".Length).Trim();
                    if (Enum.TryParse<GameType>(typeStr, true, out var gameType))
                    {
                        request.GameType = gameType;
                    }
                }
                // Parse Theme
                else if (cleanLine.StartsWith("Theme:", StringComparison.OrdinalIgnoreCase))
                {
                    request.Theme = cleanLine.Substring("Theme:".Length).Trim().ToLowerInvariant();
                }
                // Parse Features
                else if (cleanLine.StartsWith("Features:", StringComparison.OrdinalIgnoreCase))
                {
                    var featuresStr = cleanLine.Substring("Features:".Length).Trim();
                    var features = featuresStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(f => f.Trim())
                        .Where(f => !string.IsNullOrEmpty(f))
                        .ToList();
                    
                    request.Features = features;
                }
            }

            _logger.LogInformation("AI parsed game as: Type={GameType}, Theme={Theme}, Features={FeatureCount}", 
                request.GameType, SanitizeForLog(request.Theme), request.Features.Count);

            return request;
        }

        /// <summary>
        /// Create fallback request using keyword-based parsing
        /// </summary>
        private GameCreationRequest CreateFallbackRequest(string description, Guid userId, string licenseKey)
        {
            var request = new GameCreationRequest
            {
                UserId = userId,
                LicenseKey = licenseKey,
                Description = description,
                GameType = GameType.Multiplayer,
                Theme = "fantasy",
                GenerateAssets = true
            };

            var lower = description.ToLowerInvariant();

            // Detect game type using keywords
            if (lower.Contains("mmo") || lower.Contains("massively"))
                request.GameType = GameType.MMO;
            else if (lower.Contains("single player") || lower.Contains("singleplayer"))
                request.GameType = GameType.SinglePlayer;
            else if (lower.Contains("pvp") || lower.Contains("versus"))
                request.GameType = GameType.PvP;
            else if (lower.Contains("coop") || lower.Contains("cooperative"))
                request.GameType = GameType.CoOperative;
            else if (lower.Contains("sandbox"))
                request.GameType = GameType.Sandbox;
            else if (lower.Contains("rpg") || lower.Contains("role-playing"))
                request.GameType = GameType.RPG;
            else if (lower.Contains("fps") || lower.Contains("shooter"))
                request.GameType = GameType.FPS;
            else if (lower.Contains("strategy"))
                request.GameType = GameType.Strategy;

            // Detect theme using keywords
            if (lower.Contains("medieval") || lower.Contains("castle") || lower.Contains("knight"))
                request.Theme = "medieval";
            else if (lower.Contains("fantasy") || lower.Contains("magic") || lower.Contains("dragon"))
                request.Theme = "fantasy";
            else if (lower.Contains("sci-fi") || lower.Contains("space") || lower.Contains("alien"))
                request.Theme = "sci-fi";
            else if (lower.Contains("horror") || lower.Contains("zombie") || lower.Contains("scary"))
                request.Theme = "horror";
            else if (lower.Contains("modern") || lower.Contains("contemporary"))
                request.Theme = "modern";
            else if (lower.Contains("cyberpunk") || lower.Contains("cyber"))
                request.Theme = "cyberpunk";
            else if (lower.Contains("post-apocalyptic") || lower.Contains("wasteland"))
                request.Theme = "post-apocalyptic";

            // Extract features using keywords
            var features = new List<string>();
            if (lower.Contains("craft")) features.Add("Crafting System");
            if (lower.Contains("quest")) features.Add("Quest System");
            if (lower.Contains("combat") || lower.Contains("battle")) features.Add("Combat System");
            if (lower.Contains("trade") || lower.Contains("economy")) features.Add("Economy System");
            if (lower.Contains("guild") || lower.Contains("clan")) features.Add("Guild System");
            if (lower.Contains("pvp") || lower.Contains("arena")) features.Add("PvP System");
            if (lower.Contains("npc")) features.Add("NPC Dialogue");
            if (lower.Contains("procedural")) features.Add("Procedural Generation");
            if (lower.Contains("multiplayer")) features.Add("Multiplayer");
            if (lower.Contains("inventory")) features.Add("Inventory System");

            request.Features = features;

            return request;
        }

        /// <summary>
        /// Get AI-enhanced suggestions for game improvements
        /// </summary>
        public async Task<string> GetGameImprovementSuggestionsAsync(string gameId)
        {
            var project = await _gameServerModule.GetGameProjectAsync(gameId);
            
            if (project == null)
            {
                return "Game project not found";
            }

            var prompt = $@"You are a game design expert. Review this game project and provide 3-5 specific suggestions for improvement:

Game Name: {project.Name}
Type: {project.Type}
Theme: {project.Theme}
Description: {project.Description}
Current Features: {string.Join(", ", project.Features)}

Provide practical, actionable suggestions that would enhance the gameplay, engagement, or technical aspects.";

            var result = await _languageModelService.ProcessPromptAsync(prompt);
            
            return result.Success ? result.Response ?? "No suggestions available" : "Unable to generate suggestions at this time";
        }
    }
}
