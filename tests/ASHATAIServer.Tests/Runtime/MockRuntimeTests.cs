using ASHATAIServer.Runtime;
using Xunit;

namespace ASHATAIServer.Tests.Runtime
{
    public class MockRuntimeTests
    {
        [Fact]
        public async Task LoadModelAsync_SetsIsModelLoaded()
        {
            // Arrange
            var runtime = new MockRuntime();

            // Act
            var result = await runtime.LoadModelAsync("test-model.gguf");

            // Assert
            Assert.True(result);
            Assert.True(runtime.IsModelLoaded);
            Assert.Equal("test-model.gguf", runtime.LoadedModelName);
        }

        [Fact]
        public async Task UnloadModelAsync_ClearsLoadedModel()
        {
            // Arrange
            var runtime = new MockRuntime();
            await runtime.LoadModelAsync("test-model.gguf");

            // Act
            await runtime.UnloadModelAsync();

            // Assert
            Assert.False(runtime.IsModelLoaded);
            Assert.Null(runtime.LoadedModelName);
        }

        [Fact]
        public async Task GenerateAsync_ReturnsResponse()
        {
            // Arrange
            var runtime = new MockRuntime();
            await runtime.LoadModelAsync("test-model.gguf");

            // Act
            var response = await runtime.GenerateAsync("Hello, ASHAT!");

            // Assert
            Assert.NotNull(response);
            Assert.NotEmpty(response);
            Assert.Contains("ASHAT", response);
        }

        [Fact]
        public async Task GenerateAsync_HandlesCodeQuestions()
        {
            // Arrange
            var runtime = new MockRuntime();
            await runtime.LoadModelAsync("test-model.gguf");

            // Act
            var response = await runtime.GenerateAsync("How do I use C# async/await?");

            // Assert
            Assert.NotNull(response);
            Assert.Contains("C#", response);
        }

        [Fact]
        public async Task StreamAsync_ReturnsTokens()
        {
            // Arrange
            var runtime = new MockRuntime();
            await runtime.LoadModelAsync("test-model.gguf");
            var tokens = new List<string>();

            // Act
            await foreach (var token in runtime.StreamAsync("Hello"))
            {
                tokens.Add(token);
            }

            // Assert
            Assert.NotEmpty(tokens);
        }

        [Fact]
        public async Task IsModelLoaded_ReturnsFalseInitially()
        {
            // Arrange
            var runtime = new MockRuntime();

            // Assert
            Assert.False(runtime.IsModelLoaded);
            Assert.Null(runtime.LoadedModelName);
        }

        [Fact]
        public async Task LoadModelAsync_CanSwitchModels()
        {
            // Arrange
            var runtime = new MockRuntime();
            await runtime.LoadModelAsync("model1.gguf");

            // Act
            await runtime.LoadModelAsync("model2.gguf");

            // Assert
            Assert.True(runtime.IsModelLoaded);
            Assert.Equal("model2.gguf", runtime.LoadedModelName);
        }

        [Fact]
        public async Task GenerateAsync_HandlesMultiplePrompts()
        {
            // Arrange
            var runtime = new MockRuntime();
            await runtime.LoadModelAsync("test-model.gguf");

            // Act
            var response1 = await runtime.GenerateAsync("Hello");
            var response2 = await runtime.GenerateAsync("Tell me about Python");

            // Assert
            Assert.NotNull(response1);
            Assert.NotNull(response2);
            Assert.NotEqual(response1, response2);
        }
    }
}
