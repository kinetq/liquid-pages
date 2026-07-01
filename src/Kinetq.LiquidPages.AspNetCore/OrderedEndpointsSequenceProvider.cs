using System.Threading;

namespace Kinetq.LiquidPages.AspNetCore;

internal sealed class OrderedEndpointsSequenceProvider
{
    private int _current;

    public int GetNext()
    {
        return Interlocked.Increment(ref _current);
    }
}