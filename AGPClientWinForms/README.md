# AGP Client WinForms

A Windows Forms desktop application for generating projects using ASHATAIServer.

## Features

- ✅ User-friendly graphical interface
- ✅ Persistent connection settings
- ✅ Multiple project types (Console, WinForms, Web API)
- ✅ Multiple languages (C#, Python, JavaScript)
- ✅ Real-time project generation
- ✅ File preview with syntax highlighting
- ✅ Save projects to disk
- ✅ Status feedback and error handling

## Requirements

- **Windows 10 or later**
- **.NET 9.0 Runtime** or SDK

## Building from Source

### On Windows

```powershell
cd AGPClientWinForms
dotnet build --configuration Release
dotnet run --configuration Release
```

### Cross-Platform Build

This project targets `net9.0-windows` and requires Windows to build and run. If you're on Linux/macOS, you can create the project files but cannot build without Windows.

## Running the Application

1. **Start ASHATAIServer:**
   ```bash
   cd ASHATAIServer
   dotnet run
   ```
   Server will be available at: http://localhost:7077

2. **Launch AGP Client:**
   ```bash
   cd AGPClientWinForms
   dotnet run
   ```

## Usage Guide

### 1. Configure Server Connection

- **Server URL:** Enter the ASHATAIServer URL (default: http://localhost:7077)
- Settings are automatically saved and restored

### 2. Select Project Options

- **Project Type:** Choose from Console, WinForms, or Web API
- **Language:** Choose from C#, Python, or JavaScript

### 3. Enter Project Description

Write a description of what you want to generate:
- "Create a simple calculator app"
- "Build a REST API for managing todos"
- "Create a WinForms app with a login form"

### 4. Generate Project

Click **Generate Project** button:
- The app connects to the server
- Sends your request
- Displays generated files

### 5. Save to Disk

Click **Save to Disk** button:
- Choose a folder
- All files are saved automatically
- Ready to build and run!

## Interface Overview

```
┌─────────────────────────────────────────────┐
│ Server URL: http://localhost:7077           │
│                                             │
│ Project Type: [Console ▼]  Language: [C# ▼]│
│                                             │
│ Project Description:                        │
│ ┌─────────────────────────────────────────┐ │
│ │ Create a simple calculator application  │ │
│ └─────────────────────────────────────────┘ │
│                                             │
│ [Generate Project]  [Save to Disk]          │
│                                             │
│ Generated Files:                            │
│ ┌─────────────────────────────────────────┐ │
│ │ === Program.cs ===                      │ │
│ │ using System;                           │ │
│ │ ...                                     │ │
│ └─────────────────────────────────────────┘ │
│                                             │
│ Status: Ready                               │
└─────────────────────────────────────────────┘
```

## Configuration

Settings are stored in:
```
%APPDATA%\AGPClient\settings.json
```

### Settings File Format

```json
{
  "ServerUrl": "http://localhost:7077"
}
```

## Keyboard Shortcuts

- **Ctrl+G** - Generate Project (when focused on description)
- **Ctrl+S** - Save to Disk (when files are generated)
- **Ctrl+W** - Close application

## Troubleshooting

### "Connection error: No connection could be made"

**Solution:**
- Ensure ASHATAIServer is running
- Check the server URL is correct
- Verify firewall settings

### "Error: Project description is required"

**Solution:**
- Enter a description in the Project Description field

### Cannot Save Files

**Solution:**
- Check folder permissions
- Ensure you have write access to the selected folder

### Application Won't Start

**Solution:**
- Install .NET 9.0 Runtime: https://dotnet.microsoft.com/download
- Run from command line to see error messages:
  ```
  dotnet run
  ```

## Publishing

### Create Self-Contained Executable

```powershell
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish/win-x64

# Windows ARM64
dotnet publish -c Release -r win-arm64 --self-contained true -o ./publish/win-arm64
```

The executable will be in `./publish/win-x64/AGPClientWinForms.exe`

### Create Framework-Dependent Build

```powershell
dotnet publish -c Release -o ./publish
```

Requires .NET 9.0 Runtime to be installed on target machine.

## Advanced Features

### Custom Server Configuration

You can connect to remote ASHATAIServer instances:

```
http://your-server:7077
https://secure-server:7443
```

### Batch Operations

The client supports generating multiple projects by changing the description and clicking Generate again.

### File Preview

All generated files are displayed in the output window with:
- File names as headers
- Full file contents
- Syntax formatting (monospace font)

## Development

### Project Structure

```
AGPClientWinForms/
├── AGPClientWinForms.csproj
├── Program.cs              # Application entry point
├── MainForm.cs             # Main window implementation
└── README.md               # This file
```

### Adding Features

To add new features:

1. Edit `MainForm.cs`
2. Add new controls in `InitializeComponent()`
3. Add event handlers for new functionality
4. Build and test on Windows

### Code Guidelines

- Follow C# naming conventions
- Use async/await for HTTP operations
- Handle all exceptions with user-friendly messages
- Save settings persistently

## API Integration

The client uses these ASHATAIServer endpoints:

- `POST /api/ai/generate-project` - Generate complete projects

### Request Format

```json
{
  "projectDescription": "Create a simple calculator",
  "projectType": "console",
  "language": "csharp",
  "options": {
    "includeComments": true,
    "includeReadme": true,
    "maxFiles": 50,
    "maxFileSizeBytes": 1048576
  }
}
```

### Response Format

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

## License

Copyright © 2025 AGP Studios, INC. All rights reserved.

## Support

For issues and feature requests, please visit:
https://github.com/buffbot88/AGP_AISERVER/issues
