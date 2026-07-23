namespace DvdLib.Decrypt;

public class DvdCssDecryptStream : Stream
{
    private readonly DvdCss _dvdCss;
    
    public DvdCssDecryptStream(string devicePath, uint titleSetSector, uint cellStartSector, uint cellEndSector)
    {
        _dvdCss = new DvdCss();
        _positionStart = cellStartSector * Dvd.BlockSize;
        _positionEnd = (cellEndSector + 1) * Dvd.BlockSize;
        _length = _positionEnd - _positionStart;

        _bufferLength = 0;
        _bufferOffset = 0;
        
        if (!_dvdCss.Open(devicePath))
        {
            throw new IOException($"DvdCss couldn't open device: {devicePath}");
        }
        
        // Seek to the start of the title set to obtain the decryption key
        if (!_dvdCss.Seek((int)titleSetSector, DvdCssSeekFlags.Key))
        {
            throw new IOException($"DvdCss couldn't seek to the key data: {devicePath} - sector: {titleSetSector}");
        }
        
        // Seek to the initial cell data
        if (!_dvdCss.Seek((int)cellStartSector, DvdCssSeekFlags.Mpeg))
        {
            throw new IOException($"DvdCss couldn't seek to the cell data: {devicePath} - sector: {cellStartSector}");
        }
        _positionCurrent = _positionStart;
    }

    private const int BlocksInBuffer = 32;
    private const int BufferSize = Dvd.BlockSize * BlocksInBuffer;
    private readonly long _positionStart;
    private readonly long _positionEnd;
    private long _positionCurrent;
    private int _bufferOffset;
    private int _bufferLength;
    private readonly byte[] _buffer = new byte[BufferSize];
    private readonly long _length;
    private int ReadBufferAndDecrypt()
    {
        var remaining = _positionEnd - _positionCurrent - _bufferOffset;
        var blocks = (int)(remaining / Dvd.BlockSize);
        if (blocks > BlocksInBuffer) blocks = BlocksInBuffer;
        return _dvdCss.Read(_buffer, blocks, DvdCssReadFlags.Decrypt);
    }
    
    /// <inheritdoc />
    public override void Flush()
    {
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        var readTotal = 0;
        while (count > 0)
        {
            // Detect end of cell
            if (_positionCurrent + _bufferOffset >= _positionEnd)
            {
                break;
            }
            
            if (_bufferLength == 0)
            {
                var ret = ReadBufferAndDecrypt();
                if (ret < 0) break;
                _bufferLength = ret * Dvd.BlockSize;
            }
            
            // Calculate the bytes to read in this chunk
            var read = _bufferLength - _bufferOffset;
            if (read > count) read = count;

            Buffer.BlockCopy(_buffer, _bufferOffset, buffer, offset, read);

            _bufferOffset += read;
            readTotal += read;
            offset += read;
            count -= read;
            
            // Jump to next unit
            if (_bufferOffset == _bufferLength)
            {
                _positionCurrent += _bufferLength;
                _bufferOffset = 0;
                _bufferLength = 0;
            }
        }
        
        return readTotal;
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        // Ignore origin and convert to begin-position
        offset = origin switch
        {
            SeekOrigin.Current => _positionStart + Position + offset,
            SeekOrigin.End => _positionEnd - offset,
            _ => _positionStart + offset
        };


        var newBufferOffset = (int)(offset % Dvd.BlockSize);
        var newFileOffset = offset - newBufferOffset;
            
        // Unit is already loaded
        if (newFileOffset == _positionCurrent && _bufferOffset > 0)
        {
            _bufferOffset = newBufferOffset;
            return offset;
        }

        if (!_dvdCss.Seek((int)(newFileOffset / Dvd.BlockSize), DvdCssSeekFlags.Mpeg))
        {
            throw new IOException("Failed to seek DvdCss stream.");
        }
        
        _positionCurrent = newFileOffset;
        _bufferOffset = newBufferOffset;
        _bufferLength = 0;
        if (_bufferOffset != 0)
        {
            var ret = ReadBufferAndDecrypt();
            _bufferLength = ret * Dvd.BlockSize;
        }

        return offset;
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
    public override bool CanSeek => true;

    /// <inheritdoc />
    public override bool CanWrite => false;

    /// <inheritdoc />
    public override long Length => _length;

    /// <inheritdoc />
    public override long Position
    {
        get => _positionCurrent + _bufferOffset - _positionStart;
        set => Seek(value, SeekOrigin.Begin);
    }
    
    #region Static

    /// <summary>
    /// Opens a filename from a DVD.
    /// </summary>
    /// <param name="devicePath">The DVD device path.</param>
    /// <param name="titleSetSector">The absolute starting sector of the title set.</param>
    /// <param name="cellStartSector">The absolute cell start sector of the tilt set.</param>
    /// <param name="cellEndSector">The absolute cell start sector of the tilt set.</param>
    /// <returns></returns>
    public static DvdCssDecryptStream Open(string devicePath, uint titleSetSector, uint cellStartSector, uint cellEndSector)
    {
        return new DvdCssDecryptStream(devicePath, titleSetSector, cellStartSector, cellEndSector);
    }
    
    #endregion Static
    
    #region IDisposable
    

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _dvdCss.Dispose();
    }
    
    #endregion IDisposable
}