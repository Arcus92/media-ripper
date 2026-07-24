using System.Text.Json.Serialization;

namespace MediaLib.Models;

/// <summary>
/// Defines additional flags of <see cref="StreamInfo"/>.
/// </summary>
[Flags]
[JsonConverter(typeof(JsonStringEnumConverter<StreamFlags>))]
public enum StreamFlags
{
    None = 0,
    Default = 1 << 0,
    Secondary = 1 << 1,
    Forced = 1 << 2
}