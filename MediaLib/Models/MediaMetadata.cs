using System.Text;
using System.Text.Json.Serialization;
using MediaLib.Utils.IO;

namespace MediaLib.Models;

[Serializable]
public class MediaMetadata
{
    /// <summary>
    /// Gets and sets the media type.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public MediaType Type { get; set; } = MediaType.Unset;

    /// <summary>
    /// Gets and sets the name of this file.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Name { get; set; } = "";
    
    /// <summary>
    /// Gets and sets the season.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Season { get; set; }
    
    /// <summary>
    /// Gets and sets the episode.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Episode { get; set; }
    
    /// <summary>
    /// Gets and sets the IMDB id.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ImdbId { get; set; }

    /// <summary>
    /// Gets the basename if the media info.
    /// </summary>
    /// <returns>Returns the basename.</returns>
    public string GetBasename()
    {
        var builder = new StringBuilder();
        if (Season.HasValue && Episode.HasValue)
        {
            builder.Append($"S{Season.Value:00}E{Episode.Value:00} ");
        }

        builder.Append(Name);
        FileHandler.RemoveInvalidCharsFromFilename(builder);
        return builder.ToString();
    }
}