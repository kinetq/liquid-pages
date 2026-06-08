using System.Net;
using Kinetq.LiquidPages.Models;
using Microsoft.Extensions.FileProviders;

namespace Kinetq.LiquidPages.Interfaces;

public interface ILiquidRoutesManager
{
    void RegisterRoute(LiquidRoute route);
    void RegisterErrorRoute(int statusCode, LiquidRoute route);
    IList<LiquidRoute> LiquidRoutes { get; }
    IDictionary<int, LiquidRoute> ErrorRoutes { get; }
    LiquidRoute? GetRouteForPath(string path);
    LiquidRoute? GetRouteForStatusCode(HttpStatusCode statusCode);
    IFileProvider? GetFileProviderForAsset(string filePath);
}