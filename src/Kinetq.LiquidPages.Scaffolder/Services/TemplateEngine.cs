using System.Text.RegularExpressions;
using Kinetq.LiquidPages.Scaffolder.Models;

namespace Kinetq.LiquidPages.Scaffolder.Services;

/// <summary>
/// Engine for processing templates and replacing parameters
/// </summary>
public class TemplateEngine
{
    private readonly TemplateService _templateService;

    public TemplateEngine(TemplateService templateService)
    {
        _templateService = templateService;
    }

    /// <summary>
    /// Processes a template and generates files
    /// </summary>
    public async Task<bool> ProcessTemplateAsync(
        string templateName,
        Dictionary<string, string> parameters,
        string outputDirectory)
    {
        var template = _templateService.GetTemplate(templateName);
        if (template == null)
        {
            Console.Error.WriteLine($"Template '{templateName}' not found.");
            return false;
        }

        // Validate required parameters
        foreach (var param in template.Parameters)
        {
            if (param.Value.Required && !parameters.ContainsKey(param.Key))
            {
                Console.Error.WriteLine($"Required parameter '{param.Key}' is missing.");
                return false;
            }
        }

        // Apply default values for missing parameters
        foreach (var param in template.Parameters)
        {
            if (!parameters.ContainsKey(param.Key) && !string.IsNullOrEmpty(param.Value.Default))
            {
                parameters[param.Key] = param.Value.Default;
            }
        }

        // Ensure output directory exists
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        // Process each file in the template
        var createdFiles = new List<string>();
        foreach (var file in template.Files)
        {
            try
            {
                // Load source file content
                var sourceContent = _templateService.LoadTemplateFile(templateName, file.Source);
                if (sourceContent == null)
                {
                    Console.Error.WriteLine($"Could not load template file: {file.Source}");
                    return false;
                }

                // Replace parameters in content if needed
                var processedContent = file.ReplaceParameters
                    ? ReplaceParameters(sourceContent, parameters)
                    : sourceContent;

                // Replace parameters in target filename
                var targetFileName = ReplaceParameters(file.Target, parameters);
                var targetPath = Path.Combine(outputDirectory, targetFileName);

                // Check if file already exists
                if (File.Exists(targetPath))
                {
                    Console.Error.WriteLine($"File already exists: {targetPath}");
                    return false;
                }

                // Write the file
                await File.WriteAllTextAsync(targetPath, processedContent);
                createdFiles.Add(targetPath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error processing file '{file.Source}': {ex.Message}");
                return false;
            }
        }

        // Report success
        Console.WriteLine($"Successfully created {template.Name}:");
        foreach (var file in createdFiles)
        {
            Console.WriteLine($"  - {Path.GetFileName(file)}");
        }

        return true;
    }

    /// <summary>
    /// Replaces parameters in content using {{ParameterName}} syntax
    /// </summary>
    private string ReplaceParameters(string content, Dictionary<string, string> parameters)
    {
        var result = content;

        foreach (var param in parameters)
        {
            var pattern = $"{{{{{param.Key}}}}}";
            result = result.Replace(pattern, param.Value);
        }

        return result;
    }
}
