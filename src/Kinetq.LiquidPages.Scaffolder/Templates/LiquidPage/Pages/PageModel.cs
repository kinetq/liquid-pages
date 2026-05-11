using Kinetq.LiquidPages.Pages;

namespace NAMESPACE_PLACEHOLDER;

/// <summary>
/// Liquid page model for NewPage.
/// This class is the code-behind for NewPage.liquid
/// </summary>
[LiquidPage("{{RoutePath}}", "{{TemplatePath}}")]
public class NewPageModel : LiquidPageModel
{
    // Add your model properties here
    // Properties will be available in the .liquid template using snake_case naming
    // Example:
    // public string Title { get; set; } = "Welcome to NewPage";
    // public DateTime CurrentDate { get; set; } = DateTime.Now;
    // 
    // In template: {{ title }} and {{ current_date }}

    public override void OnGet()
    {
        // Initialize your model properties here
        // This method is called when the page is requested
        base.OnGet();
    }
}
