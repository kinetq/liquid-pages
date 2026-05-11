# Kinetq.LiquidPages.Scaffolder

A .NET CLI tool for scaffolding LiquidPages templates with code-behind files, similar to Razor Pages.

## Installation

### Install as a local tool

```bash
dotnet tool install --global Kinetq.LiquidPages.Scaffolder
```

Or install from a local package:

```bash
dotnet pack src/Kinetq.LiquidPages.Scaffolder
dotnet tool install --global --add-source ./src/Kinetq.LiquidPages.Scaffolder/bin/Debug Kinetq.LiquidPages.Scaffolder
```

### Verify installation

```bash
liquid-pages --help
```

## Usage

### Scaffold a new LiquidPage

The basic command to create a new LiquidPage:

```bash
liquid-pages page <PageName>
```

#### Examples

**Create a page in the project root:**

```bash
liquid-pages page Index
```

This creates:
- `Index.liquid` - The Liquid template file
- `Index.liquid.cs` - The code-behind file with `IndexModel` class

**Create a page in a subdirectory:**

```bash
liquid-pages page About --output Pages
```

or

```bash
liquid-pages page About -o Pages
```

This creates:
- `Pages/About.liquid`
- `Pages/About.liquid.cs`

**Specify a custom project file:**

```bash
liquid-pages page Contact --project path/to/MyProject.csproj
```

or

```bash
liquid-pages page Contact -p path/to/MyProject.csproj
```

**Override the root namespace:**

```bash
liquid-pages page Home --namespace MyApp.Web
```

or

```bash
liquid-pages page Home -n MyApp.Web
```

### Options

| Option | Alias | Description |
|--------|-------|-------------|
| `--output` | `-o` | Output directory relative to the project root (e.g., 'Pages', 'Views/Home') |
| `--project` | `-p` | Path to the .csproj file (if not specified, searches in current directory) |
| `--namespace` | `-n` | Root namespace for generated files (if not specified, reads from project file) |

## Generated Files

### Code-Behind File (`.liquid.cs`)

The generated code-behind file contains:

- A class that inherits from `LiquidPageModel`
- The `[LiquidPage]` attribute with route and template path
- An `OnGet()` method for initialization
- Comments showing how to add properties and use them in templates

Example:

```csharp
using Kinetq.LiquidPages.Pages;

namespace MyProject;

/// <summary>
/// Liquid page model for Index.
/// This class is the code-behind for Index.liquid
/// </summary>
[LiquidPage("/Index", "/MyProject/Index.liquid")]
public class IndexModel : LiquidPageModel
{
    // Add your model properties here
    // Properties will be available in the .liquid template using snake_case naming
    // Example:
    // public string Title { get; set; } = "Welcome to Index";
    // public DateTime CurrentDate { get; set; } = DateTime.Now;
    // 
    // In template: {{ title }} and {{ current_date }}

    public override void OnGet()
    {
        // Initialize your model properties here
        // This method is called when the page is requested
        base.OnGet();
    }
}
```

### Liquid Template File (`.liquid`)

The generated template file contains:

- A basic page structure with `{% capture page_content %}`
- Example HTML content
- An include statement for the default layout
- Comments showing how to use model properties

Example:

```liquid
{% capture page_content %}
    <h1>Index</h1>
    <p>Welcome to the Index page!</p>

    <!-- Add your liquid template content here -->
    <!-- Access model properties using {{ property_name }} -->
    <!-- Example: {{ title }}, {{ current_date }} -->
{% endcapture %}

{% include 'Layouts/default.liquid' %}
```

## Workflow

1. Navigate to your project directory
2. Run the scaffold command with your page name
3. The tool will:
   - Detect your project file
   - Read the root namespace
   - Create both `.liquid` and `.liquid.cs` files
   - Set up the proper namespace and routes
4. Add your model properties in the code-behind
5. Update the Liquid template to use those properties

## Requirements

- .NET 9.0 or later
- A project that references Kinetq.LiquidPages

## Uninstall

```bash
dotnet tool uninstall --global Kinetq.LiquidPages.Scaffolder
```
