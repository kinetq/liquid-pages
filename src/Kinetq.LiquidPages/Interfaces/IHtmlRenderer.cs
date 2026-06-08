using Kinetq.LiquidPages.Models;

namespace Kinetq.LiquidPages.Interfaces;

public interface IHtmlRenderer
{
    Task<string?> RenderHtml(RenderModel renderModel, LiquidRoute? liquidRoute);
}