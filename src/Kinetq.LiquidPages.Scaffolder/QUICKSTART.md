# Quick Start Guide

## Building and Testing Locally

### 1. Build the project

```bash
cd src/Kinetq.LiquidPages.Scaffolder
dotnet build
```

### 2. Pack the tool

```bash
dotnet pack -c Release
```

### 3. Install the tool locally

```bash
dotnet tool install --global --add-source ./bin/Release Kinetq.LiquidPages.Scaffolder
```

Or update if already installed:

```bash
dotnet tool update --global --add-source ./bin/Release Kinetq.LiquidPages.Scaffolder
```

### 4. Test the tool

Navigate to any .NET project directory and run:

```bash
# Show help
liquid-pages --help
liquid-pages page --help

# Create a simple page
liquid-pages page TestPage

# Create a page in a subfolder
liquid-pages page About -o Pages

# Create a page with custom namespace
liquid-pages page Contact -n MyApp.Web.Pages
```

## Example Output

When you run:

```bash
liquid-pages page Index
```

You'll get:

```
Using project: MyProject.csproj
Using namespace: MyProject
Successfully created LiquidPage:
  - Index.liquid.cs
  - Index.liquid
```

**Index.liquid.cs:**
```csharp
using Kinetq.LiquidPages.Pages;

namespace MyProject;

[LiquidPage("/Index", "/MyProject/Index.liquid")]
public class IndexModel : LiquidPageModel
{
    public override void OnGet()
    {
        base.OnGet();
    }
}
```

**Index.liquid:**
```liquid
{% capture page_content %}
    <h1>Index</h1>
    <p>Welcome to the Index page!</p>
{% endcapture %}

{% include 'Layouts/default.liquid' %}
```

## Uninstalling

```bash
dotnet tool uninstall --global Kinetq.LiquidPages.Scaffolder
```
