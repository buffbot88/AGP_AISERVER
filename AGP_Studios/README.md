# AGP Studios IDE - Windows Client

A Windows WPF IDE client for AGP Studios with AI Server integration, enabling administrators to create and publish code drafts, and members to browse, download, and play games.

## Features

### For Administrators
- **AI Console**: Full-featured code editor with syntax highlighting (AvalonEdit)
- **Draft Management**: Save, load, and manage code drafts locally
- **Local Storage**: Drafts stored in `%AppData%/AGP_IDE/Drafts`
- **Publishing**: Upload code/packages to AI Server
- **AI Integration**: Placeholder for AI code generation features

### For Members
- **Game Library**: Browse published games from AI Server
- **Download & Install**: Download game packages and automatically extract
- **Game Launcher**: Launch installed games with DirectX11/OpenGL support
- **Local Storage**: Games stored in `%AppData%/AGP_IDE/Games`

### Authentication & Routing
- **phpBB3 Login**: Authenticate using phpBB3 credentials via AI Server
- **Role-Based Routing**: Automatically route users to Admin Console or Game Library based on permissions
- **Secure Token Storage**: Bearer token authentication for API requests

### Configuration
- **Configurable Server**: Default AI Server URL: `http://localhost:8088`
- **Settings Persistence**: Configuration saved in `%AppData%/AGP_IDE/config.json`

## Prerequisites

- Windows 10 or later
- .NET 8.0 SDK or later
- Visual Studio 2022 (recommended) or VS Code with C# extensions

## Building the Application

### Using Visual Studio

1. Clone the repository:
   ```bash
   git clone https://github.com/buffbot88/AGP_Studios.git
   cd AGP_Studios
   ```

2. Open `AGP_Studios.sln` in Visual Studio 2022

3. Restore NuGet packages:
   - Right-click solution → Restore NuGet Packages
   - Or use: `dotnet restore`

4. Build the solution:
   - Press `Ctrl+Shift+B`
   - Or use: `dotnet build`

5. Run the application:
   - Press `F5` to debug
   - Or press `Ctrl+F5` to run without debugging

### Using Command Line

```bash
# Navigate to the repository
cd AGP_Studios

# Restore dependencies
dotnet restore

# Build the project
dotnet build --configuration Release

# Run the application
dotnet run --project AGP_Studios.IDE/AGP_Studios.IDE.csproj
```

## Project Structure

```
AGP_Studios.IDE/
├── Models/                  # Data models
│   ├── AppConfiguration.cs  # Configuration model
│   ├── User.cs             # User model
│   ├── AuthModels.cs       # Authentication DTOs
│   ├── Game.cs             # Game and installation models
│   └── CodeDraft.cs        # Code draft model
├── Services/               # Business logic and API
│   ├── ConfigurationManager.cs  # Settings management
│   ├── ApiClient.cs        # AI Server API client
│   ├── LocalRepository.cs  # Local storage for drafts/games
│   └── GameServices.cs     # Game download/launch services
├── UI/                     # User interface
│   └── Windows/
│       ├── LoginWindow.xaml           # Login screen
│       ├── AdminConsoleWindow.xaml    # Admin code editor
│       └── GameLibraryWindow.xaml     # Member game library
├── App.xaml                # Application resources and styles
└── AGP_Studios.IDE.csproj  # Project file
```

## API Endpoints

The application integrates with the AGP AI Server using the following endpoints:

### Authentication
- `POST /api/auth/login` - Login with phpBB3 credentials
  ```json
  Request: { "Username": "user", "Password": "pass" }
  Response: { "Success": true, "Token": "bearer_token" }
  ```

- `GET /api/user/me` - Get current user information
  ```json
  Response: { "UserId": 1, "Username": "user", "Email": "user@example.com", "IsAdmin": true }
  ```

### Games (Member Access)
- `GET /api/games` - List published games
  ```json
  Response: [
    {
      "Id": 1,
      "Name": "Game Name",
      "Description": "Description",
      "Version": "1.0.0",
      "Author": "Author Name",
      "DownloadUrl": "http://server/files/game.zip",
      "SizeBytes": 1048576,
      "PublishedDate": "2025-01-01T00:00:00Z"
    }
  ]
  ```

### Admin Operations
- `POST /api/admin/publish` - Publish code/package (admin only)
  ```json
  Request: { "Name": "Project Name", "Content": "code content" }
  Response: { "Success": true }
  ```

## Configuration

Configuration is stored in `%AppData%/AGP_IDE/config.json`:

```json
{
  "ServerUrl": "http://localhost:8088",
  "ServerPort": 8088,
  "AppDataPath": "C:\\Users\\{User}\\AppData\\Roaming\\AGP_IDE",
  "DraftsPath": "C:\\Users\\{User}\\AppData\\Roaming\\AGP_IDE\\Drafts",
  "GamesPath": "C:\\Users\\{User}\\AppData\\Roaming\\AGP_IDE\\Games"
}
```

You can modify the server URL in the login window or by editing this file directly.

## Local Storage

### Drafts (Admin)
- Location: `%AppData%/AGP_IDE/Drafts/`
- Format: JSON files (`{DraftId}.json`)
- Each draft contains: ID, Name, Content, Language, timestamps

### Games (Member)
- Location: `%AppData%/AGP_IDE/Games/`
- Structure:
  - `Game_{GameId}/` - Extracted game files
  - `{GameId}_install.json` - Installation metadata

## Usage

### For Administrators

1. Launch the application
2. Enter AI Server URL (default: `http://localhost:8088`)
3. Login with phpBB3 credentials
4. Admin Console will open automatically
5. Create/edit code drafts using the AvalonEdit editor
6. Save drafts locally or publish to AI Server
7. AI code generation features are placeholders for future integration

### For Members

1. Launch the application
2. Login with phpBB3 credentials
3. Game Library will open automatically
4. Browse available games in the "Available Games" tab
5. Click "Download & Install" to download a game
6. Once installed, switch to "Installed Games" tab
7. Click "▶ Play" to launch a game

## Development Notes

### Dependencies

- **AvalonEdit** (6.3.0.90): Code editor component with syntax highlighting
- **Newtonsoft.Json** (13.0.3): JSON serialization for config and storage
- **System.Net.Http.Json** (8.0.0): HTTP client helpers

### Security Considerations

- **Production Deployment**: Use HTTPS for AI Server connections
- **Token Storage**: Consider encrypting tokens for production use
- **Input Validation**: Validate all user inputs before API calls
- **File Extraction**: Implement additional security checks for downloaded packages

### Future Enhancements

- AI code generation integration with AI Server endpoints
- Real-time collaboration features
- Game patching and update system
- Enhanced game launcher with configuration options
- User profile management
- Community features (ratings, reviews, comments)

## Testing AI Server Connection

To test without a running AI Server, you can:

1. Mock the endpoints using tools like Postman or create a test server
2. Modify the `ApiClient.cs` to return sample data
3. Use the local storage features (drafts, game library) which work offline

## Troubleshooting

### Cannot Connect to AI Server
- Verify the AI Server is running on the configured URL/port
- Check firewall settings
- Ensure the URL in the login window is correct

### Games Won't Launch
- Verify the game executable exists in the installation directory
- Check that the game requires DirectX11/OpenGL and drivers are installed
- Review Windows security settings (antivirus/firewall)

### Drafts Not Saving
- Check write permissions for `%AppData%/AGP_IDE`
- Ensure sufficient disk space
- Review Windows Event Viewer for errors

## Related Repositories

- [AGP_AISERVER](https://github.com/buffbot88/AGP_AISERVER) - AI Server backend (C#, PHP, phpBB3)
- [AGP_Studios](https://github.com/buffbot88/AGP_Studios) - This repository (C#, Python)

## License

Copyright © 2025 AGP Studios. All rights reserved.

## Contributors

- @buffbot88 - Project Creator
- GitHub Copilot - Code Assistant

---

For more information or support, please visit the repository or contact the maintainers.
