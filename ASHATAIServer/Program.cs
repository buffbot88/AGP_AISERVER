using ASHATAIServer.Services;
using ASHATAIServer.Runtime;

var builder = WebApplication.CreateBuilder(args);

// Configure to listen on port 7077
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7077);
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

// Register User Database Service
builder.Services.AddSingleton<UserDatabaseService>();

// Register Authentication Service
builder.Services.AddSingleton<AuthenticationService>();

// Register Model Runtime (MockRuntime for now, can be swapped with LlamaCppAdapter)
builder.Services.AddSingleton<IModelRuntime, MockRuntime>();

// Register LanguageModelService as singleton
builder.Services.AddSingleton<LanguageModelService>();

// GameServerModule registration removed - module has been separated from AI Server
// If you need game server functionality, it should be deployed separately

// AI-enhanced game server service removed - depends on separated GameServerModule

var app = builder.Build();

// Initialize Language Model Service
var modelService = app.Services.GetRequiredService<LanguageModelService>();
await modelService.InitializeAsync();

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║    ASHATAIServer - AI Processing Server                 ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine($"Server started on port: 7077");
Console.WriteLine($"Server URL: http://localhost:7077");
Console.WriteLine();
Console.WriteLine("Available Endpoints:");
Console.WriteLine("  Authentication:");
Console.WriteLine("    POST /api/auth/login     - User login");
Console.WriteLine("    POST /api/auth/register  - User registration");
Console.WriteLine();
Console.WriteLine("  AI Services:");
Console.WriteLine("    POST /api/ai/process            - Process AI prompts");
Console.WriteLine("    POST /api/ai/process/stream     - Process AI prompts (streaming SSE)");
Console.WriteLine("    POST /api/ai/generate-project   - Generate complete projects");
Console.WriteLine("    GET  /api/ai/status             - Get model status");
Console.WriteLine("    POST /api/ai/models/scan        - Scan for models");
Console.WriteLine("    GET  /api/ai/health             - Health check");
Console.WriteLine();
Console.WriteLine("Runtime: " + (modelService.GetStatus().CurrentlyLoadedModel ?? "MockRuntime (fallback mode)"));
Console.WriteLine("Note: Game Server endpoints have been separated into a standalone module.");
Console.WriteLine();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.MapControllers();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
