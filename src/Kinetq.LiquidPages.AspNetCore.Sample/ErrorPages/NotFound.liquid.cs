using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;
using Microsoft.Extensions.FileProviders;
using System.Net;

namespace Kinetq.LiquidPages.AspNetCore.Sample.ErrorPages;

/// <summary>
/// Liquid page model for NotFound.
/// This class is the code-behind for NotFound.liquid
/// </summary>
[LiquidErrorPage(HttpStatusCode.NotFound, "ErrorPages/NotFound.liquid")]
public class NotFoundModel : LiquidPageModel
{
    public string Title { get; set; } = "Page Not Found";
    public string NotFoundMessage { get; set; } = "The page you are looking for was not found.";

    public override IFileProvider GetFileProvider()
    {
        string workingDirectory = Directory.GetCurrentDirectory();
        return new PhysicalFileProvider(workingDirectory);
    }

    public override Task OnGetAsync(LiquidRequestModel request)
    {
        return Task.CompletedTask;
    }
}
