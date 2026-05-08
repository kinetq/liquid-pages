using System.IO;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.ProjectSystem.Query;
using Kinetq.LiquidPages.Extension.Helpers;

namespace Kinetq.LiquidPages.Extension.ExtensionParts;

[VisualStudioContribution]
internal class LiquidIntellisenseExtensionPart : ExtensionPart, ITextViewChangedListener
{
    public TextViewExtensionConfiguration TextViewExtensionConfiguration => new()
    {
        AppliesTo =
        [
            DocumentFilter.FromGlobPattern(".liquid", true)
        ]
    };
    public async Task TextViewChangedAsync(TextViewChangedArgs args, CancellationToken cancellationToken)
    {
        // Normalise to a forward-slash relative path so it matches the
        // template path stored in [LiquidPage("...", "/Pages/Index.liquid")]
        string? normalizedPath = NormalizeTemplatePath(args.AfterTextView.FilePath);
        if (string.IsNullOrEmpty(normalizedPath))
        {
            return;
        }

        var workspace = Extensibility.Workspaces();
        var projects = await workspace.QueryProjectsAsync(
            project => project.With(p => p.Path),
            cancellationToken);

        IFileSnapshot modelFile = null;
        List<IProjectReferenceSnapshot> references = new List<IProjectReferenceSnapshot>();
        foreach (var projectSnapshot in projects)
        {
            foreach (var fileSnapshot in projectSnapshot.FilesByPath("Pages"))
            {
                if (!string.Equals(
                        Path.GetDirectoryName(NormalizeTemplatePath(fileSnapshot.Path)),
                        Path.GetDirectoryName(normalizedPath),
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.Equals(
                        Path.GetFileNameWithoutExtension(NormalizeTemplatePath(fileSnapshot.Path)),
                        Path.GetFileName(normalizedPath),
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }


                if (!Path.GetExtension(fileSnapshot.FileName).Equals("cs"))
                {
                    continue;
                }

                modelFile = fileSnapshot;
                break;
            }

            if (modelFile != null)
            {
                references = projectSnapshot.ProjectReferences.ToList(); 
                break;
            }
        }

        if (modelFile == null)
        {
            return;
        }

        string modelFileContents = await File.ReadAllTextAsync(modelFile.Path, cancellationToken);
        var liquidPageModelSyntaxTree = CSharpSyntaxTree.ParseText(modelFileContents);

        var metadataReferences = 
            references.Select(r => 
                    MetadataReference.CreateFromFile(r.ReferencedProjectPath))
                .ToList();

        CSharpCompilation compilation = CSharpCompilation.Create(
            $"{Path.GetFileNameWithoutExtension(modelFile.FileName)}.Intellisense",
            syntaxTrees: new List<SyntaxTree>() { liquidPageModelSyntaxTree },
            references: metadataReferences,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOverflowChecks(true)
                .WithOptimizationLevel(OptimizationLevel.Debug)
                .WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default));

        var modelType = compilation.GetTypeByMetadataName(modelFile.ItemName);
        if (modelType == null)
        {
            return;
        }

        var completions = LiquidIntelliSenseHelper.BuildCompletionItems(modelType);
    }

    private static string NormalizeTemplatePath(string fullPath) =>
        fullPath.Replace('\\', '/').TrimStart('/');

}