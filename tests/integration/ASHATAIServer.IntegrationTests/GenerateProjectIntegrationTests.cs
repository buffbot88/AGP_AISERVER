using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json;

namespace ASHATAIServer.IntegrationTests;

/// <summary>
/// Integration tests that generate actual projects and verify they can be built
/// </summary>
public class GenerateProjectIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly string _tempDir;

    public GenerateProjectIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _tempDir = Path.Combine(Path.GetTempPath(), "agp_integration_tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task GenerateConsoleApp_BuildsSuccessfully()
    {
        // Arrange
        var request = new
        {
            projectDescription = "Create a simple C# console application that prints Hello World",
            projectType = "console",
            language = "csharp",
            options = new
            {
                includeComments = true,
                includeReadme = true
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ai/generate-project", request);

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GenerateProjectResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Files);
        Assert.NotEmpty(result.Files);

        // Save files to disk
        var projectDir = Path.Combine(_tempDir, "console-app");
        Directory.CreateDirectory(projectDir);

        foreach (var file in result.Files)
        {
            var filePath = Path.Combine(projectDir, file.Key);
            var fileDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(fileDir))
            {
                Directory.CreateDirectory(fileDir);
            }
            await File.WriteAllTextAsync(filePath, file.Value);
        }

        // Verify files were created
        Assert.True(Directory.GetFiles(projectDir, "*.*", SearchOption.AllDirectories).Length > 0);

        // Attempt to build (if .csproj exists)
        var csprojFiles = Directory.GetFiles(projectDir, "*.csproj", SearchOption.AllDirectories);
        if (csprojFiles.Length > 0)
        {
            var buildResult = await RunDotnetBuild(csprojFiles[0]);
            Assert.True(buildResult, "Generated project should build successfully");
        }
    }

    [Fact]
    public async Task GeneratePythonScript_CreatesValidFiles()
    {
        // Arrange
        var request = new
        {
            projectDescription = "Create a Python script that prints Hello World",
            projectType = "console",
            language = "python",
            options = new
            {
                includeComments = true,
                includeReadme = true
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ai/generate-project", request);

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GenerateProjectResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Files);
        Assert.NotEmpty(result.Files);

        // Save files
        var projectDir = Path.Combine(_tempDir, "python-script");
        Directory.CreateDirectory(projectDir);

        foreach (var file in result.Files)
        {
            var filePath = Path.Combine(projectDir, file.Key);
            await File.WriteAllTextAsync(filePath, file.Value);
        }

        // Verify Python files exist
        var pythonFiles = Directory.GetFiles(projectDir, "*.py", SearchOption.AllDirectories);
        Assert.True(pythonFiles.Length > 0, "At least one Python file should be generated");
    }

    [Fact]
    public async Task GenerateWebApi_CreatesValidProject()
    {
        // Arrange
        var request = new
        {
            projectDescription = "Create a simple REST API with one endpoint",
            projectType = "webapi",
            language = "csharp",
            options = new
            {
                includeComments = true,
                includeReadme = true
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ai/generate-project", request);

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GenerateProjectResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Files);
        Assert.NotEmpty(result.Files);

        // Verify metadata
        Assert.NotNull(result.Metadata);
        Assert.Equal("webapi", result.Metadata.ProjectType);
        Assert.Equal("csharp", result.Metadata.Language);
        Assert.True(result.Metadata.GenerationTimeMs > 0);
    }

    [Fact]
    public async Task GenerateProject_WithInvalidRequest_ReturnsError()
    {
        // Arrange
        var request = new
        {
            projectDescription = "", // Empty description should fail
            projectType = "console",
            language = "csharp"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ai/generate-project", request);

        // Assert - Expecting error response but not necessarily a failure status code
        // as the API may return 200 with success=false
        var result = await response.Content.ReadFromJsonAsync<GenerateProjectResponse>();
        Assert.NotNull(result);
        
        // Either the request fails with bad request, or returns success=false
        Assert.True(!response.IsSuccessStatusCode || result.Success == false);
    }

    [Fact]
    public async Task GenerateMultipleProjects_InSequence_AllSucceed()
    {
        // Arrange
        var requests = new[]
        {
            new { projectDescription = "Create a calculator", projectType = "console", language = "csharp" },
            new { projectDescription = "Create a todo list", projectType = "console", language = "csharp" },
            new { projectDescription = "Create a greeting app", projectType = "console", language = "csharp" }
        };

        // Act & Assert
        foreach (var request in requests)
        {
            var response = await _client.PostAsJsonAsync("/api/ai/generate-project", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GenerateProjectResponse>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Files);
            Assert.NotEmpty(result.Files);
        }
    }

    private async Task<bool> RunDotnetBuild(string projectPath)
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{projectPath}\" --configuration Release",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process == null) return false;

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}

public class GenerateProjectResponse
{
    public bool Success { get; set; }
    public Dictionary<string, string>? Files { get; set; }
    public ProjectMetadata? Metadata { get; set; }
    public string? Error { get; set; }
}

public class ProjectMetadata
{
    public string? ModelUsed { get; set; }
    public string? ProjectType { get; set; }
    public string? Language { get; set; }
    public int FileCount { get; set; }
    public long TotalSizeBytes { get; set; }
    public int GenerationTimeMs { get; set; }
    public List<string>? Warnings { get; set; }
}
