using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;

namespace Kinetq.LiquidPages.Sample.Pages;

/// <summary>
/// Liquid page model for Home.
/// This class is the code-behind for Home.liquid
/// </summary>
[LiquidPage("^/$", "Pages/Home.liquid")]
public class HomeModel : LiquidPageModel
{
    public string Title { get; set; } = "Welcome to Home";

    public override Task OnGetAsync(LiquidRequestModel request)
    {
        // Initialize your model properties here
        // This method is called when the page is requested

        return Task.CompletedTask;
    }
}
