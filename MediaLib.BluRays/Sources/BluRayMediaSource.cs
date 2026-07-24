using BluRayLib;
using BluRayLib.Enums;
using BluRayLib.Mpls;
using MediaLib.Formats;
using MediaLib.Models;
using MediaLib.Output;
using MediaLib.Sources;

namespace MediaLib.BluRays.Sources;

public class BluRayMediaSource : IMediaSource
{
    /// <summary>
    /// The internal BluRay playlist.
    /// </summary>
    private readonly Playlist _playlist;

    public BluRayMediaSource(Playlist playlist, MediaIdentifier identifier)
    {
        if (!ushort.TryParse(identifier.Id, out var playlistId))
        {
            throw new ArgumentException("Couldn't parse playlist id.", nameof(identifier));
        }
        
        _playlist = playlist;
        Identifier = identifier;
        PlaylistId = playlistId;
        Info = BuildMediaInfo();
    }

    #region Media info
    
    /// <inheritdoc />
    public MediaIdentifier Identifier { get; }
    
    /// <inheritdoc />
    public MediaInfo Info { get; }
    
    /// <inheritdoc />
    public MediaIgnoreFlags IgnoreFlags { get; set; }

    /// <summary>
    /// Gets the playlist id.
    /// </summary>
    public ushort PlaylistId { get; }
    
    /// <summary>
    /// Builds the media info from the BluRay source.
    /// </summary>
    /// <returns></returns>
    private MediaInfo BuildMediaInfo()
    {
        // Build segments
        var playlistDuration = TimeSpan.Zero;
        var segmentInfos = new List<SegmentInfo>();
        foreach (var item in _playlist.Items)
        {
            if (!ushort.TryParse(item.Name, out var clipId))
                continue;
                
            var segmentInfo = new SegmentInfo
            {
                Id = clipId,
                Name = $"Segment {clipId}",
                Duration = BluRay.TimeSpanFromBluRayTime(item.Duration),
            };
            playlistDuration += segmentInfo.Duration;
            segmentInfos.Add(segmentInfo);
        }
        
        if (_playlist.Items.Length == 0)
            throw new ArgumentException("Cannot create output for title without segments!", nameof(PlaylistId));
        
        var segment = _playlist.Items[0];

        // Calculate total duration
        var duration = TimeSpan.Zero;
        foreach (var item in _playlist.Items)
        {
            duration += BluRay.TimeSpanFromBluRayTime(item.Duration);
        }
        
        // Collect all streams
        var streams = new List<StreamInfo>();
        var first = true;
        
        // Video streams
        foreach (var stream in segment.StreamNumberTable.PrimaryVideoStreams)
        {
            if (stream.Entry.RefToStreamId == 0) continue;
            streams.Add(new StreamInfo()
            {
                Id = stream.Entry.RefToStreamId,
                Type = StreamType.Video,
                Name = GetDescriptionFromStream(stream),
                Format = stream.Attributes.VideoFormat.ToString(),
                Flags = first ? StreamFlags.Default : StreamFlags.None,
            });
            first = false;
        }
        foreach (var stream in segment.StreamNumberTable.SecondaryVideoStream)
        {
            if (stream.Entry.RefToStreamId == 0) continue;
            streams.Add(new StreamInfo()
            {
                Id = stream.Entry.RefToStreamId,
                Type = StreamType.Video,
                Name = GetDescriptionFromStream(stream),
                Format = stream.Attributes.VideoFormat.ToString(),
                Flags = StreamFlags.Secondary | (first ? StreamFlags.Default : StreamFlags.None),
            });
            first = false;
        }
        
        // Audio streams
        first = true;
        foreach (var stream in segment.StreamNumberTable.PrimaryAudioStreams)
        {
            if (stream.Entry.RefToStreamId == 0) continue;
            streams.Add(new StreamInfo()
            {
                Id = stream.Entry.RefToStreamId,
                Type = StreamType.Audio,
                Name = GetDescriptionFromStream(stream),
                Format = stream.Attributes.AudioFormat.ToString(),
                LanguageCode = stream.Attributes.LanguageCode,
                Flags = first ? StreamFlags.Default : StreamFlags.None,
            });
            first = false;
        }
        foreach (var stream in segment.StreamNumberTable.SecondaryAudioStream)
        {
            if (stream.Entry.RefToStreamId == 0) continue;
            streams.Add(new StreamInfo()
            {
                Id = stream.Entry.RefToStreamId,
                Type = StreamType.Audio,
                Name = GetDescriptionFromStream(stream),
                Format = stream.Attributes.AudioFormat.ToString(),
                LanguageCode = stream.Attributes.LanguageCode,
                Flags = StreamFlags.Secondary | (first ? StreamFlags.Default : StreamFlags.None),
            });
            first = false;
        }
        
        // Subtitles
        first = true;
        foreach (var stream in segment.StreamNumberTable.PrimaryPgStreams)
        {
            if (stream.Entry.RefToStreamId == 0) continue;
            streams.Add(new StreamInfo()
            {
                Id = stream.Entry.RefToStreamId,
                Type = StreamType.Subtitle,
                Name = GetDescriptionFromStream(stream),
                Format = SubtitleFormats.Pgs.FFmpegFormat,
                LanguageCode = stream.Attributes.LanguageCode,
                Flags = first ? StreamFlags.Default : StreamFlags.None,
            });
            first = false;
        }
        foreach (var stream in segment.StreamNumberTable.SecondaryPgStream)
        {
            if (stream.Entry.RefToStreamId == 0) continue;
            streams.Add(new StreamInfo()
            {
                Id = stream.Entry.RefToStreamId,
                Type = StreamType.Subtitle,
                Name = GetDescriptionFromStream(stream),
                Format = SubtitleFormats.Pgs.FFmpegFormat,
                LanguageCode = stream.Attributes.LanguageCode,
                Flags = StreamFlags.Secondary | (first ? StreamFlags.Default : StreamFlags.None),
            });
            first = false;
        }
        
        // Assume the second subtitle of each language is the forced subtitle
        var languageCounter = new Dictionary<string, int>();
        foreach (var stream in streams.Where(stream => stream.Type == StreamType.Subtitle))
        {
            var languageCode = stream.LanguageCode ?? "";
            languageCounter.TryGetValue(languageCode, out var counter);

            if (counter == 1)
            {
                stream.Flags |= StreamFlags.Forced;
            }
            
            languageCounter[languageCode] = counter + 1;
        }
        
        var chapters = ChapterInfo.FromTimestamps(
            duration,
            _playlist.Marks.Select(mark =>
            {
                // Adding all previous durations
                uint offset = 0;
                for (var i = 0; i < mark.PlayItemId; i++)
                {
                    offset += _playlist.Items[i].Duration;
                }

                var item = _playlist.Items[mark.PlayItemId];
                return BluRay.TimeSpanFromBluRayTime(offset + mark.TimeStamp - item.InTime);
            }));
        
        return new MediaInfo
        {
            Identifier = Identifier,
            Name = $"Playlist {PlaylistId}",
            Duration = playlistDuration,
            Segments = segmentInfos.ToArray(),
            Streams = streams.ToArray(),
            Chapters = chapters.ToArray(),
        };
    }
    
    /// <summary>
    /// Returns a description of a BluRay stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>Returns the description as text.</returns>
    private static string GetDescriptionFromStream(PlaylistStream stream)
    {
        return stream.Attributes.CodingType switch
        {
            // Video
            StreamCodingType.MPEG1VideoStream => "MPEG1",
            StreamCodingType.MPEG2VideoStream => "MPEG2",
            StreamCodingType.MPEG4AVCVideoStream => "MPEG4 AVC",
            StreamCodingType.MPEG4MVCVideoStream => "MPEG4 MVC",
            StreamCodingType.SMTPEVC1VideoStream => "SMTPEVC1",
            StreamCodingType.HEVCVideoStream => "HEVC",
            // Audio
            StreamCodingType.MPEG1AudioStream => "MPEG1",
            StreamCodingType.MPEG2AudioStream => "MPEG2",
            StreamCodingType.LPCMAudioStream => "LPCM",
            StreamCodingType.DolbyDigitalAudioStream => "DolbyDigital",
            StreamCodingType.DtsAudioStream => "DTS",
            StreamCodingType.DolbyDigitalTrueHDAudioStream => "Dolby TrueHD",
            StreamCodingType.DolbyDigitalPlusAudioStream => "Dolby Digital Plus",
            StreamCodingType.DtsHDHighResolutionAudioStream => "DTS HD",
            StreamCodingType.DtsHDMasterAudioStream => "DTS HD Master",
            StreamCodingType.DolbyDigitalPlusSecondaryAudioStream => "Dolby Digital Plus (secondary)",
            StreamCodingType.DtsHDSecondaryAudioStream => "DTS HD (secondary)",
            // Subtitle
            StreamCodingType.PresentationGraphicsStream => "PGS",
            StreamCodingType.InteractiveGraphicsStream => "IGS",
            StreamCodingType.TextSubtitleStream => "STR",
            _ => "Stream"
        };
    }

    #endregion Media info
    
    #region Output
    
    /// <inheritdoc />
    public OutputDefinition CreateDefaultOutputDefinition(CodecOptions codec, MediaFormat containerFormat)
    {
        var baseName = $"{Identifier.DiskName}_{PlaylistId}";
        return OutputHelper.CreateDefaultOutputDefinition(baseName, Info, codec, containerFormat);
    }
    
    #endregion Output
}