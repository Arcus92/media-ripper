using System.Text.Json.Serialization;

namespace MediaRipper.Models;

/// <summary>
/// The application language.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<AppLanguage>))]
public enum AppLanguage
{
    English,
    German
}