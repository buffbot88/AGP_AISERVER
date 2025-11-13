# AGP IDE Client (Windows) — Skeleton

This is a Windows WPF IDE client for connecting to an AI Server (default: http://localhost:7077).

Features
- Login and registration through the AI Server with local user database
- Admin route → AI Console for code creation and draft management
- Member route → Library of published games, download & launch
- Local draft storage (in %AppData%/AGP_IDE)
- Configurable server URL and port

Requirements
- .NET 7.0 (or .NET 6.0) SDK
- Visual Studio 2022/2023 or `dotnet` CLI
- NuGet packages:
  - Newtonsoft.Json
  - AvalonEdit (for the editor in Admin Console) — optional but recommended

Quick start
1. Open the solution in Visual Studio or run:
   dotnet restore
   dotnet build
2. Edit `config.json` (default server: http://localhost:7077)
3. Run the app. Register a new account or login with existing credentials.
4. If you are admin, you'll be taken to the AI Console. If member, to the Game Library.

API expectations (on AI Server)
- POST /api/auth/login
  Body: { "username": "...", "password": "..." }
  Response: { "token": "JWT or session token" }

- POST /api/auth/register
  Body: { "username": "...", "email": "...", "password": "..." }
  Response: { "token": "JWT or session token" }

- GET /api/user/me
  Headers: Authorization: Bearer <token>
  Response: { "username":"...", "isAdmin": true|false }

- GET /api/games
  Headers: Authorization: Bearer <token>
  Response: [ { "id":"", "title":"", "version":"", "downloadUrl":"", "description":"" }, ... ]

- POST /api/admin/publish
  Headers: Authorization: Bearer <token>
  Body: { "title":"...", "package": <multipart/form-data> }

Adjust endpoints as needed to match your server.

Local storage
- Drafts: %AppData%\AGP_IDE\drafts\
- Downloads: %AppData%\AGP_IDE\games\

Notes & Next steps
- Wire the app to your AI generation endpoints (for admin AI-based code creation).
- For live code editing & compilation, integrate Roslyn (C#) — or for multi-language support, provide build toolchains.
- For embedding editor features, use AvalonEdit or Monaco (via WebView2).
- For launching games with DirectX11/OpenGL, the client launches the downloaded executable as a separate process.

  Current API Enpoints for gameserver;

  Available Endpoints:
  Authentication:
    POST /api/auth/login     - User login
    POST /api/auth/register  - User registration

  AI Services:
    POST /api/ai/process     - Process AI prompts
    GET  /api/ai/status      - Get model status
    POST /api/ai/models/scan - Scan for models
    GET  /api/ai/health      - Health check

  Game Server:
    GET  /api/gameserver/status              - Game server status
    GET  /api/gameserver/capabilities        - Server capabilities
    POST /api/gameserver/create              - Create new game
    POST /api/gameserver/create-ai-enhanced  - Create game with AI
    GET  /api/gameserver/projects            - List all projects
    POST /api/gameserver/deploy/{id}         - Deploy game
    GET  /api/gameserver/suggestions/{id}    - Get AI suggestions
