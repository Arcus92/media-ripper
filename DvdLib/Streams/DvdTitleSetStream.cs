namespace DvdLib.Streams;

/// <summary>
/// This stream treads a title set "VTS_XX_Y.VOB" as one continues stream.
/// </summary>
public class DvdTitleSetStream
{
    /// <summary>
    /// The DVD reference.
    /// </summary>
    private readonly Dvd _dvd;
    
    /// <summary>
    /// The VMG and title set information.
    /// </summary>
    private DvdTitleSetInfo _info;
    
    /// <summary>
    /// Gets the length of bytes for the whole title set.
    /// </summary>
    private long Length { get; }

    public DvdTitleSetStream(Dvd dvd, DvdTitleSetInfo info)
    {
        _dvd = dvd;
        _info = info;

        Length = info.FileLengths.Sum();
    }
    
    #region Read
    
    /// <summary>
    /// The current reader position.
    /// </summary>
    private long _position = 0;

    /// <summary>
    /// The index of the currently opened part file.
    /// </summary>
    private ushort _openPartIndex;

    /// <summary>
    /// The currently opened part stream.
    /// </summary>
    private Stream? _openPartStream;

    #endregion Read
}