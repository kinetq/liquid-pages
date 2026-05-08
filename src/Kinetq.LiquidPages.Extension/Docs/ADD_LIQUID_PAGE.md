# Add Liquid Page Command

## Overview

The **Add Liquid Page** command creates a new Liquid Page following the Razor Pages convention, where a template file (`.liquid`) has a code-behind file (`.liquid.cs`) nested underneath it.

## Usage

1. Open your project in Visual Studio
2. Go to **Tools > Add Liquid Page...**
3. Enter a page name (e.g., "Home", "About", "Contact")
4. The command creates two files in the `Pages` folder:
   - `PageName.liquid` - The Liquid template
   - `PageName.liquid.cs` - The code-behind model class
5. Reload the project to see the nested file structure

## File Structure

### Razor Pages Style Convention

Like ASP.NET Core Razor Pages, Liquid Pages follow a convention where the code-behind is nested under the template:

```
Pages/
  ├─ Home.liquid                    ← Template file (parent)
  │  └─ Home.liquid.cs              ← Code-behind (nested)
  ├─ About.liquid
  │  └─ About.liquid.cs
  └─ Contact.liquid
     └─ Contact.liquid.cs
```

This is implemented using MSBuild's `DependentUpon` metadata in the project file:

```xml
<ItemGroup>
  <Compile Include="Pages\Home.liquid.cs">
    <DependentUpon>Home.liquid</DependentUpon>
  </Compile>
</ItemGroup>
```

## Generated Files

### Template File (Home.liquid)

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>{{ title }}</title>
</head>
<body>
    <h1>{{ title }}</h1>
    <p>Welcome to the Home page!</p>

    <!-- Add your liquid template content here -->
    <!-- Access model properties using {{ property_name }} -->
</body>
</html>
```

### Code-Behind File (Home.liquid.cs)

```csharp
using Kinetq.LiquidPages.Pages;

namespace MyApp.Pages;

/// <summary>
/// Liquid page model for Home.
/// This class is the code-behind for Home.liquid
/// </summary>
[LiquidPage("/pages/home", "/Pages/Home.liquid")]
public class HomeModel : LiquidPageModel
{
    // Add your model properties here
    // Properties will be available in the .liquid template using snake_case naming
    // Example:
    // public string Title { get; set; } = "Welcome to Home";
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

## Naming Conventions

### Class Names

Following the Razor Pages pattern:
- Page name: **Home**
- Template file: **Home.liquid**
- Code-behind file: **Home.liquid.cs**
- Class name: **HomeModel**

This mirrors Razor Pages where:
- `Index.cshtml` → `IndexModel`
- `Contact.cshtml` → `ContactModel`

### Property Names

C# properties are automatically converted to snake_case in Liquid templates:

| C# Property | Liquid Template |
|------------|-----------------|
| `Title` | `{{ title }}` |
| `FirstName` | `{{ first_name }}` |
| `CreatedDate` | `{{ created_date }}` |
| `IsActive` | `{{ is_active }}` |

## Example: Creating a Contact Page

1. Run the **Add Liquid Page** command
2. Enter "Contact" as the page name
3. Files created:
   - `Pages/Contact.liquid`
   - `Pages/Contact.liquid.cs` (nested under Contact.liquid)

4. Edit `Contact.liquid.cs`:

```csharp
using Kinetq.LiquidPages.Pages;

namespace MyApp.Pages;

[LiquidPage("/contact", "/Pages/Contact.liquid")]
public class ContactModel : LiquidPageModel
{
    public string PageTitle { get; set; } = "Contact Us";
    public string Email { get; set; } = "contact@example.com";
    public string Phone { get; set; } = "(555) 123-4567";

    public override void OnGet()
    {
        // You can add logic here if needed
        base.OnGet();
    }
}
```

5. Edit `Contact.liquid`:

```html
<!DOCTYPE html>
<html>
<head>
    <title>{{ page_title }}</title>
</head>
<body>
    <h1>{{ page_title }}</h1>
    <div class="contact-info">
        <p>Email: <a href="mailto:{{ email }}">{{ email }}</a></p>
        <p>Phone: {{ phone }}</p>
    </div>
</body>
</html>
```

## Project File Updates

The command automatically updates your `.csproj` file to nest the code-behind:

```xml
<ItemGroup>
  <Compile Include="Pages\Contact.liquid.cs">
    <DependentUpon>Contact.liquid</DependentUpon>
  </Compile>
</ItemGroup>
```

After running the command, **reload the project** to see the nested structure in Solution Explorer.

## Comparison with Razor Pages

| Aspect | Razor Pages | Liquid Pages |
|--------|-------------|--------------|
| Template file | `.cshtml` | `.liquid` |
| Code-behind | `.cshtml.cs` | `.liquid.cs` |
| Model class | `PageModel` | `LiquidPageModel` |
| Property access | `@Model.Title` | `{{ title }}` |
| Naming convention | PascalCase in template | snake_case in template |
| Nesting | Code-behind under template | Code-behind under template ✓ |

## Limitations

### Current Limitations

1. **Manual name entry**: The current implementation uses a default name "NewPage" due to limitations in the new extensibility model's prompt API. You'll need to rename the files after creation.

2. **Manual project reload**: You need to reload the project to see the nested file structure.

3. **Fixed location**: Files are created in the `Pages` folder. Custom folder selection will be added in a future update.

### Future Enhancements

- Text input dialog for custom page names
- Context menu integration (right-click > Add > Liquid Page)
- Custom folder selection
- Template customization
- Snippet support

## Troubleshooting

### Files not nested in Solution Explorer

**Solution**: Reload the project (right-click project > Reload Project)

### Files already exist error

**Solution**: Delete or rename the existing files before creating a new page with the same name

### Namespace incorrect

**Solution**: Manually update the namespace in the generated `.liquid.cs` file to match your project structure

## Manual File Creation

If you prefer to create files manually, follow this structure:

1. Create `PageName.liquid` in the `Pages` folder
2. Create `PageName.liquid.cs` in the same folder
3. Add to your `.csproj`:

```xml
<ItemGroup>
  <Compile Include="Pages\PageName.liquid.cs">
    <DependentUpon>PageName.liquid</DependentUpon>
  </Compile>
</ItemGroup>
```

4. Reload the project

## See Also

- [Razor Pages Documentation](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/)
- [Liquid Template Language](https://shopify.github.io/liquid/)
- [MSBuild DependentUpon](https://learn.microsoft.com/en-us/visualstudio/msbuild/common-msbuild-project-items)
