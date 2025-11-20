namespace ASHATAIServer.Models
{
    /// <summary>
    /// Response model for project generation containing structured files
    /// </summary>
    public class GenerateProjectResponse
    {
        /// <summary>
        /// Indicates if generation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if generation failed
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Generated files as a dictionary of path -> content
        /// </summary>
        public Dictionary<string, string> Files { get; set; } = new();

        /// <summary>
        /// Metadata about the generated project
        /// </summary>
        public ProjectMetadata Metadata { get; set; } = new();
    }

    /// <summary>
    /// Metadata about the generated project
    /// </summary>
    public class ProjectMetadata
    {
        /// <summary>
        /// Name of the model used for generation
        /// </summary>
        public string? ModelUsed { get; set; }

        /// <summary>
        /// Project type that was generated
        /// </summary>
        public string? ProjectType { get; set; }

        /// <summary>
        /// Programming language
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Number of files generated
        /// </summary>
        public int FileCount { get; set; }

        /// <summary>
        /// Total size of all generated files in bytes
        /// </summary>
        public long TotalSizeBytes { get; set; }

        /// <summary>
        /// Time taken to generate in milliseconds
        /// </summary>
        public long GenerationTimeMs { get; set; }

        /// <summary>
        /// Warnings or notes about the generation
        /// </summary>
        public List<string> Warnings { get; set; } = new();
    }
}
