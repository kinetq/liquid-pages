using GenHTTP.Api.Protocol;

namespace Kinetq.LiquidPages.GenHTTP;

internal sealed class ByteArrayContent : IResponseContent
{
    private readonly byte[] _data;

    public ulong? Length => (ulong)_data.Length;

    public ByteArrayContent(byte[] data)
    {
        _data = data;
    }

    public ValueTask<ulong?> CalculateChecksumAsync() => ValueTask.FromResult(Length);

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        await target.WriteAsync(_data);
    }
}
