using System.Text;
using System.Text.RegularExpressions;
using EmbedIO;
using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using Microsoft.AspNetCore.Routing;

namespace Kinetq.LiquidPages.EmbedIO;

public class LiquidWebModule : WebModuleBase
{
    public ILiquidResponseMiddleware LiquidResponseMiddleware { get; init; } = null!;
    public LiquidRoute? LiquidRoute { get; init; }
    public Regex[] ExcludedPaths { get; set; } = [];

    public LiquidWebModule(string baseRoute) : base(baseRoute)
    {
    }

    protected override async Task OnRequestAsync(IHttpContext context)
    {
        var request = context.Request;
        var response = context.Response;

        if (ExcludedPaths.Any(x => x.IsMatch(request.Url.AbsolutePath)))
        {
            return;
        }

        try
        {
            var liquidRequest = new LiquidRequestModel()
            {
                Route = request.Url.AbsolutePath,
                QueryParams = request.Url.Query.GetQueryParams(),
                Headers = request.Headers,
                Method = request.HttpMethod,
                LiquidRoute = LiquidRoute
            };

            if (liquidRequest.LiquidRoute != null)
            {
                var match = MatchUrlPath(request.Url.AbsolutePath);
                if (match != null)
                {
                    liquidRequest.LiquidRoute.RouteValues = new RouteValueDictionary(match.Pairs);
                }
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

    public override bool IsFinalHandler => false;
}