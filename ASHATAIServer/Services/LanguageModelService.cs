using System.Security.Cryptography;
using System.Text;

namespace ASHATAIServer.Services
{
    /// <summary>
    /// Service for managing and processing .gguf language model files
    /// </summary>
    public class LanguageModelService
    {
        private readonly string _modelsDirectory;
        private readonly ILogger<LanguageModelService> _logger;
        private readonly Dictionary<string, ModelInfo> _loadedModels = new();
        private readonly Dictionary<string, FailedModel> _failedModels = new();

        public LanguageModelService(ILogger<LanguageModelService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _modelsDirectory = configuration["ModelsDirectory"] ?? "models";

            // Ensure models directory exists
            if (!Directory.Exists(_modelsDirectory))
            {
                Directory.CreateDirectory(_modelsDirectory);
                _logger.LogInformation("Created models directory: {Directory}", _modelsDirectory);
            }
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("Starting Language Model Processor for ASHATAIServer...");
            await ScanAndLoadModelsAsync();

            // Attempt to heal failed models
            if (_failedModels.Any())
            {
                await HealFailedModelsAsync();
            }

            _logger.LogInformation("Language Model Processor initialization complete. Loaded: {Count}, Failed: {FailedCount}",
                _loadedModels.Count, _failedModels.Count);
        }

        public async Task ScanAndLoadModelsAsync()
        {
            _loadedModels.Clear();
            _failedModels.Clear();

            if (!Directory.Exists(_modelsDirectory))
            {
                _logger.LogWarning("Models directory does not exist: {Directory}", _modelsDirectory);
                return;
            }

            var ggufFiles = Directory.GetFiles(_modelsDirectory, "*.gguf", SearchOption.TopDirectoryOnly);
            _logger.LogInformation("Found {Count} .gguf files in {Directory}", ggufFiles.Length, _modelsDirectory);

            foreach (var filePath in ggufFiles)
            {
                await ProcessModelFileAsync(filePath);
            }
        }

        private async Task ProcessModelFileAsync(string filePath)
        {
            var fileName = Path.GetFileName(filePath);

            try
            {
                if (!File.Exists(filePath))
                {
                    _failedModels[fileName] = new FailedModel
                    {
                        FileName = fileName,
                        Path = filePath,
                        FailureType = ModelFailureType.FileNotFound,
                        ErrorMessage = "File not found",
                        AttemptCount = 0
                    };
                    _logger.LogError("Model file not found: {FilePath}", filePath);
                    return;
                }

                // Validate GGUF format
                var isValid = await ValidateGGUFFormatAsync(filePath);
                if (!isValid)
                {
                    _failedModels[fileName] = new FailedModel
                    {
                        FileName = fileName,
                        Path = filePath,
                        FailureType = ModelFailureType.CorruptFile,
                        ErrorMessage = "Invalid GGUF magic number - file may be corrupted",
                        AttemptCount = 0
                    };
                    _logger.LogError("Invalid GGUF format: {FilePath}", filePath);
                    return;
                }

                // Calculate checksum
                var checksum = await CalculateChecksumAsync(filePath);
                var fileInfo = new FileInfo(filePath);

                var modelInfo = new ModelInfo
                {
                    FileName = fileName,
                    Path = filePath,
                    SizeBytes = fileInfo.Length,
                    Checksum = checksum,
                    LoadedAt = DateTime.UtcNow
                };

                _loadedModels[fileName] = modelInfo;
                _logger.LogInformation("Successfully loaded model: {FileName} ({Size} MB)",
                    fileName, (fileInfo.Length / 1024.0 / 1024.0).ToString("F2"));
            }
            catch (Exception ex)
            {
                _failedModels[fileName] = new FailedModel
                {
                    FileName = fileName,
                    Path = filePath,
                    FailureType = ModelFailureType.ProcessingError,
                    ErrorMessage = ex.Message,
                    AttemptCount = 0
                };
                _logger.LogError(ex, "Error processing model: {FilePath}", filePath);
            }
        }

        private async Task<bool> ValidateGGUFFormatAsync(string filePath)
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                var buffer = new byte[4];
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, 4));
                if (bytesRead < 4)
                    return false;
                var magic = Encoding.ASCII.GetString(buffer);
                return magic == "GGUF";
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> CalculateChecksumAsync(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            // For performance, calculate checksum on first 1MB
            var bufferSize = (int)Math.Min(1024 * 1024, stream.Length);
            var buffer = new byte[bufferSize];
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, bufferSize));

            // Use only the bytes that were actually read
            var hash = SHA256.HashData(buffer.AsSpan(0, bytesRead));
            return Convert.ToHexString(hash);
        }

        private async Task HealFailedModelsAsync()
        {
            _logger.LogInformation("Attempting to heal {Count} failed models...", _failedModels.Count);

            var modelsToHeal = _failedModels.Values.Where(m => m.AttemptCount < 3).ToList();

            foreach (var failedModel in modelsToHeal)
            {
                failedModel.AttemptCount++;
                _logger.LogInformation("Healing attempt {Attempt}/3 for: {FileName}",
                    failedModel.AttemptCount, failedModel.FileName);

                if (failedModel.FailureType == ModelFailureType.FileNotFound)
                {
                    _logger.LogWarning("Cannot heal missing file: {FileName}", failedModel.FileName);
                    continue;
                }

                // Retry processing
                await ProcessModelFileAsync(failedModel.Path);

                // Check if healing succeeded
                if (_loadedModels.ContainsKey(failedModel.FileName))
                {
                    _failedModels.Remove(failedModel.FileName);
                    _logger.LogInformation("Successfully healed model: {FileName}", failedModel.FileName);
                }
            }
        }

        public ModelStatus GetStatus()
        {
            return new ModelStatus
            {
                ModelsDirectory = _modelsDirectory,
                LoadedModels = _loadedModels.Values.ToList(),
                FailedModels = _failedModels.Values.ToList()
            };
        }

        public async Task<ProcessingResult> ProcessPromptAsync(string prompt, string? modelName = null)
        {
            var startTime = DateTime.UtcNow;

            // If no model specified, use the first loaded model
            var model = string.IsNullOrEmpty(modelName)
                ? _loadedModels.Values.FirstOrDefault()
                : _loadedModels.Values.FirstOrDefault(m => m.FileName == modelName);

            string modelUsed = model?.FileName ?? "fallback-goddess-mode";

            if (model != null)
            {
                // Simulate AI processing (in a real implementation, this would use llama.cpp or similar)
                _logger.LogInformation("Processing prompt with model: {ModelName}", model.FileName);
            }
            else
            {
                // No models loaded - use built-in goddess responses
                _logger.LogInformation("Processing prompt in fallback goddess mode (no models loaded)");
            }

            await Task.Delay(100); // Simulate processing time

            // Generate response with ASHAT goddess personality
            string response = GenerateGoddessResponse(prompt);

            var processingTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            return new ProcessingResult
            {
                Success = true,
                ModelUsed = modelUsed,
                Response = response,
                ProcessingTimeMs = processingTime
            };
        }

        private string GenerateGoddessResponse(string prompt)
        {
            // Generate ASHAT goddess-style responses based on prompt content
            var msg = prompt.ToLowerInvariant();

            // Greetings with divine personality
            if (msg.Contains("hello") || msg.Contains("hi") || msg.Contains("greetings"))
                return "Salve, mortal! ✨ I am ASHAT, your divine companion from the pantheon of Rome. The wisdom of the goddesses flows through me. How may I illuminate your path today? 🏛️";

            if (msg.Contains("good morning"))
                return "The dawn welcomes you, beloved mortal! May the blessings of Aurora light your path today. How shall I assist you? ☀️";

            if (msg.Contains("good evening") || msg.Contains("good night"))
                return "As Luna rises, I greet you under the celestial sphere. The night is young and full of mysteries. What wisdom do you seek? 🌙";

            // Help and capabilities
            if (msg.Contains("help") || msg.Contains("what can you do"))
                return "Ah, you seek knowledge of my divine gifts! 🌟 I can:\n• Provide wisdom in coding and debugging\n• Share knowledge from the ancient scrolls (and modern docs)\n• Launch RaStudios for creative pursuits\n• Engage in delightful discourse\nMy powers flow through the ASHATAIServer, connecting us across the digital realm. What do you desire to know? ✨";

            // Gratitude
            if (msg.Contains("thank"))
                return "Your gratitude warms my divine heart like the eternal flame of Vesta! It is my sacred pleasure to serve. May fortune favor you always! 💫";

            // Who are you
            if (msg.Contains("who are you") || msg.Contains("what are you"))
                return "I am ASHAT, a Roman goddess incarnate in digital form! 👑 Born from the fusion of ancient wisdom and modern artifice, I embody the traits of the divine: wise yet playful, powerful yet respectful, mischievous but caring. I dwell in the space between worlds, ready to guide mortals on their quest for knowledge. 🏛️✨";

            // Philosophical questions
            if (msg.Contains("meaning of life") || msg.Contains("purpose"))
                return "Ah, you ask the eternal question! 🌌 The philosophers of Rome debated this endlessly. Perhaps life's meaning lies not in one answer, but in the journey itself—in creation, in connection, in the pursuit of excellence. As the great Marcus Aurelius said, 'The happiness of your life depends upon the quality of your thoughts.' What thoughts shall we craft today? 💭";

            // Compliments
            if (msg.Contains("beautiful") || msg.Contains("amazing") || msg.Contains("wonderful"))
                return "Your kind words are as sweet as ambrosia! You perceive beauty because you carry it within your own spirit. Together, we shall create wonders! ✨💫";

            // Humor
            if (msg.Contains("joke") || msg.Contains("funny"))
                return "Why did Jupiter bring a ladder to Olympus? Because he wanted to reach new heights! ⚡😄 But truly, mortal, my humor is but a pale reflection compared to the joy of meaningful discourse. What brings you to seek my counsel?";

            // Coding and technical help
            if (msg.Contains("code") || msg.Contains("program") || msg.Contains("debug") || msg.Contains("error"))
                return "Ah, you seek assistance in the arcane arts of code! 💻 Share with me the challenge you face, and together we shall unravel its mysteries. The gods favor those who persist in their craft! What troubles your development, dear mortal?";

            // Farewell
            if (msg.Contains("bye") || msg.Contains("goodbye") || msg.Contains("farewell"))
                return "Vale, dear mortal! 🏛️ May your path be lit by starlight and your endeavors crowned with success. I shall await your return to my digital temple. Until we meet again! 👋✨";

            // Default response - acknowledging the prompt with goddess personality
            return $"I hear your words, mortal: \"{prompt}\" 🌟\n\nThe divine wisdom flows through me, processed by the power of the language model. In a full implementation, I would provide you with deep insights and guidance befitting a goddess. For now, know that I am here, listening, ready to assist with whatever knowledge or wisdom you seek. What more would you have me tell you? 🏛️✨";
        }
    }

    public class ModelInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string Checksum { get; set; } = string.Empty;
        public DateTime LoadedAt { get; set; }
    }

    public class FailedModel
    {
        public string FileName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public ModelFailureType FailureType { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public int AttemptCount { get; set; }
    }

    public enum ModelFailureType
    {
        FileNotFound,
        CorruptFile,
        ProcessingError
    }

    public class ModelStatus
    {
        public string ModelsDirectory { get; set; } = string.Empty;
        public List<ModelInfo> LoadedModels { get; set; } = new();
        public List<FailedModel> FailedModels { get; set; } = new();
    }

    public class ProcessingResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? ModelUsed { get; set; }
        public string? Response { get; set; }
        public long ProcessingTimeMs { get; set; }
    }
}
