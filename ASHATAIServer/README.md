# ASHATAIServer - AI Processing Server

## 🌟 Overview

**ASHATAIServer** is an AI processing server that provides:
- **AI Language Model Processing** - Using .gguf (GGUF format) model files for AI inference
- **User Authentication** - Secure login and registration system with local user database

Originally designed for the ASHAT Goddess desktop client, it now provides comprehensive AI services through a REST API.

**Note:** The Game Server functionality has been separated into a standalone module and is no longer part of this server.

## ✨ Features

### AI Services
- 🤖 **Language Model Processing** - Processes AI requests using .gguf model files
- 🔄 **Automatic Model Discovery** - Scans and loads models from the models directory
- 🛡️ **Self-Healing** - Automatically attempts to recover from failed model loads
- 📊 **Status Monitoring** - Real-time status of loaded models
- ✅ **Health Checks** - Built-in health check endpoints

### Authentication Services
- 🔐 **User Management** - Secure user registration and login
- 🗄️ **Local Database** - SQLite-based user storage
- 🔑 **Session Tokens** - Token-based authentication

### Integration
- 🔓 **CORS Enabled** - Ready for cross-origin requests from ASHAT clients
- 🌐 **REST API** - Comprehensive HTTP API for all services

## 🚀 Quick Start

### Prerequisites

- .NET 9.0 SDK or Runtime
- Optional: .gguf language model files (e.g., from Llama, Mistral, etc.)

### Installation

1. **Navigate to the server directory:**
   ```bash
   cd ASHATAIServer
   ```

2. **Build the project:**
   ```bash
   dotnet build
   ```

3. **Run the server:**
   ```bash
   dotnet run
   ```

The server will start on **port 7077** by default.

### Adding Language Models

1. Create a `models` directory in the ASHATAIServer folder (if it doesn't exist)
2. Place your .gguf model files in the models directory
3. The server will automatically scan and load them on startup
4. You can also trigger a manual scan using the `/api/ai/models/scan` endpoint

Example:
```bash
mkdir models
# Copy your .gguf files to the models directory
cp /path/to/your/model.gguf models/
```

## 📡 API Endpoints

### AI Services

#### Health Check
```http
GET /api/ai/health
```

Returns server health status.

**Response:**
```json
{
  "status": "healthy",
  "server": "ASHATAIServer",
  "timestamp": "2025-11-08T19:30:00Z"
}
```

#### Get Model Status
```http
GET /api/ai/status
```

Returns information about loaded and failed models.

**Response:**
```json
{
  "modelsDirectory": "models",
  "loadedModels": [
    {
      "fileName": "llama-2-7b.gguf",
      "path": "models/llama-2-7b.gguf",
      "sizeBytes": 4550000000,
      "checksum": "A1B2C3D4...",
      "loadedAt": "2025-11-08T19:30:00Z"
    }
  ],
  "failedModels": []
}
```

#### Process AI Prompt
```http
POST /api/ai/process
Content-Type: application/json

{
  "prompt": "Hello, ASHAT!",
  "modelName": "llama-2-7b.gguf"  // Optional, uses first available if not specified
}
```

**Response:**
```json
{
  "success": true,
  "modelUsed": "llama-2-7b.gguf",
  "response": "Generated AI response text...",
  "processingTimeMs": 250,
  "error": null
}
```

#### Scan for Models
```http
POST /api/ai/models/scan
```

Manually triggers a scan of the models directory and attempts to load any new models.

## 🔧 Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ModelsDirectory": "models"
}
```

### Environment Variables

You can override settings using environment variables:

```bash
export ModelsDirectory="/path/to/your/models"
dotnet run
```

### Port Configuration

The server is configured to run on port **7077**. To change the port, modify the `Program.cs` file:

```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7077);  // Change this to your desired port
});
```

## 🤝 Integration with ASHAT Goddess Client

The ASHATGoddessClient can connect to this server for AI processing. In the client's configuration, set the server URL to:

```
http://localhost:7077
```

Or if running on a different machine:

```
http://your-server-ip:7077
```

Example client integration:
```csharp
var client = new HttpClient();
client.BaseAddress = new Uri("http://localhost:7077");

var request = new
{
    prompt = "What is the meaning of life?"
};

var response = await client.PostAsJsonAsync("/api/ai/process", request);
var result = await response.Content.ReadFromJsonAsync<ProcessingResult>();
```

## 📁 Project Structure

```
ASHATAIServer/
├── Controllers/
│   └── AIController.cs          # REST API endpoints
├── Services/
│   └── LanguageModelService.cs  # Model management and processing
├── models/                       # Place .gguf files here
├── Program.cs                    # Server startup and configuration
├── appsettings.json             # Configuration
└── README.md                     # This file
```

## 🛠️ Development

### Building from Source

```bash
# Clone the repository
git clone https://github.com/buffbot88/ASHATOS.git
cd ASHATOS/ASHATAIServer

# Build
dotnet build

# Run
dotnet run
```

### Running in Development Mode

```bash
dotnet run --environment Development
```

### Running in Production

```bash
dotnet publish -c Release
cd bin/Release/net9.0/publish
./ASHATAIServer
```

## 📊 Model File Format

ASHATAIServer supports **GGUF format** (.gguf) language model files. These are quantized models commonly used with llama.cpp and similar inference engines.

### Supported Models

- Llama 2 models
- Mistral models
- CodeLlama models
- Any GGUF-format model

### Model Validation

The server validates each model file by:
1. Checking the GGUF magic number (first 4 bytes should be "GGUF")
2. Calculating a SHA256 checksum for integrity
3. Recording file size and metadata

### Failed Models

If a model fails to load, the server will:
1. Log the failure with details
2. Attempt automatic healing (up to 3 attempts)
3. Continue running with other available models
4. Report failed models via the `/api/ai/status` endpoint

## 🔐 Security Considerations

### Production Deployment

For production use, consider:

1. **Authentication**: Add API key authentication to protect endpoints
2. **HTTPS**: Enable HTTPS for encrypted communication
3. **Rate Limiting**: Implement rate limiting to prevent abuse
4. **Input Validation**: Additional validation for prompt inputs
5. **CORS**: Restrict CORS to specific origins instead of allowing all

### Example: Adding API Key Authentication

```csharp
// In Program.cs
app.Use(async (context, next) =>
{
    var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
    if (apiKey != "your-secret-api-key")
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }
    await next();
});
```

## 🐛 Troubleshooting

### Server won't start

- **Issue**: Port 7077 already in use
- **Solution**: Stop any other process using port 7077 or change the port in Program.cs

### Models not loading

- **Issue**: Models directory not found
- **Solution**: Create the `models` directory: `mkdir models`

### Invalid model format

- **Issue**: Model file is not valid GGUF format
- **Solution**: Verify the file is a proper .gguf file, not corrupted

### No models available

- **Issue**: No .gguf files in models directory
- **Solution**: Download and place .gguf model files in the `models` directory

## 📝 Logging

The server logs all operations to the console. Log levels:

- **Information**: Model loading, API requests, successful operations
- **Warning**: Failed healing attempts, missing directories
- **Error**: Model validation failures, processing errors

Example log output:
```
[Information] Starting Language Model Processor for ASHATAIServer...
[Information] Found 2 .gguf files in models
[Information] Successfully loaded model: llama-2-7b.gguf (4.24 GB)
[Information] Successfully loaded model: mistral-7b.gguf (4.08 GB)
[Information] Language Model Processor initialization complete. Loaded: 2, Failed: 0
```

## 🚀 Future Enhancements

Planned features:
- Integration with llama.cpp for real AI inference
- Support for streaming responses
- Model fine-tuning capabilities
- Multi-model ensemble processing
- WebSocket support for real-time communication
- Model caching and optimization
- GPU acceleration support

## 📄 License

See the main repository LICENSE file for licensing information.

## 🤝 Contributing

Contributions are welcome! Please follow the main repository's contributing guidelines.

## 📞 Support

For issues or questions:
- Open an issue on GitHub
- Check the main ASHATOS documentation
- Review the API endpoint documentation

---

**ASHATAIServer v1.0.0**  
Built with ❤️ for ASHAT Goddess  
**Port:** 7077  
**Status:** Ready for Production

---

**Copyright © 2025 AGP Studios, INC. All rights reserved.**
