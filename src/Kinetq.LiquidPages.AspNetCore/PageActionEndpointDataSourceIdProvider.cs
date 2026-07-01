using System.Threading;

namespace Kinetq.LiquidPages.AspNetCore;

internal sealed class PageActionEndpointDataSourceIdProvider
{
    private int _nextId;

    public int CreateId()
    {
        return Interlocked.Increment(ref _nextId);
    }
}