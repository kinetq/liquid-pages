namespace Kinetq.LiquidPages.Scaffolder.Models;

/// <summary>
/// Represents a template definition
/// </summary>
public class TemplateDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string DefaultName { get; set; } = string.Empty;
    public List<TemplateFile> Files { get; set; } = new();
    public Dictionary<string, TemplateParameter> Parameters { get; set; } = new();
}

/// <summary>
/// Represents a file in a template
/// </summary>
public class TemplateFile
{
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public bool ReplaceParameters { get; set; } = true;
}

/// <summary>
/// Represents a template parameter
/// </summary>
public class TemplateParameter
{
    public string Description { get; set; } = string.Empty;
    public bool Required { get; set; } = false;
    public string Default { get; set; } = string.Empty;
}
