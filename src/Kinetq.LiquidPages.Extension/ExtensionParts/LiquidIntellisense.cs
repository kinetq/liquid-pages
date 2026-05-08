using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.ServiceHub.Framework;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.ProjectSystem.Query;
using System.Configuration;
using System.Xml.Linq;

namespace Kinetq.LiquidPages.Extension.ExtensionParts;

[VisualStudioContribution]
internal class LiquidIntellisense : ExtensionPart, ITextViewChangedListener
{
    private const string PageAttributeMetadataName = "Kinetq.LiquidPages.Pages.LiquidPageAttribute";
    private const string PageModelBaseMetadataName = "Kinetq.LiquidPages.Pages.LiquidPageModel";

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

        CSharpCompilation compilation = CSharpCompilation.Create(
            $"{Path.GetFileNameWithoutExtension(modelFile.FileName)}.Intellisense",
            syntaxTrees: new List<SyntaxTree>() { liquidPageModelSyntaxTree },
            references: ,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOverflowChecks(true)
                .WithOptimizationLevel(OptimizationLevel.Debug)
                .WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default));
    }

    private static string NormalizeTemplatePath(string fullPath) =>
        fullPath.Replace('\\', '/').TrimStart('/');

}