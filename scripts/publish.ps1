#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Publish ASHATAIServer for Windows deployment
    
.DESCRIPTION
    This script builds and publishes ASHATAIServer as a self-contained,
    single-file executable for Windows. Supports both x64 and ARM64 architectures.
    
.PARAMETER Architecture
    Target architecture: x64 (default) or arm64
    
.PARAMETER OutputPath
    Output directory for published files (default: ./publish/windows-{arch})
    
.PARAMETER Configuration
    Build configuration: Release (default) or Debug
    
.PARAMETER SingleFile
    Create a single-file executable (default: true)
    
.PARAMETER SelfContained
    Include .NET runtime in the package (default: true)
    
.EXAMPLE
    .\publish.ps1
    Publishes x64 Release build to ./publish/windows-x64
    
.EXAMPLE
    .\publish.ps1 -Architecture arm64 -OutputPath C:\Deploy
    Publishes ARM64 build to C:\Deploy
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("x64", "arm64")]
    [string]$Architecture = "x64",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [bool]$SingleFile = $true,
    
    [Parameter(Mandatory=$false)]
    [bool]$SelfContained = $true
)

# Script configuration
$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$ProjectPath = Join-Path $RootDir "ASHATAIServer\ASHATAIServer.csproj"

# Set default output path if not specified
if ([string]::IsNullOrEmpty($OutputPath)) {
    $OutputPath = Join-Path $RootDir "publish\windows-$Architecture"
}

# Display configuration
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  ASHATAIServer Windows Publisher" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Architecture:   $Architecture"
Write-Host "Configuration:  $Configuration"
Write-Host "Output Path:    $OutputPath"
Write-Host "Single File:    $SingleFile"
Write-Host "Self-Contained: $SelfContained"
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Verify project file exists
if (-not (Test-Path $ProjectPath)) {
    Write-Error "Project file not found: $ProjectPath"
    exit 1
}

# Clean output directory
if (Test-Path $OutputPath) {
    Write-Host "Cleaning output directory..." -ForegroundColor Yellow
    Remove-Item -Path $OutputPath -Recurse -Force
}

# Create output directory
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

# Build publish command
$RuntimeIdentifier = "win-$Architecture"
$PublishArgs = @(
    "publish"
    $ProjectPath
    "-c", $Configuration
    "-r", $RuntimeIdentifier
    "-o", $OutputPath
    "--nologo"
)

if ($SelfContained) {
    $PublishArgs += "--self-contained", "true"
} else {
    $PublishArgs += "--self-contained", "false"
}

if ($SingleFile) {
    $PublishArgs += "/p:PublishSingleFile=true"
    $PublishArgs += "/p:IncludeNativeLibrariesForSelfExtract=true"
    $PublishArgs += "/p:EnableCompressionInSingleFile=true"
}

# Additional optimizations
$PublishArgs += "/p:PublishTrimmed=false"  # Disable trimming for compatibility
$PublishArgs += "/p:DebugType=None"
$PublishArgs += "/p:DebugSymbols=false"

# Execute publish
Write-Host "Publishing ASHATAIServer..." -ForegroundColor Green
Write-Host "Command: dotnet $($PublishArgs -join ' ')" -ForegroundColor Gray
Write-Host ""

try {
    & dotnet $PublishArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "Publish failed with exit code $LASTEXITCODE"
    }
} catch {
    Write-Error "Publish failed: $_"
    exit 1
}

# Copy additional files
Write-Host ""
Write-Host "Copying additional files..." -ForegroundColor Green

# Copy configuration files
$ConfigFiles = @(
    "appsettings.json",
    "appsettings.Production.json"
)

foreach ($file in $ConfigFiles) {
    $sourcePath = Join-Path $RootDir "ASHATAIServer\$file"
    if (Test-Path $sourcePath) {
        Copy-Item $sourcePath $OutputPath -Force
        Write-Host "  Copied: $file"
    }
}

# Copy documentation
$DocFiles = @(
    "README.md",
    "LICENSE"
)

foreach ($file in $DocFiles) {
    $sourcePath = Join-Path $RootDir $file
    if (Test-Path $sourcePath) {
        Copy-Item $sourcePath $OutputPath -Force
        Write-Host "  Copied: $file"
    }
}

# Create directories
$Directories = @("models", "data")
foreach ($dir in $Directories) {
    $dirPath = Join-Path $OutputPath $dir
    New-Item -ItemType Directory -Path $dirPath -Force | Out-Null
    Write-Host "  Created: $dir directory"
}

# Create sample configuration
$SampleConfig = @"
# ASHATAIServer Configuration

## Quick Start
1. Place .gguf model files in the 'models' directory
2. Run ASHATAIServer.exe
3. Server will start on http://localhost:7077

## Configuration
Edit appsettings.json to customize:
- Server port
- Models directory
- Database location
- Rate limiting
- TLS/HTTPS settings

## Documentation
See README.md for complete API documentation and examples.

## Support
GitHub: https://github.com/buffbot88/AGP_AISERVER
"@

Set-Content -Path (Join-Path $OutputPath "GETTING_STARTED.txt") -Value $SampleConfig
Write-Host "  Created: GETTING_STARTED.txt"

# Create batch file to run server
$BatchScript = @"
@echo off
echo Starting ASHATAIServer...
echo Server will be available at http://localhost:7077
echo Press Ctrl+C to stop the server
echo.
ASHATAIServer.exe
pause
"@

Set-Content -Path (Join-Path $OutputPath "start-server.bat") -Value $BatchScript
Write-Host "  Created: start-server.bat"

# Get file sizes
Write-Host ""
Write-Host "Build Summary:" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

$ExePath = Join-Path $OutputPath "ASHATAIServer.exe"
if (Test-Path $ExePath) {
    $FileSize = (Get-Item $ExePath).Length / 1MB
    Write-Host "Executable:     ASHATAIServer.exe ($([math]::Round($FileSize, 2)) MB)"
}

$TotalSize = (Get-ChildItem -Path $OutputPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "Total Size:     $([math]::Round($TotalSize, 2)) MB"
Write-Host "Output Path:    $OutputPath"
Write-Host "================================================" -ForegroundColor Cyan

# Create zip archive
$ZipPath = Join-Path (Split-Path $OutputPath) "ASHATAIServer-windows-$Architecture-$Configuration.zip"
Write-Host ""
Write-Host "Creating zip archive..." -ForegroundColor Green

try {
    Compress-Archive -Path "$OutputPath\*" -DestinationPath $ZipPath -Force
    $ZipSize = (Get-Item $ZipPath).Length / 1MB
    Write-Host "  Archive created: $(Split-Path $ZipPath -Leaf) ($([math]::Round($ZipSize, 2)) MB)"
} catch {
    Write-Warning "Failed to create zip archive: $_"
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "  Publish completed successfully!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""
Write-Host "To run the server:" -ForegroundColor Yellow
Write-Host "  cd $OutputPath"
Write-Host "  .\ASHATAIServer.exe"
Write-Host ""
Write-Host "Or double-click: start-server.bat" -ForegroundColor Yellow
Write-Host ""
