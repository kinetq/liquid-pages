using Kinetq.LiquidPages.Helpers;
using Kinetq.LiquidPages.Interfaces;
using Kinetq.LiquidPages.Models;
using NetCoreServer;
using System.Net;
using System.Text;

namespace Kinetq.LiquidPages.NetCoreServer
{
    public class HttpsLiquidPagesSession : HttpsSession
    {
        private readonly IRouteTree _routeTree;
        private readonly ILiquidResponseMiddleware _liquidResponseMiddleware;
        public HttpsLiquidPagesSession(HttpsServer server, IRouteTree routeTree, ILiquidResponseMiddleware liquidResponseMiddleware) : base(server)
        {
            _routeTree = routeTree;
            _liquidResponseMiddleware = liquidResponseMiddleware;
        }

        protected override void OnReceivedRequest(HttpRequest request)
        {
            var routeMatch = _routeTree.Match(request.Url);
            var liquidRoute = routeMatch?.LiquidRoute;
            var liquidRequest = new LiquidRequestModel
            {
                Route = request.Url,
                QueryParams = new NetCoreServerQueryParams(request.Url.GetQueryParams()),
                Headers = new NetCoreServerHeaderDictionary(request),
                Method = request.Method,
                LiquidRoute = liquidRoute,
                ErrorStatusCode = liquidRoute == null && routeMatch == null ? (int?)HttpStatusCode.NotFound : null,
                RouteValues = routeMatch?.RouteValues ?? EmptyRouteValuesDictionary.Instance
            };

            var responseBuilder = new NetCoreServerResponseBuilder(Response, null);
            using var contentStream = new MemoryStream();
            using var streamWriter = new StreamWriter(contentStream, Encoding.UTF8, leaveOpen: true);

            Task.Run(() => _liquidResponseMiddleware.HandleRequestAsync(liquidRequest, responseBuilder))
                .GetAwaiter()
                .GetResult();

            streamWriter.Flush();
            Response.MakeGetResponse(contentStream.GetBuffer());
        }
    }
}
