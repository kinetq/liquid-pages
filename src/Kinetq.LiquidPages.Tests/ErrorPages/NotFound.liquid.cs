using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;
using System.Net;

namespace Kinetq.LiquidPages.Tests.ErrorPages;

/// <summary>
/// Liquid page model for NotFound.
/// This class is the code-behind for NotFound.liquid
/// </summary>
[LiquidErrorPage(HttpStatusCode.NotFound, "ErrorPages/NotFound.liquid")]
public class NotFoundModel : LiquidPageModel
{
    // Add your model properties here
    // Properties will be available in the .liquid template using snake_case naming
    // Example:
    // public string Title { get; set; } = "Welcome to NotFound";
    // public DateTime CurrentDate { get; set; } = DateTime.Now;
    // 
    // In template: {{ title }} and {{ current_date }}

    public override Task OnGetAsync(LiquidRequestModel request)
    {
        // Initialize your model properties here
        // This method is called when the page is requested
    }
}
