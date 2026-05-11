using System.Xml.Linq;

namespace Kinetq.LiquidPages.Scaffolder;

/// <summary>
/// Helper class to detect and read project information
/// </summary>
public class ProjectHelper
{
    /// <summary>
    /// Finds the nearest .csproj file in the current directory or parent directories
    /// </summary>
    /// <param name="startDirectory">The directory to start searching from</param>
    /// <returns>The path to the .csproj file, or null if not found</returns>
    public static string? FindProjectFile(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);

        while (directory != null)
        {
            var projectFiles = directory.GetFiles("*.csproj", SearchOption.TopDirectoryOnly);
            if (projectFiles.Length > 0)
            {
                return projectFiles[0].FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    /// <summary>
    /// Extracts the root namespace from a .csproj file
    /// </summary>
    /// <param name="projectFilePath">Path to the .csproj file</param>
    /// <returns>The root namespace, or null if not found</returns>
    public static string? GetRootNamespace(string projectFilePath)
    {
        try
        {
            var doc = XDocument.Load(projectFilePath);

            // Try to find RootNamespace element
            var rootNamespaceElement = doc.Descendants("RootNamespace").FirstOrDefault();
            if (rootNamespaceElement != null && !string.IsNullOrWhiteSpace(rootNamespaceElement.Value))
            {
                return rootNamespaceElement.Value;
            }

            // Fallback to project file name without extension
            return Path.GetFileNameWithoutExtension(projectFilePath);
        }
        catch
        {
            // If we can't read the project file, return null
            return null;
        }
    }

    /// <summary>
    /// Gets the project directory from a .csproj file path
    /// </summary>
    /// <param name="projectFilePath">Path to the .csproj file</param>
    /// <returns>The directory containing the project file</returns>
    public static string GetProjectDirectory(string projectFilePath)
    {
        return Path.GetDirectoryName(projectFilePath) ?? Directory.GetCurrentDirectory();
    }
}
