namespace MediaLib.Models;

public class ChapterInfo : IMediaInfo
{
    /// <summary>
    /// Gets the chapter index.
    /// </summary>
    public required ushort Id { get; init; }

    /// <summary>
    /// Gets the chapter description.
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Gets the start time of the chapter.
    /// </summary>
    public TimeSpan Start { get; init; }
    
    /// <summary>
    /// Gets the end time of the chapter.
    /// </summary>
    public TimeSpan End { get; init; }
    
    /// <summary>
    /// Gets the start time of the chapter.
    /// </summary>
    public TimeSpan Duration => End - Start;
    
    /// <inheritdoc />
    public override string ToString() => Name;

    /// <summary>
    /// Builds the chapter info from the given timestamps.
    /// </summary>
    /// <param name="duration">The total duration.</param>
    /// <param name="timestamps">The chapter timestamps.</param>
    /// <returns>Returns the chapter infos.</returns>
    public static IEnumerable<ChapterInfo> FromTimestamps(TimeSpan duration, IEnumerable<TimeSpan> timestamps)
    {
        var chapterTimestamps = new List<TimeSpan> { TimeSpan.Zero };
        foreach (var timestamp in timestamps)
        {
            var time = timestamp;
            
            // Avoid negative
            if (time < TimeSpan.Zero) time = TimeSpan.Zero;
            // Avoid timestamp above max length. Also add three-second tolerance.
            if (time > duration - TimeSpan.FromSeconds(3)) time = duration;
            chapterTimestamps.Add(time);
        }
        chapterTimestamps.Add(duration);
        chapterTimestamps.Sort();
        
        ushort id = 0;
        for (var i = 0; i < chapterTimestamps.Count - 1; i++)
        {
            var start = chapterTimestamps[i];
            var end = chapterTimestamps[i + 1];
            if (start == end) continue;
            var chapterInfo = new ChapterInfo
            {
                Id = id,
                Name = $"Chapter {id+1:00}",
                Start = start,
                End = end
            };
            id++;
            yield return chapterInfo;
        }
    }
}