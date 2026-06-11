using System.Net;
using EmbedIO;
using EmbedIO.Routing;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using System.Text;

namespace Kinetq.LiquidPages.EmbedIO;

public class LiquidWebModule : RoutingModuleBase
{
    public ILiquidResponseMiddleware LiquidResponseMiddleware { get; init; } = null!;

    public LiquidWebModule(string baseRoute, ILiquidRoutesManager routesManager) : base(baseRoute)
    {
        foreach (var liquidRoute in routesManager.LiquidRoutes)
        {
            if (string.IsNullOrWhiteSpace(liquidRoute.RouteTemplate))
            {
                continue;
            }

            var matcher = RouteMatcher.Parse(liquidRoute.RouteTemplate, false);
            AddHandler(HttpVerbs.Any, matcher, (context, routeMatch) => HandleLiquidRequestAsync(context, liquidRoute, routeMatch));
        }
    }

    protected override Task OnPathNotFoundAsync(IHttpContext context)
        => HandleLiquidRequestAsync(context, null, null);

    private async Task HandleLiquidRequestAsync(IHttpContext context, LiquidRoute? liquidRoute, RouteMatch? routeMatch)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            var liquidRequest = new LiquidRequestModel()
            {
                Route = request.Url.AbsolutePath,
                QueryParams = request.Url.Query.GetQueryParams(),
                Headers = request.Headers,
                Method = request.HttpMethod,
                LiquidRoute = liquidRoute,
                ErrorStatusCode = liquidRoute == null && routeMatch == null ? (int?)HttpStatusCode.NotFound : null
            };

            if (routeMatch != null)
            {
                liquidRequest.RouteValues = (IDictionary<string, object?>)routeMatch.Pairs.ToDictionary();
            }

            if (request.HasEntityBody)
            {
                using var reader = new StreamReader(request.InputStream, Encoding.UTF8);
                liquidRequest.Body = await reader.ReadToEndAsync();
            }

            var responseModel =
                await LiquidResponseMiddleware.HandleRequestAsync(liquidRequest);

            response.ContentLength64 = responseModel.Content.Length;
            response.ContentType = responseModel.ContentType;
            response.StatusCode = responseModel.StatusCode;

            await response.OutputStream.WriteAsync(responseModel.Content);
        }
        catch (Exception ex)
        {
            response.StatusCode = 500;
            byte[] errorBuffer = Encoding.UTF8.GetBytes($"Internal Server Error: {ex.Message}");
            response.ContentLength64 = errorBuffer.Length;
            response.ContentType = "text/html";
            await response.OutputStream.WriteAsync(errorBuffer);
        }
        finally
        {
            response.Close();
        }
    }

    public override bool IsFinalHandler => true;
}