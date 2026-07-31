using MediaLib.Models;

namespace MediaRipper.Models.Sources;

public class StreamSourceModel : BaseSourceModel
{
    /// <inheritdoc cref="IsDefault" />
    private bool _isDefault;

    public StreamSourceModel(StreamInfo stream)
    {
        Stream = stream;
    }

    /// <summary>
    ///     Gets the stream info.
    /// </summary>
    public StreamInfo Stream { get; }

    /// <summary>
    ///     Gets the stream id.
    /// </summary>
    public ushort Id => Stream.Id;

    /// <summary>
    ///     Gets and sets if this stream is the default track for the export.
    /// </summary>
    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }
}