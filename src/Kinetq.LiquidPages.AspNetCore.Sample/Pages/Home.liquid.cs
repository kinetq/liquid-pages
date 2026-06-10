using Kinetq.LiquidPages.Models;
using Kinetq.LiquidPages.Pages;
using Microsoft.Extensions.FileProviders;

namespace Kinetq.LiquidPages.AspNetCore.Sample.Pages;

/// <summary>
/// Liquid page model for Home.
/// This class is the code-behind for Home.liquid
/// </summary>
[LiquidPage("/", "Pages/Home.liquid")]
public class HomeModel : LiquidPageModel
{
    private readonly ILogger<HomeModel> _logger;

    public HomeModel(ILogger<HomeModel> logger)
    {
        _logger = logger;
    }

    public string Title { get; set; } = "Welcome to Home";

    public override Task OnGetAsync(LiquidRequestModel request)
    {
        return Task.CompletedTask;
    }
}
