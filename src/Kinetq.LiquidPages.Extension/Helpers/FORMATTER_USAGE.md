# Using the Liquid Formatter in Your Extension

The `formatter.exe` is automatically built and included in your VSIX package. You can use the `LiquidFormatter` helper class to format Liquid templates.

## Example Usage

```csharp
using Kinetq.LiquidPages.Extension.Helpers;

// Format a Liquid template string
var liquidContent = @"
{% if user.name %}
<div   class='greeting'   >
Hello {{ user.name }}!
</div>
{% endif %}
";

try
{
    var formatted = await LiquidFormatter.FormatAsync(liquidContent);
    Console.WriteLine(formatted);

    // Output:
    // {% if user.name %}
    //   <div class="greeting">
    //     Hello {{ user.name }}!
    //   </div>
    // {% endif %}
}
catch (Exception ex)
{
    // Handle formatting errors
    Console.WriteLine($"Formatting failed: {ex.Message}");
}
```

## Checking Availability

```csharp
if (LiquidFormatter.IsAvailable())
{
    // Formatter is available, use it
}
else
{
    // Formatter not found, handle gracefully
}
```

## How It Works

1. **Build Time**: When you build the extension, MSBuild automatically:
   - Installs npm dependencies (if needed) in the `Static` folder
   - Runs `npm run build` to create `formatter.exe` using Node.js SEA
   - Copies `formatter.exe` to the output directory and VSIX package

2. **Runtime**: The `LiquidFormatter` class:
   - Locates the `formatter.exe` next to the extension assembly
   - Launches the formatter as a process
   - Sends the Liquid content via stdin
   - Receives the formatted content via stdout
   - Returns the formatted string

## Integration Ideas

### Document Formatting Command

```csharp
[VisualStudioContribution]
public class FormatLiquidDocumentCommand : Command
{
    public override CommandConfiguration CommandConfiguration => new("Format Liquid Document")
    {
        // Configuration...
    };

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        var documentText = await GetCurrentDocumentTextAsync(context, cancellationToken);

        if (!string.IsNullOrEmpty(documentText))
        {
            var formatted = await LiquidFormatter.FormatAsync(documentText, cancellationToken);
            await SetCurrentDocumentTextAsync(context, formatted, cancellationToken);
        }
    }
}
```

### Format on Save

```csharp
// Hook into document save events and format Liquid files automatically
private async Task OnDocumentSaveAsync(string filePath, CancellationToken cancellationToken)
{
    if (IsLiquidFile(filePath))
    {
        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        var formatted = await LiquidFormatter.FormatAsync(content, cancellationToken);
        await File.WriteAllTextAsync(filePath, formatted, cancellationToken);
    }
}
```

## Build Configuration

The formatter build is configured in `Kinetq.LiquidPages.Extension.csproj`:

```xml
<ItemGroup>
  <Content Include="Static\formatter.exe">
    <IncludeInVSIX>true</IncludeInVSIX>
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    <Link>formatter.exe</Link>
  </Content>
</ItemGroup>

<Target Name="BuildFormatter" BeforeTargets="BeforeBuild">
  <Message Text="Building Liquid formatter..." Importance="high" />
  <Exec Command="npm install" 
        WorkingDirectory="$(MSBuildProjectDirectory)\Static" 
        Condition="!Exists('$(MSBuildProjectDirectory)\Static\node_modules')" />
  <Exec Command="npm run build" 
        WorkingDirectory="$(MSBuildProjectDirectory)\Static" />
  <Message Text="Liquid formatter built successfully" Importance="high" />
</Target>
```

## Troubleshooting

- **Formatter not found**: Ensure `formatter.exe` exists in `Static/` folder and is included in the build
- **npm not found**: Install Node.js and ensure npm is in PATH
- **Build fails**: Check `Static/README.md` for build requirements
- **Formatting fails**: Check the error message - it may be invalid Liquid syntax

## See Also

- [Static/README.md](../../Static/README.md) - Formatter build documentation
- [Node.js SEA Documentation](https://nodejs.org/api/single-executable-applications.html)
- [Prettier Documentation](https://prettier.io/)
