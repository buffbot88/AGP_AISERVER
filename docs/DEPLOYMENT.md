# Deployment Guide - ASHATAIServer

This guide provides comprehensive instructions for deploying ASHATAIServer in various environments.

## Table of Contents
- [Quick Start](#quick-start)
- [Docker Deployment](#docker-deployment)
- [Windows Deployment](#windows-deployment)
- [Linux Deployment](#linux-deployment)
- [Environment Variables](#environment-variables)
- [Production Checklist](#production-checklist)
- [Troubleshooting](#troubleshooting)

---

## Quick Start

### Prerequisites
- .NET 9.0 Runtime (or SDK for building from source)
- Optional: Docker (for containerized deployment)
- Optional: GGUF model files for AI inference

### Minimum System Requirements
- **CPU**: 2 cores (4+ recommended for production)
- **RAM**: 2 GB (8+ GB recommended with large models)
- **Storage**: 500 MB for application + space for models and database
- **OS**: Windows 10/11, Linux (Ubuntu 20.04+, RHEL 8+, Debian 11+), or macOS 11+

---

## Docker Deployment

### Basic Docker Deployment

#### 1. Build the Docker image
```bash
docker build -t ashataiserver:latest .
```

#### 2. Run the container
```bash
# Create directories for persistent data
mkdir -p models data

# Run the container
docker run -d \
  --name ashataiserver \
  -p 7077:7077 \
  -v $(pwd)/models:/app/models:ro \
  -v $(pwd)/data:/app/data \
  --restart unless-stopped \
  ashataiserver:latest
```

#### 3. Verify it's running
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

### Docker Compose Deployment

#### Basic Configuration

1. **Copy the docker-compose.yml file**
```bash
# Already in the repository root
```

2. **Create required directories**
```bash
mkdir -p models data
```

3. **Start the services**
```bash
docker-compose up -d
```

4. **View logs**
```bash
docker-compose logs -f
```

5. **Stop the services**
```bash
docker-compose down
```

#### Advanced Configuration with HTTPS

For production deployments with HTTPS:

1. **Prepare TLS certificates**
```bash
mkdir -p certs

# Option 1: Use existing certificate
cp your-certificate.pfx certs/certificate.pfx

# Option 2: Generate self-signed certificate (testing only)
openssl req -x509 -newkey rsa:4096 -keyout certs/key.pem -out certs/cert.pem -days 365 -nodes
openssl pkcs12 -export -out certs/certificate.pfx -inkey certs/key.pem -in certs/cert.pem
```

2. **Set certificate password**
```bash
# Create .env file
echo "CERT_PASSWORD=YourSecurePassword" > .env
```

3. **Use advanced configuration**
```bash
docker-compose -f docker-compose.advanced.yml up -d
```

### Docker Volume Management

#### Persistent Data Locations
- **Models**: `/app/models` - Mount your GGUF model files here (read-only recommended)
- **Database**: `/app/data/users.db` - User accounts and API keys
- **Logs**: Written to stdout/stderr (use Docker logging drivers)

#### Backup Database
```bash
# Create backup
docker cp ashataiserver:/app/data/users.db ./backup-users-$(date +%Y%m%d).db

# Restore backup
docker cp ./backup-users-20251120.db ashataiserver:/app/data/users.db
docker restart ashataiserver
```

#### Update Models
```bash
# Copy new model to models directory
cp new-model.gguf models/

# Trigger model rescan via API
curl -X POST http://localhost:7077/api/ai/models/scan
```

---

## Windows Deployment

### Pre-built Binary Deployment

#### 1. Download or Build the Binary

**Option A: Build from source**
```powershell
# Run the publish script
.\scripts\publish.ps1

# Output will be in publish\windows-x64\
```

**Option B: Download release**
```powershell
# Download from GitHub Releases
# Extract the ZIP file
Expand-Archive -Path ASHATAIServer-windows-x64-Release.zip -DestinationPath C:\ASHATAIServer
```

#### 2. Configure the Application

Edit `appsettings.json`:
```json
{
  "ModelsDirectory": "models",
  "Database": {
    "Path": "data/users.db"
  }
}
```

#### 3. Run the Server

**Option A: Double-click**
```
Double-click start-server.bat
```

**Option B: PowerShell**
```powershell
cd C:\ASHATAIServer
.\ASHATAIServer.exe
```

**Option C: Command Prompt**
```cmd
cd C:\ASHATAIServer
ASHATAIServer.exe
```

### Windows Service Installation

For production deployments, run ASHATAIServer as a Windows Service:

#### Using NSSM (Non-Sucking Service Manager)

1. **Download NSSM**
```powershell
# Download from https://nssm.cc/download
# Or use Chocolatey
choco install nssm
```

2. **Install the service**
```powershell
# Open PowerShell as Administrator
cd C:\ASHATAIServer

# Install service
nssm install ASHATAIServer "C:\ASHATAIServer\ASHATAIServer.exe"

# Configure service
nssm set ASHATAIServer AppDirectory C:\ASHATAIServer
nssm set ASHATAIServer DisplayName "ASHAT AI Server"
nssm set ASHATAIServer Description "AI Processing Server for ASHAT Goddess"
nssm set ASHATAIServer Start SERVICE_AUTO_START

# Configure logging
nssm set ASHATAIServer AppStdout C:\ASHATAIServer\logs\service-stdout.log
nssm set ASHATAIServer AppStderr C:\ASHATAIServer\logs\service-stderr.log

# Start the service
nssm start ASHATAIServer
```

3. **Manage the service**
```powershell
# Check status
nssm status ASHATAIServer

# Stop service
nssm stop ASHATAIServer

# Restart service
nssm restart ASHATAIServer

# Remove service
nssm remove ASHATAIServer confirm
```

#### Using sc.exe (Built-in Windows Tool)

```powershell
# Create service
sc.exe create ASHATAIServer binPath= "C:\ASHATAIServer\ASHATAIServer.exe" start= auto

# Start service
sc.exe start ASHATAIServer

# Stop service
sc.exe stop ASHATAIServer

# Delete service
sc.exe delete ASHATAIServer
```

### Windows Firewall Configuration

```powershell
# Allow HTTP traffic on port 7077
New-NetFirewallRule -DisplayName "ASHAT AI Server HTTP" `
  -Direction Inbound `
  -Protocol TCP `
  -LocalPort 7077 `
  -Action Allow

# Allow HTTPS traffic on port 7443 (if using HTTPS)
New-NetFirewallRule -DisplayName "ASHAT AI Server HTTPS" `
  -Direction Inbound `
  -Protocol TCP `
  -LocalPort 7443 `
  -Action Allow
```

### Automatic Startup (Scheduled Task)

```powershell
# Create scheduled task to run at startup
$action = New-ScheduledTaskAction -Execute "C:\ASHATAIServer\ASHATAIServer.exe"
$trigger = New-ScheduledTaskTrigger -AtStartup
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries

Register-ScheduledTask -TaskName "ASHATAIServer" `
  -Action $action `
  -Trigger $trigger `
  -Principal $principal `
  -Settings $settings `
  -Description "ASHAT AI Server automatic startup"
```

---

## Linux Deployment

### Pre-built Binary Deployment

#### 1. Build or Download the Binary

**Build from source:**
```bash
# Run the publish script
chmod +x scripts/publish.sh
./scripts/publish.sh

# Output will be in publish/linux-x64/
```

#### 2. Install the Application

```bash
# Create installation directory
sudo mkdir -p /opt/ashataiserver

# Copy files
sudo cp -r publish/linux-x64/* /opt/ashataiserver/

# Set permissions
sudo chown -R www-data:www-data /opt/ashataiserver
sudo chmod +x /opt/ashataiserver/ASHATAIServer
```

#### 3. Configure the Application

```bash
# Edit configuration
sudo nano /opt/ashataiserver/appsettings.json
```

#### 4. Run the Server

```bash
cd /opt/ashataiserver
./ASHATAIServer
```

### systemd Service Configuration

For production deployments, run as a systemd service:

#### 1. Create Service File

```bash
sudo nano /etc/systemd/system/ashataiserver.service
```

**Service file content:**
```ini
[Unit]
Description=ASHAT AI Server
After=network.target

[Service]
Type=notify
WorkingDirectory=/opt/ashataiserver
ExecStart=/opt/ashataiserver/ASHATAIServer
Restart=on-failure
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=ashataiserver
User=www-data
Group=www-data

# Security settings
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/opt/ashataiserver/data /opt/ashataiserver/models

# Environment variables
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://+:7077
Environment=ModelsDirectory=/opt/ashataiserver/models
Environment=Database__Path=/opt/ashataiserver/data/users.db

[Install]
WantedBy=multi-user.target
```

#### 2. Enable and Start the Service

```bash
# Reload systemd configuration
sudo systemctl daemon-reload

# Enable service to start at boot
sudo systemctl enable ashataiserver.service

# Start the service
sudo systemctl start ashataiserver.service

# Check status
sudo systemctl status ashataiserver.service
```

#### 3. Manage the Service

```bash
# View logs
sudo journalctl -u ashataiserver.service -f

# Stop service
sudo systemctl stop ashataiserver.service

# Restart service
sudo systemctl restart ashataiserver.service

# Disable auto-start
sudo systemctl disable ashataiserver.service
```

### Nginx Reverse Proxy

For production deployments, use Nginx as a reverse proxy:

#### 1. Install Nginx

```bash
sudo apt update
sudo apt install nginx
```

#### 2. Configure Nginx

```bash
sudo nano /etc/nginx/sites-available/ashataiserver
```

**Configuration file:**
```nginx
upstream ashataiserver {
    server localhost:7077;
    keepalive 32;
}

server {
    listen 80;
    server_name your-domain.com;

    # Redirect HTTP to HTTPS (optional)
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name your-domain.com;

    # SSL certificate configuration
    ssl_certificate /etc/ssl/certs/your-cert.crt;
    ssl_certificate_key /etc/ssl/private/your-key.key;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    # Security headers
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
    add_header X-Content-Type-Options nosniff;
    add_header X-Frame-Options DENY;
    add_header X-XSS-Protection "1; mode=block";

    # Proxy settings
    location / {
        proxy_pass http://ashataiserver;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }

    # Server-Sent Events (SSE) support
    location /api/ai/process/stream {
        proxy_pass http://ashataiserver;
        proxy_http_version 1.1;
        proxy_set_header Connection "";
        proxy_buffering off;
        proxy_cache off;
        proxy_read_timeout 86400s;
    }

    # Logging
    access_log /var/log/nginx/ashataiserver_access.log;
    error_log /var/log/nginx/ashataiserver_error.log;
}
```

#### 3. Enable the Configuration

```bash
# Create symbolic link
sudo ln -s /etc/nginx/sites-available/ashataiserver /etc/nginx/sites-enabled/

# Test configuration
sudo nginx -t

# Reload Nginx
sudo systemctl reload nginx
```

### Firewall Configuration (UFW)

```bash
# Allow HTTP
sudo ufw allow 80/tcp

# Allow HTTPS
sudo ufw allow 443/tcp

# Allow direct access (optional, if not using Nginx)
sudo ufw allow 7077/tcp

# Enable firewall
sudo ufw enable

# Check status
sudo ufw status
```

---

## Environment Variables

### Core Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Environment name (Development/Staging/Production) |
| `ASPNETCORE_URLS` | `http://+:7077` | Server URLs and ports |
| `ModelsDirectory` | `models` | Directory containing GGUF model files |
| `Database__Path` | `users.db` | SQLite database file path |

### TLS/HTTPS Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_Kestrel__Certificates__Default__Path` | - | Path to .pfx certificate file |
| `ASPNETCORE_Kestrel__Certificates__Default__Password` | - | Certificate password |

### Security Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `RateLimit__Enabled` | `true` | Enable rate limiting |
| `RateLimit__RequestsPerMinute` | `60` | Maximum requests per minute per IP |
| `RateLimit__RequestsPerHour` | `1000` | Maximum requests per hour per IP |
| `ApiKey__RequireForEndpoints` | `false` | Require API keys for specific endpoints |
| `ApiKey__ProtectedPaths` | - | Comma-separated list of protected paths |

### Runtime Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `LlamaCpp__ExecutablePath` | `llama-cli` | Path to llama.cpp executable |
| `Runtime__Type` | `MockRuntime` | Runtime type (MockRuntime/LlamaCppAdapter) |

### Logging Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `Logging__LogLevel__Default` | `Information` | Default log level |
| `Logging__LogLevel__Microsoft.AspNetCore` | `Warning` | ASP.NET Core log level |
| `Logging__LogLevel__ASHATAIServer` | `Information` | Application log level |

### CORS Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `Cors__AllowedOrigins` | `*` | Comma-separated list of allowed origins |

### Example .env File

```env
# Server Configuration
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:7443;http://+:7077

# Paths
ModelsDirectory=/app/models
Database__Path=/app/data/users.db

# TLS Configuration
ASPNETCORE_Kestrel__Certificates__Default__Path=/app/certs/certificate.pfx
ASPNETCORE_Kestrel__Certificates__Default__Password=YourSecurePassword

# Rate Limiting
RateLimit__Enabled=true
RateLimit__RequestsPerMinute=100
RateLimit__RequestsPerHour=5000

# API Key Protection
ApiKey__RequireForEndpoints=true
ApiKey__ProtectedPaths=/api/ai/process,/api/ai/generate-project

# Logging
Logging__LogLevel__Default=Information
Logging__LogLevel__Microsoft.AspNetCore=Warning

# CORS (restrict in production)
Cors__AllowedOrigins=https://yourdomain.com,https://app.yourdomain.com
```

---

## Production Checklist

### Security
- [ ] Enable HTTPS with valid TLS certificate
- [ ] Configure rate limiting (default is enabled)
- [ ] Enable API key authentication for sensitive endpoints
- [ ] Restrict CORS to specific origins
- [ ] Configure firewall rules
- [ ] Use strong passwords for admin accounts
- [ ] Regular security updates

### Performance
- [ ] Allocate sufficient CPU and RAM based on model size
- [ ] Configure appropriate rate limits
- [ ] Set up log rotation
- [ ] Monitor disk space for models and database
- [ ] Use SSD storage for better performance

### Reliability
- [ ] Configure automatic service restart (systemd/Windows Service)
- [ ] Set up database backups
- [ ] Configure health check monitoring
- [ ] Set up log aggregation and monitoring
- [ ] Document disaster recovery procedures

### Monitoring
- [ ] Health check endpoint configured
- [ ] Log monitoring configured
- [ ] Disk space monitoring
- [ ] CPU and memory monitoring
- [ ] API response time monitoring

### Documentation
- [ ] Document deployment environment
- [ ] Document configuration changes
- [ ] Document backup procedures
- [ ] Create runbook for common operations
- [ ] Document incident response procedures

---

## Troubleshooting

### Common Issues

#### Port Already in Use
```bash
# Linux: Find process using port 7077
sudo lsof -i :7077
sudo netstat -tulpn | grep 7077

# Kill the process
sudo kill -9 <PID>

# Windows: Find process using port 7077
netstat -ano | findstr :7077

# Kill the process
taskkill /PID <PID> /F
```

#### Permission Denied

**Linux:**
```bash
# Set execute permissions
chmod +x ASHATAIServer

# Check ownership
ls -l ASHATAIServer

# Fix ownership
sudo chown $USER:$USER ASHATAIServer
```

**Windows:**
```powershell
# Run as Administrator
Right-click start-server.bat -> Run as Administrator
```

#### Database Locked
```bash
# Check for multiple instances
ps aux | grep ASHATAIServer  # Linux
tasklist | findstr ASHATAIServer  # Windows

# Stop all instances and restart
```

#### Models Not Loading
```bash
# Check models directory exists
ls -la models/  # Linux
dir models\     # Windows

# Verify .gguf files
file models/*.gguf  # Linux

# Check permissions
chmod 755 models/         # Linux
chmod 644 models/*.gguf   # Linux

# Trigger manual scan
curl -X POST http://localhost:7077/api/ai/models/scan
```

#### TLS Certificate Issues
```bash
# Verify certificate file
openssl pkcs12 -info -in certificate.pfx

# Check certificate expiration
openssl pkcs12 -in certificate.pfx -nokeys | openssl x509 -noout -dates

# Generate new self-signed certificate (testing only)
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes
openssl pkcs12 -export -out certificate.pfx -inkey key.pem -in cert.pem
```

### Getting Help

- **Documentation**: See [README.md](../README.md) for API documentation
- **Security**: See [SECURITY.md](SECURITY.md) for security configuration
- **GitHub Issues**: [Report bugs or request features](https://github.com/buffbot88/AGP_AISERVER/issues)
- **Logs**: Check application logs for detailed error messages

### Debug Mode

Run in debug mode for detailed logging:

**Docker:**
```bash
docker run -e ASPNETCORE_ENVIRONMENT=Development ashataiserver:latest
```

**Binary:**
```bash
# Linux
ASPNETCORE_ENVIRONMENT=Development ./ASHATAIServer

# Windows
$env:ASPNETCORE_ENVIRONMENT="Development"
.\ASHATAIServer.exe
```

---

## Next Steps

After successful deployment:

1. **Test the API**: Use the examples in [README.md](../README.md)
2. **Configure Security**: Follow the [SECURITY.md](SECURITY.md) guide
3. **Set up Monitoring**: Configure health checks and logging
4. **Create Backups**: Set up automated database backups
5. **Document Your Setup**: Create internal documentation for your team

---

**ASHATAIServer Deployment Guide v1.0** | Built with ❤️ for ASHAT Goddess
