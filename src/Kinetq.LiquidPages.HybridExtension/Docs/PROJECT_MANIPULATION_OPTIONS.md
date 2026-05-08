# Project Manipulation in New Extensibility Model

## Current Limitations (v17.14)

The new VisualStudio.Extensibility SDK **does not yet provide** APIs for:

❌ Adding items to projects with metadata  
❌ Setting `<DependentUpon>` or other MSBuild properties  
❌ Manipulating project structure directly  
❌ Solution Explorer tree manipulation  
❌ File nesting configuration  

## What IS Available

### 1. Workspace Queries (Read-Only)

```csharp
// Query projects in solution
var projects = await this.Extensibility.Workspaces()
    .QueryProjectsAsync(
        project => project.With(p => p.Name)
                         .With(p => p.Path)
                         .With(p => p.Files),
        cancellationToken);
```

**Limitations:**
- Read-only access
- Cannot modify project structure
- Cannot add files with metadata

### 2. Document Creation

```csharp
// Create a new text document
var doc = await this.Extensibility.Documents()
    .OpenTextDocumentAsync(filePath, cancellationToken);
```

**Limitations:**
- Only creates documents
- Does not add to project
- No project integration

### 3. Shell Operations

```csharp
// Show prompts, open files
await this.Extensibility.Shell()
    .ShowPromptAsync("Message", PromptOptions.OK, cancellationToken);
```

**Limitations:**
- UI interactions only
- No project manipulation

## Current Best Practice: Direct File Modification

Your current implementation is the **recommended approach** for the new extensibility model:

```csharp
private async Task NestCodeBehindInProjectAsync(
    string projectFilePath, 
    string liquidFileName, 
    string csFileName, 
    CancellationToken cancellationToken)
{
    // Read .csproj file
    var projectContent = await File.ReadAllTextAsync(projectFilePath, cancellationToken);

    // Add nesting XML
    var nestingXml = $@"
  <ItemGroup>
    <Compile Include=""Pages\{csFileName}"">
      <DependentUpon>{liquidFileName}</DependentUpon>
    </Compile>
  </ItemGroup>";

    // Insert and save
    var insertPosition = projectContent.LastIndexOf("</ItemGroup>");
    projectContent = projectContent.Insert(insertPosition, nestingXml);
    await File.WriteAllTextAsync(projectFilePath, projectContent, cancellationToken);
}
```

### Why This is OK

✅ **Reliable** - Direct file I/O works consistently  
✅ **Transparent** - Easy to debug and understand  
✅ **Compatible** - Works with all project types  
✅ **Future-proof** - Will work even when new APIs arrive  

## Alternative Approaches (Not Recommended)

### 1. Using MSBuild APIs Directly

```csharp
// NOT RECOMMENDED - Heavy dependency
using Microsoft.Build.Evaluation;

var project = new Project(projectFilePath);
project.AddItem("Compile", "Pages\\Home.liquid.cs", new[] {
    new KeyValuePair<string, string>("DependentUpon", "Home.liquid")
});
project.Save();
```

**Problems:**
- ❌ Requires heavy MSBuild NuGet packages
- ❌ Can interfere with VS's project system
- ❌ May cause project reload issues
- ❌ Version conflicts possible

### 2. Using Old VSSDK Interop (Bridge)

```csharp
// NOT AVAILABLE in new extensibility model
// Old code (doesn't work):
var project = (IVsProject)...;
project.AddItem(...);
```

**Problems:**
- ❌ Not supported in new extensibility model
- ❌ Would require old VSSDK packages
- ❌ Defeats purpose of new model

### 3. Calling devenv.exe

```csharp
// REALLY NOT RECOMMENDED
Process.Start("devenv.exe", "/edit \"Home.liquid.cs\"");
```

**Problems:**
- ❌ Unreliable
- ❌ No control over result
- ❌ User experience issues

## Potential Improvements to Current Approach

### 1. Use XML APIs for Safety

Instead of string insertion, use `XDocument`:

```csharp
private async Task NestCodeBehindInProjectAsync(
    string projectFilePath,
    string liquidFileName,
    string csFileName,
    CancellationToken cancellationToken)
{
    try
    {
        var doc = XDocument.Load(projectFilePath);
        var ns = doc.Root?.Name.Namespace ?? XNamespace.None;

        // Check if already exists
        var existing = doc.Descendants(ns + "Compile")
            .FirstOrDefault(e => e.Attribute("Include")?.Value == $"Pages\\{csFileName}");

        if (existing != null)
            return; // Already configured

        // Find or create ItemGroup
        var lastItemGroup = doc.Descendants(ns + "ItemGroup").LastOrDefault();

        if (lastItemGroup == null)
        {
            lastItemGroup = new XElement(ns + "ItemGroup");
            doc.Root?.Add(lastItemGroup);
        }

        // Add new Compile element with DependentUpon
        var compileItem = new XElement(ns + "Compile",
            new XAttribute("Include", $"Pages\\{csFileName}"),
            new XElement(ns + "DependentUpon", liquidFileName));

        lastItemGroup.Add(compileItem);

        // Save
        doc.Save(projectFilePath);

        this.logger.TraceEvent(TraceEventType.Information, 0,
            $"Added nesting configuration to {projectFilePath}");
    }
    catch (Exception ex)
    {
        this.logger.TraceEvent(TraceEventType.Warning, 0,
            $"Could not modify project file for nesting: {ex.Message}");
    }
}
```

**Benefits:**
✅ Proper XML parsing and validation  
✅ Handles namespaces correctly  
✅ Safer than string manipulation  
✅ Preserves formatting better  

### 2. Add User Confirmation

```csharp
var modifyProject = await this.Extensibility.Shell().ShowPromptAsync(
    "Modify project file to nest the code-behind under the template file?",
    PromptOptions.OKCancel,
    cancellationToken);

if (modifyProject)
{
    await NestCodeBehindInProjectAsync(...);
}
```

### 3. Provide Manual Instructions as Fallback

```csharp
catch (Exception ex)
{
    await this.Extensibility.Shell().ShowPromptAsync(
        $"Could not automatically nest files. Please add this to your .csproj:\n\n" +
        $"<ItemGroup>\n" +
        $"  <Compile Include=\"Pages\\{csFileName}\">\n" +
        $"    <DependentUpon>{liquidFileName}</DependentUpon>\n" +
        $"  </Compile>\n" +
        $"</ItemGroup>",
        PromptOptions.OK,
        cancellationToken);
}
```

## Future Outlook

Microsoft is working on expanding the new extensibility model. Future versions **may** include:

🔮 **Project System APIs** - Direct project manipulation  
🔮 **File Nesting APIs** - Native nesting support  
🔮 **Template Integration** - Better item template support  
🔮 **MSBuild Integration** - Safe property modification  

### How to Track Progress

Monitor these resources:
- [VisualStudio.Extensibility GitHub](https://github.com/microsoft/VSExtensibility)
- [VS Extensibility Roadmap](https://learn.microsoft.com/en-us/visualstudio/extensibility/visualstudio.extensibility/)
- [Release Notes](https://learn.microsoft.com/en-us/visualstudio/releases/2022/release-notes)

## Recommendation

**Keep your current implementation** with these optional improvements:

1. ✅ **Use `XDocument` for XML manipulation** (safer)
2. ✅ **Add better error handling** (show manual instructions on failure)
3. ✅ **Consider user confirmation** (optional, for transparency)
4. ⚠️ **Don't use MSBuild APIs** (too heavy, can cause conflicts)
5. ⚠️ **Don't try to use old VSSDK** (not compatible)

Your approach is **currently the best practice** for the new extensibility model! 🎯

## Summary

| Approach | Status | Recommendation |
|----------|--------|----------------|
| Direct .csproj file modification | ✅ Current | **Use this** |
| XDocument for XML safety | ✅ Enhancement | **Consider adding** |
| MSBuild API | ❌ Available but problematic | Avoid |
| Old VSSDK interop | ❌ Not available | Not possible |
| New extensibility project APIs | ❌ Not yet available | Wait for future SDK |

**Bottom line:** Your current implementation is correct. The new extensibility model simply doesn't provide better alternatives yet.
