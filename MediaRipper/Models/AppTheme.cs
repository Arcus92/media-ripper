using System.Text.Json.Serialization;

namespace MediaRipper.Models;

/// <summary>
/// The app theme mode.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<AppTheme>))]
public enum AppTheme
{
    Default,
    Light,
    Dark
}