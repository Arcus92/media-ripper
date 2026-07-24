using MediaLib.Formats;
using MediaLib.Models;
using MediaLib.Output;
using MediaLib.Sources;

namespace MediaLib.FileSystem.Sources;

/// <summary>
/// A media source implementation for local files. 
/// </summary>
public class FileSystemMediaSource : IMediaSource
{
    private readonly string _name;
    private readonly string _path;
    private readonly FFmpeg.InputMetadata _metadata;
    
    public FileSystemMediaSource(string path, MediaIdentifier identifier, FFmpeg.InputMetadata metadata)
    {
        _path = path;
        _name = Path.GetFileNameWithoutExtension(path);
        _metadata = metadata;
        Identifier = identifier;
        Info = BuildMediaInfo(_name, metadata);
        IgnoreFlags = MediaIgnoreFlags.None;
    }

    /// <inheritdoc />
    public MediaIdentifier Identifier { get; }
    
    /// <inheritdoc />
    public MediaInfo Info { get; }
    
    /// <inheritdoc />
    public MediaIgnoreFlags IgnoreFlags { get; set; }
    
    /// <summary>
    /// Gets the media info from the given file.
    /// </summary>
    /// <param name="name">The name to the source file.</param>
    /// <param name="metadata">The FFmpeg metadata.</param>
    /// <returns>Returns the media info.</returns>
    private MediaInfo BuildMediaInfo(string name, FFmpeg.InputMetadata metadata)
    {
        return new MediaInfo
        {
            Identifier = Identifier,
            Name = name,
            Duration = metadata.Duration,
            Segments = [new SegmentInfo
            {
                Id = 0,
                Name = name,
                Duration = metadata.Duration
            }],
            Streams = metadata.Streams.Select(stream => new StreamInfo()
            {
                Id = (ushort)stream.Id,
                Name = stream.Title ?? stream.Type.ToString(),
                Type = MapStreamType(stream.Type),
                Format = MapFormat(stream.Format),
                Flags = MapStreamFlags(stream),
                Enabled = true,
                LanguageCode = stream.Language
            }).ToArray(),
            Chapters = metadata.Chapters.Select((chapter, index) => new ChapterInfo()
            {
                Name = chapter.Title ?? $"Chapter {index + 1:00}",
                Start = chapter.Start,
                End = chapter.End,
            }).ToArray(),
        };
    }
    
    /// <inheritdoc />
    public OutputDefinition CreateDefaultOutputDefinition(CodecOptions codec, MediaFormat containerFormat)
    {
        return OutputHelper.CreateDefaultOutputDefinition(_name, Info, codec, containerFormat);
    }

    /// <summary>
    /// Maps the FFmpeg stream type to the output stream type.
    /// </summary>
    /// <param name="streamType">The FFmpeg stream type.</param>
    /// <returns>Returns the stream output type.</returns>
    private static StreamType MapStreamType(FFmpeg.StreamType streamType)
    {
        return streamType switch
        {
            FFmpeg.StreamType.Video => StreamType.Video,
            FFmpeg.StreamType.Audio => StreamType.Audio,
            FFmpeg.StreamType.Subtitle => StreamType.Subtitle,
            FFmpeg.StreamType.Attachment => StreamType.Attachment,
            _ => throw new ArgumentOutOfRangeException(nameof(streamType), streamType, null)
        };
    }

    /// <summary>
    /// Maps the FFMpeg stream metadata to the output flags.
    /// </summary>
    /// <param name="streamMetadata">The FFmpeg stream metadata.</param>
    /// <returns>Returns the stream output flags.</returns>
    private static StreamFlags MapStreamFlags(FFmpeg.StreamMetadata streamMetadata)
    {
        var flags = StreamFlags.None;

        if (streamMetadata.IsDefault)
        {
            flags |= StreamFlags.Default;
        }
        if (streamMetadata.IsForced)
        {
            flags |= StreamFlags.Forced;
        }
        
        return flags;
    }
    
    /// <summary>
    /// Maps the format to an FFmpeg format name. 
    /// </summary>
    /// <param name="format">The input format name.</param>
    /// <returns>Returns the FFmpeg format name.</returns>
    private static string MapFormat(string format)
    {
        return format switch
        {
            "subrip" => SubtitleFormats.Subrip.FFmpegFormat,
            _ => format
        };
    }
}