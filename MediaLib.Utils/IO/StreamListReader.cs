namespace MediaLib.Utils.IO;

/// <summary>
/// A reader that combines multiple streams into one concatenated stream.
/// The streams are created via a factory function and are opened once needed, when the previous stream was read.
/// </summary>
public class StreamListReader : Stream
{
    private readonly Func<Stream>[] _streamFactories;
    private Stream? _currentStream;
    private int _currentStreamIndex;
    private long _position;

    /// <summary>
    /// Creates the stream list with the given stream factories.
    /// </summary>
    /// <param name="factories">The factory functions to create all streams.</param>
    public StreamListReader(IEnumerable<Func<Stream>> factories)
    {
        _streamFactories = factories.ToArray();
    }
    
    /// <inheritdoc />
    public override void Flush()
    {
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        do
        {
            if (_currentStream is null)
            {
                if (_currentStreamIndex >= _streamFactories.Length)
                {
                    return 0;
                }

                _currentStream = _streamFactories[_currentStreamIndex].Invoke();
            }

            var read = _currentStream.Read(buffer, offset, count);
            _position += read;

            // End of current stream
            if (read == 0)
            {
                _currentStream.Dispose();
                _currentStream = null;
                _currentStreamIndex++;
                continue;
            }

            return read;
        } while (true);
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override bool CanRead => true;

    /// <inheritdoc />
    public override bool CanSeek => false;

    /// <inheritdoc />
    public override bool CanWrite => false;

    /// <inheritdoc />
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc />
    public override long Position
    {
        get => _position;
        set => Seek(value, SeekOrigin.Begin);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (_currentStream is null) return;
        _currentStream.Dispose();
        _currentStream = null;
    }
}