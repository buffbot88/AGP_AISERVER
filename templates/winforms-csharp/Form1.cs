namespace WinFormsApp;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "WinForms Application";
        this.Size = new System.Drawing.Size(800, 600);
        this.StartPosition = FormStartPosition.CenterScreen;

        var label = new Label
        {
            Text = "Hello, World!",
            Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold),
            AutoSize = true,
            Location = new System.Drawing.Point(50, 50)
        };

        var button = new Button
        {
            Text = "Click Me",
            Location = new System.Drawing.Point(50, 100),
            Size = new System.Drawing.Size(150, 40)
        };

        button.Click += (sender, e) =>
        {
            MessageBox.Show("Button clicked!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };

        this.Controls.Add(label);
        this.Controls.Add(button);
    }
}
