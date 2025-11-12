# AGP Studios IDE - Quick Start Guide

Get up and running with AGP Studios IDE in minutes!

## Prerequisites

Before you begin, ensure you have:
- ✅ Windows 10 or later
- ✅ .NET 8.0 SDK ([Download here](https://dotnet.microsoft.com/download/dotnet/8.0))
- ✅ Visual Studio 2022 (optional, for building from source)

## Installation

### Option 1: Build from Source (Recommended)

1. **Clone the repository:**
   ```bash
   git clone https://github.com/buffbot88/AGP_Studios.git
   cd AGP_Studios
   ```

2. **Build the application:**
   ```bash
   dotnet build --configuration Release
   ```

3. **Run the application:**
   ```bash
   cd AGP_Studios.IDE/bin/Release/net8.0-windows
   AGP_Studios.IDE.exe
   ```

### Option 2: Open in Visual Studio

1. Clone the repository (as above)
2. Open `AGP_Studios.sln` in Visual Studio 2022
3. Press `F5` to build and run

## First Launch

### 1. Configure AI Server

On first launch, you'll see the login window:

![Login Window](docs/images/login-window.png)

- **Default Server URL:** `http://localhost:8088`
- Modify if your AI Server is on a different host/port
- The configuration will be saved for future use

### 2. Login

Enter your phpBB3 credentials:
- **Username:** Your forum username
- **Password:** Your forum password
- Click **Sign In**

### 3. Auto-Routing

After successful login, you'll be automatically routed based on your role:

#### If you're an **Admin** 👨‍💼
You'll see the **AI Console** where you can:
- Create and edit code drafts
- Save drafts locally
- Publish code to AI Server
- Access AI code generation (placeholder)

#### If you're a **Member** 👤
You'll see the **Game Library** where you can:
- Browse available games
- Download and install games
- Play installed games

## Using the Admin Console

### Creating Your First Draft

1. Click **New Draft** button
2. Enter a name for your draft
3. Start coding in the AvalonEdit editor
4. Click **Save Draft** to save locally

### Publishing Code

1. Ensure your draft is saved
2. Review your code
3. Click **Publish to Server**
4. Confirm the publish action

Your code will be uploaded to the AI Server!

### Managing Drafts

- **Left Panel:** Shows all your local drafts
- **Click a draft:** To load it in the editor
- **Drafts are stored:** In `%AppData%/AGP_IDE/Drafts`

## Using the Game Library

### Browsing Games

1. **Available Games Tab:** Shows all published games
2. Each game card displays:
   - Game name
   - Description
   - Version
   - Author

### Installing a Game

1. Find a game you want to play
2. Click **Download & Install**
3. Wait for the download to complete
4. Progress bar shows download status

### Playing Games

1. Switch to **Installed Games** tab
2. Find your installed game
3. Click **▶ Play**
4. The game will launch in a new window

Games are stored in `%AppData%/AGP_IDE/Games`

## Configuration

### Config File Location
`%AppData%/AGP_IDE/config.json`

### Default Configuration
```json
{
  "ServerUrl": "http://localhost:8088",
  "ServerPort": 8088,
  "AppDataPath": "C:\\Users\\{You}\\AppData\\Roaming\\AGP_IDE",
  "DraftsPath": "C:\\Users\\{You}\\AppData\\Roaming\\AGP_IDE\\Drafts",
  "GamesPath": "C:\\Users\\{You}\\AppData\\Roaming\\AGP_IDE\\Games"
}
```

### Modifying Configuration

You can modify the server URL in two ways:
1. **Via UI:** Change in login window
2. **Via File:** Edit `config.json` directly

## Troubleshooting

### Cannot Connect to Server
**Problem:** Login fails with connection error

**Solutions:**
- ✅ Verify AI Server is running
- ✅ Check server URL and port
- ✅ Ensure firewall allows connection
- ✅ Try `http://localhost:8088` for local server

### Games Won't Download
**Problem:** Download button doesn't work

**Solutions:**
- ✅ Check internet connection
- ✅ Verify server is accessible
- ✅ Check disk space in `%AppData%/AGP_IDE/Games`
- ✅ Restart application

### Game Won't Launch
**Problem:** Play button shows error

**Solutions:**
- ✅ Verify game files exist in install folder
- ✅ Check if executable was found during installation
- ✅ Ensure DirectX/OpenGL runtime is installed
- ✅ Check Windows Defender/Antivirus isn't blocking
- ✅ Try running as administrator

### Drafts Not Saving
**Problem:** Save button doesn't work

**Solutions:**
- ✅ Check write permissions for `%AppData%/AGP_IDE`
- ✅ Ensure sufficient disk space
- ✅ Try saving with a different name
- ✅ Restart application

## Keyboard Shortcuts

### General
- `Enter` - Confirm/Login (on login screen)
- `Esc` - Cancel operations

### Admin Console
- `Ctrl+S` - Save draft (coming soon)
- `Ctrl+N` - New draft (coming soon)
- `Ctrl+O` - Open draft (coming soon)

## Tips & Tricks

### For Admins 👨‍💼

1. **Save Frequently:** Drafts are saved locally, so save often!
2. **Use Descriptive Names:** Name your drafts clearly
3. **Test Before Publishing:** Review your code before publishing
4. **Organize Drafts:** Delete old drafts you no longer need

### For Members 👤

1. **Check Game Info:** Read description before downloading
2. **Monitor Disk Space:** Games can be large
3. **Update Games:** Re-download when new versions are available
4. **Report Issues:** Contact admins if games don't work

## Getting Help

### Resources
- 📖 **Full Documentation:** See [README.md](README.md)
- 🤝 **Contributing:** See [CONTRIBUTING.md](CONTRIBUTING.md)
- 🐛 **Report Issues:** [GitHub Issues](https://github.com/buffbot88/AGP_Studios/issues)

### Community
- Join the phpBB3 forum
- Connect with other developers
- Share your games and code

## Next Steps

Now that you're set up:

### As an Admin
1. ✅ Create your first code draft
2. ✅ Explore the AvalonEdit features
3. ✅ Publish something to the server
4. ✅ Check out AI generation (when available)

### As a Member
1. ✅ Browse the game library
2. ✅ Download your first game
3. ✅ Play and enjoy!
4. ✅ Share feedback with developers

## Need More Help?

- Review the full [README.md](README.md)
- Check [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md) for technical details
- Visit the AGP Studios forum
- Open an issue on GitHub

---

**Welcome to AGP Studios IDE!** 🎮

We hope you enjoy creating and playing games on our platform.

*Happy coding and gaming!*
