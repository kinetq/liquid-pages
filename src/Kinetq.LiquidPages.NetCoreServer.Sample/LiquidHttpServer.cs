using System.Net;
using Kinetq.LiquidPages.Interfaces;
using NetCoreServer;

namespace Kinetq.LiquidPages.NetCoreServer.Sample;

public class LiquidHttpServer : HttpServer
{
    private readonly IRouteTree _routeTree;
    private readonly ILiquidResponseMiddleware _liquidResponseMiddleware;
    public LiquidHttpServer(
        IPAddress address, 
        int port, IRouteTree routeTree, ILiquidResponseMiddleware liquidResponseMiddleware) : base(address, port)
    {
        _routeTree = routeTree;
        _liquidResponseMiddleware = liquidResponseMiddleware;
    }

    public LiquidHttpServer(string address, int port, IRouteTree routeTree, ILiquidResponseMiddleware liquidResponseMiddleware) : base(address, port)
    {
        _routeTree = routeTree;
        _liquidResponseMiddleware = liquidResponseMiddleware;
    }

    public LiquidHttpServer(DnsEndPoint endpoint, IRouteTree routeTree, ILiquidResponseMiddleware liquidResponseMiddleware) : base(endpoint)
    {
        _routeTree = routeTree;
        _liquidResponseMiddleware = liquidResponseMiddleware;
    }

    public LiquidHttpServer(IPEndPoint endpoint, IRouteTree routeTree, ILiquidResponseMiddleware liquidResponseMiddleware) : base(endpoint)
    {
        _routeTree = routeTree;
        _liquidResponseMiddleware = liquidResponseMiddleware;
    }

    protected override TcpSession CreateSession()
    {
        return new HttpLiquidPagesSession(this, _routeTree, _liquidResponseMiddleware);
    }
}