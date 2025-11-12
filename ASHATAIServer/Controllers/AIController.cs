using ASHATAIServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASHATAIServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly LanguageModelService _modelService;
        private readonly ILogger<AIController> _logger;

        public AIController(LanguageModelService modelService, ILogger<AIController> logger)
        {
            _modelService = modelService;
            _logger = logger;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessPrompt([FromBody] PromptRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { error = "Prompt cannot be empty" });
            }

            _logger.LogInformation("Received prompt request from client");
            var result = await _modelService.ProcessPromptAsync(request.Prompt, request.ModelName);

            if (!result.Success)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result);
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var status = _modelService.GetStatus();
            return Ok(status);
        }

        [HttpPost("models/scan")]
        public async Task<IActionResult> ScanModels()
        {
            _logger.LogInformation("Manual model scan requested");
            await _modelService.ScanAndLoadModelsAsync();
            var status = _modelService.GetStatus();
            return Ok(status);
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                server = "ASHATAIServer",
                timestamp = DateTime.UtcNow
            });
        }
    }

    public class PromptRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public string? ModelName { get; set; }
    }
}
