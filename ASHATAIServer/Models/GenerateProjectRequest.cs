namespace ASHATAIServer.Models
{
    /// <summary>
    /// Request model for generating a complete project with multiple files
    /// </summary>
    public class GenerateProjectRequest
    {
        /// <summary>
        /// Description of the project to generate
        /// </summary>
        public string ProjectDescription { get; set; } = string.Empty;

        /// <summary>
        /// Type of project (e.g., "console", "winforms", "webapi", "library")
        /// </summary>
        public string? ProjectType { get; set; }

        /// <summary>
        /// Programming language (e.g., "csharp", "python", "javascript")
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Optional specific model to use for generation
        /// </summary>
        public string? ModelName { get; set; }

        /// <summary>
        /// Additional generation options
        /// </summary>
        public GenerationOptions? Options { get; set; }
    }

    /// <summary>
    /// Options for customizing project generation
    /// </summary>
    public class GenerationOptions
    {
        /// <summary>
        /// Include comments in generated code
        /// </summary>
        public bool IncludeComments { get; set; } = true;

        /// <summary>
        /// Include unit tests
        /// </summary>
        public bool IncludeTests { get; set; } = false;

        /// <summary>
        /// Include README.md
        /// </summary>
        public bool IncludeReadme { get; set; } = true;

        /// <summary>
        /// Maximum number of files to generate (safety limit)
        /// </summary>
        public int MaxFiles { get; set; } = 50;

        /// <summary>
        /// Maximum size per file in bytes (safety limit)
        /// </summary>
        public int MaxFileSizeBytes { get; set; } = 1048576; // 1MB default
    }
}
