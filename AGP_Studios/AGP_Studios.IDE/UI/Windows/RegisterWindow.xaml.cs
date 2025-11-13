using System.Windows;
using AGP_Studios.IDE.Services;
using AGP_Studios.IDE.Models;

namespace AGP_Studios.IDE.UI.Windows;

/// <summary>
/// Registration window for new user sign-up
/// </summary>
public partial class RegisterWindow : Window
{
    private readonly ApiClient _apiClient;
    
    public RegisterWindow()
    {
        InitializeComponent();
        _apiClient = new ApiClient();
        
        // Load server URL from configuration
        ServerUrlTextBox.Text = ConfigurationManager.Instance.GetFullServerUrl();
    }
    
    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        var username = UsernameTextBox.Text.Trim();
        var email = EmailTextBox.Text.Trim();
        var password = PasswordBox.Password;
        var confirmPassword = ConfirmPasswordBox.Password;
        var serverUrl = ServerUrlTextBox.Text.Trim();
        
        // Validate input
        if (string.IsNullOrEmpty(username))
        {
            ShowStatus("Please enter a username.", true);
            return;
        }
        
        if (string.IsNullOrEmpty(email))
        {
            ShowStatus("Please enter an email address.", true);
            return;
        }
        
        // Basic email validation
        if (!email.Contains("@") || !email.Contains("."))
        {
            ShowStatus("Please enter a valid email address.", true);
            return;
        }
        
        if (string.IsNullOrEmpty(password))
        {
            ShowStatus("Please enter a password.", true);
            return;
        }
        
        if (password.Length < 6)
        {
            ShowStatus("Password must be at least 6 characters long.", true);
            return;
        }
        
        if (password != confirmPassword)
        {
            ShowStatus("Passwords do not match.", true);
            return;
        }
        
        // Update server URL in configuration if changed
        if (serverUrl != ConfigurationManager.Instance.GetFullServerUrl())
        {
            ConfigurationManager.Instance.Configuration.ServerUrl = serverUrl;
            ConfigurationManager.Instance.SaveConfiguration();
        }
        
        // Disable register button during registration
        RegisterButton.IsEnabled = false;
        RegisterButton.Content = "Creating account...";
        ShowStatus("Creating your account...", false);
        
        try
        {
            // Attempt registration
            var registerResponse = await _apiClient.RegisterAsync(username, email, password);
            
            if (registerResponse.Success && !string.IsNullOrEmpty(registerResponse.Token))
            {
                // Set authentication token
                _apiClient.SetAuthToken(registerResponse.Token);
                
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
                        Token = registerResponse.Token
                    };
                    
                    // Save session for auto-login
                    var sessionData = new SessionData
                    {
                        Token = registerResponse.Token,
                        UserId = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        IsAdmin = user.IsAdmin,
                        ExpiresAt = DateTime.UtcNow.AddDays(1) // 24 hour session
                    };
                    SessionManager.Instance.SaveSession(sessionData);
                    
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
                    
                    // Close registration window
                    Close();
                }
                else
                {
                    ShowStatus("Registration successful but failed to retrieve user information.", true);
                    ResetRegisterButton();
                }
            }
            else
            {
                ShowStatus(registerResponse.Message ?? "Registration failed. Please try again.", true);
                ResetRegisterButton();
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"Connection error: {ex.Message}", true);
            ResetRegisterButton();
        }
    }
    
    private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
    {
        var loginWindow = new LoginWindow();
        loginWindow.Show();
        Close();
    }
    
    private void ShowStatus(string message, bool isError)
    {
        StatusTextBlock.Text = message;
        StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
            isError ? System.Windows.Media.Colors.Red : System.Windows.Media.Colors.Green);
        StatusTextBlock.Visibility = Visibility.Visible;
    }
    
    private void ResetRegisterButton()
    {
        RegisterButton.IsEnabled = true;
        RegisterButton.Content = "Create Account";
    }
}
