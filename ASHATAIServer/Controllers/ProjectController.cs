using ASHATAIServer.Models;
using ASHATAIServer.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace ASHATAIServer.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class ProjectController : ControllerBase
    {
        private readonly LanguageModelService _modelService;
        private readonly ILogger<ProjectController> _logger;

        // Allowed file extensions for security
        private static readonly HashSet<string> AllowedExtensions = new()
        {
            ".cs", ".csproj", ".sln", ".json", ".xml", ".config",
            ".py", ".txt", ".md", ".gitignore", ".yml", ".yaml",
            ".js", ".ts", ".jsx", ".tsx", ".html", ".css", ".scss",
            ".java", ".go", ".rs", ".cpp", ".h", ".hpp", ".c",
            ".sql", ".sh", ".bat", ".ps1"
        };

        public ProjectController(LanguageModelService modelService, ILogger<ProjectController> logger)
        {
            _modelService = modelService;
            _logger = logger;
        }

        [HttpPost("generate-project")]
        public async Task<IActionResult> GenerateProject([FromBody] GenerateProjectRequest request)
        {
            var startTime = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(request.ProjectDescription))
            {
                return BadRequest(new GenerateProjectResponse
                {
                    Success = false,
                    Error = "Project description is required"
                });
            }

            _logger.LogInformation("Generating project: {Description}", request.ProjectDescription);

            try
            {
                // Set defaults for options
                var options = request.Options ?? new GenerationOptions();
                
                // Build the prompt for project generation
                var prompt = BuildProjectPrompt(request);

                // Generate project content using the model
                var response = await _modelService.ProcessPromptAsync(prompt, request.ModelName);

                if (!response.Success)
                {
                    return BadRequest(new GenerateProjectResponse
                    {
                        Success = false,
                        Error = response.Error ?? "Failed to generate project"
                    });
                }

                // Parse the response into structured files
                var files = ParseProjectFiles(response.Response ?? string.Empty, options);

                // Validate and sanitize files
                var sanitizedFiles = SanitizeFiles(files, options);

                var generationTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
                var totalSize = sanitizedFiles.Values.Sum(content => content.Length);

                return Ok(new GenerateProjectResponse
                {
                    Success = true,
                    Files = sanitizedFiles,
                    Metadata = new ProjectMetadata
                    {
                        ModelUsed = response.ModelUsed,
                        ProjectType = request.ProjectType ?? "unknown",
                        Language = request.Language ?? "unknown",
                        FileCount = sanitizedFiles.Count,
                        TotalSizeBytes = totalSize,
                        GenerationTimeMs = generationTime
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating project");
                return StatusCode(500, new GenerateProjectResponse
                {
                    Success = false,
                    Error = $"Internal error: {ex.Message}"
                });
            }
        }

        private string BuildProjectPrompt(GenerateProjectRequest request)
        {
            var promptParts = new List<string>
            {
                "Generate a complete project with the following requirements:",
                $"Description: {request.ProjectDescription}"
            };

            if (!string.IsNullOrEmpty(request.ProjectType))
                promptParts.Add($"Project Type: {request.ProjectType}");

            if (!string.IsNullOrEmpty(request.Language))
                promptParts.Add($"Language: {request.Language}");

            promptParts.Add("\nProvide the project files in the following format:");
            promptParts.Add("FILE: path/to/file.ext");
            promptParts.Add("```");
            promptParts.Add("file content here");
            promptParts.Add("```");
            promptParts.Add("END_FILE");
            
            if (request.Options?.IncludeComments == true)
                promptParts.Add("\nInclude helpful comments in the code.");

            if (request.Options?.IncludeTests == true)
                promptParts.Add("Include unit tests.");

            if (request.Options?.IncludeReadme == true)
                promptParts.Add("Include a README.md file.");

            return string.Join("\n", promptParts);
        }

        private Dictionary<string, string> ParseProjectFiles(string response, GenerationOptions options)
        {
            var files = new Dictionary<string, string>();

            // Pattern to match FILE: path ... ``` content ``` END_FILE blocks
            var filePattern = @"FILE:\s*([^\r\n]+)\s*```[^\r\n]*\r?\n(.*?)```\s*END_FILE";
            var matches = Regex.Matches(response, filePattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                if (files.Count >= options.MaxFiles)
                {
                    _logger.LogWarning("Reached maximum file limit: {MaxFiles}", options.MaxFiles);
                    break;
                }

                var filePath = match.Groups[1].Value.Trim();
                var content = match.Groups[2].Value.Trim();

                if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(content))
                {
                    files[filePath] = content;
                }
            }

            // If no files were parsed with the structured format, create a default file
            if (files.Count == 0)
            {
                _logger.LogInformation("No structured files found, creating default Program file");
                var language = DetermineLanguage(response);
                var extension = GetExtensionForLanguage(language);
                files[$"Program{extension}"] = response;
            }

            return files;
        }

        private Dictionary<string, string> SanitizeFiles(Dictionary<string, string> files, GenerationOptions options)
        {
            var sanitized = new Dictionary<string, string>();

            foreach (var kvp in files)
            {
                var filePath = SanitizeFilePath(kvp.Key);
                var content = kvp.Value;

                // Check file extension
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    _logger.LogWarning("Skipping file with disallowed extension: {FilePath}", filePath);
                    continue;
                }

                // Check file size
                if (content.Length > options.MaxFileSizeBytes)
                {
                    _logger.LogWarning("Truncating large file: {FilePath} ({Size} bytes)", filePath, content.Length);
                    content = content.Substring(0, options.MaxFileSizeBytes);
                }

                // Sanitize content (remove potential harmful content)
                content = SanitizeContent(content);

                sanitized[filePath] = content;
            }

            return sanitized;
        }

        private string SanitizeFilePath(string path)
        {
            // Remove any directory traversal attempts
            path = path.Replace("..", "").Replace("~", "");
            
            // Remove leading slashes
            path = path.TrimStart('/', '\\');

            // Replace backslashes with forward slashes
            path = path.Replace('\\', '/');

            // Remove any potentially harmful characters
            path = Regex.Replace(path, @"[^\w\-./]", "_");

            return path;
        }

        private string SanitizeContent(string content)
        {
            // Remove any null bytes
            content = content.Replace("\0", "");

            return content;
        }

        private string DetermineLanguage(string content)
        {
            if (content.Contains("class") && content.Contains("public") && content.Contains("{"))
                return "csharp";
            if (content.Contains("def ") && content.Contains("import "))
                return "python";
            if (content.Contains("function") || content.Contains("const ") || content.Contains("let "))
                return "javascript";
            
            return "text";
        }

        private string GetExtensionForLanguage(string language)
        {
            return language.ToLowerInvariant() switch
            {
                "csharp" or "c#" => ".cs",
                "python" => ".py",
                "javascript" or "js" => ".js",
                "typescript" or "ts" => ".ts",
                "java" => ".java",
                "go" => ".go",
                "rust" => ".rs",
                _ => ".txt"
            };
        }
    }
}
