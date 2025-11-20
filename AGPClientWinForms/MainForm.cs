using System.Net.Http.Json;
using System.Text.Json;

namespace AGPClientWinForms;

public partial class MainForm : Form
{
    private TextBox txtServerUrl;
    private TextBox txtPrompt;
    private TextBox txtOutput;
    private Button btnGenerate;
    private Button btnSave;
    private ComboBox cmbProjectType;
    private ComboBox cmbLanguage;
    private Label lblStatus;
    private HttpClient _httpClient;
    private Dictionary<string, string>? _generatedFiles;

    public MainForm()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
        LoadSettings();
    }

    private void InitializeComponent()
    {
        this.Text = "AGP Client - Project Generator";
        this.Size = new System.Drawing.Size(1000, 700);
        this.StartPosition = FormStartPosition.CenterScreen;

        // Server URL
        var lblServer = new Label
        {
            Text = "Server URL:",
            Location = new System.Drawing.Point(20, 20),
            AutoSize = true
        };

        txtServerUrl = new TextBox
        {
            Location = new System.Drawing.Point(120, 18),
            Size = new System.Drawing.Size(400, 25),
            Text = "http://localhost:7077"
        };

        // Project Type
        var lblType = new Label
        {
            Text = "Project Type:",
            Location = new System.Drawing.Point(20, 55),
            AutoSize = true
        };

        cmbProjectType = new ComboBox
        {
            Location = new System.Drawing.Point(120, 53),
            Size = new System.Drawing.Size(150, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbProjectType.Items.AddRange(new[] { "console", "winforms", "webapi" });
        cmbProjectType.SelectedIndex = 0;

        // Language
        var lblLanguage = new Label
        {
            Text = "Language:",
            Location = new System.Drawing.Point(290, 55),
            AutoSize = true
        };

        cmbLanguage = new ComboBox
        {
            Location = new System.Drawing.Point(370, 53),
            Size = new System.Drawing.Size(150, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbLanguage.Items.AddRange(new[] { "csharp", "python", "javascript" });
        cmbLanguage.SelectedIndex = 0;

        // Prompt
        var lblPrompt = new Label
        {
            Text = "Project Description:",
            Location = new System.Drawing.Point(20, 90),
            AutoSize = true
        };

        txtPrompt = new TextBox
        {
            Location = new System.Drawing.Point(20, 115),
            Size = new System.Drawing.Size(940, 100),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };

        // Generate Button
        btnGenerate = new Button
        {
            Text = "Generate Project",
            Location = new System.Drawing.Point(20, 225),
            Size = new System.Drawing.Size(150, 40)
        };
        btnGenerate.Click += BtnGenerate_Click;

        // Save Button
        btnSave = new Button
        {
            Text = "Save to Disk",
            Location = new System.Drawing.Point(180, 225),
            Size = new System.Drawing.Size(150, 40),
            Enabled = false
        };
        btnSave.Click += BtnSave_Click;

        // Output
        var lblOutput = new Label
        {
            Text = "Generated Files:",
            Location = new System.Drawing.Point(20, 275),
            AutoSize = true
        };

        txtOutput = new TextBox
        {
            Location = new System.Drawing.Point(20, 300),
            Size = new System.Drawing.Size(940, 300),
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            ReadOnly = true,
            Font = new System.Drawing.Font("Consolas", 9F)
        };

        // Status
        lblStatus = new Label
        {
            Text = "Ready",
            Location = new System.Drawing.Point(20, 610),
            Size = new System.Drawing.Size(940, 20),
            ForeColor = System.Drawing.Color.Blue
        };

        this.Controls.AddRange(new Control[]
        {
            lblServer, txtServerUrl,
            lblType, cmbProjectType,
            lblLanguage, cmbLanguage,
            lblPrompt, txtPrompt,
            btnGenerate, btnSave,
            lblOutput, txtOutput,
            lblStatus
        });

        this.FormClosing += MainForm_FormClosing;
    }

    private void LoadSettings()
    {
        try
        {
            var settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AGPClient",
                "settings.json");

            if (File.Exists(settingsPath))
            {
                var json = File.ReadAllText(settingsPath);
                var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (settings != null && settings.ContainsKey("ServerUrl"))
                {
                    txtServerUrl.Text = settings["ServerUrl"];
                }
            }
        }
        catch
        {
            // Ignore errors loading settings
        }
    }

    private void SaveSettings()
    {
        try
        {
            var settingsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AGPClient");

            Directory.CreateDirectory(settingsDir);

            var settings = new Dictionary<string, string>
            {
                ["ServerUrl"] = txtServerUrl.Text
            };

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(settingsDir, "settings.json"), json);
        }
        catch
        {
            // Ignore errors saving settings
        }
    }

    private async void BtnGenerate_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtPrompt.Text))
        {
            MessageBox.Show("Please enter a project description.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnGenerate.Enabled = false;
        btnSave.Enabled = false;
        lblStatus.Text = "Connecting to server...";
        lblStatus.ForeColor = System.Drawing.Color.Blue;
        txtOutput.Text = "";

        try
        {
            _httpClient.BaseAddress = new Uri(txtServerUrl.Text);

            var request = new
            {
                projectDescription = txtPrompt.Text,
                projectType = cmbProjectType.SelectedItem?.ToString() ?? "console",
                language = cmbLanguage.SelectedItem?.ToString() ?? "csharp",
                options = new
                {
                    includeComments = true,
                    includeReadme = true,
                    maxFiles = 50,
                    maxFileSizeBytes = 1048576
                }
            };

            lblStatus.Text = "Generating project...";
            var response = await _httpClient.PostAsJsonAsync("/api/ai/generate-project", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Server error: {response.StatusCode}\n{error}");
            }

            var result = await response.Content.ReadFromJsonAsync<GenerateProjectResponse>();
            if (result == null || !result.Success)
            {
                throw new Exception(result?.Error ?? "Unknown error");
            }

            _generatedFiles = result.Files;

            // Display files
            var output = new System.Text.StringBuilder();
            output.AppendLine($"✓ Project generated successfully!");
            output.AppendLine($"Model: {result.Metadata?.ModelUsed ?? "N/A"}");
            output.AppendLine($"Files: {result.Files?.Count ?? 0}");
            output.AppendLine($"Generation time: {result.Metadata?.GenerationTimeMs ?? 0}ms");
            output.AppendLine();
            output.AppendLine("Generated files:");
            output.AppendLine("─────────────────────────────────────────");

            foreach (var file in result.Files ?? new Dictionary<string, string>())
            {
                output.AppendLine();
                output.AppendLine($"═══ {file.Key} ═══");
                output.AppendLine(file.Value);
            }

            txtOutput.Text = output.ToString();
            btnSave.Enabled = true;
            lblStatus.Text = "✓ Generation complete! Click 'Save to Disk' to save files.";
            lblStatus.ForeColor = System.Drawing.Color.Green;

            SaveSettings();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Generation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = $"✗ Error: {ex.Message}";
            lblStatus.ForeColor = System.Drawing.Color.Red;
        }
        finally
        {
            btnGenerate.Enabled = true;
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (_generatedFiles == null || _generatedFiles.Count == 0)
        {
            MessageBox.Show("No files to save.", "No Files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dialog = new FolderBrowserDialog
        {
            Description = "Select folder to save project files",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                lblStatus.Text = "Saving files...";
                lblStatus.ForeColor = System.Drawing.Color.Blue;

                foreach (var file in _generatedFiles)
                {
                    var filePath = Path.Combine(dialog.SelectedPath, file.Key);
                    var fileDir = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(fileDir))
                    {
                        Directory.CreateDirectory(fileDir);
                    }

                    File.WriteAllText(filePath, file.Value);
                }

                lblStatus.Text = $"✓ Files saved to: {dialog.SelectedPath}";
                lblStatus.ForeColor = System.Drawing.Color.Green;

                MessageBox.Show($"Project files saved successfully!\n\nLocation: {dialog.SelectedPath}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving files: {ex.Message}", "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = $"✗ Save error: {ex.Message}";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
        }
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        SaveSettings();
        _httpClient.Dispose();
    }
}

public class GenerateProjectResponse
{
    public bool Success { get; set; }
    public Dictionary<string, string>? Files { get; set; }
    public ProjectMetadata? Metadata { get; set; }
    public string? Error { get; set; }
}

public class ProjectMetadata
{
    public string? ModelUsed { get; set; }
    public string? ProjectType { get; set; }
    public string? Language { get; set; }
    public int FileCount { get; set; }
    public long TotalSizeBytes { get; set; }
    public int GenerationTimeMs { get; set; }
    public List<string>? Warnings { get; set; }
}
