namespace MediaLib.Models;

/// <summary>
/// Information of a single media item.
/// </summary>
public class MediaInfo
{
    /// <summary>
    /// Gets the media identifier.
    /// </summary>
    public required MediaIdentifier Identifier { get; init; }
    
    /// <summary>
    /// Gets the description if this media item.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Gets the playlist duration.
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Gets the segments / clips.
    /// </summary>
    public SegmentInfo[] Segments { get; set; } = [];

    /// <summary>
    /// Gets the streams.
    /// </summary>
    public StreamInfo[] Streams { get; init; } = [];
    
    /// <summary>
    /// Gets the chapters.
    /// </summary>
    public ChapterInfo[] Chapters { get; set; } = [];
    
    /// <inheritdoc />
    public override string ToString() => Name;
    
    #region Equals

    /// <summary>
    /// Compares this playlist with another one and return true if it matches.
    /// </summary>
    /// <param name="other">The other playlist.</param>
    /// <returns></returns>
    public virtual bool Matches(MediaInfo other)
    {
        if (ReferenceEquals(null, other)) return false;
        
        // Compare segments
        if (Segments.Length != other.Segments.Length) return false;
        for (var i = 0; i < Segments.Length; i++)
        {
            var segment = Segments[i];
            var otherSegment = other.Segments[i];
            
            if (segment.Id != otherSegment.Id) return false;
        }
        
        // Compare streams
        if (Streams.Length != other.Streams.Length) return false;
        for (var j = 0; j < Streams.Length; j++)
        {
            var stream = Streams[j];
            var otherStream = other.Streams[j];
                
            if (stream.Id != otherStream.Id || 
                stream.Type != otherStream.Type) 
                return false;
        }
        
        // Compare chapters
        if (Chapters.Length != other.Chapters.Length) return false;
        for (var i = 0; i < Chapters.Length; i++)
        {
            var chapter = Chapters[i];
            var otherChapter = other.Chapters[i];
            
            if (chapter.Start != otherChapter.Start) return false;
            if (chapter.End != otherChapter.End) return false;
        }
        
        return true;
    }
    
    #endregion Equals
}