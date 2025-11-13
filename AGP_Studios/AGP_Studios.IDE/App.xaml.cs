using System.Windows;
using AGP_Studios.IDE.Services;
using AGP_Studios.IDE.Models;
using AGP_Studios.IDE.UI.Windows;

namespace AGP_Studios.IDE;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Initialize configuration
        Services.ConfigurationManager.Instance.LoadConfiguration();
        
        // Set up unhandled exception handling
        DispatcherUnhandledException += (sender, args) =>
        {
            MessageBox.Show(
                $"An unexpected error occurred:\n\n{args.Exception.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };
        
        // Check for existing session and attempt auto-login
        await TryAutoLoginAsync();
    }
    
    private async Task TryAutoLoginAsync()
    {
        var sessionData = SessionManager.Instance.LoadSession();
        
        if (sessionData != null)
        {
            try
            {
                // Validate session with server
                var apiClient = new ApiClient();
                apiClient.SetAuthToken(sessionData.Token);
                
                var userInfo = await apiClient.GetUserInfoAsync();
                
                if (userInfo != null)
                {
                    // Session is valid, create user object
                    var user = new User
                    {
                        Id = userInfo.UserId,
                        Username = userInfo.Username,
                        Email = userInfo.Email,
                        IsAdmin = userInfo.IsAdmin,
                        Token = sessionData.Token
                    };
                    
                    // Update session expiry
                    sessionData.ExpiresAt = DateTime.UtcNow.AddDays(1);
                    SessionManager.Instance.SaveSession(sessionData);
                    
                    // Navigate directly to appropriate window
                    Window mainWindow;
                    if (user.IsAdmin)
                    {
                        mainWindow = new AdminConsoleWindow(user, apiClient);
                    }
                    else
                    {
                        mainWindow = new GameLibraryWindow(user, apiClient);
                    }
                    
                    mainWindow.Show();
                    
                    // Close the default login window if it was created
                    if (MainWindow != null && MainWindow.IsVisible)
                    {
                        MainWindow.Close();
                    }
                    
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Auto-login failed: {ex.Message}");
            }
            
            // If we get here, session validation failed
            SessionManager.Instance.ClearSession();
        }
        
        // No valid session, show login window (already handled by StartupUri in App.xaml)
    }
}
