using System.Windows;
using System.Windows.Controls;
using AGP_Studios.IDE.Models;
using AGP_Studios.IDE.Services;

namespace AGP_Studios.IDE.UI.Windows;

/// <summary>
/// Game library window for members to browse, download, and play games
/// </summary>
public partial class GameLibraryWindow : Window
{
    private readonly User _user;
    private readonly ApiClient _apiClient;
    private readonly LocalRepository _localRepository;
    private readonly GameDownloader _gameDownloader;
    private readonly GameLauncher _gameLauncher;
    
    public GameLibraryWindow(User user, ApiClient apiClient)
    {
        InitializeComponent();
        
        _user = user;
        _apiClient = apiClient;
        _localRepository = new LocalRepository();
        _gameDownloader = new GameDownloader(_apiClient, _localRepository);
        _gameLauncher = new GameLauncher();
        
        // Set up UI
        UserInfoTextBlock.Text = $"Member: {_user.Username}";
        
        // Load data
        LoadAvailableGames();
        LoadInstalledGames();
    }
    
    private async void LoadAvailableGames()
    {
        try
        {
            StatusTextBlock.Text = "Loading available games...";
            
            var games = await _apiClient.GetGamesAsync();
            GamesItemsControl.ItemsSource = games;
            
            StatusTextBlock.Text = $"Found {games.Count} available game(s)";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Error loading games: {ex.Message}";
            MessageBox.Show(
                $"Failed to load games from server:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
    
    private void LoadInstalledGames()
    {
        try
        {
            var installedGames = _localRepository.GetInstalledGames();
            InstalledGamesListBox.ItemsSource = installedGames;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to load installed games:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
    
    private async void DownloadGameButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Game game)
        {
            var result = MessageBox.Show(
                $"Download and install '{game.Name}'?\n\nSize: {FormatBytes(game.SizeBytes)}",
                "Confirm Download",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                await DownloadAndInstallGame(game, button);
            }
        }
    }
    
    private async Task DownloadAndInstallGame(Game game, Button button)
    {
        var originalContent = button.Content;
        button.IsEnabled = false;
        button.Content = "Downloading...";
        
        try
        {
            var progress = new Progress<int>(percent =>
            {
                Dispatcher.Invoke(() =>
                {
                    button.Content = $"Downloading {percent}%";
                    StatusTextBlock.Text = $"Downloading '{game.Name}': {percent}%";
                });
            });
            
            var success = await _gameDownloader.DownloadAndInstallGameAsync(game, progress);
            
            if (success)
            {
                MessageBox.Show(
                    $"'{game.Name}' has been installed successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                
                // Refresh installed games list
                LoadInstalledGames();
                StatusTextBlock.Text = $"'{game.Name}' installed successfully";
            }
            else
            {
                MessageBox.Show(
                    $"Failed to download/install '{game.Name}'.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                StatusTextBlock.Text = "Installation failed";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error during installation:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            StatusTextBlock.Text = "Installation error";
        }
        finally
        {
            button.IsEnabled = true;
            button.Content = originalContent;
        }
    }
    
    private void PlayGameButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is GameInstallation installation)
        {
            try
            {
                if (_gameLauncher.LaunchGame(installation))
                {
                    StatusTextBlock.Text = $"Launched '{installation.Name}'";
                }
                else
                {
                    MessageBox.Show(
                        $"Failed to launch '{installation.Name}'.\n\nExecutable not found or invalid.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error launching game:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
    
    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadAvailableGames();
        LoadInstalledGames();
    }
    
    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to logout?",
            "Logout",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
    
    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }
}
