using System.Net;
using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;

namespace Kinetq.LiquidPages.Tests.Pages;

[LiquidPageError(HttpStatusCode.NotFound, "Pages/NotFound.liquid")]
public class NotFound : LiquidPageModel
{
    public override Task OnGetAsync(LiquidRequestModel request)
    {
        return Task.CompletedTask;
    }
}
