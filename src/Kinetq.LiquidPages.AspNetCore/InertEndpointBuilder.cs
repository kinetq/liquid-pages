using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Kinetq.LiquidPages.AspNetCore;

internal sealed class InertEndpointBuilder : EndpointBuilder
{
    public override Endpoint Build()
    {
        if (RequestDelegate is null)
        {
            throw new InvalidOperationException("RequestDelegate must be provided.");
        }

        return new Endpoint(
            RequestDelegate,
            new EndpointMetadataCollection(Metadata),
            DisplayName);
    }
}