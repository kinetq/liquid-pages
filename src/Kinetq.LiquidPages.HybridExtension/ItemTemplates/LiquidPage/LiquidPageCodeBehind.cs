using Kinetq.LiquidPages.Pages;

namespace $rootnamespace$;

/// <summary>
/// Liquid page model for $fileinputname$.
/// This class is the code-behind for $fileinputname$.liquid
/// </summary>
[LiquidPage("/$fileinputname$", "/$rootnamespace$/$fileinputname$.liquid")]
public class $fileinputname$Model : LiquidPageModel
{
    // Add your model properties here
    // Properties will be available in the .liquid template using snake_case naming
    // Example:
    // public string Title { get; set; } = "Welcome to $fileinputname$";
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
