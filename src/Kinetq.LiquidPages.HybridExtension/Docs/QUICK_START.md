# Quick Start Guide - Adding Liquid Pages

## Two Ways to Add Liquid Pages

### Method 1: Item Template (Recommended) 🎯

**Best for**: Natural Visual Studio workflow, works anywhere

**Setup (One-time):**
```
1. Tools > Install Liquid Page Template...
2. Click OK
3. Restart Visual Studio
```

**Alternative setup:**
```powershell
# Or run PowerShell script
.\Install-ItemTemplate.ps1 -VSVersion 2026
```

**Usage:**
1. Right-click on any folder in Solution Explorer
2. Select **Add > New Item** (or press Ctrl+Shift+A)
3. Search for **"Liquid Page"**
4. Enter name: `Home`
5. Click **Add**

**Result:**
```
YourFolder/
  ├─ Home.liquid         ← Template
  └─ Home.liquid.cs      ← Code-behind (needs manual nesting*)
```

*After creation, add to `.csproj`:
```xml
<ItemGroup>
  <Compile Include="YourFolder\Home.liquid.cs">
    <DependentUpon>Home.liquid</DependentUpon>
  </Compile>
</ItemGroup>
```

### Method 2: Extension Command

**Best for**: Automatic nesting, no template installation needed

**Usage:**
1. Go to **Tools > Add Liquid Page...**
2. Click OK (uses default name "NewPage")
3. Files created in `Pages` folder with automatic nesting
4. Reload project
5. Rename files as needed

**Result:**
```
Pages/
  ├─ NewPage.liquid
  │  └─ NewPage.liquid.cs  ← Automatically nested!
```

## Comparison

| Feature | Item Template | Extension Command |
|---------|--------------|-------------------|
| Setup Required | One-time installation | None |
| Location | Any folder you choose | Always in `Pages` folder |
| File Nesting | Manual `.csproj` edit | ✅ Automatic |
| Naming | ✅ You choose the name | ⚠️ Default "NewPage" |
| VS Experience | ✅ Native "Add > New Item" | Custom command |
| Workflow | Right-click folder | Tools menu |

## Recommended Workflow

**For new projects:**
1. Install the item template once
2. Use **Add > New Item** for all pages
3. Manually add nesting to `.csproj` (one-time per page)

**For quick testing:**
1. Use **Tools > Add Liquid Page**
2. Automatic nesting
3. Rename files after creation

## Complete Example: Creating a Contact Page

### Using Item Template

```powershell
# 1. Right-click Pages folder
# 2. Add > New Item > Liquid Page
# 3. Name: Contact
# 4. Edit .csproj:
```

```xml
<ItemGroup>
  <Compile Include="Pages\Contact.liquid.cs">
    <DependentUpon>Contact.liquid</DependentUpon>
  </Compile>
</ItemGroup>
```

```powershell
# 5. Reload project
```

### Using Extension Command

```powershell
# 1. Tools > Add Liquid Page
# 2. Confirm dialog
# 3. Reload project
# 4. Rename NewPage.liquid to Contact.liquid
# 5. Rename NewPage.liquid.cs to Contact.liquid.cs
# 6. Update class name from NewPageModel to ContactModel
```

## File Structure

Both methods create:

**Contact.liquid:**
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <title>{{ title }}</title>
</head>
<body>
    <h1>Contact</h1>
    <p>Welcome to the Contact page!</p>
</body>
</html>
```

**Contact.liquid.cs:**
```csharp
using Kinetq.LiquidPages.Pages;

namespace YourApp.Pages;

[LiquidPage("/contact", "/Pages/Contact.liquid")]
public class ContactModel : LiquidPageModel
{
    public string Title { get; set; } = "Contact Us";
    public string Email { get; set; } = "contact@example.com";

    public override void OnGet()
    {
        base.OnGet();
    }
}
```

## Troubleshooting

### Item template doesn't appear

```powershell
# Clear VS cache
Remove-Item "$env:LOCALAPPDATA\Microsoft\VisualStudio\*\ComponentModelCache" -Recurse -Force

# Or run
devenv /installvstemplates
```

### Files not nested (Item Template)

Edit your `.csproj`:
```xml
<ItemGroup>
  <Compile Include="Path\To\YourPage.liquid.cs">
    <DependentUpon>YourPage.liquid</DependentUpon>
  </Compile>
</ItemGroup>
```

### Files not nested (Extension Command)

Reload the project (right-click project > Reload Project)

## Tips

💡 **Create Pages folder first** - Keep all pages organized  
💡 **Use descriptive names** - `Home`, `About`, `Contact`, not `Page1`  
💡 **Follow Razor naming** - Helps team members familiar with Razor Pages  
💡 **Add Title property** - Make it easy to set page titles  

## Next Steps

- [Complete Item Template Documentation](ITEM_TEMPLATE_INSTALL.md)
- [Extension Command Documentation](ADD_LIQUID_PAGE.md)
- [Naming Conventions](NAMING_CONVENTION.md)
- [Migration from Razor Pages](NAMING_CONVENTION.md#migration-from-razor-to-liquid)

## Quick Reference

| Task | Item Template | Extension Command |
|------|--------------|-------------------|
| Add new page | Right-click → Add > New Item | Tools > Add Liquid Page |
| Choose location | ✅ Any folder | ⚠️ Always `Pages` |
| Choose name | ✅ Dialog prompt | ⚠️ Default "NewPage" |
| Auto-nesting | ❌ Manual | ✅ Automatic |
| Setup needed | ✅ One-time install | ❌ None |

Choose the method that fits your workflow!
