namespace Kinetq.LiquidPages.Extension.Helpers;

public static class StringHelpers
{
    public static string ToPascalCase(this string input)
    {
        input = input.Trim();
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // Remove invalid characters and split by common separators
        var parts = input.Split(new[] { ' ', '-', '_', '.' }, StringSplitOptions.RemoveEmptyEntries);

        var result = new System.Text.StringBuilder();

        foreach (var part in parts)
        {
            if (part.Length == 0)
                continue;

            // Capitalize first letter, lowercase the rest
            result.Append(char.ToUpperInvariant(part[0]));

            if (part.Length > 1)
            {
                result.Append(part.Substring(1).ToLowerInvariant());
            }
        }

        return result.ToString();
    }
}