using System.Windows;
using AGP_Studios.IDE.Services;
using AGP_Studios.IDE.Models;

namespace AGP_Studios.IDE.UI.Windows;

/// <summary>
/// Login window for user authentication
/// </summary>
public partial class LoginWindow : Window
{
    private readonly ApiClient _apiClient;
    
    public LoginWindow()
    {
        InitializeComponent();
        _apiClient = new ApiClient();
        
        // Load server URL from configuration
        ServerUrlTextBox.Text = ConfigurationManager.Instance.GetFullServerUrl();
    }
    
    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        var username = UsernameTextBox.Text.Trim();
        var password = PasswordBox.Password;
        var serverUrl = ServerUrlTextBox.Text.Trim();
        
        // Validate input
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Please enter both username and password.", true);
            return;
        }
        
        // Update server URL in configuration if changed
        if (serverUrl != ConfigurationManager.Instance.GetFullServerUrl())
        {
            ConfigurationManager.Instance.Configuration.ServerUrl = serverUrl;
            ConfigurationManager.Instance.SaveConfiguration();
        }
        
        // Disable login button during authentication
        LoginButton.IsEnabled = false;
        LoginButton.Content = "Signing in...";
        ShowStatus("Authenticating...", false);
        
        try
        {
            // Attempt login
            var loginResponse = await _apiClient.LoginAsync(username, password);
            
            if (loginResponse.Success && !string.IsNullOrEmpty(loginResponse.Token))
            {
                // Set authentication token
                _apiClient.SetAuthToken(loginResponse.Token);
                
                // Get user info to check admin status
                var userInfo = await _apiClient.GetUserInfoAsync();
                
                if (userInfo != null)
                {
                    // Create user object
                    var user = new User
                    {
                        Id = userInfo.UserId,
                        Username = userInfo.Username,
                        Email = userInfo.Email,
                        IsAdmin = userInfo.IsAdmin,
                        Token = loginResponse.Token
                    };
                    
                    // Route to appropriate window based on user role
                    if (user.IsAdmin)
                    {
                        var adminConsole = new AdminConsoleWindow(user, _apiClient);
                        adminConsole.Show();
                    }
                    else
                    {
                        var gameLibrary = new GameLibraryWindow(user, _apiClient);
                        gameLibrary.Show();
                    }
                    
                    // Close login window
                    Close();
                }
                else
                {
                    ShowStatus("Failed to retrieve user information.", true);
                    ResetLoginButton();
                }
            }
            else
            {
                ShowStatus(loginResponse.Message ?? "Login failed. Please check your credentials.", true);
                ResetLoginButton();
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"Connection error: {ex.Message}", true);
            ResetLoginButton();
        }
    }
    
    private void ShowStatus(string message, bool isError)
    {
        StatusTextBlock.Text = message;
        StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
            isError ? System.Windows.Media.Colors.Red : System.Windows.Media.Colors.Green);
        StatusTextBlock.Visibility = Visibility.Visible;
    }
    
    private void ResetLoginButton()
    {
        LoginButton.IsEnabled = true;
        LoginButton.Content = "Sign In";
    }
}
