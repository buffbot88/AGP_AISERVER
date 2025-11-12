namespace AGP_Studios.IDE.Models;

/// <summary>
/// User authentication model
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public string Token { get; set; } = string.Empty;
}
