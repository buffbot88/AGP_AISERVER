#!/bin/bash

echo "Starting ASHATAIServer..."
echo "Server will be available at http://localhost:7077"
echo "Press Ctrl+C to stop the server"
echo ""

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Change to script directory
cd "$SCRIPT_DIR"

# Make sure the executable has execute permissions
chmod +x ASHATAIServer 2>/dev/null || true

# Run the server
./ASHATAIServer
