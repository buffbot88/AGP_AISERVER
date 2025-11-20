#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Windows Installation Helper for ASHATAIServer
    
.DESCRIPTION
    This script automates the installation and initial setup of ASHATAIServer on Windows.
    It can install as a standalone application or as a Windows Service.
    
.PARAMETER InstallPath
    Installation directory (default: C:\Program Files\ASHATAIServer)
    
.PARAMETER ServiceMode
    Install as a Windows Service using NSSM
    
.PARAMETER DownloadUrl
    URL to download the ASHATAIServer release (if not already downloaded)
    
.PARAMETER SkipFirewall
    Skip Windows Firewall configuration
    
.EXAMPLE
    .\install-windows.ps1
    Basic installation to default location
    
.EXAMPLE
    .\install-windows.ps1 -InstallPath "D:\ASHATAIServer" -ServiceMode
    Install to D:\ as a Windows Service
    
.EXAMPLE
    .\install-windows.ps1 -DownloadUrl "https://github.com/buffbot88/AGP_AISERVER/releases/latest/download/ASHATAIServer-windows-x64.zip"
    Download and install from release URL
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$InstallPath = "C:\Program Files\ASHATAIServer",
    
    [Parameter(Mandatory=$false)]
    [switch]$ServiceMode,
    
    [Parameter(Mandatory=$false)]
    [string]$DownloadUrl = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipFirewall
)

$ErrorActionPreference = "Stop"

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please right-click PowerShell and select 'Run as Administrator', then run this script again." -ForegroundColor Yellow
    exit 1
}

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  ASHATAIServer Windows Installer" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET 9.0 Runtime is installed
Write-Host "Checking for .NET 9.0 Runtime..." -ForegroundColor Yellow
$dotnetVersion = & dotnet --version 2>$null
if ($LASTEXITCODE -ne 0 -or -not $dotnetVersion) {
    Write-Host "WARNING: .NET 9.0 Runtime not found" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Please install .NET 9.0 Runtime from:" -ForegroundColor White
    Write-Host "  https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Cyan
    Write-Host ""
    $response = Read-Host "Continue anyway? (y/n)"
    if ($response -ne 'y') {
        exit 1
    }
} else {
    Write-Host "  Found: .NET $dotnetVersion" -ForegroundColor Green
}

Write-Host ""

# Download if URL provided
if ($DownloadUrl) {
    Write-Host "Downloading ASHATAIServer from release..." -ForegroundColor Yellow
    $tempZip = Join-Path $env:TEMP "ASHATAIServer.zip"
    $tempExtract = Join-Path $env:TEMP "ASHATAIServer-Extract"
    
    try {
        Invoke-WebRequest -Uri $DownloadUrl -OutFile $tempZip
        Write-Host "  Download complete" -ForegroundColor Green
        
        Write-Host "Extracting archive..." -ForegroundColor Yellow
        if (Test-Path $tempExtract) {
            Remove-Item -Path $tempExtract -Recurse -Force
        }
        Expand-Archive -Path $tempZip -DestinationPath $tempExtract -Force
        Write-Host "  Extraction complete" -ForegroundColor Green
        
        $SourcePath = $tempExtract
    } catch {
        Write-Host "ERROR: Failed to download or extract: $_" -ForegroundColor Red
        exit 1
    }
} else {
    # Assume we're installing from publish directory or current location
    $publishPath = Join-Path (Split-Path -Parent $PSScriptRoot) "publish\windows-x64"
    
    if (Test-Path $publishPath) {
        $SourcePath = $publishPath
        Write-Host "Installing from: $SourcePath" -ForegroundColor Green
    } elseif (Test-Path ".\ASHATAIServer.exe") {
        $SourcePath = Get-Location
        Write-Host "Installing from: $SourcePath" -ForegroundColor Green
    } else {
        Write-Host "ERROR: Could not find ASHATAIServer files" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please either:" -ForegroundColor Yellow
        Write-Host "  1. Run this script from the extracted release directory" -ForegroundColor White
        Write-Host "  2. Use -DownloadUrl parameter to download a release" -ForegroundColor White
        Write-Host "  3. Run the publish script first: .\scripts\publish.ps1" -ForegroundColor White
        exit 1
    }
}

Write-Host ""

# Create installation directory
Write-Host "Creating installation directory: $InstallPath" -ForegroundColor Yellow
if (Test-Path $InstallPath) {
    Write-Host "  Directory exists, files will be overwritten" -ForegroundColor Yellow
} else {
    New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null
    Write-Host "  Directory created" -ForegroundColor Green
}

# Copy files
Write-Host "Copying files..." -ForegroundColor Yellow
Copy-Item -Path "$SourcePath\*" -Destination $InstallPath -Recurse -Force
Write-Host "  Files copied successfully" -ForegroundColor Green

# Create data and models directories
Write-Host "Creating data directories..." -ForegroundColor Yellow
$dataDir = Join-Path $InstallPath "data"
$modelsDir = Join-Path $InstallPath "models"
New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
New-Item -ItemType Directory -Path $modelsDir -Force | Out-Null
Write-Host "  Directories created" -ForegroundColor Green

# Configure Windows Firewall
if (-not $SkipFirewall) {
    Write-Host ""
    Write-Host "Configuring Windows Firewall..." -ForegroundColor Yellow
    
    try {
        # Remove existing rule if it exists
        $existingRule = Get-NetFirewallRule -DisplayName "ASHAT AI Server HTTP" -ErrorAction SilentlyContinue
        if ($existingRule) {
            Remove-NetFirewallRule -DisplayName "ASHAT AI Server HTTP"
        }
        
        # Add new rule
        New-NetFirewallRule -DisplayName "ASHAT AI Server HTTP" `
            -Direction Inbound `
            -Protocol TCP `
            -LocalPort 7077 `
            -Action Allow `
            -Profile Any | Out-Null
        
        Write-Host "  Firewall rule created for port 7077" -ForegroundColor Green
    } catch {
        Write-Host "  WARNING: Failed to configure firewall: $_" -ForegroundColor Yellow
    }
}

# Service installation
if ($ServiceMode) {
    Write-Host ""
    Write-Host "Installing as Windows Service..." -ForegroundColor Yellow
    
    # Check if NSSM is installed
    $nssm = Get-Command nssm -ErrorAction SilentlyContinue
    
    if (-not $nssm) {
        Write-Host "  NSSM (Non-Sucking Service Manager) not found" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "To install NSSM:" -ForegroundColor White
        Write-Host "  1. Install Chocolatey: https://chocolatey.org/install" -ForegroundColor Cyan
        Write-Host "  2. Run: choco install nssm" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "OR download manually from: https://nssm.cc/download" -ForegroundColor Cyan
        Write-Host ""
        $response = Read-Host "Skip service installation? (y/n)"
        if ($response -ne 'y') {
            exit 1
        }
    } else {
        $exePath = Join-Path $InstallPath "ASHATAIServer.exe"
        $serviceName = "ASHATAIServer"
        
        # Check if service already exists
        $existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
        if ($existingService) {
            Write-Host "  Removing existing service..." -ForegroundColor Yellow
            & nssm stop $serviceName
            & nssm remove $serviceName confirm
        }
        
        # Install service
        & nssm install $serviceName $exePath
        & nssm set $serviceName AppDirectory $InstallPath
        & nssm set $serviceName DisplayName "ASHAT AI Server"
        & nssm set $serviceName Description "AI Processing Server for ASHAT Goddess"
        & nssm set $serviceName Start SERVICE_AUTO_START
        
        # Configure logging
        $logsDir = Join-Path $InstallPath "logs"
        New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
        & nssm set $serviceName AppStdout (Join-Path $logsDir "service-stdout.log")
        & nssm set $serviceName AppStderr (Join-Path $logsDir "service-stderr.log")
        
        # Start service
        Write-Host "  Starting service..." -ForegroundColor Yellow
        & nssm start $serviceName
        
        Start-Sleep -Seconds 2
        
        $serviceStatus = Get-Service -Name $serviceName
        if ($serviceStatus.Status -eq "Running") {
            Write-Host "  Service installed and started successfully" -ForegroundColor Green
        } else {
            Write-Host "  WARNING: Service installed but not running" -ForegroundColor Yellow
            Write-Host "  Check logs in: $logsDir" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host ""
    Write-Host "Standalone installation complete" -ForegroundColor Green
}

# Test the installation
Write-Host ""
Write-Host "Testing installation..." -ForegroundColor Yellow

if ($ServiceMode) {
    Start-Sleep -Seconds 3
}

try {
    $healthUrl = "http://localhost:7077/api/ai/health"
    $response = Invoke-WebRequest -Uri $healthUrl -TimeoutSec 5 -ErrorAction Stop
    if ($response.StatusCode -eq 200) {
        Write-Host "  Health check passed!" -ForegroundColor Green
    }
} catch {
    Write-Host "  Could not connect to server (this is OK if not started yet)" -ForegroundColor Yellow
}

# Display summary
Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "  Installation Complete!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Installation Path:  $InstallPath" -ForegroundColor White
Write-Host "Server URL:         http://localhost:7077" -ForegroundColor White
Write-Host ""

if ($ServiceMode) {
    Write-Host "Service Management:" -ForegroundColor Cyan
    Write-Host "  Start:   nssm start ASHATAIServer" -ForegroundColor White
    Write-Host "  Stop:    nssm stop ASHATAIServer" -ForegroundColor White
    Write-Host "  Restart: nssm restart ASHATAIServer" -ForegroundColor White
    Write-Host "  Status:  nssm status ASHATAIServer" -ForegroundColor White
    Write-Host ""
    Write-Host "View logs: $InstallPath\logs\" -ForegroundColor White
} else {
    Write-Host "To start the server:" -ForegroundColor Cyan
    Write-Host "  cd `"$InstallPath`"" -ForegroundColor White
    Write-Host "  .\ASHATAIServer.exe" -ForegroundColor White
    Write-Host ""
    Write-Host "OR double-click: $InstallPath\start-server.bat" -ForegroundColor White
}

Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Place .gguf model files in: $InstallPath\models\" -ForegroundColor White
Write-Host "  2. Configure settings in: $InstallPath\appsettings.json" -ForegroundColor White
Write-Host "  3. Access health check: http://localhost:7077/api/ai/health" -ForegroundColor White
Write-Host "  4. See documentation: $InstallPath\README.md" -ForegroundColor White
Write-Host ""
Write-Host "For detailed deployment information:" -ForegroundColor Cyan
Write-Host "  See docs/DEPLOYMENT.md in the repository" -ForegroundColor White
Write-Host ""
