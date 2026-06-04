namespace Kinetq.LiquidPages.Statiq;

public static class LiquidKeys
{
    /// <summary>
    /// Document metadata key under which <see cref="ExecutePageModel"/> stores
    /// the executed <see cref="Kinetq.LiquidPages.Pages.LiquidPageModel"/> instance.
    /// <see cref="Modules.RenderLiquidTemplate"/> reads this key to use the model
    /// as the Fluid root context, matching the behaviour of the HTTP server integrations.
    /// </summary>
    public const string PageModel = nameof(PageModel);
}
