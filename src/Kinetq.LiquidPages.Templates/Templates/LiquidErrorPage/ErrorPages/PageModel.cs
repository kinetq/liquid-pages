using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;
using System.Net;

namespace NAMESPACE_PLACEHOLDER.ErrorPages;

/// <summary>
/// Liquid page model for NewPage.
/// This class is the code-behind for NewPage.liquid
/// </summary>
[LiquidErrorPage({{HttpStatusCode}}, "ErrorPages/{{Name}}.liquid")]
public class NewPageModel : LiquidPageModel
{
    // Add your model properties here
    // Properties will be available in the .liquid template using snake_case naming
    // Example:
    // public string Title { get; set; } = "Welcome to NewPage";
    // public DateTime CurrentDate { get; set; } = DateTime.Now;
    // 
    // In template: {{ title }} and {{ current_date }}

    public override Task OnGetAsync(LiquidRequestModel request)
    {
        // Initialize your model properties here
        // This method is called when the page is requested
        return Task.CompletedTask;
    }
}
