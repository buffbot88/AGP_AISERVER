using System.Runtime.CompilerServices;

namespace ASHATAIServer.Runtime
{
    /// <summary>
    /// Mock runtime implementation for testing purposes
    /// Simulates model behavior without requiring actual model files
    /// </summary>
    public class MockRuntime : IModelRuntime
    {
        private string? _loadedModelPath;
        private readonly ILogger<MockRuntime>? _logger;

        public bool IsModelLoaded => _loadedModelPath != null;
        public string? LoadedModelName => _loadedModelPath != null ? Path.GetFileName(_loadedModelPath) : null;

        public MockRuntime(ILogger<MockRuntime>? logger = null)
        {
            _logger = logger;
        }

        public Task<bool> LoadModelAsync(string modelPath, CancellationToken cancellationToken = default)
        {
            _logger?.LogInformation("MockRuntime: Loading model from {ModelPath}", modelPath);
            _loadedModelPath = modelPath;
            return Task.FromResult(true);
        }

        public Task UnloadModelAsync()
        {
            _logger?.LogInformation("MockRuntime: Unloading model {ModelName}", LoadedModelName);
            _loadedModelPath = null;
            return Task.CompletedTask;
        }

        public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
        {
            _logger?.LogInformation("MockRuntime: Generating response for prompt");
            
            // Simulate processing delay
            await Task.Delay(100, cancellationToken);

            // Generate contextual mock responses based on prompt
            return GenerateMockResponse(prompt);
        }

        public async IAsyncEnumerable<string> StreamAsync(string prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _logger?.LogInformation("MockRuntime: Streaming response for prompt");
            
            var response = GenerateMockResponse(prompt);
            var words = response.Split(' ');

            foreach (var word in words)
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;

                await Task.Delay(50, cancellationToken);
                yield return word + " ";
            }
        }

        private string GenerateMockResponse(string prompt)
        {
            var msg = prompt.ToLowerInvariant();

            // Language-specific queries
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

            // Simple greetings
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

            // Help and capabilities
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

            // Coding and technical help
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

            // Default response
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
}
