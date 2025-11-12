using Abstractions;
using ASHATAIServer.Services;
using LegendaryGameSystem;
using Microsoft.AspNetCore.Mvc;

namespace ASHATAIServer.Controllers
{
    /// <summary>
    /// Controller for Game Server operations integrated into ASHATAIServer
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GameServerController : ControllerBase
    {
        private readonly GameServerModule _gameServerModule;
        private readonly AIEnhancedGameServerService _aiEnhancedService;
        private readonly ILogger<GameServerController> _logger;

        public GameServerController(
            GameServerModule gameServerModule,
            AIEnhancedGameServerService aiEnhancedService,
            ILogger<GameServerController> logger)
        {
            _gameServerModule = gameServerModule;
            _aiEnhancedService = aiEnhancedService;
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
        /// Get game server status
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            _logger.LogInformation("Getting game server status");
            var status = _gameServerModule.Process("gameserver status");
            return Ok(new { status });
        }

        /// <summary>
        /// Get game server capabilities
        /// </summary>
        [HttpGet("capabilities")]
        public async Task<IActionResult> GetCapabilities()
        {
            _logger.LogInformation("Getting game server capabilities");
            var capabilities = await _gameServerModule.GetCapabilitiesAsync();
            return Ok(capabilities);
        }

        /// <summary>
        /// List all game projects
        /// </summary>
        [HttpGet("projects")]
        public IActionResult ListProjects()
        {
            _logger.LogInformation("Listing all game projects");
            var projects = _gameServerModule.Process("gameserver list");
            return Ok(new { projects });
        }

        /// <summary>
        /// Create a new game from description
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromBody] GameCreationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest(new { error = "Game description cannot be empty" });
            }

            _logger.LogInformation("Creating game from description: {Description}", SanitizeForLog(request.Description));
            var response = await _gameServerModule.CreateGameFromDescriptionAsync(request);

            if (!response.Success)
            {
                return BadRequest(new { error = response.Message });
            }

            return Ok(response);
        }

        /// <summary>
        /// Get game project preview
        /// </summary>
        [HttpGet("preview/{gameId}")]
        public async Task<IActionResult> GetPreview(string gameId)
        {
            _logger.LogInformation("Getting preview for game: {GameId}", SanitizeForLog(gameId));
            var preview = await _gameServerModule.GetGamePreviewAsync(gameId);
            
            if (preview.Name == "Not Found")
            {
                return NotFound(new { error = "Game project not found" });
            }

            return Ok(preview);
        }

        /// <summary>
        /// Deploy a game server
        /// </summary>
        [HttpPost("deploy/{gameId}")]
        public async Task<IActionResult> DeployGame(string gameId, [FromBody] DeploymentOptions? options = null)
        {
            _logger.LogInformation("Deploying game server: {GameId}", SanitizeForLog(gameId));
            
            var deploymentOptions = options ?? new DeploymentOptions();
            var response = await _gameServerModule.DeployGameServerAsync(gameId, deploymentOptions);

            if (!response.Success)
            {
                return BadRequest(new { error = response.Message });
            }

            return Ok(response);
        }

        /// <summary>
        /// Update a game project
        /// </summary>
        [HttpPut("update/{gameId}")]
        public async Task<IActionResult> UpdateGame(string gameId, [FromBody] UpdateGameRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UpdateDescription))
            {
                return BadRequest(new { error = "Update description cannot be empty" });
            }

            _logger.LogInformation("Updating game: {GameId}", SanitizeForLog(gameId));
            var response = await _gameServerModule.UpdateGameAsync(gameId, request.UpdateDescription);

            if (!response.Success)
            {
                return BadRequest(new { error = response.Message });
            }

            return Ok(response);
        }

        /// <summary>
        /// Delete a game project
        /// </summary>
        [HttpDelete("{gameId}")]
        public async Task<IActionResult> DeleteGame(string gameId)
        {
            _logger.LogInformation("Deleting game project: {GameId}", SanitizeForLog(gameId));
            var response = await _gameServerModule.DeleteGameProjectAsync(gameId);

            if (!response.Success)
            {
                return BadRequest(new { error = response.Message });
            }

            return Ok(response);
        }

        /// <summary>
        /// Export a game project
        /// </summary>
        [HttpPost("export/{gameId}")]
        public async Task<IActionResult> ExportGame(string gameId, [FromBody] ExportGameRequest? request = null)
        {
            var format = request?.Format ?? ExportFormat.Complete;
            
            _logger.LogInformation("Exporting game: {GameId} in format: {Format}", SanitizeForLog(gameId), format);
            var response = await _gameServerModule.ExportGameProjectAsync(gameId, format);

            if (!response.Success)
            {
                return BadRequest(new { error = response.Message });
            }

            return Ok(response);
        }

        /// <summary>
        /// Get game project details
        /// </summary>
        [HttpGet("project/{gameId}")]
        public async Task<IActionResult> GetProject(string gameId)
        {
            _logger.LogInformation("Getting game project: {GameId}", SanitizeForLog(gameId));
            var project = await _gameServerModule.GetGameProjectAsync(gameId);

            if (project == null)
            {
                return NotFound(new { error = "Game project not found" });
            }

            return Ok(project);
        }

        /// <summary>
        /// List games for a specific user
        /// </summary>
        [HttpGet("user/{userId}/games")]
        public async Task<IActionResult> GetUserGames(Guid userId)
        {
            _logger.LogInformation("Getting games for user: {UserId}", userId);
            var games = await _gameServerModule.ListUserGamesAsync(userId);
            return Ok(games);
        }

        /// <summary>
        /// Create a game with AI-enhanced description parsing
        /// </summary>
        [HttpPost("create-ai-enhanced")]
        public async Task<IActionResult> CreateGameWithAI([FromBody] AIGameCreationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest(new { error = "Game description cannot be empty" });
            }

            _logger.LogInformation("Creating game with AI enhancement: {Description}", SanitizeForLog(request.Description));
            var response = await _aiEnhancedService.CreateGameWithAIAsync(
                request.Description, 
                request.UserId, 
                request.LicenseKey ?? "DEMO");

            if (!response.Success)
            {
                return BadRequest(new { error = response.Message });
            }

            return Ok(response);
        }

        /// <summary>
        /// Get AI-generated suggestions for game improvements
        /// </summary>
        [HttpGet("suggestions/{gameId}")]
        public async Task<IActionResult> GetGameSuggestions(string gameId)
        {
            _logger.LogInformation("Getting AI suggestions for game: {GameId}", SanitizeForLog(gameId));
            var suggestions = await _aiEnhancedService.GetGameImprovementSuggestionsAsync(gameId);
            return Ok(new { gameId, suggestions });
        }
    }

    /// <summary>
    /// Request model for AI-enhanced game creation
    /// </summary>
    public class AIGameCreationRequest
    {
        public string Description { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string? LicenseKey { get; set; }
    }

    /// <summary>
    /// Request model for updating a game
    /// </summary>
    public class UpdateGameRequest
    {
        public string UpdateDescription { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for exporting a game
    /// </summary>
    public class ExportGameRequest
    {
        public ExportFormat Format { get; set; } = ExportFormat.Complete;
    }
}
