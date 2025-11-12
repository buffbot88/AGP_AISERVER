using System.Windows;
using AGP_Studios.IDE.Models;
using AGP_Studios.IDE.Services;
using ICSharpCode.AvalonEdit.Highlighting;

namespace AGP_Studios.IDE.UI.Windows;

/// <summary>
/// Admin console window for code drafting and publishing
/// </summary>
public partial class AdminConsoleWindow : Window
{
    private readonly User _user;
    private readonly ApiClient _apiClient;
    private readonly LocalRepository _localRepository;
    private CodeDraft? _currentDraft;
    
    public AdminConsoleWindow(User user, ApiClient apiClient)
    {
        InitializeComponent();
        
        _user = user;
        _apiClient = apiClient;
        _localRepository = new LocalRepository();
        
        // Set up UI
        UserInfoTextBlock.Text = $"Admin: {_user.Username}";
        
        // Configure code editor
        CodeEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
        CodeEditor.Options.ConvertTabsToSpaces = true;
        CodeEditor.Options.IndentationSize = 4;
        
        // Load existing drafts
        LoadDraftsList();
        
        // Create or load a default draft
        CreateNewDraft();
    }
    
    private void LoadDraftsList()
    {
        var drafts = _localRepository.GetAllDrafts();
        DraftsListBox.ItemsSource = drafts;
    }
    
    private void CreateNewDraft()
    {
        _currentDraft = new CodeDraft
        {
            Name = $"Draft_{DateTime.Now:yyyyMMdd_HHmmss}",
            Content = "// New code draft\n// Write your code here\n\n",
            Language = "csharp"
        };
        
        DraftNameTextBox.Text = _currentDraft.Name;
        CodeEditor.Text = _currentDraft.Content;
        UpdateStatusBar("New draft created");
    }
    
    private void NewDraftButton_Click(object sender, RoutedEventArgs e)
    {
        // Ask to save current draft if modified
        if (_currentDraft != null && CodeEditor.Text != _currentDraft.Content)
        {
            var result = MessageBox.Show(
                "Do you want to save the current draft before creating a new one?",
                "Save Draft",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                SaveCurrentDraft();
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return;
            }
        }
        
        CreateNewDraft();
        LoadDraftsList();
    }
    
    private void SaveDraftButton_Click(object sender, RoutedEventArgs e)
    {
        SaveCurrentDraft();
    }
    
    private void SaveCurrentDraft()
    {
        if (_currentDraft == null) return;
        
        _currentDraft.Name = DraftNameTextBox.Text;
        _currentDraft.Content = CodeEditor.Text;
        
        if (_localRepository.SaveDraft(_currentDraft))
        {
            UpdateStatusBar($"Draft '{_currentDraft.Name}' saved successfully");
            LoadDraftsList();
        }
        else
        {
            MessageBox.Show("Failed to save draft.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void LoadDraftButton_Click(object sender, RoutedEventArgs e)
    {
        if (DraftsListBox.SelectedItem is CodeDraft selectedDraft)
        {
            LoadDraft(selectedDraft);
        }
        else
        {
            MessageBox.Show("Please select a draft to load.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    
    private void DraftsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (DraftsListBox.SelectedItem is CodeDraft selectedDraft)
        {
            // Auto-load on selection
            LoadDraft(selectedDraft);
        }
    }
    
    private void LoadDraft(CodeDraft draft)
    {
        _currentDraft = draft;
        DraftNameTextBox.Text = draft.Name;
        CodeEditor.Text = draft.Content;
        UpdateStatusBar($"Loaded draft '{draft.Name}'");
    }
    
    private async void PublishButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentDraft == null || string.IsNullOrWhiteSpace(CodeEditor.Text))
        {
            MessageBox.Show("Cannot publish empty code.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        // Save current state
        _currentDraft.Name = DraftNameTextBox.Text;
        _currentDraft.Content = CodeEditor.Text;
        
        var result = MessageBox.Show(
            $"Are you sure you want to publish '{_currentDraft.Name}' to the AI Server?",
            "Confirm Publish",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            PublishButton.IsEnabled = false;
            UpdateStatusBar("Publishing to server...");
            
            try
            {
                var success = await _apiClient.PublishCodeAsync(_currentDraft.Name, _currentDraft.Content);
                
                if (success)
                {
                    _currentDraft.IsPublished = true;
                    _localRepository.SaveDraft(_currentDraft);
                    
                    MessageBox.Show(
                        "Code published successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    
                    UpdateStatusBar("Published successfully");
                }
                else
                {
                    MessageBox.Show(
                        "Failed to publish code. Please check your connection and try again.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    
                    UpdateStatusBar("Publish failed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error publishing: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatusBar("Publish error");
            }
            finally
            {
                PublishButton.IsEnabled = true;
            }
        }
    }
    
    private void GenerateCodeButton_Click(object sender, RoutedEventArgs e)
    {
        // Placeholder for AI code generation
        MessageBox.Show(
            "AI code generation feature is a placeholder.\n\nThis would integrate with the AI Server's code generation endpoints.",
            "AI Feature",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
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
    
    private void UpdateStatusBar(string message)
    {
        StatusBarTextBlock.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
    }
}
