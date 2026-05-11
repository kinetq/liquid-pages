using System.Text.RegularExpressions;
using Kinetq.LiquidPages.Scaffolder.Services;

namespace Kinetq.LiquidPages.Scaffolder;

/// <summary>
/// Generates files from templates
/// </summary>
public class ScaffoldGenerator
{
    private readonly string _projectDirectory;
    private readonly string? _rootNamespace;
    private readonly TemplateEngine _templateEngine;

    public ScaffoldGenerator(string projectDirectory, string? rootNamespace, TemplateEngine templateEngine)
    {
        _projectDirectory = projectDirectory;
        _rootNamespace = rootNamespace;
        _templateEngine = templateEngine;
    }

    /// <summary>
    /// Scaffolds using a template
    /// </summary>
    /// <param name="templateName">The name of the template to use</param>
    /// <param name="fileName">The name of the file/page (e.g., "Index", "About")</param>
    /// <param name="outputPath">Optional output path relative to project directory</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> ScaffoldAsync(string templateName, string fileName, string? outputPath = null)
    {
        try
        {
            // Sanitize file name
            var sanitizedFileName = SanitizeFileName(fileName);
            if (string.IsNullOrEmpty(sanitizedFileName))
            {
                Console.Error.WriteLine("Invalid file name provided.");
                return false;
            }

            // Determine output directory
            var targetDirectory = string.IsNullOrEmpty(outputPath)
                ? _projectDirectory
                : Path.Combine(_projectDirectory, outputPath);

            // Determine namespace
            var namespaceName = DetermineNamespace(outputPath);

            // Calculate route and template paths for LiquidPage
            var routePath = DetermineRoutePath(sanitizedFileName, outputPath);
            var templatePath = DetermineTemplatePath(sanitizedFileName, namespaceName, outputPath);

            // Build parameters for template engine
            var parameters = new Dictionary<string, string>
            {
                ["FileName"] = sanitizedFileName,
                ["Namespace"] = namespaceName,
                ["OutputPath"] = outputPath ?? string.Empty,
                ["RoutePath"] = routePath,
                ["TemplatePath"] = templatePath
            };

            // Process the template
            return await _templateEngine.ProcessTemplateAsync(templateName, parameters, targetDirectory);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error scaffolding: {ex.Message}");
            return false;
        }
    }

    private string SanitizeFileName(string fileName)
    {
        // Remove invalid characters and ensure valid C# identifier
        var sanitized = Regex.Replace(fileName, @"[^\w\d_]", "");

        // Ensure it starts with a letter or underscore
        if (!string.IsNullOrEmpty(sanitized) && char.IsDigit(sanitized[0]))
        {
            sanitized = "_" + sanitized;
        }

        return sanitized;
    }

    private string DetermineNamespace(string? outputPath)
    {
        if (!string.IsNullOrEmpty(_rootNamespace))
        {
            if (!string.IsNullOrEmpty(outputPath))
            {
                // Convert path separators to namespace separators
                var namespaceSuffix = outputPath.Replace(Path.DirectorySeparatorChar, '.')
                                                .Replace(Path.AltDirectorySeparatorChar, '.');
                return $"{_rootNamespace}.{namespaceSuffix}";
            }
            return _rootNamespace;
        }

        // Fallback to directory-based namespace
        var projectName = Path.GetFileName(_projectDirectory) ?? "DefaultNamespace";
        if (!string.IsNullOrEmpty(outputPath))
        {
            var namespaceSuffix = outputPath.Replace(Path.DirectorySeparatorChar, '.')
                                            .Replace(Path.AltDirectorySeparatorChar, '.');
            return $"{projectName}.{namespaceSuffix}";
        }
        return projectName;
    }

    private string DetermineRoutePath(string fileName, string? outputPath)
    {
        if (string.IsNullOrEmpty(outputPath))
        {
            return $"/{fileName}";
        }

        var normalizedPath = outputPath.Replace(Path.DirectorySeparatorChar, '/')
                                      .Replace(Path.AltDirectorySeparatorChar, '/');
        return $"/{normalizedPath}/{fileName}";
    }

    private string DetermineTemplatePath(string fileName, string namespaceName, string? outputPath)
    {
        if (string.IsNullOrEmpty(outputPath))
        {
            return $"/{namespaceName}/{fileName}.liquid";
        }

        return $"/{namespaceName.Replace('.', '/')}/{fileName}.liquid";
    }
}
