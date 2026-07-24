namespace MediaLib.Models;

public class SegmentInfo
{
    /// <summary>
    /// Gets the clip id of the segment.
    /// </summary>
    public required ushort Id { get; init; }

    /// <summary>
    /// Gets the segment description.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the duration.
    /// </summary>
    public TimeSpan Duration { get; init; }
    
    /// <inheritdoc />
    public override string ToString() => Name;
}