using Kinetq.LiquidPages.Pages;
using Microsoft.Extensions.FileProviders;

namespace Kinetq.LiquidPages.Tests.Pages;

[LiquidPage("/home", "/Pages/home.liquid")]
public class Home : LiquidPageModel
{
    public override IFileProvider GetFileProvider()
    {
        return new EmbeddedFileProvider(typeof(LiquidResponseMiddlewareTests).Assembly, "Kinetq.LiquidPages.Tests.Pages");
    }
}