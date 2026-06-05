using Microsoft.AspNetCore.Builder;

namespace Kinetq.LiquidPages.AspNetCore;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseLiquidPages(this IApplicationBuilder app)
    {
        return app.UseMiddleware<LiquidPagesMiddleware>();
    }
}
