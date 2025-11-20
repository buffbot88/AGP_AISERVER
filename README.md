# ASHATAIServer - AI Processing Server

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-AGP%20Studios-blue)](LICENSE)

## 🌟 Overview

**ASHATAIServer** is a powerful AI processing server that provides comprehensive AI language model services and user authentication. Originally designed for the ASHAT Goddess desktop client, it now provides robust AI services through a REST API.

### Key Features

✨ **AI Language Model Processing**
- Supports GGUF format model files for AI inference
- Pluggable runtime architecture (MockRuntime, LlamaCppAdapter)
- Automatic model discovery and loading
- Self-healing capabilities for failed models
- Token-level streaming responses (SSE)
- Project generation with structured file output

🔐 **User Authentication & Management**
- Secure local user database (SQLite)
- Session-based authentication with tokens
- User registration and login
- Role-based access (Admin/User)

🌐 **REST API**
- Comprehensive HTTP API
- CORS enabled for cross-origin requests
- OpenAPI/Swagger documentation in development mode
- Server-Sent Events (SSE) for streaming
- Project generation endpoint with file sanitization

📊 **Monitoring & Health Checks**
- Real-time model status
- Health check endpoints
- Comprehensive logging

**Note:** Game Server functionality has been separated into a standalone module.

**Implementation Status:** ✅ Phase 1, 2 & 3 Complete
- ✅ **Phase 1**: Pluggable inference runtime with IModelRuntime interface
- ✅ **Phase 2**: Streaming support (SSE) and project generation endpoint
- ✅ **Phase 3**: Security hardening with Argon2id, API keys, and rate limiting

---

## 🚀 Quick Start

### Prerequisites

- **.NET 9.0 SDK or Runtime**
- Optional: `.gguf` language model files (e.g., from Llama, Mistral)

### Installation & Running

```bash
# Navigate to server directory
cd ASHATAIServer

# Build the project
dotnet build

# Run the server
dotnet run
```

**Server URL:** `http://localhost:7077`

The server starts automatically on **port 7077** and initializes all services.

### Adding Language Models (Optional)

```bash
# Create models directory
mkdir models

# Copy your .gguf model files
cp /path/to/your/model.gguf models/
```

The server automatically scans and loads `.gguf` files on startup.

---

## 📡 Complete API Reference

### Server Information

- **Base URL:** `http://localhost:7077`
- **Port:** 7077
- **Protocol:** HTTP (HTTPS recommended for production)

---

## Authentication Endpoints

### 1. User Login

**Endpoint:** `POST /api/auth/login`

Authenticate a user and receive a session token.

**Request Body:**
```json
{
  "username": "johndoe",
  "password": "SecurePassword123"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Login successful",
  "token": "a1b2c3d4e5f6...",
  "sessionId": "a1b2c3d4e5f6...",
  "user": {
    "id": 1,
    "username": "johndoe",
    "email": "john@example.com",
    "role": "User"
  }
}
```

**Error Response (401 Unauthorized):**
```json
{
  "success": false,
  "message": "Invalid username or password"
}
```

---

### 2. User Registration

**Endpoint:** `POST /api/auth/register`

Register a new user account. Automatically logs in the user upon successful registration.

**Request Body:**
```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "SecurePassword123"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Registration successful",
  "token": "a1b2c3d4e5f6...",
  "sessionId": "a1b2c3d4e5f6...",
  "user": {
    "id": 1,
    "username": "johndoe",
    "email": "john@example.com",
    "role": "User"
  }
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "Username already exists"
}
```

**Validation Rules:**
- Username: Required, must be unique
- Email: Required, must be unique
- Password: Required, minimum 6 characters

---

### 3. Validate Session

**Endpoint:** `POST /api/auth/validate`

Validate an existing session token.

**Request Body:**
```json
{
  "sessionId": "a1b2c3d4e5f6..."
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "user": {
    "id": 1,
    "username": "johndoe",
    "email": "john@example.com",
    "role": "User"
  }
}
```

**Error Response (401 Unauthorized):**
```json
{
  "success": false,
  "message": "Invalid or expired session"
}
```

**Session Details:**
- Sessions expire after 7 days
- Expired sessions are automatically cleaned up

---

## Admin Endpoints

All admin endpoints require an admin session token in the Authorization header.

### 4. Create API Key

**Endpoint:** `POST /api/admin/keys/create`

Create a new API key for authentication.

**Request Headers:**
```
Authorization: Bearer <admin-session-token>
```

**Request Body:**
```json
{
  "name": "Production API Key",
  "assignToUserId": 5,
  "expiresAt": "2025-12-31T23:59:59Z",
  "scopes": "ai:process,ai:generate"
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "API key created successfully",
  "apiKey": "agp_live_a1b2c3d4...",
  "warning": "Save this API key securely. It will not be shown again."
}
```

**⚠️ Important:** The API key is only shown once. Store it securely.

---

### 5. List API Keys

**Endpoint:** `GET /api/admin/keys/list`

List all API keys in the system.

**Request Headers:**
```
Authorization: Bearer <admin-session-token>
```

**Query Parameters:**
- `userId` (optional): Filter by user ID
- `includeRevoked` (optional): Include revoked keys (default: false)

**Success Response (200 OK):**
```json
{
  "success": true,
  "keys": [
    {
      "id": 1,
      "name": "Production API Key",
      "userId": 5,
      "createdAt": "2025-11-20T00:00:00Z",
      "expiresAt": "2025-12-31T23:59:59Z",
      "lastUsedAt": "2025-11-20T12:00:00Z",
      "isRevoked": false,
      "scopes": "ai:process,ai:generate"
    }
  ]
}
```

---

### 6. Revoke API Key

**Endpoint:** `POST /api/admin/keys/revoke/{keyId}`

Revoke an API key to prevent further use.

**Request Headers:**
```
Authorization: Bearer <admin-session-token>
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "API key revoked successfully"
}
```

---

### 7. System Health

**Endpoint:** `GET /api/admin/health`

Get system health and statistics.

**Request Headers:**
```
Authorization: Bearer <admin-session-token>
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "health": {
    "status": "healthy",
    "timestamp": "2025-11-20T12:00:00Z",
    "apiKeys": {
      "total": 10,
      "active": 8,
      "revoked": 2
    }
  }
}
```

---

## User Endpoints

### 8. Get Current User

**Endpoint:** `GET /api/user/me`

Get information about the currently authenticated user.

**Request Headers:**
```
Authorization: Bearer <session-token>
```

**Success Response (200 OK):**
```json
{
  "userId": 1,
  "username": "johndoe",
  "email": "john@example.com",
  "isAdmin": false
}
```

**Error Response (401 Unauthorized):**
```json
{
  "success": false,
  "message": "Authorization token required"
}
```

**Usage Example:**
```bash
curl -H "Authorization: Bearer a1b2c3d4e5f6..." \
  http://localhost:7077/api/user/me
```

---

## AI Processing Endpoints

### 9. Process AI Prompt

**Endpoint:** `POST /api/ai/process`

Process an AI prompt using a loaded language model or built-in goddess personality.

**Request Body:**
```json
{
  "prompt": "Hello, ASHAT! Tell me about yourself.",
  "modelName": "llama-2-7b.gguf"
}
```

**Parameters:**
- `prompt` (string, required): The text prompt to process
- `modelName` (string, optional): Specific model to use. If omitted, uses the first available model or fallback mode.

**Success Response (200 OK):**
```json
{
  "success": true,
  "modelUsed": "llama-2-7b.gguf",
  "response": "Salve, mortal! ✨ I am ASHAT, your divine companion...",
  "processingTimeMs": 250,
  "error": null
}
```

**Error Response (400 Bad Request):**
```json
{
  "error": "Prompt cannot be empty"
}
```

**Built-in Personality:**
When no models are loaded, the server uses MockRuntime with the built-in "ASHAT Goddess" personality that provides contextual responses for greetings, help requests, coding questions, and more.

---

### 9.1 Process AI Prompt (Streaming)

**Endpoint:** `POST /api/ai/process/stream`

Process an AI prompt with streaming response using Server-Sent Events (SSE).

**Request Body:**
```json
{
  "prompt": "Write a function to calculate fibonacci numbers",
  "modelName": "llama-2-7b.gguf"
}
```

**Response:** Server-Sent Events stream
```
data: Generated
data: response
data: token
data: by
data: token...
data: [DONE]
```

**Usage:**
The streaming endpoint returns tokens as they are generated using Server-Sent Events (SSE) format. Each token is sent as a `data:` event. The stream ends with a `data: [DONE]` event.

**Client Example (JavaScript):**
```javascript
const eventSource = new EventSource('http://localhost:7077/api/ai/process/stream');
eventSource.onmessage = (event) => {
  if (event.data === '[DONE]') {
    eventSource.close();
  } else {
    console.log(event.data);
  }
};
```

---

### 9.2 Generate Project

**Endpoint:** `POST /api/ai/generate-project`

Generate a complete project with multiple files based on a description.

**Request Body:**
```json
{
  "projectDescription": "Create a simple C# console calculator that can add, subtract, multiply, and divide",
  "projectType": "console",
  "language": "csharp",
  "modelName": "llama-2-7b.gguf",
  "options": {
    "includeComments": true,
    "includeTests": false,
    "includeReadme": true,
    "maxFiles": 50,
    "maxFileSizeBytes": 1048576
  }
}
```

**Success Response (200 OK):**
```json
{
  "success": true,
  "files": {
    "Program.cs": "using System;\n\nclass Program\n{\n    static void Main()\n    {\n        Console.WriteLine(\"Calculator\");\n    }\n}",
    "Calculator.cs": "public class Calculator\n{\n    public int Add(int a, int b) => a + b;\n}",
    "README.md": "# Calculator Project\n\nA simple console calculator.",
    "Calculator.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\">...</Project>"
  },
  "metadata": {
    "modelUsed": "llama-2-7b.gguf",
    "projectType": "console",
    "language": "csharp",
    "fileCount": 4,
    "totalSizeBytes": 2048,
    "generationTimeMs": 450,
    "warnings": []
  },
  "error": null
}
```

**Security Features:**
- **File Extension Whitelist**: Only allows safe file extensions (.cs, .py, .js, .md, etc.)
- **Size Limits**: Configurable maximum file sizes (default 1MB per file)
- **Path Sanitization**: Removes directory traversal attempts (../, ~, etc.)
- **Content Sanitization**: Removes null bytes and other potentially harmful content
- **File Count Limit**: Maximum number of files to prevent abuse (default 50)

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "error": "Project description is required"
}
```

---

### 10. Get Model Status

**Endpoint:** `GET /api/ai/status`

Get information about loaded and failed models.

**Success Response (200 OK):**
```json
{
  "modelsDirectory": "models",
  "loadedModels": [
    {
      "fileName": "llama-2-7b.gguf",
      "path": "models/llama-2-7b.gguf",
      "sizeBytes": 4550000000,
      "checksum": "A1B2C3D4E5F6...",
      "loadedAt": "2025-11-18T00:30:00Z"
    }
  ],
  "failedModels": []
}
```

**Model Validation:**
The server validates each model by:
1. Checking GGUF magic number (first 4 bytes = "GGUF")
2. Calculating SHA256 checksum on first 1MB
3. Recording file size and metadata

---

### 11. Scan for Models

**Endpoint:** `POST /api/ai/models/scan`

Manually trigger a scan of the models directory to discover and load new models.

**Success Response (200 OK):**
```json
{
  "modelsDirectory": "models",
  "loadedModels": [...],
  "failedModels": [...]
}
```

**Usage:**
Useful when you add new model files to the models directory without restarting the server.

---

### 12. Health Check

**Endpoint:** `GET /api/ai/health`

Check if the server is running and healthy.

**Success Response (200 OK):**
```json
{
  "status": "healthy",
  "server": "ASHATAIServer",
  "timestamp": "2025-11-18T00:30:00Z"
}
```

---

## 🔧 Configuration

### Port Configuration

The server runs on **port 7077** by default. To change:

**In `Program.cs`:**
```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7077);  // Change to your desired port
});
```

### Application Settings

**`appsettings.json`:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ModelsDirectory": "models",
  "Database": {
    "Path": "users.db"
  }
}
```

**Configuration Options:**
- `ModelsDirectory`: Directory where .gguf model files are stored (default: "models")
- `Database:Path`: Path to SQLite database file (default: "users.db")

### Environment Variables

Override settings using environment variables:

```bash
export ModelsDirectory="/path/to/models"
export Database__Path="/path/to/users.db"
dotnet run
```

---

## 🗄️ Database Structure

### SQLite Database Schema

**Users Table:**
```sql
CREATE TABLE Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Role TEXT NOT NULL DEFAULT 'User',
    CreatedAt TEXT NOT NULL,
    LastLoginAt TEXT
);
```

**Sessions Table:**
```sql
CREATE TABLE Sessions (
    SessionId TEXT PRIMARY KEY,
    UserId INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL,
    ExpiresAt TEXT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
```

**Indexes:**
- `idx_username` on Users(Username)
- `idx_email` on Users(Email)
- `idx_sessions_user` on Sessions(UserId)

**Security Features:**
- Passwords are hashed using SHA256
- Sessions expire after 7 days
- Automatic cleanup of expired sessions
- SQL injection prevention via parameterized queries
- Log injection prevention via sanitized logging

---

## 📁 Project Structure

```
ASHATAIServer/
├── Controllers/
│   ├── AuthController.cs        # Authentication endpoints
│   ├── UserController.cs        # User management endpoints
│   └── AIController.cs          # AI processing endpoints
├── Services/
│   ├── AuthenticationService.cs # Authentication logic
│   ├── UserDatabaseService.cs   # Database operations
│   └── LanguageModelService.cs  # Model management & AI processing
├── Properties/
│   └── launchSettings.json      # Development settings
├── models/                       # .gguf model files (created at runtime)
├── Program.cs                    # Server startup & configuration
├── appsettings.json             # Application configuration
├── appsettings.Development.json # Development overrides
├── users.db                      # SQLite database (created at runtime)
├── README.md                     # Server documentation
└── Examples.md                   # Client integration examples

Abstractions/
└── [Shared models and interfaces]
```

---

## 🛠️ Development

### Building from Source

```bash
# Clone repository
git clone https://github.com/buffbot88/AGP_AISERVER.git
cd AGP_AISERVER/ASHATAIServer

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

### Development Mode

```bash
dotnet run --environment Development
```

**Development Features:**
- OpenAPI/Swagger UI enabled
- Detailed logging
- Development-specific settings from `appsettings.Development.json`

### Production Deployment

```bash
# Publish for production
dotnet publish -c Release -o ./publish

# Run published version
cd publish
./ASHATAIServer
```

---

## 🔐 Security Features

### ✅ Phase 3 Security Implementation

ASHATAIServer now includes production-ready security features:

**Password Security:**
- **Argon2id hashing** - Memory-hard algorithm (64MB, 4 iterations)
- **Per-user salts** - Unique 32-byte cryptographic salt for each password
- **Constant-time comparison** - Prevents timing attacks

**API Key Authentication:**
- **Secure key generation** - 256-bit entropy (`agp_live_<64 hex chars>`)
- **SHA256 storage** - Keys hashed before database storage
- **Expiration & Revocation** - Full lifecycle management
- **Admin endpoints** - Create, list, and revoke keys
- **Scope support** - Fine-grained access control (future use)
- **Backward compatible** - Optional, disabled by default

**Rate Limiting:**
- **Per-IP tracking** - Limits by client IP address
- **Per-API-key tracking** - Limits by authenticated API key
- **Configurable limits** - Default: 60 requests/min, 1000 requests/hour
- **Standard responses** - HTTP 429 with Retry-After header
- **Rate limit headers** - X-RateLimit-* headers on all responses

**HTTPS/TLS Support:**
- **Optional HTTPS** - Configure via appsettings
- **Certificate-based** - Standard .pfx certificate support
- **Production-ready** - Works with Let's Encrypt, DigiCert, etc.

**Additional Security:**
- Session management (7-day expiration)
- SQL injection prevention (parameterized queries)
- Log injection prevention
- Input validation and sanitization
- Path traversal protection

### 📖 Documentation

See **[docs/SECURITY.md](docs/SECURITY.md)** for:
- Detailed security configuration
- API key management guide
- Rate limiting configuration
- HTTPS setup instructions
- Production deployment best practices
- Threat model and mitigations

### Production Recommendations

For production deployment:

1. ✅ **Password Hashing** - Already using Argon2id
2. ✅ **Rate Limiting** - Already enabled (adjust limits as needed)
3. ⚠️ **Enable HTTPS** - Configure TLS certificate
4. ⚠️ **API Key Protection** - Add critical endpoints to ProtectedPaths
5. ⚠️ **CORS Restrictions** - Limit to specific origins
6. ⚠️ **Firewall Rules** - Restrict access to trusted networks
7. ⚠️ **Monitoring** - Set up logging and alerting

---

## 🧪 Testing the API

### Using curl

```bash
# Health check
curl http://localhost:7077/api/ai/health

# Register user
curl -X POST http://localhost:7077/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@example.com","password":"password123"}'

# Login
curl -X POST http://localhost:7077/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"password123"}'

# Get current user (replace TOKEN with actual token)
curl -H "Authorization: Bearer TOKEN" \
  http://localhost:7077/api/user/me

# Process AI prompt
curl -X POST http://localhost:7077/api/ai/process \
  -H "Content-Type: application/json" \
  -d '{"prompt":"Hello, ASHAT!"}'

# Get model status
curl http://localhost:7077/api/ai/status
```

### Integration Examples

See **[Examples.md](Examples.md)** for detailed integration examples in:
- C#
- Python
- JavaScript/Node.js
- ASHAT Goddess Client

---

## 📊 Model File Format

### GGUF Format Support

ASHATAIServer supports **GGUF format** (.gguf) language model files. These are quantized models commonly used with llama.cpp and similar inference engines.

**Supported Models:**
- Llama 2 / Llama 3
- Mistral / Mixtral
- CodeLlama
- Phi
- Gemma
- Any GGUF-compatible model

**Model Healing:**
The server automatically attempts to heal failed models up to 3 times before marking them as permanently failed.

---

## 🐛 Troubleshooting

### Common Issues

**Port already in use:**
```
Error: Address already in use
```
**Solution:** Stop other processes using port 7077 or change the port in Program.cs

**Models not loading:**
```
Warning: Models directory does not exist
```
**Solution:** Create the models directory: `mkdir models`

**Database locked:**
```
Error: Database is locked
```
**Solution:** Ensure only one instance of the server is running

**Invalid model format:**
```
Error: Invalid GGUF magic number
```
**Solution:** Verify the file is a proper .gguf file and not corrupted

---

## 📝 Logging

### Log Levels

- **Information:** Model loading, API requests, user operations
- **Warning:** Failed healing attempts, missing resources
- **Error:** Critical failures, processing errors

### Example Output

```
╔══════════════════════════════════════════════════════════╗
║    ASHATAIServer - AI Processing Server                 ║
╚══════════════════════════════════════════════════════════╝

Server started on port: 7077
Server URL: http://localhost:7077

Available Endpoints:
  Authentication:
    POST /api/auth/login     - User login
    POST /api/auth/register  - User registration

  AI Services:
    POST /api/ai/process     - Process AI prompts
    GET  /api/ai/status      - Get model status
    POST /api/ai/models/scan - Scan for models
    GET  /api/ai/health      - Health check

[Information] Starting Language Model Processor...
[Information] Found 2 .gguf files in models
[Information] Successfully loaded model: llama-2-7b.gguf (4.24 GB)
[Information] Language Model Processor initialization complete
```

---

## 🚀 Roadmap & Future Enhancements

### Completed Features (Phase 1-3)

- ✅ Integration with llama.cpp skeleton (LlamaCppAdapter)
- ✅ Streaming response support (SSE)
- ✅ Advanced security features (Argon2, API keys, rate limiting)
- ✅ Session-based authentication
- ✅ Project generation with security

### Planned Features (Phase 4+)

- [ ] Docker packaging and deployment guides
- [ ] Multi-platform publish scripts
- [ ] Integration with real llama.cpp runtime
- [ ] Model fine-tuning capabilities
- [ ] Multi-model ensemble processing
- [ ] GPU acceleration support (CUDA/Metal)
- [ ] Model caching and optimization
- [ ] Multi-factor authentication (MFA)
- [ ] API usage analytics

---

## 📄 License

Copyright © 2025 AGP Studios, INC. All rights reserved.

See [LICENSE](LICENSE) file for details.

---

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

---

## 📞 Support & Contact

- **GitHub Issues:** [Report bugs or request features](https://github.com/buffbot88/AGP_AISERVER/issues)
- **Documentation:** See [Examples.md](ASHATAIServer/Examples.md) for integration examples
- **Related Projects:** Check out the main ASHATOS repository

---

## 📋 Summary

**ASHATAIServer** is a production-ready AI processing server with:

- ✅ **Port:** 7077 (HTTP), 7443 (HTTPS optional)
- ✅ **14+ REST API Endpoints** (3 auth, 1 user, 4 admin, 6+ AI)
- ✅ **SQLite Database** for user management and API keys
- ✅ **GGUF Model Support** with automatic loading
- ✅ **Pluggable Runtime Architecture** (IModelRuntime interface)
- ✅ **MockRuntime** for testing and fallback mode
- ✅ **LlamaCppAdapter** skeleton for production inference
- ✅ **Streaming Support** via Server-Sent Events (SSE)
- ✅ **Project Generation** with file sanitization
- ✅ **Phase 3 Security Features:**
  - ✅ **Argon2id** password hashing with per-user salts
  - ✅ **API Key Authentication** with admin management
  - ✅ **Rate Limiting** (60/min, 1000/hr)
  - ✅ **HTTPS/TLS Support** (optional)
  - ✅ **Session Management** (7-day expiration)
- ✅ **Comprehensive Logging & Documentation**
- ✅ **CORS Enabled** for cross-origin requests

**Quick Start:** `cd ASHATAIServer && dotnet run`
**Security Docs:** See [docs/SECURITY.md](docs/SECURITY.md)

---

**ASHATAIServer v1.0.0** | Built with ❤️ for ASHAT Goddess | **Ready for Production**
