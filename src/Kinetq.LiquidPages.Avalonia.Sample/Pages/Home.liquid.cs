using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;

namespace Kinetq.LiquidPages.Avalonia.Sample.Pages;

[LiquidPage("/", "Pages/Home.liquid")]
public class HomeModel : LiquidPageModel
{
    public string Title { get; set; } = "LiquidPages Avalonia Sample";
    public string Description { get; set; } = "Rendered through IRouteTree without running a web server.";

    public override Task OnGetAsync(LiquidRequestModel request)
    {
        return Task.CompletedTask;
    }
}
