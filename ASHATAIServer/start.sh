#!/bin/bash
# Quick start script for ASHATAIServer

echo "╔══════════════════════════════════════════════════════════╗"
echo "║         ASHATAIServer - Quick Start Script              ║"
echo "╚══════════════════════════════════════════════════════════╝"
echo ""

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ Error: .NET SDK is not installed."
    echo "Please install .NET 9.0 SDK from: https://dotnet.microsoft.com/download"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
echo "✅ .NET SDK version: $DOTNET_VERSION"

# Navigate to ASHATAIServer directory
cd "$(dirname "$0")"

# Check if models directory exists
if [ ! -d "models" ]; then
    mkdir -p models
    echo "📁 Created models directory"
fi

# Check for .gguf files
GGUF_COUNT=$(find models -name "*.gguf" 2>/dev/null | wc -l)
echo "📊 Found $GGUF_COUNT .gguf model file(s) in models directory"

if [ $GGUF_COUNT -eq 0 ]; then
    echo ""
    echo "⚠️  No .gguf model files found!"
    echo "   Place your .gguf language model files in the 'models' directory"
    echo "   The server will still start, but AI processing will be limited."
    echo ""
fi

echo ""
echo "🚀 Starting ASHATAIServer on port 8088..."
echo ""

# Run the server
dotnet run
