using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;

namespace Kinetq.LiquidPages.Tests.Pages;

/// <summary>
/// Liquid page model for AboutUs.
/// This class is the code-behind for AboutUs.liquid
/// </summary>
[LiquidPage("about-us", "Pages/NewPage.liquid")]
public class AboutUsModel : LiquidPageModel
{
    // Add your model properties here
    // Properties will be available in the .liquid template using snake_case naming
    // Example:
    // public string Title { get; set; } = "Welcome to AboutUs";
    // public DateTime CurrentDate { get; set; } = DateTime.Now;
    // 
    // In template: {{ title }} and {{ current_date }}

    public override Task OnGetAsync(LiquidRequestModel request)
    {
        // Initialize your model properties here
        // This method is called when the page is requested
    }
}
