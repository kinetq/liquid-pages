using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;

namespace Kinetq.LiquidPages.GenHTTP.Sample.Pages;

/// <summary>
/// Liquid page model for ContactUs.
/// This class is the code-behind for ContactUs.liquid
/// </summary>
[LiquidPage("/test/contact-us", "Pages/ContactUs.liquid")]
public class ContactUsModel : LiquidPageModel
{
    // Add your model properties here
    // Properties will be available in the .liquid template using snake_case naming
    // Example: {{ view_model.title }}

    public string Title { get; set; } = "Welcome to ContactUs";

    public override Task OnGetAsync(LiquidRequestModel request)
    {
        // Initialize your model properties here
        // This method is called when the page is requested

        return Task.CompletedTask;
    }
}
