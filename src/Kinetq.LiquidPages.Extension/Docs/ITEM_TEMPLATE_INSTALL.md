# Installing the Liquid Page Item Template

## Overview

The Liquid Page item template allows you to add new Liquid Pages via **Add > New Item** in Visual Studio.

## Installation Methods

### Method 1: Extension Command (Easiest) ⭐

The extension includes a command to automatically install the template:

1. **Run the command**: **Tools > Install Liquid Page Template...**
2. **Confirm** the dialog (or choose to reinstall if already installed)
3. **Restart Visual Studio**
4. **Use the template:**
   - Right-click on a folder in Solution Explorer
   - Select **Add > New Item** (Ctrl+Shift+A)
   - Search for "Liquid Page" or find it under Visual C# items
   - Enter a name (e.g., "Home")
   - Click Add

This command:
- ✅ Automatically detects your Visual Studio version (2022/2026)
- ✅ Copies template files to the correct location
- ✅ Handles overwrites if already installed
- ✅ Shows clear error messages if something goes wrong

### Method 2: PowerShell Script

If you prefer command-line installation:

1. **Open PowerShell** in the extension directory
2. **Run the installation script:**
   ```powershell
   .\Install-ItemTemplate.ps1 -VSVersion 2026
   ```
3. **Restart Visual Studio**

### Method 3: Manual Installation

For full control, manually install the template:

1. **Locate the template files** in the extension output directory:
   ```
   src\Kinetq.LiquidPages.Extension\bin\Debug\net8.0-windows8.0\ItemTemplates\LiquidPage\
   ```

2. **Copy the entire `LiquidPage` folder** to your Visual Studio item templates directory:

   **For Visual Studio 2022:**
   ```
   %USERPROFILE%\Documents\Visual Studio 2022\Templates\ItemTemplates\Visual C#\
   ```

   **For Visual Studio 2026:**
   ```
   %USERPROFILE%\Documents\Visual Studio 2026\Templates\ItemTemplates\Visual C#\
   ```

3. **Restart Visual Studio**

4. **Use the template:**
   - Right-click on a folder in Solution Explorer
   - Select **Add > New Item**
   - Search for "Liquid Page" or find it under Visual C# items
   - Enter a name (e.g., "Home")
   - Click Add

### Method 2: Export as Template (Alternative)

You can also use Visual Studio's built-in template export:

1. Create a sample Liquid Page manually:
   - `Sample.liquid`
   - `Sample.liquid.cs`

2. Go to **Project > Export Template**

3. Choose **Item Template**

4. Select both files and configure:
   - Name: "Liquid Page"
   - Description: "Creates a new Liquid Page with code-behind"
   - Icon: (optional)

5. Finish the wizard

6. The template will be available in **Add > New Item**

## Template Structure

The template includes:

### Files Created
```
📄 YourPageName.liquid          ← Template file
📄 YourPageName.liquid.cs       ← Code-behind (should be nested)
```

### LiquidPage.vstemplate
```xml
<?xml version="1.0" encoding="utf-8"?>
<VSTemplate Version="3.0.0" Type="Item">
  <TemplateData>
    <Name>Liquid Page</Name>
    <Description>Creates a new Liquid Page with code-behind (Razor Pages style)</Description>
    <ProjectType>CSharp</ProjectType>
    <DefaultName>NewPage</DefaultName>
  </TemplateData>
  <TemplateContent>
    <ProjectItem TargetFileName="$fileinputname$.liquid.cs">LiquidPageCodeBehind.cs</ProjectItem>
    <ProjectItem TargetFileName="$fileinputname$.liquid">LiquidPageTemplate.liquid</ProjectItem>
  </TemplateContent>
</VSTemplate>
```

### Template Variables

The following Visual Studio template parameters are used:

| Parameter | Description | Example |
|-----------|-------------|---------|
| `$fileinputname$` | Name entered by user | `Home` |
| `$rootnamespace$` | Project namespace | `MyApp.Pages` |
| `$modelclassname$` | Generated class name | `HomeModel` |

## Using the Template

1. **Right-click** on the Pages folder (or create it)
2. Select **Add > New Item** (Ctrl+Shift+A)
3. Search for **"Liquid Page"** or browse Visual C# items
4. Enter a name: **Home**
5. Click **Add**

This creates:
```
Pages/
  ├─ Home.liquid
  └─ Home.liquid.cs
```

## Manual Nesting

After creating files via the template, you may need to manually nest the code-behind:

1. **Edit your `.csproj` file** (right-click project > Edit Project File)

2. **Add the nesting configuration:**
```xml
<ItemGroup>
  <Compile Include="Pages\Home.liquid.cs">
    <DependentUpon>Home.liquid</DependentUpon>
  </Compile>
</ItemGroup>
```

3. **Reload the project** to see the nested structure

## Alternative: Use the Command

If the item template installation is complex, you can use the extension command instead:

1. Go to **Tools > Add Liquid Page...**
2. Creates files with automatic nesting configuration

## Troubleshooting

### Template doesn't appear in Add New Item

**Solution 1:** Clear the template cache
```powershell
# Close Visual Studio first
Remove-Item "$env:LOCALAPPDATA\Microsoft\VisualStudio\*\ComponentModelCache" -Recurse -Force
```

**Solution 2:** Run the template cache refresh
- Open Developer Command Prompt for VS
- Run: `devenv /installvstemplates`

### Files not nested after creation

**Cause:** Visual Studio item templates don't automatically support file nesting via `DependentUpon`

**Solution:** Manually add to `.csproj` as shown above, or use the extension command instead

### Wrong namespace generated

**Cause:** Template uses `$rootnamespace$` which may not match your folder structure

**Solution:** Manually edit the generated `.liquid.cs` file to correct the namespace

## Future Improvements

When the new VisualStudio.Extensibility model adds support for:
- Item template integration
- Context menu contributions
- File nesting APIs

The template will be automatically installed and integrated with the extension.

## See Also

- [Visual Studio Item Templates](https://learn.microsoft.com/en-us/visualstudio/ide/creating-project-and-item-templates)
- [Template Parameters](https://learn.microsoft.com/en-us/visualstudio/ide/template-parameters)
- [Add Liquid Page Command](ADD_LIQUID_PAGE.md)
