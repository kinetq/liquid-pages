using System.Reflection;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;
using Microsoft.Extensions.DependencyInjection;
using Statiq.Common;

namespace Kinetq.LiquidPages.Statiq.Modules;

/// <summary>
/// Matches each input document to a <see cref="LiquidPageModel"/> subclass by comparing the
/// document's source path against <see cref="LiquidPageAttribute.TemplatePath"/>, then resolves
/// a fresh instance from DI, calls <see cref="LiquidPageModel.OnGetAsync"/>, and stores the
/// executed model in document metadata under <see cref="LiquidKeys.PageModel"/>.
///
/// Place this module before <see cref="RenderLiquidTemplate"/> in the pipeline.
/// Documents with no matching page model pass through unchanged.
/// </summary>
public class ExecutePageModel : ParallelModule
{
    private bool _typesRegistered;

    private void EnsureTypesRegistered(IExecutionContext context)
    {
        if (_typesRegistered)
        {
            return;
        }

        var typesManager = context.GetRequiredService<ILiquidRegisteredTypesManager>();
        foreach (var configurator in context.GetServices<IConfigureLiquidPageModel>())
        {
            RegisterTypeRecursively(configurator.PageModelType, typesManager, new HashSet<Type>());
        }

        _typesRegistered = true;
    }

    protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(
        IDocument input, IExecutionContext context)
    {
        EnsureTypesRegistered(context);

        LiquidPageModel? pageModel = ResolvePageModel(input, context);
        if (pageModel is null)
        {
            return input.Yield();
        }

        var request = new LiquidRequestModel
        {
            Route = input.Destination.IsNullOrEmpty ? input.Source.FullPath : input.Destination.FullPath,
            QueryParams = new Dictionary<string, string>(),
            Method = "GET",
            LiquidPageModel = pageModel
        };

        await pageModel.OnGetAsync(request);

        return input
            .Clone(new MetadataItems { { LiquidKeys.PageModel, pageModel } })
            .Yield();
    }

    private static LiquidPageModel? ResolvePageModel(IDocument input, IExecutionContext context)
    {
        if (input.Source.IsNullOrEmpty)
        {
            return null;
        }

        string sourcePath = input.Source.FullPath;

        foreach (var configurator in context.GetServices<IConfigureLiquidPageModel>())
        {
            var attr = configurator.PageModelType
                .GetCustomAttribute<LiquidPageAttribute>();

            if (attr is null)
            {
                continue;
            }

            // TemplatePath may be a bare filename ("index.liquid") or a relative path
            // ("pages/index.liquid"). Match against the end of the document source path
            // using normalised separators so it works on all platforms.
            string normalised = sourcePath.Replace('\\', '/');
            string templatePath = attr.TemplatePath.TrimStart('/', '\\').Replace('\\', '/');

            if (!normalised.EndsWith(templatePath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return (LiquidPageModel?)context.GetService(configurator.PageModelType);
        }

        return null;
    }

    // Mirrors LiquidStartup.RegisterTypeRecursively so property types on the model
    // are accessible in Fluid templates without manual registration.
    private static void RegisterTypeRecursively(
        Type type,
        ILiquidRegisteredTypesManager typesManager,
        HashSet<Type> visited)
    {
        if (!visited.Add(type))
        {
            return;
        }

        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) ||
            type == typeof(DateTime) || type == typeof(DateTimeOffset) ||
            type == typeof(TimeSpan) || type == typeof(Guid) || type.IsEnum)
        {
            return;
        }

        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            if (type.IsGenericType)
            {
                foreach (var arg in type.GetGenericArguments())
                {
                    RegisterTypeRecursively(arg, typesManager, visited);
                }
            }
            else if (type.IsArray)
            {
                RegisterTypeRecursively(type.GetElementType()!, typesManager, visited);
            }

            return;
        }

        typesManager.RegisterType(type);

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            RegisterTypeRecursively(prop.PropertyType, typesManager, visited);
        }
    }
}
