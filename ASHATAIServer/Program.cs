using ASHATAIServer.Services;
using LegendaryGameSystem;

var builder = WebApplication.CreateBuilder(args);

// Configure to listen on port 8088
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8088);
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add HttpClient factory for authentication service
builder.Services.AddHttpClient();

// Add CORS for ASHAT Goddess client
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register Authentication Service
builder.Services.AddSingleton<AuthenticationService>();

// Register LanguageModelService as singleton
builder.Services.AddSingleton<LanguageModelService>();

// Register GameServerModule as singleton
builder.Services.AddSingleton<GameServerModule>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<GameServerModule>>();
    var module = new GameServerModule();
    // Initialize the module - pass null as manager for standalone operation
    module.Initialize(null);
    return module;
});

// Register AI-enhanced game server service
builder.Services.AddSingleton<AIEnhancedGameServerService>();

var app = builder.Build();

// Initialize Language Model Service
var modelService = app.Services.GetRequiredService<LanguageModelService>();
await modelService.InitializeAsync();

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║    ASHATAIServer - AI Processing & Game Server          ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine($"Server started on port: 8088");
Console.WriteLine($"Server URL: http://localhost:8088");
Console.WriteLine();
Console.WriteLine("Available Endpoints:");
Console.WriteLine("  Authentication:");
Console.WriteLine("    POST /api/auth/login     - User login");
Console.WriteLine("    POST /api/auth/register  - User registration");
Console.WriteLine();
Console.WriteLine("  AI Services:");
Console.WriteLine("    POST /api/ai/process     - Process AI prompts");
Console.WriteLine("    GET  /api/ai/status      - Get model status");
Console.WriteLine("    POST /api/ai/models/scan - Scan for models");
Console.WriteLine("    GET  /api/ai/health      - Health check");
Console.WriteLine();
Console.WriteLine("  Game Server:");
Console.WriteLine("    GET  /api/gameserver/status              - Game server status");
Console.WriteLine("    GET  /api/gameserver/capabilities        - Server capabilities");
Console.WriteLine("    POST /api/gameserver/create              - Create new game");
Console.WriteLine("    POST /api/gameserver/create-ai-enhanced  - Create game with AI");
Console.WriteLine("    GET  /api/gameserver/projects            - List all projects");
Console.WriteLine("    POST /api/gameserver/deploy/{id}         - Deploy game");
Console.WriteLine("    GET  /api/gameserver/suggestions/{id}    - Get AI suggestions");
Console.WriteLine();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.MapControllers();

app.Run();
