using System.Net;
using System.Net.Http.Json;
using ASHATAIServer.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ASHATAIServer.Tests
{
    public class ProjectControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProjectControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GenerateProject_RequiresDescription()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new GenerateProjectRequest
            {
                ProjectDescription = ""
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/ai/generate-project", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GenerateProject_ReturnsSuccessWithValidRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new GenerateProjectRequest
            {
                ProjectDescription = "Create a simple C# console application",
                ProjectType = "console",
                Language = "csharp"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/ai/generate-project", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<GenerateProjectResponse>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Files);
            Assert.NotEmpty(result.Files);
        }

        [Fact]
        public async Task GenerateProject_ReturnsMetadata()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new GenerateProjectRequest
            {
                ProjectDescription = "Create a Python hello world script",
                Language = "python"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/ai/generate-project", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<GenerateProjectResponse>();
            Assert.NotNull(result);
            Assert.NotNull(result.Metadata);
            Assert.NotNull(result.Metadata.ModelUsed);
            Assert.True(result.Metadata.FileCount > 0);
            Assert.True(result.Metadata.GenerationTimeMs >= 0);
        }

        [Fact]
        public async Task GenerateProject_WithOptions()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new GenerateProjectRequest
            {
                ProjectDescription = "Create a simple calculator class",
                Language = "csharp",
                Options = new GenerationOptions
                {
                    IncludeComments = true,
                    IncludeTests = true,
                    IncludeReadme = true,
                    MaxFiles = 10,
                    MaxFileSizeBytes = 524288 // 512KB
                }
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/ai/generate-project", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<GenerateProjectResponse>();
            Assert.NotNull(result);
            Assert.True(result.Success);
        }
    }
}
