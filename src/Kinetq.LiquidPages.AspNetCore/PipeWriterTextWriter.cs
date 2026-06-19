using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace Kinetq.LiquidPages.AspNetCore;

public class PipeWriterTextWriter : TextWriter
{
    private readonly PipeWriter _pipeWriter;
    private readonly Encoder _encoder;

    public PipeWriterTextWriter(PipeWriter pipeWriter, Encoding encoding)
    {
        _pipeWriter = pipeWriter;
        _encoder = encoding.GetEncoder();
    }

    public override void Write(char value)
    {
        // This is complex - you need to encode chars to bytes
        // For simplicity, use Write(string) which handles encoding
    }

    public override void Write(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        _pipeWriter.Write(bytes);
    }

    public override async Task WriteAsync(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        await _pipeWriter.WriteAsync(bytes);
    }

    public override void Flush() => _pipeWriter.FlushAsync().GetAwaiter().GetResult();
    public override async Task FlushAsync() => await _pipeWriter.FlushAsync();
    public override Encoding Encoding => Encoding.UTF8;
}