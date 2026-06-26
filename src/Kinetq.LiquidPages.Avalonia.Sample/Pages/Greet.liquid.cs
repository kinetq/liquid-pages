using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;

namespace Kinetq.LiquidPages.Avalonia.Sample.Pages;

[LiquidPage("/hello/{name}", "Pages/Greet.liquid")]
public class GreetModel : LiquidPageModel
{
    public string Name { get; set; } = "friend";

    public override Task OnGetAsync(LiquidRequestModel request)
    {
        if (request.RouteValues.TryGetValue("name", out var value) && value is not null)
        {
            Name = value.ToString() ?? "friend";
        }

        return Task.CompletedTask;
    }
}
