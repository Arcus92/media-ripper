using System.Text.Json.Serialization;
using MediaLib.Output;

namespace MediaLib.Models;

/// <summary>
/// Defines a stream in an <see cref="OutputFile"/>.
/// </summary>
[Serializable]
public class StreamInfo
{
    /// <summary>
    /// Gets and sets the stream id.
    /// </summary>
    public ushort Id { get; set; }

    /// <summary>
    /// Gets and sets the stream name.
    /// </summary>
    [JsonIgnore]
    public string? Name { get; set; }
    
    /// <summary>
    /// Gets and sets the stream type.
    /// </summary>
    public StreamType Type { get; set; }
    
    /// <summary>
    /// Gets and sets if this stream is enabled for export.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Gets and sets the language code of this stream.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LanguageCode { get; set; }
    
    /// <summary>
    /// Gets the media format of this stream.
    /// </summary>
    public string? Format { get; set; }
    
    /// <summary>
    /// Gets the additional stream flags.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public StreamFlags Flags { get; set; }
}