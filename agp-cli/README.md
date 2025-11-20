# AGP CLI - Project Generator

Command-line tool for generating projects using ASHATAIServer.

## Installation

### As a .NET Tool

```bash
# Install globally
dotnet tool install --global --add-source ./nupkg AGP.CLI

# Or install locally in your project
dotnet tool install AGP.CLI
```

### Build from Source

```bash
cd agp-cli
dotnet build
dotnet pack -o nupkg
```

## Usage

### Generate a Project

```bash
agp generate --description "Create a simple calculator" --output ./my-calculator
```

### Options

- `--description, -d` (required): Project description
- `--output, -o`: Output directory (default: current directory)
- `--type, -t`: Project type - console, winforms, webapi (default: console)
- `--language, -l`: Programming language - csharp, python, javascript (default: csharp)
- `--server, -s`: Server URL (default: http://localhost:7077)
- `--model, -m`: Model name (optional)

### Examples

**C# Console Application:**
```bash
agp generate -d "Create a simple calculator app" -o ./calculator
```

**Python Script:**
```bash
agp generate -d "Create a web scraper" -o ./scraper -l python
```

**C# Web API:**
```bash
agp generate -d "Create a REST API for todo items" -o ./todo-api -t webapi
```

**Using a Specific Model:**
```bash
agp generate -d "Create a game" -o ./game -m llama-2-7b.gguf
```

**Custom Server:**
```bash
agp generate -d "Create an app" -o ./app -s http://remote-server:7077
```

## Requirements

- .NET 9.0 or later
- ASHATAIServer running and accessible

## Output

The CLI will:
1. Connect to the ASHATAIServer
2. Generate project files based on your description
3. Save all files to the specified output directory
4. Display build instructions for the generated project

## Example Output

```
╔══════════════════════════════════════════════════════════╗
║    AGP CLI - Project Generator                           ║
╚══════════════════════════════════════════════════════════╝

Description: Create a simple calculator
Type: console
Language: csharp
Output: ./calculator
Server: http://localhost:7077

Connecting to server...
✓ Connected
Generating project...
✓ Project generated

Files generated: 3
Model used: MockRuntime
Generation time: 150ms

Saving files...
  ✓ Program.cs
  ✓ Calculator.cs
  ✓ README.md

✓ Project saved to: /home/user/calculator

Next steps:
  cd ./calculator
  dotnet build
  dotnet run
```

## Troubleshooting

**Connection Error:**
```
❌ Connection error: No connection could be made
Make sure ASHATAIServer is running on the specified URL.
```

Solution: Start ASHATAIServer on the correct port:
```bash
cd ASHATAIServer
dotnet run
```

**Invalid Output Directory:**
The CLI will automatically create the output directory if it doesn't exist.

## License

Copyright © 2025 AGP Studios, INC. All rights reserved.
