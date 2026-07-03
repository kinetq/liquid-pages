using Microsoft.Extensions.FileProviders;

namespace Kinetq.LiquidPages.Helpers;

public static class FileProviderHelpers
{
    internal static async Task<string> GetFileContents(this IFileInfo fileInfo)
    {
        string liquidTemplate;
        // Check if file is embedded (no physical path) or physical file
        if (string.IsNullOrEmpty(fileInfo.PhysicalPath))
        {
            // Embedded file - use FileProvider stream
            using var stream = fileInfo.CreateReadStream();
            using var reader = new StreamReader(stream);
            liquidTemplate = await reader.ReadToEndAsync();
        }
        else
        {
            // Physical file - use File.ReadAllTextAsync
            liquidTemplate = await File.ReadAllTextAsync(fileInfo.PhysicalPath);
        }

        return liquidTemplate;
    }
}