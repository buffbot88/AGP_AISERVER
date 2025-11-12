namespace AGP_Studios.IDE.Models;

/// <summary>
/// Code draft model for admin console
/// </summary>
public class CodeDraft
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Language { get; set; } = "csharp";
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastModified { get; set; } = DateTime.Now;
    public bool IsPublished { get; set; }
}
