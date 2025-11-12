using Newtonsoft.Json;
using AGP_Studios.IDE.Models;

namespace AGP_Studios.IDE.Services;

/// <summary>
/// Local repository for managing drafts and game installations
/// </summary>
public class LocalRepository
{
    private readonly string _draftsPath;
    private readonly string _gamesPath;
    
    public LocalRepository()
    {
        var config = ConfigurationManager.Instance.Configuration;
        _draftsPath = config.DraftsPath;
        _gamesPath = config.GamesPath;
        
        EnsureDirectoriesExist();
    }
    
    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(_draftsPath);
        Directory.CreateDirectory(_gamesPath);
    }
    
    #region Draft Management
    
    /// <summary>
    /// Save a code draft locally
    /// </summary>
    public bool SaveDraft(CodeDraft draft)
    {
        try
        {
            draft.LastModified = DateTime.Now;
            var filePath = Path.Combine(_draftsPath, $"{draft.Id}.json");
            var json = JsonConvert.SerializeObject(draft, Formatting.Indented);
            File.WriteAllText(filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving draft: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Load a code draft by ID
    /// </summary>
    public CodeDraft? LoadDraft(string id)
    {
        try
        {
            var filePath = Path.Combine(_draftsPath, $"{id}.json");
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<CodeDraft>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading draft: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Get all local drafts
    /// </summary>
    public List<CodeDraft> GetAllDrafts()
    {
        var drafts = new List<CodeDraft>();
        
        try
        {
            var files = Directory.GetFiles(_draftsPath, "*.json");
            
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var draft = JsonConvert.DeserializeObject<CodeDraft>(json);
                    if (draft != null)
                    {
                        drafts.Add(draft);
                    }
                }
                catch
                {
                    // Skip invalid files
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting drafts: {ex.Message}");
        }
        
        return drafts.OrderByDescending(d => d.LastModified).ToList();
    }
    
    /// <summary>
    /// Delete a draft
    /// </summary>
    public bool DeleteDraft(string id)
    {
        try
        {
            var filePath = Path.Combine(_draftsPath, $"{id}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting draft: {ex.Message}");
            return false;
        }
    }
    
    #endregion
    
    #region Game Installation Management
    
    /// <summary>
    /// Save game installation info
    /// </summary>
    public bool SaveGameInstallation(GameInstallation installation)
    {
        try
        {
            var filePath = Path.Combine(_gamesPath, $"{installation.GameId}_install.json");
            var json = JsonConvert.SerializeObject(installation, Formatting.Indented);
            File.WriteAllText(filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving game installation: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Get all installed games
    /// </summary>
    public List<GameInstallation> GetInstalledGames()
    {
        var installations = new List<GameInstallation>();
        
        try
        {
            var files = Directory.GetFiles(_gamesPath, "*_install.json");
            
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var installation = JsonConvert.DeserializeObject<GameInstallation>(json);
                    if (installation != null)
                    {
                        installations.Add(installation);
                    }
                }
                catch
                {
                    // Skip invalid files
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting installed games: {ex.Message}");
        }
        
        return installations;
    }
    
    /// <summary>
    /// Get game installation directory
    /// </summary>
    public string GetGameInstallPath(int gameId)
    {
        return Path.Combine(_gamesPath, $"Game_{gameId}");
    }
    
    #endregion
}
