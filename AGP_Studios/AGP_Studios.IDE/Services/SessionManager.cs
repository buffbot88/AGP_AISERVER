using System.IO;
using System.Text.Json;
using AGP_Studios.IDE.Models;

namespace AGP_Studios.IDE.Services;

/// <summary>
/// Manages user session persistence
/// </summary>
public class SessionManager
{
    private static SessionManager? _instance;
    private static readonly object _lock = new object();
    
    private readonly string _sessionFilePath;
    
    public static SessionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SessionManager();
                    }
                }
            }
            return _instance;
        }
    }
    
    private SessionManager()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var agpIdePath = Path.Combine(appDataPath, "AGP_IDE");
        
        // Ensure directory exists
        if (!Directory.Exists(agpIdePath))
        {
            Directory.CreateDirectory(agpIdePath);
        }
        
        _sessionFilePath = Path.Combine(agpIdePath, "session.json");
    }
    
    /// <summary>
    /// Save session data to disk
    /// </summary>
    public void SaveSession(SessionData sessionData)
    {
        try
        {
            var json = JsonSerializer.Serialize(sessionData, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(_sessionFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving session: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Load session data from disk
    /// </summary>
    public SessionData? LoadSession()
    {
        try
        {
            if (!File.Exists(_sessionFilePath))
            {
                return null;
            }
            
            var json = File.ReadAllText(_sessionFilePath);
            var sessionData = JsonSerializer.Deserialize<SessionData>(json);
            
            // Check if session has expired (24 hours)
            if (sessionData != null && 
                sessionData.ExpiresAt > DateTime.UtcNow)
            {
                return sessionData;
            }
            
            // Session expired, delete it
            ClearSession();
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading session: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Clear saved session
    /// </summary>
    public void ClearSession()
    {
        try
        {
            if (File.Exists(_sessionFilePath))
            {
                File.Delete(_sessionFilePath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error clearing session: {ex.Message}");
        }
    }
}

/// <summary>
/// Session data model
/// </summary>
public class SessionData
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public DateTime ExpiresAt { get; set; }
}
