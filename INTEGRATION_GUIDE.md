# ASHAT Goddess - Integration Guide

This guide explains how to run ASHATGoddess with the local ASHATAIServer for the complete experience.

## 🌟 Overview

The ASHAT Goddess system consists of two applications:

1. **ASHATAIServer** - AI processing server that provides goddess-personality responses
2. **ASHATGoddess** - Desktop client with animated goddess mascot and chat interface

Both applications work together to create an immersive AI assistant experience with a Roman goddess personality.

## 🚀 Quick Start

### Step 1: Start the AI Server

First, start the ASHATAIServer on port 8088:

```bash
cd ASHATAIServer
dotnet run
```

You should see:
```
╔══════════════════════════════════════════════════════════╗
║         ASHATAIServer - AI Processing Server            ║
╚══════════════════════════════════════════════════════════╝

Server started on port: 8088
Server URL: http://localhost:8088
```

Leave this running in one terminal.

### Step 2: Launch the Goddess Client

In a new terminal, start the ASHATGoddess client:

```bash
cd ASHATGoddess
dotnet run
```

The goddess will automatically:
- Try to connect to the local server at `localhost:8088`
- Display an animated goddess mascot on your desktop
- Greet you with a divine message
- Be ready to chat!

## 💬 Using the Applications

### GUI Mode (Default)

When you run `dotnet run` in the ASHATGoddess directory:

1. **The Goddess Appears** - An animated goddess figure appears on your screen with:
   - Golden glowing effects
   - Laurel wreath crown
   - Flowing toga appearance
   - Animated breathing and reactions

2. **Chat Interface** - At the bottom of the window you can:
   - Type messages to ASHAT
   - Receive responses with goddess personality
   - See her animations react to the conversation

3. **Interactive Features**:
   - **Click** the goddess - She responds with a greeting
   - **Double-click** - Minimize/restore the chat interface
   - **Right-click** - Access options menu
   - **Drag** - Move her around your screen
   - **Space bar** - Trigger a greeting
   - **F key** - Fly to random location
   - **C key** - Center on screen
   - **ESC** - Exit

### Headless Mode

For running without GUI (servers, testing, etc.):

```bash
cd ASHATGoddess
dotnet run -- --headless
```

This starts an interactive console session where you can chat with ASHAT.

## 🔌 How They Connect

The ASHATGoddess client intelligently connects to servers:

1. **First**: Tries to connect to local ASHATAIServer at `localhost:8088`
2. **Fallback**: If local server unavailable, tries the configured external server
3. **Final Fallback**: If no servers available, uses built-in responses

This means:
- ✅ You get the best experience when both apps run together
- ✅ It still works if only one app is running
- ✅ No configuration changes needed!

## 📡 API Endpoints

The ASHATAIServer provides these endpoints:

### Health Check
```bash
GET http://localhost:8088/api/ai/health
```

Returns server status.

### Process AI Prompt
```bash
POST http://localhost:8088/api/ai/process
Content-Type: application/json

{
  "prompt": "Hello ASHAT!"
}
```

Returns goddess-personality response.

### Get Server Status
```bash
GET http://localhost:8088/api/ai/status
```

Shows loaded models and server information.

## 🎭 Goddess Personality

ASHAT responds with a Roman goddess personality that includes:

- **Divine Greetings**: "Salve, mortal! ✨"
- **Classical References**: References to Roman gods, philosophy, and culture
- **Playful Wisdom**: Combines ancient wisdom with modern helpfulness
- **Respectful Demeanor**: Always polite and encouraging
- **Mischievous Charm**: Occasional jokes and playful responses

Example interactions:
- **"Hello"** → "Salve, mortal! ✨ I am ASHAT, your divine companion from the pantheon of Rome..."
- **"Help"** → "Ah, you seek knowledge of my divine gifts! 🌟 I can: provide wisdom, share knowledge..."
- **"Thank you"** → "Your gratitude warms my divine heart like the eternal flame of Vesta! 💫"
- **"Goodbye"** → "Vale, dear mortal! 🏛️ May your path be lit by starlight..."

## 🛠️ Troubleshooting

### Goddess Doesn't Appear

The goddess visibility issue has been fixed. If you still have problems:

1. **Check Console Output**: Look for errors in the terminal
2. **Update Graphics Drivers**: Ensure your display drivers are current
3. **Window Transparency**: Some older systems may not support transparency
4. **Disable Other Mascots**: Close other desktop mascot applications

### No Connection to Server

If ASHATGoddess can't connect to ASHATAIServer:

1. **Verify Server is Running**: Check that `dotnet run` is active in ASHATAIServer
2. **Check Port 8088**: Make sure nothing else is using port 8088
   ```bash
   lsof -i :8088  # Linux/Mac
   netstat -ano | findstr :8088  # Windows
   ```
3. **Firewall**: Ensure your firewall allows local connections on port 8088

### No Sound (Windows)

On Windows, ASHAT uses System.Speech for voice:

1. **Check Audio Settings**: Ensure speakers/headphones are connected
2. **Volume**: Check system volume is not muted
3. **Console Fallback**: If sound fails, responses appear in the console

**Note**: On Linux/macOS, voice output is logged to console only (platform limitation).

## 🔄 Stopping the Applications

To properly stop both applications:

1. **Stop ASHATGoddess**:
   - GUI: Click "Exit" in right-click menu or press ESC
   - Headless: Type `exit` or press Ctrl+C

2. **Stop ASHATAIServer**:
   - Press Ctrl+C in the server terminal

## 🎨 Customization

### Visual Settings

You can customize the goddess appearance in `ASHATGoddess/appsettings.json`:

```json
{
  "Visual": {
    "EnableAnimations": true,
    "AnimationSpeed": 1.0,
    "GlowColor": "#8A2BE2",
    "AccentColor": "#FFD700",
    "EnableTransparency": true,
    "WindowOpacity": 0.95
  }
}
```

### Personality Settings

Adjust ASHAT's personality in `ASHATGoddess/appsettings.json`:

```json
{
  "Persona": {
    "Name": "ASHAT",
    "Personality": "wise, playful, mischievous, respectful",
    "SystemPrompt": "You are ASHAT, a Roman goddess AI assistant..."
  }
}
```

## 📚 Additional Resources

- **ASHATAIServer README**: See `ASHATAIServer/README.md` for server details
- **ASHATGoddess README**: See `ASHATGoddess/README.md` for client features
- **Visibility Fix**: See `ASHATGoddess/VISIBILITY_FIX_SUMMARY.md` for technical details
- **Advanced Features**: See `ASHATGoddess/ADVANCED_FEATURES.md` for game engine features

## 🧪 Testing

Run integration tests to verify everything works:

```bash
./test_integration.sh
```

This tests:
- Both applications build successfully
- Server starts and responds correctly
- Goddess personality responses work
- Headless mode functions properly

## 📝 License

See LICENSE file in the repository root.

## 🤝 Contributing

Both applications are actively developed. Feel free to report issues or contribute enhancements!

---

**Enjoy your divine companion! ✨🏛️**
