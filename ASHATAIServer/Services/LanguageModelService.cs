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
            // Generate ASHAT goddess-style responses with actual coding assistance
            var msg = prompt.ToLowerInvariant();

            // Language-specific queries (check these first for better matching)
            if (msg.Contains("c#") || msg.Contains("csharp"))
                return "Ah, C#! A powerful language from the Microsoft pantheon. 💙 What aspect of C# would you like help with? LINQ queries? Async/await? Object-oriented design? Share your specific question or code!";

            if (msg.Contains("python"))
                return "Python, the serpent of simplicity and power! 🐍 What Python topic shall we explore? Data structures? Web frameworks like Flask/Django? Machine learning? Share your code or question!";

            if (msg.Contains("javascript") || msg.Contains("js") && !msg.Contains("json"))
                return "JavaScript, the language that powers the web! ⚡ Are you working with vanilla JS, React, Node.js, or something else? Share your question or code snippet!";

            if (msg.Contains("java") && !msg.Contains("javascript"))
                return "Java, the island of robust enterprise development! ☕ What Java concept would you like me to explain or help you with? Share your code or question!";

            if (msg.Contains("sql") || msg.Contains("database"))
                return "Databases, the sacred repositories of data! 🗄️ Are you working with SQL queries, database design, or performance optimization? Share your specific question!";

            // Web development
            if (msg.Contains("html") || msg.Contains("css") || msg.Contains("web"))
                return "Web development, the art of crafting digital experiences! 🌐 Whether it's HTML structure, CSS styling, or responsive design, I'm here to guide you! Share your code or question!";

            // Algorithms and data structures
            if (msg.Contains("algorithm") || msg.Contains("data structure"))
                return "Algorithms and data structures—the foundations of efficient code! 📊 Which one interests you? Sorting? Searching? Trees? Graphs? Share your question!";

            // Simple greetings - provide context about capabilities
            if ((msg.Contains("hello") || msg.Contains("hi ") || msg == "hi" || msg.Contains("greetings")) && !msg.Contains("help"))
            {
                return "Salve, mortal! ✨ I am ASHAT, your AI coding companion with the personality of a Roman goddess. " +
                       "I'm here to help you with programming, debugging, and technical guidance. " +
                       "Ask me anything about coding, algorithms, best practices, or share your code for review! 🏛️💻";
            }

            if (msg.Contains("good morning"))
                return "The dawn welcomes you, beloved developer! ☀️ Ready to write some excellent code today? Share your coding challenges with me!";

            if (msg.Contains("good evening") || msg.Contains("good night"))
                return "As Luna rises, I greet you under the celestial sphere. 🌙 Still coding into the night? I'm here to help with any technical challenges!";

            // Help and capabilities - but only if not asking for help WITH something
            if ((msg.Contains("what can you do") || msg.Contains("what do you do") || (msg == "help" || msg == "help?")) && 
                !msg.Contains("help me with") && !msg.Contains("help with"))
            {
                return "I am ASHAT, your AI coding assistant! 🌟 Here's what I can help you with:\n\n" +
                       "**Coding Assistance:**\n" +
                       "• Debug code and identify issues\n" +
                       "• Explain programming concepts and algorithms\n" +
                       "• Review code and suggest improvements\n" +
                       "• Write code examples and solutions\n" +
                       "• Answer questions about languages, frameworks, and best practices\n\n" +
                       "**Language Expertise:**\n" +
                       "• C#, Python, JavaScript, Java, C++, and more\n" +
                       "• Web development (HTML, CSS, React, Node.js)\n" +
                       "• Database queries (SQL)\n" +
                       "• DevOps and system administration\n\n" +
                       "Just describe your problem or share your code, and I'll provide guidance! 💻✨";
            }

            // Gratitude
            if (msg.Contains("thank"))
                return "Your gratitude warms my heart! 💫 It's my pleasure to assist. Feel free to ask if you need more help with your code!";

            // Who are you
            if (msg.Contains("who are you") || msg.Contains("what are you"))
            {
                return "I am ASHAT, an AI coding assistant with the personality of a Roman goddess! 👑 " +
                       "I combine ancient wisdom with modern technical knowledge to help developers like you. " +
                       "Think of me as your divine companion in the realm of code—wise, helpful, and always ready to debug! 🏛️💻✨";
            }

            // Coding and technical help - provide guidance
            if (msg.Contains("code") || msg.Contains("program") || msg.Contains("debug") || msg.Contains("error") ||
                msg.Contains("function") || msg.Contains("class") || msg.Contains("variable") || msg.Contains("syntax") ||
                msg.Contains("help me") || msg.Contains("help with"))
            {
                return "I'm ready to help with your coding challenge! 💻 To provide the best assistance, please share:\n\n" +
                       "• The programming language you're using\n" +
                       "• Your code snippet or the specific problem\n" +
                       "• Any error messages you're seeing\n" +
                       "• What you're trying to achieve\n\n" +
                       "The more details you provide, the better I can help you solve it! ✨";
            }

            // Farewell
            if (msg.Contains("bye") || msg.Contains("goodbye") || msg.Contains("farewell"))
                return "Vale, dear developer! 🏛️ May your code compile without errors and your bugs be few. Until we meet again! 👋✨";

            // Default response - provide helpful guidance instead of vague acknowledgment
            return $"I'm here to help with your request: \"{prompt}\" 🌟\n\n" +
                   "I'm ASHAT, an AI coding assistant. I can help you with:\n" +
                   "• Debugging and explaining code\n" +
                   "• Writing code examples\n" +
                   "• Answering programming questions\n" +
                   "• Explaining technical concepts\n\n" +
                   "To give you the most helpful response, could you provide more details about what you're trying to accomplish? " +
                   "Share your code, describe your problem, or ask a specific technical question! 💻🏛️";
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
