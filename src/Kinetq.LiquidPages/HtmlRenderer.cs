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
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly ITemplateOptionsManager _templateOptionsManager;

    public HtmlRenderer(
        IFluidParserManager fluidParserManager,
        ILiquidTemplateManager liquidTemplateManager, 
        ITemplateOptionsManager templateOptionsManager)
    {
        _fluidParserManager = fluidParserManager;
        _liquidTemplateManager = liquidTemplateManager;
        _templateOptionsManager = templateOptionsManager;
    }

    public async Task<string?> RenderHtml(RenderModel renderModel, LiquidRoute? liquidRoute)
    {
        if (liquidRoute == null)
        {
            return null;
        }

        var options = _templateOptionsManager.GetTemplateOptions(liquidRoute.RouteTemplate);
        var fileInfo = options.FileProvider.GetFileInfo(liquidRoute.LiquidTemplatePath);
        if (!fileInfo.Exists)
        {
            return null;
        }

        string liquidTemplate = await fileInfo.GetFileContents();

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