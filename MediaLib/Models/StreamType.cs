using System.Text.Json.Serialization;

namespace MediaLib.Models;

/// <summary>
/// Defines the type of <see cref="StreamInfo"/>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<StreamType>))]
public enum StreamType
{
    Video,
    Audio,
    Subtitle,
    Attachment,
}