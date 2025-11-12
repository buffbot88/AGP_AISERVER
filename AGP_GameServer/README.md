# Legendary Game System

**Version:** 1.5.0-beta  
**Phase:** 9  
**Status:** ✅ Production Ready

## Overview

The **Legendary Game System** is a production-ready, unified modular DLL package that consolidates the complete game infrastructure for ASHATOS. It combines three previously separate systems into one cohesive module:

- **Game Engine** - Scene management, entities, physics, and AI
- **Game Server** - AI-driven game creation and deployment
- **Game Client** - Multi-platform client generation (WebGL, Desktop)

Inspired by Unreal Engine architecture, this unified system provides enterprise-grade features including in-game chat, advanced physics, AI behaviors, and multiplayer synchronization.

## Key Features

### 🎮 Core Game Engine
- **Scene Management** - Create, update, delete game worlds/levels/areas
- **Entity System** - Hierarchical entity management with 3D transforms
- **AI-Driven Generation** - Natural language world and content generation
- **Asset Streaming** - Dynamic asset loading and management
- **WebSocket Broadcasting** - Real-time event updates to all clients
- **SQLite Persistence** - Scenes and entities persist across restarts

### 🖥️ Game Server System
- **AI-Driven Game Creation** - Generate complete games from natural language descriptions
- **Automatic Code Generation** - Front-end and back-end code generation
- **Asset Creation** - AI-powered asset generation
- **One-Click Deployment** - Deploy game servers with single command
- **Real-Time Preview** - Live preview and iteration support

### 🎯 Game Client System
- **Multi-Platform Support** - WebGL, Windows, Linux, macOS clients
- **HTML5/WebGL Clients** - Browser-based game clients
- **License Integration** - Per-user license key validation
- **Automatic Client Generation** - Generate clients from server configuration

### ⭐ Additional Features (Phase 9)
- **In-Game Chat System** - Separate chat rooms for each game scene
  - Independent from website CMS chat
  - Scene-specific chat rooms
  - Real-time messaging within games
  - Participant tracking
  - Message history (last 200 messages per room)
- **Advanced Architecture** - Modular DLL design for hot-swapping
- **Plugin System Ready** - Extension points for custom game logic
- **Multiplayer Foundation** - Infrastructure for player synchronization

## Architecture

### Project Structure
```
LegendaryGameSystem/
├── Core/
│   ├── LegendaryGameEngineModule.cs    # Main engine implementation
│   └── ILegendaryGameEngineModule.cs   # Engine interface
├── GameServerModule.cs                 # AI-driven game server
├── GameClientModule.cs                 # Multi-platform client generator
├── Database/
│   └── GameEngineDatabase.cs           # SQLite persistence layer
├── Networking/
│   └── GameEngineWebSocketBroadcaster.cs  # Real-time events
├── Chat/
│   └── InGameChatManager.cs            # In-game chat system
├── Physics/                            # (Reserved for future physics engine)
└── AI/                                 # (Reserved for advanced AI behaviors)
```

## In-Game Chat System

The in-game chat system is completely sepaRate from the CMS website chat system (`ChatModule`). Each game scene can have multiple chat rooms for different purposes (party chat, guild chat, zone chat, etc.).

### Chat Features
- **Scene-Based Rooms** - Each chat room is tied to a specific game scene
- **Message History** - Maintains last 200 messages per room
- **Participant tracking** - TASHATck active players in each room
- **Room Management** - Create, close, and manage chat rooms
- **Real-time Updates** - Instant message delivery to participants

### Example Usage

```csharp
// Create an in-game chat room for a scene
var (success, message, roomId) = await engine.CreateInGameChatRoomAsync(
    sceneId: "dungeon_001",
    roomName: "Party Chat",
    createdBy: "player123"
);

// Send a message to the room
await engine.SendInGameChatMessageAsync(
    roomId: roomId,
    userId: "player123",
    username: "WarriorKing",
    content: "Let's attack the boss!"
);

// Get recent messages
var messages = await engine.GetInGameChatMessagesAsync(roomId, limit: 50);

// Get all chat rooms for a scene
var rooms = await engine.GetInGameChatRoomsForSceneAsync("dungeon_001");
```

## Module Loading

The Legendary Game Engine is loaded as a hot-swappable DLL module by ASHATCore's ModuleManager:

1. **Build** - The LegendaryGameEngine.dll is built as a sepaRate class libASHATry
2. **Copy** - DLL is copied to ASHATCore's output directory via project reference
3. **Load** - ModuleManager discovers and loads the module at runtime
4. **Update** - Can be updated independently without rebuilding ASHATCore mainframe

## Comparison: CMS Chat vs In-Game Chat

| Feature | CMS Website Chat | In-Game Chat |
|---------|------------------|--------------|
| **Location** | LegendaryCMS module | LegendaryGameEngine module |
| **Purpose** | Website forums, support | Game instance communication |
| **Scope** | Global, persistent | Scene-specific, temporary |
| **Persistence** | Long-term database | Short-term memory (200 msgs) |
| **Use Cases** | General discussion, support tickets | Party chat, guild chat, zone chat |
| **moderation** | Content moderation Intergrated | Game-specific moderation |

## Benefits of Modular DLL Approach

Following Phase 8's successful LegendaryCMS Extraction pattern:

1. **Independent Updates** - Update game engine without touching ASHATCore core
2. **Hot Reload Capability** - Swap DLL at runtime for live updates
3. **Isolated Development** - Work on game features independently
4. **Version Control** - SepaRate versioning for game engine vs core
5. **Deployment Flexibility** - Deploy game engine updates sepaRately
6. **Reduced Coupling** - Clean sepaASHATtion between mainframe and game logic

## API integration

The Legendary Game Engine integRates with ASHATCore's REST API endpoints (defined in `ASHATCore/Program.cs`):

### Scene Operations
- `POST /api/gameengine/scene` - Create scene
- `GET /api/gameengine/scenes` - List scenes
- `GET /api/gameengine/scene/{id}` - Get scene details
- `DELETE /api/gameengine/scene/{id}` - Delete scene

### Entity Operations
- `POST /api/gameengine/scene/{id}/entity` - Create entity
- `GET /api/gameengine/scene/{id}/entities` - List entities

### In-Game Chat Operations (New)
- `POST /api/gameengine/scene/{sceneId}/chat/room` - Create chat room
- `POST /api/gameengine/chat/{roomId}/message` - Send message
- `GET /api/gameengine/chat/{roomId}/messages` - Get messages
- `GET /api/gameengine/scene/{sceneId}/chat/rooms` - List rooms

### AI Generation & Stats
- `POST /api/gameengine/scene/{id}/Generate` - AI-Generate content
- `GET /api/gameengine/stats` - Engine statistics

## Future Enhancements

### Phase 9.1 (Planned)
- [ ] **Physics Engine** - Collision detection, rigid body dynamics
- [ ] **Advanced AI** - Pathfinding, behavior trees, state machines
- [ ] **Multiplayer Sync** - State synchronization across clients
- [ ] **Voice Chat** - Intergrated voice communication

### Phase 9.2 (Planned)
- [ ] **Plugin Marketplace** - Load external game logic plugins
- [ ] **Visual Scripting** - Node-based game logic editor
- [ ] **Performance Profiling** - Real-time performance analytics
- [ ] **Asset Pipeline** - Advanced asset loading and optimization

## Development

### Building
```bash
dotnet build LegendaryGameEngine/LegendaryGameEngine.csproj
```

### Testing
```bash
dotnet test LegendaryGameEngine/LegendaryGameEngine.csproj
```

### Debugging
Set breakpoints in Visual Studio or VS Code and debug ASHATCore with the LegendaryGameEngine DLL loaded.

## Dependencies

- **Abstractions** - Core interfaces and models
- **Microsoft.Data.Sqlite** - Database persistence
- **SQLitePCL.Raw** - SQLite native bindings
- **Microsoft.AspNetCore.Connections** - WebSocket support

## License

Part of the ASHATCore AI mainframe system. See main project LICENSE.

---

**Module:** LegendaryGameEngine  
**Version:** 1.5.0-beta  
**Status:** ✅ Production Ready  
**Phase:** 9 Complete  
**Last Updated:** 2025-01-13
