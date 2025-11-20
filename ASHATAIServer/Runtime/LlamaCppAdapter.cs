using System.Runtime.CompilerServices;
using LLama;
using LLama.Common;

namespace ASHATAIServer.Runtime
{
    /// <summary>
    /// Adapter for llama.cpp runtime to execute GGUF models using LLamaSharp
    /// </summary>
    public class LlamaCppAdapter : IModelRuntime
    {
        private readonly ILogger<LlamaCppAdapter> _logger;
        private string? _loadedModelPath;
        private LLamaWeights? _model;
        private LLamaContext? _context;
        private InteractiveExecutor? _executor;

        public bool IsModelLoaded => _loadedModelPath != null && _model != null;
        public string? LoadedModelName => _loadedModelPath != null ? Path.GetFileName(_loadedModelPath) : null;

        public LlamaCppAdapter(ILogger<LlamaCppAdapter> logger)
        {
            _logger = logger;
        }

        public async Task<bool> LoadModelAsync(string modelPath, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("LlamaCppAdapter: Loading model from {ModelPath}", modelPath);

            if (!File.Exists(modelPath))
            {
                _logger.LogError("Model file not found: {ModelPath}", modelPath);
                return false;
            }

            // Verify it's a GGUF file
            if (!modelPath.EndsWith(".gguf", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("Model file must be in GGUF format: {ModelPath}", modelPath);
                return false;
            }

            try
            {
                // Unload existing model if any
                await UnloadModelAsync();

                // Load model with LLamaSharp
                var parameters = new ModelParams(modelPath)
                {
                    ContextSize = 2048, // Context size
                    GpuLayerCount = 0,  // Use CPU only (0 GPU layers)
                    UseMemorymap = true,
                    UseMemoryLock = false
                };

                _model = await Task.Run(() => LLamaWeights.LoadFromFile(parameters), cancellationToken);
                _context = _model.CreateContext(parameters);
                _executor = new InteractiveExecutor(_context);

                _loadedModelPath = modelPath;
                _logger.LogInformation("Model loaded successfully: {ModelName}", LoadedModelName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load model: {ModelPath}", modelPath);
                return false;
            }
        }

        public Task UnloadModelAsync()
        {
            _logger.LogInformation("LlamaCppAdapter: Unloading model {ModelName}", LoadedModelName);

            _executor = null;
            _context?.Dispose();
            _context = null;
            _model?.Dispose();
            _model = null;
            _loadedModelPath = null;

            return Task.CompletedTask;
        }

        public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
        {
            if (!IsModelLoaded || _executor == null)
            {
                throw new InvalidOperationException("No model is loaded. Call LoadModelAsync first.");
            }

            _logger.LogInformation("LlamaCppAdapter: Generating response for prompt");

            try
            {
                var inferenceParams = new InferenceParams
                {
                    MaxTokens = 512,
                    AntiPrompts = new List<string> { "User:", "\n\nUser" }
                };

                // Use DefaultSamplingPipeline for sampling configuration
                inferenceParams.SamplingPipeline = new LLama.Sampling.DefaultSamplingPipeline
                {
                    Temperature = 0.7f,
                    TopP = 0.9f,
                    TopK = 40
                };

                var result = new System.Text.StringBuilder();
                
                await foreach (var token in _executor.InferAsync(prompt, inferenceParams, cancellationToken))
                {
                    result.Append(token);
                }

                return result.ToString().Trim();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Generation was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during generation");
                throw;
            }
        }

        public async IAsyncEnumerable<string> StreamAsync(string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!IsModelLoaded || _executor == null)
            {
                throw new InvalidOperationException("No model is loaded. Call LoadModelAsync first.");
            }

            _logger.LogInformation("LlamaCppAdapter: Streaming response for prompt");

            var inferenceParams = new InferenceParams
            {
                MaxTokens = 512,
                AntiPrompts = new List<string> { "User:", "\n\nUser" }
            };

            // Use DefaultSamplingPipeline for sampling configuration
            inferenceParams.SamplingPipeline = new LLama.Sampling.DefaultSamplingPipeline
            {
                Temperature = 0.7f,
                TopP = 0.9f,
                TopK = 40
            };

            await foreach (var token in _executor.InferAsync(prompt, inferenceParams, cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;

                yield return token;
            }
        }
    }
}
