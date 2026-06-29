using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Interfaces;

public interface IHtmlRenderer
{
    Task<string?> RenderHtml(
        string prefix,
        string templatePath,
        RenderModel renderModel);
    Task RenderHtml(RenderModel renderModel, LiquidRoute liquidRoute, TextWriter streamWriter);
    Task<string?> RenderHtml(RenderModel renderModel, LiquidRoute liquidRoute);
}