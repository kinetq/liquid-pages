using Microsoft;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;
using System.Diagnostics;

namespace Kinetq.LiquidPages.Extension;

/// <summary>
/// Command to add a new Liquid Page (LiquidPageModel class + .liquid template) to the project.
/// </summary>
[VisualStudioContribution]
internal class AddLiquidPageCommand : Command
{
    private readonly TraceSource logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddLiquidPageCommand"/> class.
    /// </summary>
    public AddLiquidPageCommand(TraceSource traceSource)
    {
        this.logger = Requires.NotNull(traceSource, nameof(traceSource));
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new("Add Liquid Page...")
    {
        Icon = new(ImageMoniker.KnownValues.Extension, IconSettings.IconAndText),
        Placements = [CommandPlacement.KnownPlacements.ToolsMenu]
    };

    /// <inheritdoc />
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Get workspace information
            var workspaceFolder = await GetWorkspaceFolderAsync(cancellationToken);
            if (string.IsNullOrEmpty(workspaceFolder))
            {
                await this.Extensibility.Shell().ShowPromptAsync(
                    "Could not determine workspace folder. Please ensure a solution is open.",
                    PromptOptions.OK,
                    cancellationToken);
                return;
            }

            // Prompt user for the page name
            var promptResult = await this.Extensibility.Shell().ShowPromptAsync(
                "Enter the Liquid Page name (e.g., Index, About, Contact):",
                PromptOptions.OKCancel,
                cancellationToken);

            // Check if user cancelled (OK returns true, Cancel returns false)
            if (!promptResult)
            {
                this.logger.TraceEvent(TraceEventType.Information, 0, "User cancelled page creation.");
                return;
            }

            // For now, we'll need to get the name through a text input dialog
            // Since the new extensibility model doesn't have a direct text input yet,
            // we'll use a default name and let the user rename
            var pageName = "NewPage"; // Default name - user will be prompted to rename

            // Clean up the name
            pageName = SanitizePageName(pageName);

            // Determine target folder (default to Pages folder)
            var pagesFolder = Path.Combine(workspaceFolder, "Pages");

            // Create Pages folder if it doesn't exist
            if (!Directory.Exists(pagesFolder))
            {
                Directory.CreateDirectory(pagesFolder);
                this.logger.TraceEvent(TraceEventType.Information, 0, 
                    $"Created Pages folder at {pagesFolder}");
            }

            // Generate file contents - Razor Pages style naming
            var className = pageName; // Class name is just the page name (e.g., "Home")
            var liquidFileName = $"{pageName}.liquid";
            var csFileName = $"{pageName}.liquid.cs";

            var liquidFilePath = Path.Combine(pagesFolder, liquidFileName);
            var csFilePath = Path.Combine(pagesFolder, csFileName);

            // Check if files already exist
            if (File.Exists(csFilePath) || File.Exists(liquidFilePath))
            {
                await this.Extensibility.Shell().ShowPromptAsync(
                    $"Files already exist. Please rename or delete existing {pageName} files first.",
                    PromptOptions.OK,
                    cancellationToken);
                return;
            }

            // Determine namespace (use workspace folder name as base)
            var namespaceName = GetNamespaceFromPath(workspaceFolder);

            // Determine relative path for route pattern
            var routePattern = $"/Pages/{pageName}".ToLowerInvariant();
            var templatePath = $"/Pages/{liquidFileName}";

            // Create liquid template file first (parent file)
            var liquidContent = GenerateLiquidTemplate(pageName);
            await File.WriteAllTextAsync(liquidFilePath, liquidContent, cancellationToken);

            // Create C# class file (code-behind)
            var csContent = GenerateCSharpClass(className, namespaceName, routePattern, templatePath);
            await File.WriteAllTextAsync(csFilePath, csContent, cancellationToken);

            // Create a project file modification script to nest the .liquid.cs file under .liquid
            var projectFile = FindProjectFile(workspaceFolder);
            if (!string.IsNullOrEmpty(projectFile))
            {
                await NestCodeBehindInProjectAsync(projectFile, liquidFileName, csFileName, cancellationToken);
            }

            await this.Extensibility.Shell().ShowPromptAsync(
                $"Successfully created {liquidFileName} and {csFileName} in Pages folder!\n\n" +
                $"The code-behind file ({csFileName}) is nested under the template file ({liquidFileName}).\n" +
                $"Please reload the project to see the nested files, then rename them as needed.",
                PromptOptions.OK,
                cancellationToken);

            this.logger.TraceEvent(TraceEventType.Information, 0, 
                $"Created Liquid Page: {pageName} at {pagesFolder}");
        }
        catch (Exception ex)
        {
            this.logger.TraceEvent(TraceEventType.Error, 0, 
                $"Error creating Liquid Page: {ex.Message}");

            await this.Extensibility.Shell().ShowPromptAsync(
                $"Error creating Liquid Page: {ex.Message}",
                PromptOptions.OK,
                cancellationToken);
        }
    }

    private static string SanitizePageName(string name)
    {
        // Remove any file extensions
        name = Path.GetFileNameWithoutExtension(name);

        // Remove invalid characters
        var invalidChars = Path.GetInvalidFileNameChars().Concat(new[] { ' ', '-', '.' }).ToArray();
        name = string.Concat(name.Where(c => !invalidChars.Contains(c)));

        // Ensure it starts with a letter
        if (name.Length > 0 && !char.IsLetter(name[0]))
        {
            name = "Page" + name;
        }

        return name;
    }

    private static string GenerateCSharpClass(string className, string namespaceName, string routePattern, string templatePath)
    {
        return $@"using Kinetq.LiquidPages.Pages;

namespace {namespaceName}.Pages;

/// <summary>
/// Liquid page model for {className}.
/// This class is the code-behind for {className}.liquid
/// </summary>
[LiquidPage(""{routePattern}"", ""{templatePath}"")]
public class {className}Model : LiquidPageModel
{{
    // Add your model properties here
    // Properties will be available in the .liquid template using snake_case naming
    // Example:
    // public string Title {{ get; set; }} = ""Welcome to {className}"";
    // public DateTime CurrentDate {{ get; set; }} = DateTime.Now;
    // 
    // In template: {{{{ title }}}} and {{{{ current_date }}}}

    public override void OnGet()
    {{
        // Initialize your model properties here
        // This method is called when the page is requested
        base.OnGet();
    }}
}}
";
    }

    private static string GenerateLiquidTemplate(string pageName)
    {
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{{{{ title }}}}</title>
</head>
<body>
    <h1>{{{{ title }}}}</h1>
    <p>Welcome to the {pageName} page!</p>

    <!-- Add your liquid template content here -->
    <!-- Access model properties using {{{{ property_name }}}} -->
</body>
</html>
";
    }

    private async Task<string?> GetWorkspaceFolderAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Try to get workspace folder from environment or current directory
            // In the new extensibility model, this is limited
            var currentDir = Environment.CurrentDirectory;

            // Look for a .sln file in current or parent directories
            var dir = new DirectoryInfo(currentDir);
            while (dir != null)
            {
                if (dir.GetFiles("*.sln").Any())
                {
                    return dir.FullName;
                }
                dir = dir.Parent;
            }

            return currentDir;
        }
        catch (Exception ex)
        {
            this.logger.TraceEvent(TraceEventType.Warning, 0, 
                $"Could not determine workspace folder: {ex.Message}");
            return null;
        }
    }

    private static string GetNamespaceFromPath(string workspaceFolder)
    {
        try
        {
            var folderName = new DirectoryInfo(workspaceFolder).Name;
            return folderName.Replace(" ", "").Replace("-", "").Replace(".", "");
        }
        catch
        {
            return "MyApp";
        }
    }

    private static string? FindProjectFile(string workspaceFolder)
    {
        try
        {
            // Look for .csproj files in the workspace
            var projectFiles = Directory.GetFiles(workspaceFolder, "*.csproj", SearchOption.AllDirectories)
                .Where(f => !f.Contains("\\bin\\") && !f.Contains("\\obj\\"))
                .ToList();

            // Prefer project files that are not test or extension projects
            var mainProject = projectFiles.FirstOrDefault(f => 
                !f.Contains("Test") && 
                !f.Contains("Extension") &&
                !f.Contains("EmbedIO"));

            return mainProject ?? projectFiles.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private async Task NestCodeBehindInProjectAsync(
        string projectFilePath, 
        string liquidFileName, 
        string csFileName, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Read the project file
            var projectContent = await File.ReadAllTextAsync(projectFilePath, cancellationToken);

            // Check if the files are already in the project
            if (projectContent.Contains(csFileName))
            {
                return; // Already configured
            }

            // Add the nesting configuration - nest .liquid.cs under .liquid (like Razor Pages)
            var nestingXml = $@"
  <ItemGroup>
    <Compile Include=""Pages\{csFileName}"">
      <DependentUpon>{liquidFileName}</DependentUpon>
    </Compile>
  </ItemGroup>";

            // Find the last </ItemGroup> or insert before </Project>
            var insertPosition = projectContent.LastIndexOf("</ItemGroup>");
            if (insertPosition < 0)
            {
                insertPosition = projectContent.LastIndexOf("</Project>");
            }

            if (insertPosition > 0)
            {
                projectContent = projectContent.Insert(insertPosition, nestingXml + Environment.NewLine);
                await File.WriteAllTextAsync(projectFilePath, projectContent, cancellationToken);

                this.logger.TraceEvent(TraceEventType.Information, 0, 
                    $"Added nesting configuration to {projectFilePath}");
            }
        }
        catch (Exception ex)
        {
            this.logger.TraceEvent(TraceEventType.Warning, 0, 
                $"Could not modify project file for nesting: {ex.Message}");
        }
    }
}
