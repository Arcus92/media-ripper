using System.Text.Json.Serialization;

namespace MediaLib.Models;

/// <summary>
/// Defines the type of media for the <see cref="MediaMetadata"/>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<MediaType>))]
public enum MediaType
{
    Unset,
    Movie,
    Episode,
    Extra,
    MakingOf,
    BehindTheScenes,
    DeletedScenes,
    Interview,
    Trailer,
}