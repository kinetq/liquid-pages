# Automatic Template Installation - FAQ

## Can the extension automatically install templates on installation?

**Short Answer:** No, not automatically. But there's a **one-click command** that makes it easy!

## Why Not Automatic?

The **new VisualStudio.Extensibility model** is more secure and sandboxed compared to the old VSIX model:

### Old Model (VSSDK) - Had Automatic Installation
```xml
<!-- In source.extension.vsixmanifest -->
<Installation>
  <InstallationTarget ... />
  <CustomExtension Type="ItemTemplate" Path="Templates\..." />
</Installation>
```
- ✅ Templates automatically installed with extension
- ✅ VSIX could write to user directories
- ⚠️ Security concerns with automatic file system access

### New Model (VisualStudio.Extensibility) - Requires Manual Step
```csharp
// Extensions are sandboxed and isolated
[VisualStudioContribution]
internal class ExtensionEntrypoint : Extension
{
    // Cannot automatically write to user directories
    // No installation hooks available
}
```
- ✅ More secure (sandboxed)
- ✅ Better isolation
- ❌ No automatic template installation
- ❌ No installation hooks
- ❌ Limited file system access

## Solution: One-Click Installation Command

Instead of automatic installation, the extension provides a **simple command**:

### User Experience

```
1. Install extension (normal process)
2. Open Visual Studio
3. Tools > Install Liquid Page Template... (one click!)
4. Restart Visual Studio
5. Templates available in Add > New Item
```

### Implementation

```csharp
[VisualStudioContribution]
internal class InstallItemTemplateCommand : Command
{
    public override async Task ExecuteCommandAsync(...)
    {
        // Copies template files from extension directory
        // To: %USERPROFILE%\Documents\Visual Studio 2026\Templates\...

        // Shows success/error dialog
        // Prompts user to restart VS
    }
}
```

## Comparison

| Approach | Old VSIX Model | New Extensibility Model |
|----------|----------------|------------------------|
| **Automatic** | ✅ Yes | ❌ No |
| **Setup Required** | None | One command (one-time) |
| **Security** | ⚠️ Lower | ✅ Higher (sandboxed) |
| **User Control** | None | ✅ User chooses when to install |
| **Uninstall** | Automatic | Manual or via extension |

## Alternative Installation Methods

The extension provides **three options**:

### 1. Extension Command (Easiest)
```
Tools > Install Liquid Page Template...
```
- ✅ One click
- ✅ Auto-detects VS version
- ✅ Clear error messages
- ✅ No command-line needed

### 2. PowerShell Script
```powershell
.\Install-ItemTemplate.ps1 -VSVersion 2026
```
- ✅ Scriptable/automatable
- ✅ Good for CI/CD
- ✅ Can be run remotely

### 3. Manual Copy
```
Copy from: bin\...\ItemTemplates\LiquidPage\
To: Documents\Visual Studio 2026\Templates\ItemTemplates\Visual C#\
```
- ✅ Full control
- ✅ No dependencies
- ⚠️ Most manual

## User Guidance

### First-Time Setup

**Include in your extension documentation:**

```markdown
## Getting Started

1. Install the LiquidPages Extension (one-time)
2. Go to **Tools > Install Liquid Page Template...** (one-time)
3. Restart Visual Studio
4. You're ready! Use **Add > New Item** to create Liquid Pages
```

### In Extension Description

```
After installation, run "Tools > Install Liquid Page Template..." 
to enable the Add > New Item template.
```

## Technical Details

### Why Can't We Copy Files on Extension Load?

```csharp
// Extension load happens in isolated process
[VisualStudioContribution]
internal class ExtensionEntrypoint : Extension
{
    protected override void InitializeServices(IServiceCollection services)
    {
        // This runs in isolated extension host
        // Cannot access user file system
        // Cannot run installation scripts
        base.InitializeServices(services);
    }
}
```

### Extension File Locations

```
Extension files:
  C:\Users\{User}\.vs\extensions\{ExtensionId}\
    └─ ItemTemplates\LiquidPage\

User template directory:
  C:\Users\{User}\Documents\Visual Studio 2026\Templates\ItemTemplates\Visual C#\
    └─ LiquidPage\  ← Must copy here
```

## Future Possibilities

Microsoft may add template installation APIs in future versions of VisualStudio.Extensibility:

```csharp
// Hypothetical future API
await this.Extensibility.Templates()
    .InstallItemTemplateAsync("ItemTemplates/LiquidPage", cancellationToken);
```

## Recommendation

**For your users:**

1. **Make it clear** in documentation that one setup step is required
2. **Provide the command** prominently: Tools > Install Liquid Page Template
3. **Offer alternatives** (PowerShell script, manual) for power users
4. Consider a **first-run prompt** that detects if template is installed

**Example first-run prompt:**
```csharp
public override async Task InitializeAsync(...)
{
    if (!IsTemplateInstalled())
    {
        var install = await ShowPromptAsync(
            "Would you like to install the Liquid Page template for Add > New Item?",
            PromptOptions.OKCancel);

        if (install)
        {
            await InstallTemplateAsync();
        }
    }
}
```

## Summary

❌ **Can't auto-install** on extension installation (new model limitation)  
✅ **Can provide** one-click installation command  
✅ **Can offer** multiple installation methods  
✅ **Can make** it very easy for users  

The one-click command (`Tools > Install Liquid Page Template...`) is the best compromise between security and user experience!
