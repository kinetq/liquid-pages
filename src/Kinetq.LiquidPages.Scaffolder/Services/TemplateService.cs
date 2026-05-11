using System.Reflection;
using System.Text.Json;
using Kinetq.LiquidPages.Scaffolder.Models;

namespace Kinetq.LiquidPages.Scaffolder.Services;

/// <summary>
/// Service for loading and managing templates
/// </summary>
public class TemplateService
{
    private const string TemplatesNamespace = "Kinetq.LiquidPages.Scaffolder.Templates";
    private readonly Dictionary<string, TemplateDefinition> _templates = new();

    public TemplateService()
    {
        LoadTemplates();
    }

    /// <summary>
    /// Gets all available template names
    /// </summary>
    public IEnumerable<string> GetTemplateNames() => _templates.Keys;

    /// <summary>
    /// Gets a template definition by name
    /// </summary>
    public TemplateDefinition? GetTemplate(string name)
    {
        _templates.TryGetValue(name, out var template);
        return template;
    }

    /// <summary>
    /// Checks if a template exists
    /// </summary>
    public bool TemplateExists(string name) => _templates.ContainsKey(name);

    /// <summary>
    /// Loads a template file content from embedded resources
    /// </summary>
    public string? LoadTemplateFile(string templateName, string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{TemplatesNamespace}.{templateName}.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private void LoadTemplates()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();

        // Find all template.json files
        var templateJsonResources = resourceNames
            .Where(r => r.StartsWith(TemplatesNamespace) && r.EndsWith("template.json"))
            .ToList();

        foreach (var resourceName in templateJsonResources)
        {
            try
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) continue;

                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();

                var template = JsonSerializer.Deserialize<TemplateDefinition>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (template != null && !string.IsNullOrEmpty(template.Name))
                {
                    _templates[template.Name] = template;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error loading template from {resourceName}: {ex.Message}");
            }
        }
    }
}
