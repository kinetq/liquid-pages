using Fluid;
using HtmlAgilityPack;
using Kinetq.LiquidPages.Exceptions;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages;

public class HtmlRenderer : IHtmlRenderer
{
    private readonly IFluidParserManager _fluidParserManager;
    private readonly ILiquidFilterManager _liquidFilterManager;
    private readonly ILiquidRegisteredTypesManager _liquidRegisteredTypesManager;
    private readonly ILiquidTemplateManager _liquidTemplateManager;

    public HtmlRenderer(
        IFluidParserManager fluidParserManager,
        ILiquidFilterManager liquidFilterManager,
        ILiquidRegisteredTypesManager liquidRegisteredTypesManager,
        ILiquidTemplateManager liquidTemplateManager)
    {
        _fluidParserManager = fluidParserManager;
        _liquidFilterManager = liquidFilterManager;
        _liquidRegisteredTypesManager = liquidRegisteredTypesManager;
        _liquidTemplateManager = liquidTemplateManager;
    }

    public async Task<string?> RenderHtml(RenderModel renderModel, LiquidRoute? liquidRoute)
    {
        if (liquidRoute == null)
        {
            return null;
        }

        var fileInfo = liquidRoute.FileProvider.GetFileInfo(liquidRoute.LiquidTemplatePath);
        if (!fileInfo.Exists)
        {
            return null;
        }

        string liquidTemplate = await fileInfo.GetFileContents();
        string templateKey = $"{liquidTemplate}";

        _liquidTemplateManager.FluidTemplates.TryGetValue(liquidTemplate, out var cachedTemplate);

        var parser = _fluidParserManager.FluidParser;
        if (cachedTemplate == null && parser.TryParse(liquidTemplate, out IFluidTemplate template, out string error))
        {
            if (!string.IsNullOrEmpty(error))
            {
                throw new LiquidSyntaxException(error);
            }

            cachedTemplate = template;
            _liquidTemplateManager.RegisterTemplate(liquidTemplate, cachedTemplate);
        }

        var options = new TemplateOptions
        {
            FileProvider = liquidRoute.FileProvider,
            MemberAccessStrategy = new DefaultMemberAccessStrategy()
            {
                MemberNameStrategy = MemberNameStrategies.SnakeCase
            }
        };

        foreach (var registeredType in _liquidRegisteredTypesManager.RegisteredTypes)
        {
            options.MemberAccessStrategy.Register(registeredType);
        }

        foreach (var filterDelegate in _liquidFilterManager.LiquidFilters)
        {
            options.Filters.AddFilter(filterDelegate.Key, filterDelegate.Value);
        }

        var templateContext = new TemplateContext(renderModel, options);

        string html = await cachedTemplate.RenderAsync(templateContext);


#if DEBUG
        // Validate HTML using HtmlAgilityPack
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Any())
        {
            throw new HtmlSyntaxException(htmlDoc.ParseErrors);
        }
#endif


        return html;
    }
}