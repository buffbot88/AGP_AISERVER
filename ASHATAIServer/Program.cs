using ASHATAIServer.Services;
using ASHATAIServer.Runtime;
using ASHATAIServer.Services.Auth;
using ASHATAIServer.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure to listen on port 7077 (HTTP) and optionally 7443 (HTTPS)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7077); // HTTP
    
    // Configure HTTPS if enabled
    var httpsEnabled = builder.Configuration.GetValue<bool>("Https:Enabled");
    if (httpsEnabled)
    {
        var httpsPort = builder.Configuration.GetValue<int>("Https:Port", 7443);
        var certPath = builder.Configuration.GetValue<string>("Https:CertificatePath");
        var certPassword = builder.Configuration.GetValue<string>("Https:CertificatePassword");
        
        if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
        {
            options.ListenAnyIP(httpsPort, listenOptions =>
            {
                listenOptions.UseHttps(certPath, certPassword);
            });
        }
    }
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

// Register API Key Service
builder.Services.AddSingleton<ApiKeyService>();

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
var httpsEnabled = app.Configuration.GetValue<bool>("Https:Enabled");
if (httpsEnabled)
{
    var httpsPort = app.Configuration.GetValue<int>("Https:Port", 7443);
    Console.WriteLine($"HTTPS URL: https://localhost:{httpsPort}");
}
Console.WriteLine();
Console.WriteLine("Security Features:");
Console.WriteLine("  ✓ Argon2id password hashing with per-user salts");
Console.WriteLine("  ✓ API key authentication middleware");
Console.WriteLine("  ✓ Rate limiting (60/min, 1000/hr by default)");
var rateLimitEnabled = app.Configuration.GetValue<bool>("RateLimit:Enabled", true);
Console.WriteLine($"  ✓ Rate limiting: {(rateLimitEnabled ? "Enabled" : "Disabled")}");
Console.WriteLine();
Console.WriteLine("Available Endpoints:");
Console.WriteLine("  Authentication:");
Console.WriteLine("    POST /api/auth/login     - User login");
Console.WriteLine("    POST /api/auth/register  - User registration");
Console.WriteLine();
Console.WriteLine("  Admin (requires admin session):");
Console.WriteLine("    POST /api/admin/keys/create       - Create API key");
Console.WriteLine("    GET  /api/admin/keys/list         - List API keys");
Console.WriteLine("    POST /api/admin/keys/revoke/{id}  - Revoke API key");
Console.WriteLine("    GET  /api/admin/health            - System health");
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

// Add rate limiting middleware
app.UseRateLimiting();

// Add API key authentication middleware
app.UseApiKeyAuthentication();

app.MapControllers();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
