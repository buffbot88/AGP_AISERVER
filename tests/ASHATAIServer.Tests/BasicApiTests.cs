using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ASHATAIServer.Tests
{
    public class BasicApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public BasicApiTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task HealthEndpoint_ReturnsHealthyStatus()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/ai/health");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadFromJsonAsync<HealthResponse>();
            Assert.NotNull(result);
            Assert.Equal("healthy", result.Status);
            Assert.Equal("ASHATAIServer", result.Server);
            Assert.NotEqual(default, result.Timestamp);
        }

        [Fact]
        public async Task StatusEndpoint_ReturnsModelStatus()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/ai/status");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadAsStringAsync();
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task ProcessEndpoint_RejectsMissingPrompt()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new { Prompt = "" };

            // Act
            var response = await client.PostAsJsonAsync("/api/ai/process", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ProcessEndpoint_AcceptsValidPrompt()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new { Prompt = "Hello, ASHAT!" };

            // Act
            var response = await client.PostAsJsonAsync("/api/ai/process", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var result = await response.Content.ReadAsStringAsync();
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }

    // Response DTOs for testing
    public class HealthResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
