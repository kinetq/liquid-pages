using Fluid;
using Kinetq.LiquidPages.Exceptions;
using Kinetq.LiquidPages.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Statiq.Common;

namespace Kinetq.LiquidPages.Statiq.Modules;

public class RenderLiquidTemplate : ParallelModule
{
    private bool _servicesConfigured;

    private void EnsureServicesConfigured(IExecutionContext context)
    {
        if (_servicesConfigured)
        {
            return;
        }

        var filterManager = context.GetRequiredService<ILiquidFilterManager>();
        foreach (var configurator in context.GetServices<IConfigureLiquidFilter>())
        {
            filterManager.RegisterFilter(configurator.Name, configurator.FilterDelegate);
        }

        var typesManager = context.GetRequiredService<ILiquidRegisteredTypesManager>();
        foreach (var configurator in context.GetServices<IConfigureLiquidType>())
        {
            typesManager.RegisterType(configurator.Type);
        }

        _servicesConfigured = true;
    }

    protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(
        IDocument input, IExecutionContext context)
    {
        EnsureServicesConfigured(context);

        var fluidParserManager = context.GetRequiredService<IFluidParserManager>();
        var filterManager = context.GetRequiredService<ILiquidFilterManager>();
        var typesManager = context.GetRequiredService<ILiquidRegisteredTypesManager>();

        string templateContent = await input.GetContentStringAsync();

        var parser = fluidParserManager.FluidParser;
        if (!parser.TryParse(templateContent, out IFluidTemplate fluidTemplate, out string error))
        {
            throw new LiquidSyntaxException(error);
        }

        var options = new TemplateOptions
        {
            MemberAccessStrategy = new DefaultMemberAccessStrategy
            {
                MemberNameStrategy = MemberNameStrategies.SnakeCase
            }
        };

        if (!input.Source.IsNullOrEmpty && input.Source.IsAbsolute)
        {
            string parentDirectory = input.Source.Parent.FullPath;
            if (Directory.Exists(parentDirectory))
            {
                options.FileProvider = new PhysicalFileProvider(parentDirectory);
            }
        }

        foreach (var type in typesManager.RegisteredTypes)
        {
            options.MemberAccessStrategy.Register(type);
        }

        foreach (var filter in filterManager.LiquidFilters)
        {
            options.Filters.AddFilter(filter.Key, filter.Value);
        }

        // When a page model was executed upstream by ExecutePageModel, use it as the
        // Fluid root context so its properties are top-level template variables —
        // the same behaviour as HtmlRenderer's new TemplateContext(renderModel, options).
        // Without a page model, fall back to an empty context and expose document
        // metadata as individual variables instead.
        object? pageModel = input.GetRaw(LiquidKeys.PageModel);
        var templateContext = pageModel is not null
            ? new TemplateContext(pageModel, options)
            : new TemplateContext(options);

        if (pageModel is null)
        {
            foreach (var kvp in input)
            {
                templateContext.SetValue(kvp.Key, kvp.Value);
            }
        }

        if (!input.Destination.IsNullOrEmpty)
        {
            templateContext.SetValue("route", input.Destination.FullPath);
        }

        string html = await fluidTemplate.RenderAsync(templateContext);

        return input.Clone(context.GetContentProvider(html, MediaTypes.Html)).Yield();
    }
}
