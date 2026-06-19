using Fluid;
using Kinetq.LiquidPages.Exceptions;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using System.Text.Encodings.Web;

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

    public async Task<string?> RenderHtml(RenderModel renderModel, LiquidRoute liquidRoute)
    {
        if (liquidRoute == null)
        {
            return null;
        }

        var options = _templateOptionsManager.GetTemplateOptions(liquidRoute.RouteTemplate);

        string liquidTemplateCacheKey = $"{liquidRoute.RouteTemplate}-{liquidRoute.LiquidTemplatePath}";

        _liquidTemplateManager.FluidTemplates.TryGetValue(liquidTemplateCacheKey, out var cachedTemplate);
        if (cachedTemplate == null)
        {
            var parser = _fluidParserManager.FluidParser; 
            var fileInfo = options.FileProvider.GetFileInfo(liquidRoute.LiquidTemplatePath);
            if (!fileInfo.Exists)
            {
                return null;
            }
            
            string liquidTemplate = await fileInfo.GetFileContents();
            if (parser.TryParse(liquidTemplate, out IFluidTemplate template, out string error))
            {
                if (!string.IsNullOrEmpty(error))
                {
                    throw new LiquidSyntaxException(error);
                }

                cachedTemplate = template;
                _liquidTemplateManager.RegisterTemplate(liquidTemplateCacheKey, cachedTemplate);
            }
        }

        var templateContext = new TemplateContext(renderModel, options);
        string html = await cachedTemplate.RenderAsync(templateContext);

        return html;
    }

    public async Task RenderHtml(RenderModel renderModel, LiquidRoute liquidRoute, StreamWriter streamWriter)
    {
        var options = _templateOptionsManager.GetTemplateOptions(liquidRoute.RouteTemplate);

        string liquidTemplateCacheKey = $"{liquidRoute.RouteTemplate}-{liquidRoute.LiquidTemplatePath}";

        _liquidTemplateManager.FluidTemplates.TryGetValue(liquidTemplateCacheKey, out var cachedTemplate);
        if (cachedTemplate == null)
        {
            var parser = _fluidParserManager.FluidParser;
            var fileInfo = options.FileProvider.GetFileInfo(liquidRoute.LiquidTemplatePath);
            if (!fileInfo.Exists)
            {
                return;
            }

            string liquidTemplate = await fileInfo.GetFileContents();
            if (parser.TryParse(liquidTemplate, out IFluidTemplate template, out string error))
            {
                if (!string.IsNullOrEmpty(error))
                {
                    throw new LiquidSyntaxException(error);
                }

                cachedTemplate = template;
                _liquidTemplateManager.RegisterTemplate(liquidTemplateCacheKey, cachedTemplate);
            }
        }

        var templateContext = new TemplateContext(renderModel, options);
        await cachedTemplate.RenderAsync(streamWriter, HtmlEncoder.Default, templateContext);
    }
}