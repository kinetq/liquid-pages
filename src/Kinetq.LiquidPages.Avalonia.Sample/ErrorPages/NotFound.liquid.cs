using System.Net;
using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;

namespace Kinetq.LiquidPages.Avalonia.Sample.ErrorPages;

[LiquidErrorPage(HttpStatusCode.NotFound, "ErrorPages/NotFound.liquid")]
public class NotFoundModel : LiquidPageModel
{
    public string Title { get; set; } = "404 - Not Found";
    public string Message { get; set; } = "No route or static asset matched this request.";

    public override Task OnGetAsync(LiquidRequestModel request)
    {
        return Task.CompletedTask;
    }
}
