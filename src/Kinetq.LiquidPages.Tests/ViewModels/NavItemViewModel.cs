namespace Kinetq.LiquidPages.Tests.ViewModels;

public class NavItemViewModel
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public bool Active { get; set; }
    public bool IsRoot { get; set; }
    public IList<NavItemViewModel> Children { get; set; }
}