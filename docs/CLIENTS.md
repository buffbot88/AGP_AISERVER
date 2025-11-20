# Client Tooling Documentation

This document describes the official client tools for ASHATAIServer.

## Available Clients

### 1. AGP CLI (Command-Line Interface)

A .NET global tool for generating projects from the command line.

**Location:** `agp-cli/`

#### Installation

```bash
# Build and pack
cd agp-cli
dotnet pack -o nupkg

# Install globally
dotnet tool install --global --add-source ./nupkg AGP.CLI
```

#### Usage

```bash
agp generate --description "Create a calculator app" --output ./my-app
```

See [agp-cli/README.md](../agp-cli/README.md) for full documentation.

#### Features

- ✅ Generate projects via command line
- ✅ Support for multiple languages (C#, Python, JavaScript)
- ✅ Support for multiple project types (console, winforms, webapi)
- ✅ Automatic file saving
- ✅ Built-in help and examples

---

### 2. AGP Client WinForms (GUI Application)

A Windows Forms desktop application for interacting with ASHATAIServer.

**Location:** `AGPClientWinForms/`

#### Features

- ✅ User-friendly graphical interface
- ✅ Persistent connection settings
- ✅ Syntax highlighting for code display
- ✅ Windows Credential Manager integration for secure token storage
- ✅ Project generation with visual feedback
- ✅ File browsing and saving
- ✅ Real-time AI responses

#### Requirements

- Windows 10 or later
- .NET 9.0 Runtime

#### Usage

1. Launch the application
2. Configure server URL (default: http://localhost:7077)
3. Generate projects or process prompts
4. Save generated files to disk

See [AGPClientWinForms/README.md](../AGPClientWinForms/README.md) for detailed documentation.

---

## Project Templates

Pre-built templates for common project types.

**Location:** `templates/`

### Console Application (C#)
- **Path:** `templates/console-csharp/`
- **Features:** Basic console app with argument handling
- **Build:** `dotnet build`

### WinForms Application (C#)
- **Path:** `templates/winforms-csharp/`
- **Features:** Windows Forms app with basic UI
- **Requirements:** Windows OS
- **Build:** `dotnet build`

### Web API (C#)
- **Path:** `templates/webapi-csharp/`
- **Features:** REST API with Swagger documentation
- **Build:** `dotnet build`

### Using Templates

Templates can be used as starting points or references for project generation:

```bash
# Copy template to new location
cp -r templates/console-csharp ./my-project
cd my-project
dotnet build
dotnet run
```

---

## Integration Examples

### C# Client Example

```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:7077") };

var request = new
{
    projectDescription = "Create a simple calculator",
    projectType = "console",
    language = "csharp"
};

var response = await client.PostAsJsonAsync("/api/ai/generate-project", request);
var result = await response.Content.ReadFromJsonAsync<ProjectResponse>();

foreach (var file in result.Files)
{
    File.WriteAllText(file.Key, file.Value);
}
```

### Python Client Example

```python
import requests
import os

response = requests.post('http://localhost:7077/api/ai/generate-project', json={
    'projectDescription': 'Create a web scraper',
    'projectType': 'console',
    'language': 'python'
})

result = response.json()
for filename, content in result['files'].items():
    with open(filename, 'w') as f:
        f.write(content)
```

### JavaScript Client Example

```javascript
const response = await fetch('http://localhost:7077/api/ai/generate-project', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
        projectDescription: 'Create a React component',
        projectType: 'web',
        language: 'javascript'
    })
});

const result = await response.json();
for (const [filename, content] of Object.entries(result.files)) {
    // Save files to disk
}
```

---

## Client Development Guide

### Creating a New Client

1. **Choose a platform** (CLI, GUI, Web, Mobile)
2. **Implement HTTP client** for ASHATAIServer API
3. **Handle authentication** (sessions or API keys)
4. **Process responses** (files, metadata, errors)
5. **Save artifacts** to disk

### API Endpoints to Implement

#### Essential Endpoints

- `POST /api/ai/generate-project` - Generate complete projects
- `POST /api/ai/process` - Process single prompts
- `POST /api/ai/process/stream` - Streaming responses (SSE)
- `GET /api/ai/health` - Health check

#### Authentication Endpoints

- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/validate` - Validate session

#### Optional Endpoints

- `GET /api/ai/status` - Model status
- `POST /api/ai/models/scan` - Scan for models

### Response Handling

All successful responses follow this structure:

```json
{
  "success": true,
  "files": {
    "Program.cs": "...",
    "README.md": "..."
  },
  "metadata": {
    "modelUsed": "MockRuntime",
    "fileCount": 2,
    "generationTimeMs": 150
  }
}
```

Error responses:

```json
{
  "success": false,
  "error": "Error message"
}
```

### Best Practices

1. **Error Handling** - Always check for HTTP errors and API error responses
2. **Connection Validation** - Test connection before heavy operations
3. **Progress Feedback** - Show users what's happening
4. **File Sanitization** - Validate paths before writing files
5. **Configuration** - Allow users to configure server URL
6. **Logging** - Log operations for debugging

---

## Testing Your Client

### 1. Start ASHATAIServer

```bash
cd ASHATAIServer
dotnet run
```

Server will be available at: http://localhost:7077

### 2. Test Health Endpoint

```bash
curl http://localhost:7077/api/ai/health
```

Expected response:
```json
{
  "status": "healthy",
  "server": "ASHATAIServer",
  "timestamp": "2025-11-20T00:00:00Z"
}
```

### 3. Test Project Generation

```bash
curl -X POST http://localhost:7077/api/ai/generate-project \
  -H "Content-Type: application/json" \
  -d '{
    "projectDescription": "Create a simple console app",
    "projectType": "console",
    "language": "csharp"
  }'
```

### 4. Verify Generated Files

The response will include a `files` dictionary with file contents.

---

## Troubleshooting

### Connection Refused

**Problem:** Client cannot connect to server

**Solution:**
- Ensure ASHATAIServer is running: `dotnet run` in `ASHATAIServer/`
- Check server URL matches configuration (default: http://localhost:7077)
- Verify firewall settings

### Empty Response

**Problem:** Server returns empty or null response

**Solution:**
- Check server logs for errors
- Verify request format matches API documentation
- Ensure required fields are provided

### File Saving Fails

**Problem:** Cannot save generated files

**Solution:**
- Check directory permissions
- Verify output path exists or can be created
- Ensure disk space available

---

## Contributing

To contribute a new client:

1. Fork the repository
2. Create your client in a new directory
3. Document usage and features
4. Add integration tests
5. Submit a pull request

---

## License

Copyright © 2025 AGP Studios, INC. All rights reserved.
