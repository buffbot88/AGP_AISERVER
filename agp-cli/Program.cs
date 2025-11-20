using System.CommandLine;
using System.Net.Http.Json;
using System.Text.Json;

namespace AGP.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("AGP CLI - Generate projects using ASHATAIServer");

        // Generate command
        var generateCommand = new Command("generate", "Generate a new project");
        
        var descriptionOption = new Option<string>(
            aliases: new[] { "--description", "-d" },
            description: "Project description")
        { IsRequired = true };
        
        var outputOption = new Option<string>(
            aliases: new[] { "--output", "-o" },
            description: "Output directory",
            getDefaultValue: () => Directory.GetCurrentDirectory());
        
        var typeOption = new Option<string>(
            aliases: new[] { "--type", "-t" },
            description: "Project type (console, winforms, webapi)",
            getDefaultValue: () => "console");
        
        var languageOption = new Option<string>(
            aliases: new[] { "--language", "-l" },
            description: "Programming language (csharp, python, javascript)",
            getDefaultValue: () => "csharp");
        
        var serverOption = new Option<string>(
            aliases: new[] { "--server", "-s" },
            description: "Server URL",
            getDefaultValue: () => "http://localhost:7077");
        
        var modelOption = new Option<string?>(
            aliases: new[] { "--model", "-m" },
            description: "Model name (optional)");

        generateCommand.AddOption(descriptionOption);
        generateCommand.AddOption(outputOption);
        generateCommand.AddOption(typeOption);
        generateCommand.AddOption(languageOption);
        generateCommand.AddOption(serverOption);
        generateCommand.AddOption(modelOption);

        generateCommand.SetHandler(async (description, output, type, language, server, model) =>
        {
            await GenerateProject(description, output, type, language, server, model);
        }, descriptionOption, outputOption, typeOption, languageOption, serverOption, modelOption);

        rootCommand.AddCommand(generateCommand);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task GenerateProject(string description, string output, string type, string language, string server, string? model)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    AGP CLI - Project Generator                           ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine($"Description: {description}");
        Console.WriteLine($"Type: {type}");
        Console.WriteLine($"Language: {language}");
        Console.WriteLine($"Output: {output}");
        Console.WriteLine($"Server: {server}");
        if (!string.IsNullOrEmpty(model))
            Console.WriteLine($"Model: {model}");
        Console.WriteLine();

        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(server) };
            
            var request = new
            {
                projectDescription = description,
                projectType = type,
                language = language,
                modelName = model,
                options = new
                {
                    includeComments = true,
                    includeReadme = true,
                    maxFiles = 50,
                    maxFileSizeBytes = 1048576
                }
            };

            Console.WriteLine("Connecting to server...");
            var response = await client.PostAsJsonAsync("/api/ai/generate-project", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Error: {response.StatusCode}");
                Console.WriteLine(error);
                return;
            }

            Console.WriteLine("✓ Connected");
            Console.WriteLine("Generating project...");

            var result = await response.Content.ReadFromJsonAsync<GenerateProjectResponse>();
            if (result == null || !result.Success)
            {
                Console.WriteLine($"❌ Failed: {result?.Error ?? "Unknown error"}");
                return;
            }

            Console.WriteLine("✓ Project generated");
            Console.WriteLine();
            Console.WriteLine($"Files generated: {result.Files?.Count ?? 0}");
            Console.WriteLine($"Model used: {result.Metadata?.ModelUsed ?? "N/A"}");
            Console.WriteLine($"Generation time: {result.Metadata?.GenerationTimeMs ?? 0}ms");
            Console.WriteLine();

            // Create output directory
            Directory.CreateDirectory(output);

            // Save files
            Console.WriteLine("Saving files...");
            foreach (var file in result.Files ?? new Dictionary<string, string>())
            {
                var filePath = Path.Combine(output, file.Key);
                var fileDir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(fileDir))
                {
                    Directory.CreateDirectory(fileDir);
                }

                await File.WriteAllTextAsync(filePath, file.Value);
                Console.WriteLine($"  ✓ {file.Key}");
            }

            Console.WriteLine();
            Console.WriteLine($"✓ Project saved to: {Path.GetFullPath(output)}");
            Console.WriteLine();
            
            // Show build instructions if applicable
            if (language.ToLowerInvariant() == "csharp")
            {
                Console.WriteLine("Next steps:");
                Console.WriteLine($"  cd {output}");
                Console.WriteLine("  dotnet build");
                Console.WriteLine("  dotnet run");
            }
            else if (language.ToLowerInvariant() == "python")
            {
                Console.WriteLine("Next steps:");
                Console.WriteLine($"  cd {output}");
                Console.WriteLine("  python main.py");
            }
            else if (language.ToLowerInvariant() == "javascript")
            {
                Console.WriteLine("Next steps:");
                Console.WriteLine($"  cd {output}");
                Console.WriteLine("  npm install");
                Console.WriteLine("  npm start");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ Connection error: {ex.Message}");
            Console.WriteLine("Make sure ASHATAIServer is running on the specified URL.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}

class GenerateProjectResponse
{
    public bool Success { get; set; }
    public Dictionary<string, string>? Files { get; set; }
    public ProjectMetadata? Metadata { get; set; }
    public string? Error { get; set; }
}

class ProjectMetadata
{
    public string? ModelUsed { get; set; }
    public string? ProjectType { get; set; }
    public string? Language { get; set; }
    public int FileCount { get; set; }
    public long TotalSizeBytes { get; set; }
    public int GenerationTimeMs { get; set; }
    public List<string>? Warnings { get; set; }
}
