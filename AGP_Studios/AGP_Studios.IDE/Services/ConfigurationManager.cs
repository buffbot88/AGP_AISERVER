using Newtonsoft.Json;
using AGP_Studios.IDE.Models;

namespace AGP_Studios.IDE.Services;

/// <summary>
/// Configuration manager singleton for app settings
/// </summary>
public class ConfigurationManager
{
    private static ConfigurationManager? _instance;
    private static readonly object _lock = new object();
    
    public static ConfigurationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ConfigurationManager();
                }
            }
            return _instance;
        }
    }
    
    private string ConfigFilePath { get; }
    public AppConfiguration Configuration { get; private set; }
    
    private ConfigurationManager()
    {
        Configuration = new AppConfiguration();
        ConfigFilePath = Path.Combine(Configuration.AppDataPath, "config.json");
    }
    
    public void LoadConfiguration()
    {
        try
        {
            // Ensure directory exists
            if (!Directory.Exists(Configuration.AppDataPath))
            {
                Directory.CreateDirectory(Configuration.AppDataPath);
            }
            
            // Load config if exists
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                var loadedConfig = JsonConvert.DeserializeObject<AppConfiguration>(json);
                if (loadedConfig != null)
                {
                    Configuration = loadedConfig;
                }
            }
            else
            {
                // Save default configuration
                SaveConfiguration();
            }
            
            // Ensure all directories exist
            EnsureDirectoriesExist();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading configuration: {ex.Message}");
        }
    }
    
    public void SaveConfiguration()
    {
        try
        {
            var json = JsonConvert.SerializeObject(Configuration, Formatting.Indented);
            File.WriteAllText(ConfigFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving configuration: {ex.Message}");
        }
    }
    
    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(Configuration.AppDataPath);
        Directory.CreateDirectory(Configuration.DraftsPath);
        Directory.CreateDirectory(Configuration.GamesPath);
    }
    
    public string GetFullServerUrl()
    {
        return Configuration.ServerUrl.TrimEnd('/');
    }
}
