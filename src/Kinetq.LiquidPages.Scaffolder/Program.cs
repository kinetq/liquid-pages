using System.CommandLine;
using Kinetq.LiquidPages.Scaffolder;
using Kinetq.LiquidPages.Scaffolder.Services;

var templateService = new TemplateService();
var templateEngine = new TemplateEngine(templateService);

var rootCommand = new RootCommand("LiquidPages Scaffolder - Generate files from templates");

// Add 'list' command to show available templates
var listCommand = new Command("list", "List all available templates");
listCommand.SetHandler(() =>
{
    Console.WriteLine("Available templates:");
    foreach (var templateName in templateService.GetTemplateNames())
    {
        var template = templateService.GetTemplate(templateName);
        if (template != null)
        {
            Console.WriteLine($"  {templateName,-15} - {template.Description}");
        }
    }
});

// Add 'page' command for scaffolding new pages (using LiquidPage template)
var pageCommand = new Command("page", "Scaffold a new LiquidPage with code-behind");

var nameArgument = new Argument<string>(
    name: "name",
    description: "The name of the page to create (e.g., 'Index', 'About', 'Contact')");

var outputOption = new Option<string?>(
    aliases: new[] { "--output", "-o" },
    description: "Output directory relative to the project root (e.g., 'Pages', 'Views/Home')");

var projectOption = new Option<string?>(
    aliases: new[] { "--project", "-p" },
    description: "Path to the .csproj file (if not specified, will search in current directory)");

var namespaceOption = new Option<string?>(
    aliases: new[] { "--namespace", "-n" },
    description: "Root namespace for the generated files (if not specified, will read from project file)");

pageCommand.AddArgument(nameArgument);
pageCommand.AddOption(outputOption);
pageCommand.AddOption(projectOption);
pageCommand.AddOption(namespaceOption);

pageCommand.SetHandler(async (name, output, project, namespaceOverride) =>
{
    var currentDirectory = Directory.GetCurrentDirectory();

    // Find project file
    var projectFile = project ?? ProjectHelper.FindProjectFile(currentDirectory);
    if (projectFile == null)
    {
        Console.Error.WriteLine("Error: Could not find a .csproj file in the current directory or parent directories.");
        Console.Error.WriteLine("Please specify the project file using --project option or run from within a project directory.");
        Environment.ExitCode = 1;
        return;
    }

    Console.WriteLine($"Using project: {Path.GetFileName(projectFile)}");

    // Get project directory
    var projectDirectory = ProjectHelper.GetProjectDirectory(projectFile);

    // Get root namespace
    var rootNamespace = namespaceOverride ?? ProjectHelper.GetRootNamespace(projectFile);
    if (!string.IsNullOrEmpty(rootNamespace))
    {
        Console.WriteLine($"Using namespace: {rootNamespace}");
    }

    // Create generator and scaffold page
    var generator = new ScaffoldGenerator(projectDirectory, rootNamespace, templateEngine);
    var success = await generator.ScaffoldAsync("LiquidPage", name, output);

    Environment.ExitCode = success ? 0 : 1;
}, nameArgument, outputOption, projectOption, namespaceOption);

// Add 'new' command for scaffolding using any template
var newCommand = new Command("new", "Scaffold files using a specified template");

var templateArgument = new Argument<string>(
    name: "template",
    description: "The template to use (e.g., 'LiquidPage')");

var nameArgumentNew = new Argument<string>(
    name: "name",
    description: "The name of the file/item to create");

var outputOptionNew = new Option<string?>(
    aliases: new[] { "--output", "-o" },
    description: "Output directory relative to the project root");

var projectOptionNew = new Option<string?>(
    aliases: new[] { "--project", "-p" },
    description: "Path to the .csproj file (if not specified, will search in current directory)");

var namespaceOptionNew = new Option<string?>(
    aliases: new[] { "--namespace", "-n" },
    description: "Root namespace for the generated files (if not specified, will read from project file)");

newCommand.AddArgument(templateArgument);
newCommand.AddArgument(nameArgumentNew);
newCommand.AddOption(outputOptionNew);
newCommand.AddOption(projectOptionNew);
newCommand.AddOption(namespaceOptionNew);

newCommand.SetHandler(async (template, name, output, project, namespaceOverride) =>
{
    // Check if template exists
    if (!templateService.TemplateExists(template))
    {
        Console.Error.WriteLine($"Error: Template '{template}' not found.");
        Console.WriteLine("\nAvailable templates:");
        foreach (var templateName in templateService.GetTemplateNames())
        {
            Console.WriteLine($"  - {templateName}");
        }
        Environment.ExitCode = 1;
        return;
    }

    var currentDirectory = Directory.GetCurrentDirectory();

    // Find project file
    var projectFile = project ?? ProjectHelper.FindProjectFile(currentDirectory);
    if (projectFile == null)
    {
        Console.Error.WriteLine("Error: Could not find a .csproj file in the current directory or parent directories.");
        Console.Error.WriteLine("Please specify the project file using --project option or run from within a project directory.");
        Environment.ExitCode = 1;
        return;
    }

    Console.WriteLine($"Using project: {Path.GetFileName(projectFile)}");
    Console.WriteLine($"Using template: {template}");

    // Get project directory
    var projectDirectory = ProjectHelper.GetProjectDirectory(projectFile);

    // Get root namespace
    var rootNamespace = namespaceOverride ?? ProjectHelper.GetRootNamespace(projectFile);
    if (!string.IsNullOrEmpty(rootNamespace))
    {
        Console.WriteLine($"Using namespace: {rootNamespace}");
    }

    // Create generator and scaffold
    var generator = new ScaffoldGenerator(projectDirectory, rootNamespace, templateEngine);
    var success = await generator.ScaffoldAsync(template, name, output);

    Environment.ExitCode = success ? 0 : 1;
}, templateArgument, nameArgumentNew, outputOptionNew, projectOptionNew, namespaceOptionNew);

rootCommand.AddCommand(listCommand);
rootCommand.AddCommand(pageCommand);
rootCommand.AddCommand(newCommand);

return await rootCommand.InvokeAsync(args);


return await rootCommand.InvokeAsync(args);

