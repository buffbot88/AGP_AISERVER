namespace AGP_Studios.IDE.Models;

/// <summary>
/// Configuration model for AI Server connection
/// </summary>
public class AppConfiguration
{
    public string ServerUrl { get; set; } = "http://localhost:8088";
    public int ServerPort { get; set; } = 8088;
    public string AppDataPath { get; set; } = string.Empty;
    public string DraftsPath { get; set; } = string.Empty;
    public string GamesPath { get; set; } = string.Empty;
    
    public AppConfiguration()
    {
        // Initialize default paths
        AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AGP_IDE");
        DraftsPath = Path.Combine(AppDataPath, "Drafts");
        GamesPath = Path.Combine(AppDataPath, "Games");
    }
}
