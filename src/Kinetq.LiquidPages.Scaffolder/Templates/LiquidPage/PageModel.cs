using Kinetq.LiquidPages.Pages;

namespace {{Namespace}};

/// <summary>
/// Liquid page model for {{FileName}}.
/// This class is the code-behind for {{FileName}}.liquid
/// </summary>
[LiquidPage("{{RoutePath}}", "{{TemplatePath}}")]
public class {{FileName}}Model : LiquidPageModel
{
    // Add your model properties here
    // Properties will be available in the .liquid template using snake_case naming
    // Example:
    // public string Title { get; set; } = "Welcome to {{FileName}}";
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
