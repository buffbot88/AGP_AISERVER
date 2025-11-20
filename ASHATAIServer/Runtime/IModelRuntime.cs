namespace ASHATAIServer.Runtime
{
    /// <summary>
    /// Interface for model runtime implementations that can load and execute language models
    /// </summary>
    public interface IModelRuntime
    {
        /// <summary>
        /// Load a model from the specified path
        /// </summary>
        /// <param name="modelPath">Path to the model file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if model loaded successfully</returns>
        Task<bool> LoadModelAsync(string modelPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unload the currently loaded model
        /// </summary>
        Task UnloadModelAsync();

        /// <summary>
        /// Generate a response for the given prompt
        /// </summary>
        /// <param name="prompt">The input prompt</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generated response text</returns>
        Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generate a streaming response for the given prompt
        /// </summary>
        /// <param name="prompt">The input prompt</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Async enumerable of token chunks</returns>
        IAsyncEnumerable<string> StreamAsync(string prompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if a model is currently loaded
        /// </summary>
        bool IsModelLoaded { get; }

        /// <summary>
        /// Get the name of the currently loaded model
        /// </summary>
        string? LoadedModelName { get; }
    }
}
