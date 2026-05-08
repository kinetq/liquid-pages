# Liquid Pages vs Razor Pages - Naming Convention Reference

## Side-by-Side Comparison

| Component | Razor Pages | Liquid Pages |
|-----------|-------------|--------------|
| **Template Extension** | `.cshtml` | `.liquid` |
| **Code-Behind Extension** | `.cshtml.cs` | `.liquid.cs` |
| **Base Class** | `PageModel` | `LiquidPageModel` |
| **Attribute** | `[PageAttribute]` (optional) | `[LiquidPage]` (required) |

## Example: Home Page

### Razor Pages Structure
```
Pages/
  └─ Index.cshtml
     └─ Index.cshtml.cs
```

**Index.cshtml.cs**
```csharp
public class IndexModel : PageModel
{
    public string Title { get; set; } = "Home";

    public void OnGet()
    {
        // Initialize
    }
}
```

**Index.cshtml**
```html
@page
@model IndexModel

<h1>@Model.Title</h1>
```

### Liquid Pages Structure
```
Pages/
  └─ Home.liquid
     └─ Home.liquid.cs
```

**Home.liquid.cs**
```csharp
[LiquidPage("/home", "/Pages/Home.liquid")]
public class HomeModel : LiquidPageModel
{
    public string Title { get; set; } = "Home";

    public override void OnGet()
    {
        // Initialize
        base.OnGet();
    }
}
```

**Home.liquid**
```html
<!DOCTYPE html>
<html>
<body>
    <h1>{{ title }}</h1>
</body>
</html>
```

## Naming Patterns

### File Names

| Page Purpose | Razor Pages | Liquid Pages |
|--------------|-------------|--------------|
| Home/Index | `Index.cshtml` | `Home.liquid` or `Index.liquid` |
| About | `About.cshtml` | `About.liquid` |
| Contact | `Contact.cshtml` | `Contact.liquid` |
| Privacy | `Privacy.cshtml` | `Privacy.liquid` |

### Class Names

| Page Name | Razor Model Class | Liquid Model Class |
|-----------|-------------------|-------------------|
| Index | `IndexModel` | `IndexModel` or `HomeModel` |
| About | `AboutModel` | `AboutModel` |
| Contact | `ContactModel` | `ContactModel` |
| Privacy | `PrivacyModel` | `PrivacyModel` |

**Pattern**: `{PageName}Model`

### Property Access

| C# Property | Razor Syntax | Liquid Syntax |
|-------------|--------------|---------------|
| `Title` | `@Model.Title` | `{{ title }}` |
| `FirstName` | `@Model.FirstName` | `{{ first_name }}` |
| `IsActive` | `@Model.IsActive` | `{{ is_active }}` |
| `CreatedDate` | `@Model.CreatedDate` | `{{ created_date }}` |
| `TotalCount` | `@Model.TotalCount` | `{{ total_count }}` |

**Liquid Convention**: PascalCase → snake_case

## Full Example: Product Details Page

### Razor Pages

**ProductDetails.cshtml.cs**
```csharp
namespace MyStore.Pages;

public class ProductDetailsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public bool InStock { get; set; }

    public void OnGet()
    {
        // Load product by Id
        ProductName = "Sample Product";
        Price = 29.99m;
        InStock = true;
    }
}
```

**ProductDetails.cshtml**
```html
@page "/product/{id:int}"
@model ProductDetailsModel

<h1>@Model.ProductName</h1>
<p>Price: $@Model.Price.ToString("F2")</p>
<p>@(Model.InStock ? "In Stock" : "Out of Stock")</p>
```

### Liquid Pages

**ProductDetails.liquid.cs**
```csharp
namespace MyStore.Pages;

[LiquidPage("/product/{id:int}", "/Pages/ProductDetails.liquid")]
public class ProductDetailsModel : LiquidPageModel
{
    public int Id { get; set; }

    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public bool InStock { get; set; }

    public override void OnGet()
    {
        // Load product by Id
        ProductName = "Sample Product";
        Price = 29.99m;
        InStock = true;

        base.OnGet();
    }
}
```

**ProductDetails.liquid**
```html
<h1>{{ product_name }}</h1>
<p>Price: ${{ price | round: 2 }}</p>
{% if in_stock %}
    <p>In Stock</p>
{% else %}
    <p>Out of Stock</p>
{% endif %}
```

## Key Differences

### 1. Property Access

**Razor**: Uses `@Model.PropertyName` syntax with C# expressions
```csharp
@Model.Price.ToString("F2")
@Model.Items.Count()
```

**Liquid**: Uses `{{ property_name }}` with filters
```liquid
{{ price | round: 2 }}
{{ items | size }}
```

### 2. Conditionals

**Razor**: C# syntax
```csharp
@if (Model.IsActive)
{
    <p>Active</p>
}
```

**Liquid**: Liquid syntax
```liquid
{% if is_active %}
    <p>Active</p>
{% endif %}
```

### 3. Loops

**Razor**: C# foreach
```csharp
@foreach (var item in Model.Items)
{
    <li>@item.Name</li>
}
```

**Liquid**: Liquid for loop
```liquid
{% for item in items %}
    <li>{{ item.name }}</li>
{% endfor %}
```

### 4. Attribute Configuration

**Razor**: Route in template with `@page`
```html
@page "/product/{id:int}"
```

**Liquid**: Route in `[LiquidPage]` attribute
```csharp
[LiquidPage("/product/{id:int}", "/Pages/ProductDetails.liquid")]
```

## File Structure in Solution Explorer

### Razor Pages
```
📁 Pages
  📄 Index.cshtml
    📄 Index.cshtml.cs
  📄 About.cshtml
    📄 About.cshtml.cs
  📄 Contact.cshtml
    📄 Contact.cshtml.cs
```

### Liquid Pages
```
📁 Pages
  📄 Home.liquid
    📄 Home.liquid.cs
  📄 About.liquid
    📄 About.liquid.cs
  📄 Contact.liquid
    📄 Contact.liquid.cs
```

## Best Practices

### 1. Consistent Naming
- Use the same base name for template and code-behind
- ✅ `Home.liquid` + `Home.liquid.cs`
- ❌ `Home.liquid` + `HomePage.liquid.cs`

### 2. Class Naming
- Always append "Model" to the page name
- ✅ `HomeModel`, `AboutModel`, `ContactModel`
- ❌ `Home`, `AboutPage`, `ContactPageModel`

### 3. Folder Organization
- Keep all pages in the `Pages` folder
- Use subfolders for logical grouping
```
Pages/
  ├─ Home.liquid
  ├─ About.liquid
  └─ Products/
      ├─ Index.liquid
      └─ Details.liquid
```

### 4. Property Naming
- Use PascalCase in C# (standard C# convention)
- Trust the automatic snake_case conversion for Liquid
```csharp
public string FirstName { get; set; }  // Becomes {{ first_name }} in Liquid
```

## Migration from Razor to Liquid

If you're familiar with Razor Pages and migrating to Liquid Pages:

1. **Rename files**: `.cshtml` → `.liquid`, `.cshtml.cs` → `.liquid.cs`
2. **Update class**: `PageModel` → `LiquidPageModel`
3. **Add attribute**: `[LiquidPage("/route", "/Path/To/Template.liquid")]`
4. **Convert template syntax**:
   - `@Model.Property` → `{{ property }}`
   - `@if` → `{% if %}`
   - `@foreach` → `{% for %}`
5. **Update nesting**: Change `DependentUpon` in `.csproj`

## See Also

- [Razor Pages Documentation](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/)
- [Liquid Template Language](https://shopify.github.io/liquid/)
- [Add Liquid Page Command](ADD_LIQUID_PAGE.md)
