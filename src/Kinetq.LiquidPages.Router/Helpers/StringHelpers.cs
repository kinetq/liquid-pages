namespace Kinetq.LiquidPages.Router.Helpers;

public static class StringHelpers
{
    public static string RemoveAppScheme(this string url)
    {
        const string appScheme = "app://";
        if (url.StartsWith(appScheme, StringComparison.OrdinalIgnoreCase))
        {
            return url.Substring(appScheme.Length);
        }
        return url;
    }
}