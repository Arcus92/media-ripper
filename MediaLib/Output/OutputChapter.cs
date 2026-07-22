using MediaLib.Models;

namespace MediaLib.Output;

/// <summary>
/// Defines a chapter in the <see cref="OutputFile"/>.
/// </summary>
[Serializable]
public class OutputChapter
{
    /// <summary>
    /// Gets and sets the chapter name.
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// Gets and sets the start time of the chapter.
    /// </summary>
    public TimeSpan Start { get; set; }
    
    /// <summary>
    /// Gets and sets the end time of the chapter.
    /// </summary>
    public TimeSpan End { get; set; }

    /// <summary>
    /// Returns the output chapters from the list of chapter infos.
    /// </summary>
    /// <param name="chapters">The chapter infos.</param>
    /// <returns>Returns the chapter outputs.</returns>
    public static IEnumerable<OutputChapter> FromChapterInfos(IEnumerable<ChapterInfo> chapters)
    {
        foreach (var chapter in chapters)
        {
            yield return new OutputChapter()
            {
                Name = chapter.Name,
                Start = chapter.Start,
                End = chapter.End
            };
        }
    }
}