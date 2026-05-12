//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.VisualStudio.Extensibility;
//using Microsoft.VisualStudio.Extensibility.Editor;
//using Microsoft.VisualStudio.ProjectSystem.Query;
//using Kinetq.LiquidPages.Extension.Helpers;

//namespace Kinetq.LiquidPages.Extension.ExtensionParts;

//[VisualStudioContribution]
//internal class LiquidIntellisenseExtensionPart : ExtensionPart, ITextViewChangedListener
//{
//    public TextViewExtensionConfiguration TextViewExtensionConfiguration => new()
//    {
//        AppliesTo =
//        [
//            DocumentFilter.FromDocumentType(LiquidDocumentTypeConfiguration.LiquidDocumentType)
//        ]
//    };
//    public async Task TextViewChangedAsync(TextViewChangedArgs args, CancellationToken cancellationToken)
//    {
//        // Normalise to a forward-slash relative path so it matches the
//        // template path stored in [LiquidPage("...", "/Pages/Index.liquid")]
//        string? filePath = args.AfterTextView.FilePath;

//        var workspace = Extensibility.Workspaces();
//        IProjectSnapshot activeProject = null;
//        try
//        {
//            var projects = await workspace.QueryProjectsAsync(
//                project => project.With(p => p.Path),
//                cancellationToken);
//            activeProject = projects.FirstOrDefault(p => !string.IsNullOrEmpty(p.Path) && filePath.StartsWith(p.Path, StringComparison.OrdinalIgnoreCase));
//        }
//        catch (Exception ex)
//        {
//            return;
//        }

//        if (activeProject == null)
//        {
//            return;
//        }

//        var references = activeProject.ProjectReferences.ToList();
//        string modelFilePath = Path.Combine(filePath, ".cs");

//        if (!File.Exists(modelFilePath))
//        {
//            return;
//        }

//        string modelFileName = Path.GetFileNameWithoutExtension(modelFilePath);
//        string modelFileContents = await File.ReadAllTextAsync(modelFilePath, cancellationToken);
//        var liquidPageModelSyntaxTree = CSharpSyntaxTree.ParseText(modelFileContents);

//        var metadataReferences =
//            references.Select(r =>
//                    MetadataReference.CreateFromFile(r.ReferencedProjectPath))
//                .ToList();

//        CSharpCompilation compilation = CSharpCompilation.Create(
//            $"{Path.GetFileNameWithoutExtension(modelFileName)}.Intellisense",
//            syntaxTrees: new List<SyntaxTree>() { liquidPageModelSyntaxTree },
//            references: metadataReferences,
//            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
//                .WithOverflowChecks(true)
//                .WithOptimizationLevel(OptimizationLevel.Debug)
//                .WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default));

//        var modelType = compilation.GetTypeByMetadataName(modelFileName);
//        if (modelType == null)
//        {
//            return;
//        }

//        var completions = LiquidIntelliSenseHelper.BuildCompletionItems(modelType);
//    }

//    private static string NormalizeTemplatePath(string fullPath) =>
//        fullPath.Replace('\\', '/').TrimStart('/');

//}