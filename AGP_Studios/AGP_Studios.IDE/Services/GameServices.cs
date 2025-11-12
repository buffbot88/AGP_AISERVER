using System.Diagnostics;
using System.IO.Compression;
using AGP_Studios.IDE.Models;

namespace AGP_Studios.IDE.Services;

/// <summary>
/// Game downloader and installer service
/// </summary>
public class GameDownloader
{
    private readonly ApiClient _apiClient;
    private readonly LocalRepository _localRepository;
    
    public GameDownloader(ApiClient apiClient, LocalRepository localRepository)
    {
        _apiClient = apiClient;
        _localRepository = localRepository;
    }
    
    /// <summary>
    /// Download and install a game
    /// </summary>
    public async Task<bool> DownloadAndInstallGameAsync(Game game, IProgress<int>? progress = null)
    {
        try
        {
            progress?.Report(10);
            
            // Download game package
            var gameData = await _apiClient.DownloadGameAsync(game.DownloadUrl);
            if (gameData == null || gameData.Length == 0)
            {
                return false;
            }
            
            progress?.Report(50);
            
            // Create installation directory
            var installPath = _localRepository.GetGameInstallPath(game.Id);
            Directory.CreateDirectory(installPath);
            
            // Save zip file temporarily
            var tempZipPath = Path.Combine(installPath, "game_package.zip");
            await File.WriteAllBytesAsync(tempZipPath, gameData);
            
            progress?.Report(70);
            
            // Extract zip file
            ZipFile.ExtractToDirectory(tempZipPath, installPath, true);
            File.Delete(tempZipPath);
            
            progress?.Report(90);
            
            // Find executable
            var exePath = FindExecutable(installPath);
            
            // Save installation info
            var installation = new GameInstallation
            {
                GameId = game.Id,
                Name = game.Name,
                Version = game.Version,
                InstallPath = installPath,
                ExecutablePath = exePath ?? string.Empty,
                InstalledDate = DateTime.Now
            };
            
            _localRepository.SaveGameInstallation(installation);
            
            progress?.Report(100);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error downloading/installing game: {ex.Message}");
            return false;
        }
    }
    
    private string? FindExecutable(string directory)
    {
        try
        {
            // Look for .exe files
            var exeFiles = Directory.GetFiles(directory, "*.exe", SearchOption.AllDirectories);
            
            // Prefer executables in root or bin directories
            var rootExe = exeFiles.FirstOrDefault(f => Path.GetDirectoryName(f) == directory);
            if (rootExe != null) return rootExe;
            
            var binExe = exeFiles.FirstOrDefault(f => Path.GetDirectoryName(f)?.EndsWith("bin") == true);
            if (binExe != null) return binExe;
            
            // Return first found
            return exeFiles.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Game launcher service for running installed games
/// </summary>
public class GameLauncher
{
    /// <summary>
    /// Launch an installed game
    /// </summary>
    public bool LaunchGame(GameInstallation installation)
    {
        try
        {
            if (string.IsNullOrEmpty(installation.ExecutablePath) || 
                !File.Exists(installation.ExecutablePath))
            {
                return false;
            }
            
            var startInfo = new ProcessStartInfo
            {
                FileName = installation.ExecutablePath,
                WorkingDirectory = Path.GetDirectoryName(installation.ExecutablePath) ?? string.Empty,
                UseShellExecute = true
            };
            
            Process.Start(startInfo);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error launching game: {ex.Message}");
            return false;
        }
    }
}
