# ASHATAIServer - AI Processing & Game Server

## 🌟 Overview

**ASHATAIServer** is a unified AI processing and game server that combines:
- **AI Language Model Processing** - Using .gguf (GGUF format) model files for AI inference
- **Game Server Integration** - Complete game creation, deployment, and management system
- **AI-Enhanced Game Creation** - Uses AI to intelligently parse game descriptions and generate suggestions

Originally designed for the ASHAT Goddess desktop client, it now provides comprehensive game development and AI services through a REST API.

## ✨ Features

### AI Services
- 🤖 **Language Model Processing** - Processes AI requests using .gguf model files
- 🔄 **Automatic Model Discovery** - Scans and loads models from the models directory
- 🛡️ **Self-Healing** - Automatically attempts to recover from failed model loads
- 📊 **Status Monitoring** - Real-time status of loaded models
- ✅ **Health Checks** - Built-in health check endpoints

### Game Server Features
- 🎮 **AI-Driven Game Creation** - Create complete games from natural language descriptions
- 🚀 **Automatic Code Generation** - Generates front-end and back-end code
- 🎨 **Asset Management** - Creates and manages game assets
- 📦 **One-Click Deployment** - Deploy game servers with a single command
- 🤖 **AI-Enhanced Parsing** - Uses AI models to intelligently parse game requirements
- 💡 **AI Suggestions** - Get AI-powered improvement suggestions for games
- 📊 **Project Management** - List, update, delete, and export game projects

### Integration
- 🔓 **CORS Enabled** - Ready for cross-origin requests from ASHAT clients
- 🌐 **REST API** - Comprehensive HTTP API for all services
- 🔗 **Unified Platform** - AI and Game Server work together seamlessly

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

The server will start on **port 8088** by default.

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

## 🤝 How AI and Game Server Work Together

The ASHATAIServer integrates AI language models with the game server to provide enhanced functionality:

### AI-Enhanced Game Creation
When you use the `/api/gameserver/create-ai-enhanced` endpoint:
1. Your game description is sent to the AI language model
2. The AI analyzes the description to extract:
   - **Game Type**: RPG, MMO, FPS, Strategy, etc.
   - **Theme**: Fantasy, Sci-Fi, Horror, Medieval, etc.
   - **Features**: Specific gameplay features mentioned in the description
3. The parsed information is used to create a properly configured game project
4. If AI parsing fails, the system falls back to keyword-based parsing

**Example:**
```
Input: "A space exploration game with alien encounters and resource management"

AI Extracts:
- GameType: Simulation
- Theme: sci-fi
- Features: ["Exploration System", "Resource Management", "NPC Encounters"]
```

### AI-Powered Improvement Suggestions
The `/api/gameserver/suggestions/{gameId}` endpoint uses AI to:
1. Analyze your existing game project
2. Review the game type, theme, features, and description
3. Generate 3-5 specific, actionable suggestions for improvements
4. Return practical recommendations for gameplay, engagement, or technical enhancements

**Example Suggestions:**
- "Add a progression system with skill trees to increase player engagement"
- "Implement daily quests to encourage regular player return"
- "Create unique boss encounters with special mechanics"
- "Add a crafting system that ties into resource management"

### Benefits of Integration
- 🧠 **Smarter Game Creation** - AI understands natural language descriptions better than keyword matching
- 💡 **Creative Suggestions** - Get ideas for game improvements you might not have considered
- 🚀 **Faster Development** - Less time configuring, more time creating
- 🎯 **Better Results** - AI helps extract the full intent from your game descriptions

### Game Server Services

#### Get Game Server Status
```http
GET /api/gameserver/status
```

Returns the current status of the game server including active projects and deployments.

#### Get Server Capabilities
```http
GET /api/gameserver/capabilities
```

Returns information about supported game types, features, and system limits.

**Response:**
```json
{
  "maxConcurrentServers": 50,
  "activeServers": 2,
  "supportedGameTypes": ["RPG", "MMO", "FPS", "Strategy", ...],
  "availableFeatures": ["Natural Language Design", "AI Asset Creation", ...]
}
```

#### Create New Game
```http
POST /api/gameserver/create
Content-Type: application/json

{
  "userId": "00000000-0000-0000-0000-000000000000",
  "description": "A medieval MMO with castle siege battles and crafting",
  "gameType": "MMO",
  "theme": "medieval",
  "features": ["Combat System", "Crafting System"],
  "licenseKey": "DEMO",
  "generateAssets": true,
  "autoDeploy": false
}
```

#### Create Game with AI Enhancement
```http
POST /api/gameserver/create-ai-enhanced
Content-Type: application/json

{
  "description": "A space exploration game with alien encounters and resource management",
  "userId": "00000000-0000-0000-0000-000000000000",
  "licenseKey": "DEMO"
}
```

This endpoint uses AI to intelligently parse the game description and extract:
- Game type (RPG, MMO, FPS, etc.)
- Theme (fantasy, sci-fi, horror, etc.)
- Key features mentioned in the description

**Response:**
```json
{
  "success": true,
  "message": "Game created successfully",
  "gameId": "abc123def456",
  "projectPath": "/path/to/project",
  "project": { ... },
  "generatedFiles": [...]
}
```

#### List All Projects
```http
GET /api/gameserver/projects
```

Returns a list of all game projects.

#### Get Project Details
```http
GET /api/gameserver/project/{gameId}
```

Returns detailed information about a specific game project.

#### Deploy Game Server
```http
POST /api/gameserver/deploy/{gameId}
Content-Type: application/json

{
  "port": 5000,
  "maxPlayers": 100,
  "enableWebSocket": true,
  "enableDatabase": true
}
```

Deploys the game to a server and makes it available for players.

#### Get AI Improvement Suggestions
```http
GET /api/gameserver/suggestions/{gameId}
```

Uses AI to analyze the game project and provide specific suggestions for improvements.

**Response:**
```json
{
  "gameId": "abc123def456",
  "suggestions": "1. Add a progression system...\n2. Implement daily quests...\n3. Create boss encounters..."
}
```

#### Update Game
```http
PUT /api/gameserver/update/{gameId}
Content-Type: application/json

{
  "updateDescription": "Add multiplayer chat and leaderboards"
}
```

#### Delete Game
```http
DELETE /api/gameserver/{gameId}
```

Deletes a game project and all its associated files.

#### Export Game
```http
POST /api/gameserver/export/{gameId}
Content-Type: application/json

{
  "format": "Complete"  // Options: ZipArchive, SourceCode, Binary, Complete
}
```

Exports the game project in the specified format.

#### List User Games
```http
GET /api/gameserver/user/{userId}/games
```

Returns all games created by a specific user.

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

The server is configured to run on port **8088**. To change the port, modify the `Program.cs` file:

```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8088);  // Change this to your desired port
});
```

## 🤝 Integration with ASHAT Goddess Client

The ASHATGoddessClient can connect to this server for AI processing. In the client's configuration, set the server URL to:

```
http://localhost:8088
```

Or if running on a different machine:

```
http://your-server-ip:8088
```

Example client integration:
```csharp
var client = new HttpClient();
client.BaseAddress = new Uri("http://localhost:8088");

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

- **Issue**: Port 8088 already in use
- **Solution**: Stop any other process using port 8088 or change the port in Program.cs

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
**Port:** 8088  
**Status:** Ready for Production

---

**Copyright © 2025 AGP Studios, INC. All rights reserved.**
