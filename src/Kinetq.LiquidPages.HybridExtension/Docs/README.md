# LiquidPages Extension

**Visual Studio extension for IntelliSense support in .liquid template files backed by LiquidPageModel classes.**

## Current Status

This extension has been **migrated** from the legacy MEF-based extensibility model to the new **VisualStudio.Extensibility** model.

### What Works

✅ Extension loads in Visual Studio 2022+ (17.8 or later)  
✅ Syntax highlighting for `.liquid` files (via TextMate grammar)  
✅ Core model resolution logic (available for future features)  
✅ Modern .NET 8 architecture with dependency injection  
✅ **Add Liquid Page** command to scaffold new pages (Razor Pages style)

### Temporarily Unavailable

⚠️ **IntelliSense completion** for model properties  
⚠️ **QuickInfo tooltips** when hovering over properties  

These features require Language Server Protocol (LSP) support, which will be added when the new extensibility model provides the necessary APIs.

## Installation

1. Build the `Kinetq.LiquidPages.Extension` project
2. The extension will be deployed to the Experimental Instance
3. Press F5 to launch and test

## Features

### Add Liquid Page Command

Create new Liquid Pages using the **Tools > Add Liquid Page...** command. This generates:

- **PageName.liquid** - The Liquid template file
- **PageName.liquid.cs** - The code-behind model class (nested under the template, like Razor Pages)

### Item Template (One-Click Installation)

For a better workflow, install the Liquid Page item template:

1. **Go to Tools > Install Liquid Page Template...**
2. **Click OK** to install
3. **Restart Visual Studio**
4. **Use the template:**
   - Right-click on a folder → **Add > New Item**
   - Search for "Liquid Page"
   - Enter a name and click Add

**Alternative installation methods:**
- Run PowerShell script: `.\Install-ItemTemplate.ps1 -VSVersion 2026`
- Manual copy from `bin\...\ItemTemplates\LiquidPage\` to VS templates folder

See [ITEM_TEMPLATE_INSTALL.md](ITEM_TEMPLATE_INSTALL.md) for detailed instructions.

### File Structure Example
```
Pages/
  ├─ Home.liquid
  │  └─ Home.liquid.cs
  ├─ About.liquid
  │  └─ About.liquid.cs
```

The generated files follow the Razor Pages convention where:
- The template is the "parent" file (e.g., `Home.liquid`)
- The code-behind is nested underneath (e.g., `Home.liquid.cs`)
- The class is named `HomeModel` (matching Razor's `IndexModel` pattern)

### Future IntelliSense Features

Once fully implemented, this extension will provide:

- **Autocomplete** for model properties in `{{ }}` and `{% %}` expressions
- **Type information** on hover
- **Documentation** from XML comments

Example:
```liquid
<!-- Given a LiquidPageModel with properties: -->
<!-- public string Title { get; set; } -->
<!-- public DateTime CreatedDate { get; set; } -->

<h1>{{ title }}</h1>  <!-- IntelliSense suggests 'title' -->
<p>{{ created_date }}</p>  <!-- IntelliSense suggests 'created_date' -->
```

## Architecture

The extension analyzes C# code using Roslyn to find `LiquidPageModel` classes decorated with `[LiquidPage]` attributes, then provides IntelliSense for their properties (converted to snake_case).

See [MIGRATION.md](MIGRATION.md) for detailed technical information.

## Legacy Version

The original MEF-based extension with full IntelliSense is preserved in:
```
src/Kinetq.LiquidPages.ExtensionOld/
```

This version works with Visual Studio 2022 17.x but uses the legacy extensibility model.

## Contributing

Contributions welcome, especially for implementing Language Server Protocol support!

See [MIGRATION.md](MIGRATION.md) for technical details and future work.

## License

[Your License Here]

## References

- [Liquid Template Language](https://shopify.github.io/liquid/)
- [Visual Studio Extensibility](https://learn.microsoft.com/en-us/visualstudio/extensibility/visualstudio.extensibility/)
- [Razor Pages](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/) (inspiration for file structure)

