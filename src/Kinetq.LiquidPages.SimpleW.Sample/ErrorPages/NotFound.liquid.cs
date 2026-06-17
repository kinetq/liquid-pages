using System.Net;
using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;

namespace Kinetq.LiquidPages.SimpleW.Sample.ErrorPages;

/// <summary>
/// Liquid page model for NotFound.
/// This class is the code-behind for NotFound.liquid
/// </summary>
[LiquidErrorPage(HttpStatusCode.NotFound, "ErrorPages/NotFound.liquid")]
public class NotFoundModel : LiquidPageModel
{
    public string Title { get; set; } = "Page Not Found";
    public string NotFoundMessage { get; set; } = "The page you are looking for was not found.";
    public override Task OnGetAsync(LiquidRequestModel request)
    {
        // Initialize your model properties here
        // This method is called when the page is requested
        return Task.CompletedTask;
    }
}
