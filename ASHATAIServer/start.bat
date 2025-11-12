@echo off
REM Quick start script for ASHATAIServer

echo ╔══════════════════════════════════════════════════════════╗
echo ║         ASHATAIServer - Quick Start Script              ║
echo ╚══════════════════════════════════════════════════════════╝
echo.

REM Check if .NET is installed
dotnet --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Error: .NET SDK is not installed.
    echo Please install .NET 9.0 SDK from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

REM Check .NET version
for /f "delims=" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo ✅ .NET SDK version: %DOTNET_VERSION%

REM Navigate to ASHATAIServer directory
cd /d "%~dp0"

REM Check if models directory exists
if not exist "models" (
    mkdir models
    echo 📁 Created models directory
)

REM Check for .gguf files
set GGUF_COUNT=0
for %%f in (models\*.gguf) do set /a GGUF_COUNT+=1
echo 📊 Found %GGUF_COUNT% .gguf model file(s) in models directory

if %GGUF_COUNT% EQU 0 (
    echo.
    echo ⚠️  No .gguf model files found!
    echo    Place your .gguf language model files in the 'models' directory
    echo    The server will still start, but AI processing will be limited.
    echo.
)

echo.
echo 🚀 Starting ASHATAIServer on port 8088...
echo.

REM Run the server
dotnet run

pause
