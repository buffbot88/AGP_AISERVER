#!/bin/bash

#
# Publish ASHATAIServer for Linux deployment
#
# This script builds and publishes ASHATAIServer as a self-contained,
# single-file executable for Linux. Supports x64, ARM64, and ARM architectures.
#
# Usage:
#   ./publish.sh [options]
#
# Options:
#   -a, --arch <arch>      Target architecture: x64 (default), arm64, or arm
#   -o, --output <path>    Output directory (default: ./publish/linux-{arch})
#   -c, --config <config>  Build configuration: Release (default) or Debug
#   --no-single-file       Don't create a single-file executable
#   --framework-dependent  Don't include .NET runtime
#   -h, --help             Show this help message
#
# Examples:
#   ./publish.sh
#   ./publish.sh -a arm64 -o /opt/ashataiserver
#   ./publish.sh --config Debug --no-single-file
#

set -e

# Default values
ARCHITECTURE="x64"
OUTPUT_PATH=""
CONFIGURATION="Release"
SINGLE_FILE=true
SELF_CONTAINED=true

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -a|--arch)
            ARCHITECTURE="$2"
            shift 2
            ;;
        -o|--output)
            OUTPUT_PATH="$2"
            shift 2
            ;;
        -c|--config)
            CONFIGURATION="$2"
            shift 2
            ;;
        --no-single-file)
            SINGLE_FILE=false
            shift
            ;;
        --framework-dependent)
            SELF_CONTAINED=false
            shift
            ;;
        -h|--help)
            grep '^#' "$0" | grep -v '#!/bin/bash' | sed 's/^# //; s/^#//'
            exit 0
            ;;
        *)
            echo -e "${RED}Error: Unknown option $1${NC}"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

# Validate architecture
case $ARCHITECTURE in
    x64|arm64|arm)
        ;;
    *)
        echo -e "${RED}Error: Invalid architecture: $ARCHITECTURE${NC}"
        echo "Valid options: x64, arm64, arm"
        exit 1
        ;;
esac

# Validate configuration
case $CONFIGURATION in
    Release|Debug)
        ;;
    *)
        echo -e "${RED}Error: Invalid configuration: $CONFIGURATION${NC}"
        echo "Valid options: Release, Debug"
        exit 1
        ;;
esac

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"
PROJECT_PATH="$ROOT_DIR/ASHATAIServer/ASHATAIServer.csproj"

# Set default output path if not specified
if [ -z "$OUTPUT_PATH" ]; then
    OUTPUT_PATH="$ROOT_DIR/publish/linux-$ARCHITECTURE"
fi

# Display configuration
echo -e "${CYAN}================================================${NC}"
echo -e "${CYAN}  ASHATAIServer Linux Publisher${NC}"
echo -e "${CYAN}================================================${NC}"
echo "Architecture:   $ARCHITECTURE"
echo "Configuration:  $CONFIGURATION"
echo "Output Path:    $OUTPUT_PATH"
echo "Single File:    $SINGLE_FILE"
echo "Self-Contained: $SELF_CONTAINED"
echo -e "${CYAN}================================================${NC}"
echo ""

# Verify project file exists
if [ ! -f "$PROJECT_PATH" ]; then
    echo -e "${RED}Error: Project file not found: $PROJECT_PATH${NC}"
    exit 1
fi

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Error: dotnet SDK not found${NC}"
    echo "Please install .NET 9.0 SDK from https://dotnet.microsoft.com/download"
    exit 1
fi

# Clean output directory
if [ -d "$OUTPUT_PATH" ]; then
    echo -e "${YELLOW}Cleaning output directory...${NC}"
    rm -rf "$OUTPUT_PATH"
fi

# Create output directory
mkdir -p "$OUTPUT_PATH"

# Build publish command
RUNTIME_IDENTIFIER="linux-$ARCHITECTURE"
PUBLISH_ARGS=(
    "publish"
    "$PROJECT_PATH"
    "-c" "$CONFIGURATION"
    "-r" "$RUNTIME_IDENTIFIER"
    "-o" "$OUTPUT_PATH"
    "--nologo"
)

if [ "$SELF_CONTAINED" = true ]; then
    PUBLISH_ARGS+=("--self-contained" "true")
else
    PUBLISH_ARGS+=("--self-contained" "false")
fi

if [ "$SINGLE_FILE" = true ]; then
    PUBLISH_ARGS+=(
        "/p:PublishSingleFile=true"
        "/p:IncludeNativeLibrariesForSelfExtract=true"
        "/p:EnableCompressionInSingleFile=true"
    )
fi

# Additional optimizations
PUBLISH_ARGS+=(
    "/p:PublishTrimmed=false"  # Disable trimming for compatibility
    "/p:DebugType=None"
    "/p:DebugSymbols=false"
)

# Execute publish
echo -e "${GREEN}Publishing ASHATAIServer...${NC}"
echo -e "${NC}Command: dotnet ${PUBLISH_ARGS[*]}${NC}"
echo ""

if ! dotnet "${PUBLISH_ARGS[@]}"; then
    echo -e "${RED}Error: Publish failed${NC}"
    exit 1
fi

# Copy additional files
echo ""
echo -e "${GREEN}Copying additional files...${NC}"

# Copy configuration files
for file in appsettings.json appsettings.Production.json; do
    SOURCE_PATH="$ROOT_DIR/ASHATAIServer/$file"
    if [ -f "$SOURCE_PATH" ]; then
        cp "$SOURCE_PATH" "$OUTPUT_PATH/"
        echo "  Copied: $file"
    fi
done

# Copy documentation
for file in README.md LICENSE; do
    SOURCE_PATH="$ROOT_DIR/$file"
    if [ -f "$SOURCE_PATH" ]; then
        cp "$SOURCE_PATH" "$OUTPUT_PATH/"
        echo "  Copied: $file"
    fi
done

# Create directories
for dir in models data; do
    mkdir -p "$OUTPUT_PATH/$dir"
    echo "  Created: $dir directory"
done

# Create sample configuration
cat > "$OUTPUT_PATH/GETTING_STARTED.txt" << 'EOF'
# ASHATAIServer Configuration

## Quick Start
1. Place .gguf model files in the 'models' directory
2. Run ./ASHATAIServer (or ./start-server.sh)
3. Server will start on http://localhost:7077

## Configuration
Edit appsettings.json to customize:
- Server port
- Models directory
- Database location
- Rate limiting
- TLS/HTTPS settings

## Linux-Specific Notes
- The executable may need execute permissions: chmod +x ASHATAIServer
- For systemd service: see docs/DEPLOYMENT.md
- For development: use the provided start-server.sh script

## Documentation
See README.md for complete API documentation and examples.

## Support
GitHub: https://github.com/buffbot88/AGP_AISERVER
EOF

echo "  Created: GETTING_STARTED.txt"

# Create shell script to run server
cat > "$OUTPUT_PATH/start-server.sh" << 'EOF'
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
EOF

chmod +x "$OUTPUT_PATH/start-server.sh"
echo "  Created: start-server.sh"

# Make the main executable executable
if [ -f "$OUTPUT_PATH/ASHATAIServer" ]; then
    chmod +x "$OUTPUT_PATH/ASHATAIServer"
fi

# Get file sizes
echo ""
echo -e "${CYAN}Build Summary:${NC}"
echo -e "${CYAN}================================================${NC}"

if [ -f "$OUTPUT_PATH/ASHATAIServer" ]; then
    FILE_SIZE=$(du -h "$OUTPUT_PATH/ASHATAIServer" | cut -f1)
    echo "Executable:     ASHATAIServer ($FILE_SIZE)"
fi

TOTAL_SIZE=$(du -sh "$OUTPUT_PATH" | cut -f1)
echo "Total Size:     $TOTAL_SIZE"
echo "Output Path:    $OUTPUT_PATH"
echo -e "${CYAN}================================================${NC}"

# Create tar.gz archive
ARCHIVE_DIR="$(dirname "$OUTPUT_PATH")"
ARCHIVE_NAME="ASHATAIServer-linux-$ARCHITECTURE-$CONFIGURATION.tar.gz"
ARCHIVE_PATH="$ARCHIVE_DIR/$ARCHIVE_NAME"

echo ""
echo -e "${GREEN}Creating tar.gz archive...${NC}"

if tar -czf "$ARCHIVE_PATH" -C "$OUTPUT_PATH" .; then
    ARCHIVE_SIZE=$(du -h "$ARCHIVE_PATH" | cut -f1)
    echo "  Archive created: $ARCHIVE_NAME ($ARCHIVE_SIZE)"
else
    echo -e "${YELLOW}Warning: Failed to create tar.gz archive${NC}"
fi

echo ""
echo -e "${GREEN}================================================${NC}"
echo -e "${GREEN}  Publish completed successfully!${NC}"
echo -e "${GREEN}================================================${NC}"
echo ""
echo -e "${YELLOW}To run the server:${NC}"
echo "  cd $OUTPUT_PATH"
echo "  ./ASHATAIServer"
echo ""
echo -e "${YELLOW}Or use the convenience script:${NC}"
echo "  ./start-server.sh"
echo ""
echo -e "${YELLOW}For systemd service installation:${NC}"
echo "  See docs/DEPLOYMENT.md"
echo ""
