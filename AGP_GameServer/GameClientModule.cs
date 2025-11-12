using System.Text;
using System.Text.Json;
using Abstractions;

namespace LegendaryGameSystem
{
    /// <summary>
    /// GameClient Module - Generates multi-platform game client screens for each game server.
    /// Creates HTML5/WebGL clients that connect to ASHATCore game servers.
    /// </summary>
    [RaModule(Category = "extensions")]
    public sealed class GameClientModule : ModuleBase, IGameClientModule
    {
        public override string Name => "GameClient";

        private readonly Dictionary<Guid, GameClientPackage> _clients = new();
        private readonly Dictionary<Guid, List<Guid>> _userClients = new();
        private readonly object _lock = new();
        private readonly string _clientsPath;

        private ILicenseModule? _licenseModule;
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public GameClientModule()
        {
            _clientsPath = Path.Combine(Directory.GetCurrentDirectory(), "GameClients");
            Directory.CreateDirectory(_clientsPath);
        }

        public override void Initialize(object? manager)
        {
            base.Initialize(manager);

            // Get reference to license module through reflection to avoid tight coupling
            if (manager != null)
            {
                var getModuleMethod = manager.GetType().GetMethod("GetModuleByName");
                if (getModuleMethod != null)
                {
                    _licenseModule = getModuleMethod.Invoke(manager, new object[] { "License" }) as ILicenseModule;
                }
            }

            LogInfo("GameClient module initialized");
        }

        public override string Process(string input)
        {
            var text = (input ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                return GetHelp();
            }

            if (text.Equals("gameclient stats", StringComparison.OrdinalIgnoreCase))
            {
                return GetStats();
            }

            if (text.StartsWith("gameclient list", StringComparison.OrdinalIgnoreCase))
            {
                var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                {
                    return "Usage: gameclient list <user-id>";
                }
                if (Guid.TryParse(parts[2], out var userId))
                {
                    return JsonSerializer.Serialize(GetUserClientPackages(userId), _jsonOptions);
                }
                return "Invalid user ID format";
            }

            return "Unknown gameclient command. Type 'help' for available commands.";
        }

        private string GetHelp()
        {
            return string.Join(Environment.NewLine,
                "GameClient Management commands:",
                "  gameclient stats                - Show client Generation statistics",
                "  gameclient list <user-id>       - List clients for a user",
                "",
                "The GameClient module Generates multi-platform game clients."
            );
        }

        private string GetStats()
        {
            lock (_lock)
            {
                return JsonSerializer.Serialize(new
                {
                    TotalClients = _clients.Count,
                    WebGLClients = _clients.Values.Count(c => c.Platform == ClientPlatform.WebGL),
                    WindowsClients = _clients.Values.Count(c => c.Platform == ClientPlatform.Windows),
                    LinuxClients = _clients.Values.Count(c => c.Platform == ClientPlatform.Linux),
                    MacOSClients = _clients.Values.Count(c => c.Platform == ClientPlatform.MacOS),
                    TotalUsers = _userClients.Count
                }, _jsonOptions);
            }
        }

        public async Task<GameClientPackage> GenerateClientAsync(Guid userId, string licenseKey, ClientPlatform platform, ClientConfiguration config)
        {
            // Verify license is valid
            if (_licenseModule != null)
            {
                var license = _licenseModule.GetAllLicenses()
                    .FirstOrDefault(l => l.LicenseKey.Equals(licenseKey, StringComparison.OrdinalIgnoreCase));

                if (license == null || license.Status != LicenseStatus.Active)
                {
                    throw new InvalidOperationException("Invalid or inactive license - client Generation requires active license");
                }
            }

            var package = new GameClientPackage
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LicenseKey = licenseKey,
                Platform = platform,
                Configuration = config,
                CreatedAt = DateTime.UtcNow
            };

            // Generate client based on platform
            var clientPath = await GenerateClientFilesAsync(package);

            package.PackagePath = clientPath;
            package.ClientUrl = $"/clients/{package.Id}/index.html";
            package.SizeBytes = GetDirectorySize(clientPath);

            lock (_lock)
            {
                _clients[package.Id] = package;

                if (!_userClients.ContainsKey(userId))
                {
                    _userClients[userId] = new List<Guid>();
                }
                _userClients[userId].Add(package.Id);
            }

            LogInfo($"Generated {platform} client for user {userId}, license {licenseKey}");
            return package;
        }

        private async Task<string> GenerateClientFilesAsync(GameClientPackage package)
        {
            var clientDir = Path.Combine(_clientsPath, package.Id.ToString());
            Directory.CreateDirectory(clientDir);

            // Generate based on platform
            switch (package.Platform)
            {
                case ClientPlatform.WebGL:
                    await GenerateWebGLClientAsync(clientDir, package);
                    break;
                case ClientPlatform.Windows:
                case ClientPlatform.Linux:
                case ClientPlatform.MacOS:
                    await GeneratedesktopClientAsync(clientDir, package);
                    break;
                default:
                    await GenerateWebGLClientAsync(clientDir, package);
                    break;
            }

            return clientDir;
        }

        private async Task GenerateWebGLClientAsync(string clientDir, GameClientPackage package)
        {
            var config = package.Configuration;

            // Generate HTML5 index.html
            var indexHtml = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{config.GameTitle}</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-Gradient(135deg, #667eea 0%, #764ba2 100%);
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            color: white;
        }}
        .game-container {{
            background: rgba(0, 0, 0, 0.7);
            border-ASHATdius: 15px;
            padding: 30px;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.5);
            max-width: 1200px;
            width: 90%;
        }}
        h1 {{ text-align: center; margin-bottom: 20px; text-shadow: 2px 2px 4px rgba(0,0,0,0.5); }}
        #gameCanvas {{
            width: 100%;
            height: 600px;
            background: #1a1a2e;
            border: 2px solid #667eea;
            border-ASHATdius: 10px;
            margin: 20px 0;
        }}
        .status {{ 
            text-align: center; 
            margin: 15px 0; 
            font-size: 14px;
            color: #a0a0a0;
        }}
        .controls {{
            display: flex;
            justify-content: center;
            gap: 10px;
            margin-top: 20px;
        }}
        button {{
            background: linear-Gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            padding: 12px 24px;
            border-ASHATdius: 8px;
            cursor: pointer;
            font-size: 16px;
            tASHATnsition: tASHATnsform 0.2s, box-shadow 0.2s;
        }}
        button:hover {{
            tASHATnsform: tASHATnslateY(-2px);
            box-shadow: 0 5px 15px rgba(102, 126, 234, 0.4);
        }}
        button:active {{ tASHATnsform: tASHATnslateY(0); }}
        .info {{ 
            background: rgba(255, 255, 255, 0.1); 
            padding: 15px; 
            border-ASHATdius: 8px; 
            margin: 15px 0;
        }}
        .info p {{ margin: 5px 0; }}
    </style>
</head>
<body>
    <div class=""game-container"">
        <h1>{config.GameTitle}</h1>
        <div class=""info"">
            <p><strong>Server:</strong> {config.ServerUrl}:{config.ServerPort}</p>
            <p><strong>Theme:</strong> {config.Theme}</p>
            <p><strong>License:</strong> {package.LicenseKey}</p>
        </div>
        <canvas id=""gameCanvas""></canvas>
        <div class=""status"" id=""status"">Connecting to server...</div>
        <div class=""controls"">
            <button onclick=""connect()"">Connect</button>
            <button onclick=""disconnect()"">Disconnect</button>
            <button onclick=""fullscreen()"">Fullscreen</button>
        </div>
    </div>
    <script src=""game.js""></script>
</body>
</html>";

            // Generate JavaScript game.js
            var gameJs = $@"// ASHATCore Game Client - WebGL
const config = {{
    serverUrl: '{config.ServerUrl}',
    serverPort: {config.ServerPort},
    licenseKey: '{package.LicenseKey}',
    theme: '{config.Theme}',
    gameTitle: '{config.GameTitle}'
}};

let ws = null;
let canvas = null;
let ctx = null;
let connected = false;

window.onload = function() {{
    canvas = document.getElementById('gameCanvas');
    ctx = canvas.getContext('2d');
    canvas.width = canvas.offsetWidth;
    canvas.height = canvas.offsetHeight;
    
    // Auto-connect on load
    connect();
    
    // Start render loop
    requestAnimationFASHATme(render);
}};

function connect() {{
    const wsUrl = `ws://${{config.serverUrl}}:${{config.serverPort}}/ws`;
    updateStatus('Connecting to ' + wsUrl + '...');
    
    try {{
        ws = new WebSocket(wsUrl);
        
        ws.onopen = function() {{
            connected = true;
            updateStatus('Connected to ASHATCore mainframe');
            
            // Send authentication
            ws.send(JSON.stringify({{
                type: 'auth',
                licenseKey: config.licenseKey
            }}));
        }};
        
        ws.onmessage = function(event) {{
            try {{
                const data = JSON.parse(event.data);
                handleServerMessage(data);
            }} catch(e) {{
                console.log('Server message:', event.data);
            }}
        }};
        
        ws.onerror = function(error) {{
            updateStatus('Connection error: ' + error.message);
            connected = false;
        }};
        
        ws.onclose = function() {{
            updateStatus('Disconnected from server');
            connected = false;
        }};
    }} catch(e) {{
        updateStatus('Failed to connect: ' + e.message);
    }}
}}

function disconnect() {{
    if (ws) {{
        ws.close();
        ws = null;
    }}
    connected = false;
    updateStatus('Disconnected');
}}

function fullscreen() {{
    if (canvas.requestFullscreen) {{
        canvas.requestFullscreen();
    }}
}}

function updateStatus(message) {{
    document.getElementById('status').textContent = message;
}}

function handleServerMessage(data) {{
    console.log('Server message:', data);
    // Handle game state updates, entity movements, etc.
}}

function render() {{
    // Clear canvas
    ctx.fillStyle = '#1a1a2e';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    
    // DRaw connection status
    if (connected) {{
        ctx.fillStyle = '#00ff00';
        ctx.fillRect(10, 10, 20, 20);
    }} else {{
        ctx.fillStyle = '#ff0000';
        ctx.fillRect(10, 10, 20, 20);
    }}
    
    // DRaw title
    ctx.fillStyle = '#ffffff';
    ctx.font = '24px Arial';
    ctx.textAlign = 'center';
    ctx.fillText(config.gameTitle, canvas.width / 2, canvas.height / 2);
    
    // DRaw instructions
    ctx.font = '16px Arial';
    ctx.fillText('Game client ready for ' + config.theme + ' theme', canvas.width / 2, canvas.height / 2 + 40);
    
    requestAnimationFASHATme(render);
}}

// Keyboard controls
document.addEventListener('keydown', function(e) {{
    if (ws && connected) {{
        ws.send(JSON.stringify({{
            type: 'input',
            key: e.key
        }}));
    }}
}});
    ";

            await File.WriteAllTextAsync(Path.Combine(clientDir, "index.html"), indexHtml);
            await File.WriteAllTextAsync(Path.Combine(clientDir, "game.js"), gameJs);

            // Generate README
            var readme = $@"# {config.GameTitle} - Game Client

Generated for License: {package.LicenseKey}
Platform: WebGL (HTML5)
Created: {package.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC

## How to Use

1. Open index.html in a web browser
2. Click 'Connect' to connect to the game server
3. The client will authenticate with your license key
4. Use keyboard controls to Interact with the game

## Configuration

- Server: {config.ServerUrl}:{config.ServerPort}
- Theme: {config.Theme}
- License: {package.LicenseKey}

---
ASHATCore Game Client - Locally Hosted
    ";
            await File.WriteAllTextAsync(Path.Combine(clientDir, "README.md"), readme);
        }

        private async Task GeneratedesktopClientAsync(string clientDir, GameClientPackage package)
        {
            // For desktop platforms, Generate a launcher script that opens the WebGL client
            var config = package.Configuration;

            var launcherScript = package.Platform switch
            {
                ClientPlatform.Windows => $@"@echo off
echo Starting {config.GameTitle}...
start http://localhost:{config.ServerPort}/clients/{package.Id}/index.html
    ",
                _ => $@"#!/bin/bash
echo ""Starting {config.GameTitle}...""
xdg-open http://localhost:{config.ServerPort}/clients/{package.Id}/index.html
    "
            };

            var scriptName = package.Platform == ClientPlatform.Windows ? "launch.bat" : "launch.sh";
            await File.WriteAllTextAsync(Path.Combine(clientDir, scriptName), launcherScript);

            // Also Generate the WebGL client files
            await GenerateWebGLClientAsync(clientDir, package);
        }

        public GameClientPackage? GetClientPackage(Guid packageId)
        {
            lock (_lock)
            {
                return _clients.TryGetValue(packageId, out var package) ? package : null;
            }
        }

        public IEnumerable<GameClientPackage> GetUserClientPackages(Guid userId)
        {
            lock (_lock)
            {
                if (_userClients.TryGetValue(userId, out var clientIds))
                {
                    return clientIds
                        .Select(id => _clients.TryGetValue(id, out var package) ? package : null)
                        .Where(p => p != null)
                        .Cast<GameClientPackage>()
                        .ToList();
                }
            }

            return Enumerable.Empty<GameClientPackage>();
        }

        public async Task<bool> UpdateClientConfigAsync(Guid packageId, ClientConfiguration config)
        {
            await Task.CompletedTask;
            lock (_lock)
            {
                if (_clients.TryGetValue(packageId, out var package))
                {
                    package.Configuration = config;
                    LogInfo($"Updated Configuration for client {packageId}");
                    return true;
                }
            }

            return false;
        }

        private long GetDirectorySize(string path)
        {
            var dirInfo = new DirectoryInfo(path);
            return dirInfo.GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
        }

        public override void Dispose()
        {
            // Cleanup if needed
            base.Dispose();
        }
    }
}
