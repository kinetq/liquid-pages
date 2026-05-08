using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.LanguageServices;

namespace Kinetq.LiquidPages.HybridExtension
{

    [Export]
    internal sealed class LiquidModelResolver
    {
        private readonly VisualStudioWorkspace _workspace;

        private const string PageAttributeMetadataName = "Kinetq.LiquidPages.Pages.LiquidPageAttribute";
        private const string PageModelBaseMetadataName = "Kinetq.LiquidPages.Pages.LiquidPageModel";

        [ImportingConstructor]
        public LiquidModelResolver(VisualStudioWorkspace workspace)
        {
            _workspace = workspace;
        }

        /// <summary>
        /// Given the full disk path of an open .liquid file, resolves the paired
        /// <c>LiquidPageModel</c> subclass via its <c>[LiquidPage]</c> attribute.
        /// </summary>
        public async Task<INamedTypeSymbol> ResolveModelAsync(
            string liquidFilePath, CancellationToken cancellationToken)
        {
            // Normalise to a forward-slash relative path so it matches the
            // template path stored in [LiquidPage("...", "/Pages/Index.liquid")]
            string normalizedPath = NormalizeTemplatePath(liquidFilePath);

            foreach (var project in _workspace.CurrentSolution.Projects)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var compilation = await project.GetCompilationAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (compilation is null) continue;

                var attrSymbol = compilation.GetTypeByMetadataName(PageAttributeMetadataName);
                var baseSymbol = compilation.GetTypeByMetadataName(PageModelBaseMetadataName);
                if (attrSymbol is null || baseSymbol is null) continue;

                foreach (var type in GetAllTypes(compilation.GlobalNamespace))
                {
                    if (!InheritsFrom(type, baseSymbol)) continue;

                    var attr = type.GetAttributes()
                        .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(
                            a.AttributeClass, attrSymbol));

                    if (attr is null) continue;

                    // Constructor: LiquidPageAttribute(routePattern, templatePath)
                    var templatePath = attr.ConstructorArguments.ElementAtOrDefault(1).Value as string;
                    if (templatePath != null &&
                        normalizedPath.EndsWith(
                            templatePath.TrimStart('/'),
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return type;
                    }
                }
            }

            return null;
        }

        private static string NormalizeTemplatePath(string fullPath) =>
            fullPath.Replace('\\', '/').TrimStart('/');

        private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
        {
            foreach (var type in ns.GetTypeMembers())
                yield return type;

            foreach (var nested in ns.GetNamespaceMembers())
                foreach (var type in GetAllTypes(nested))
                    yield return type;
        }

        private static bool InheritsFrom(INamedTypeSymbol type, INamedTypeSymbol baseType)
        {
            var current = type.BaseType;
            while (current != null)
            {
                if (SymbolEqualityComparer.Default.Equals(current, baseType)) return true;
                current = current.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Mirrors <c>MemberNameStrategies.SnakeCase</c> used by Fluid so that
        /// C# <c>Title</c> maps to liquid <c>title</c>, <c>FirstName</c> to <c>first_name</c>, etc.
        /// </summary>
        internal static string ToSnakeCase(string name) =>
            string.Concat(name.Select((c, i) =>
                i > 0 && char.IsUpper(c)
                    ? "_" + char.ToLower(c)
                    : char.ToLower(c).ToString()));
    }
}