using DvdLib;
using MediaLib.Formats;
using MediaLib.Models;
using MediaLib.Output;
using MediaLib.Sources;

namespace MediaLib.Dvds.Sources;

public class DvdMediaSource : IMediaSource
{
    private readonly DvdTitleInfo _dvdTitleInfo;
    
    public DvdMediaSource(DvdTitleInfo dvdTitleInfo, MediaIdentifier identifier)
    {
        _dvdTitleInfo = dvdTitleInfo;
        Identifier = identifier;
        IgnoreFlags = MediaIgnoreFlags.None;
        Info = BuildMediaInfo();
    }
    
    #region Media info
    
    /// <inheritdoc />
    public MediaIdentifier Identifier { get; }
    
    /// <inheritdoc />
    public MediaInfo Info { get; }
    
    /// <inheritdoc />
    public MediaIgnoreFlags IgnoreFlags { get; }

    /// <summary>
    /// Gets the chapter infos.
    /// </summary>
    /// <returns></returns>
    private IEnumerable<ChapterInfo> GetChapterInfos()
    {
        var timespan = TimeSpan.Zero;
        return ChapterInfo.FromTimestamps(
            _dvdTitleInfo.Pgc.PlaybackTime.AsTimeSpan(),
            _dvdTitleInfo.Pgc.CellPlayback.Select(cell =>
            {
                timespan += cell.PlaybackTime.AsTimeSpan();
                return timespan;
            }));
    }
    
    /// <summary>
    /// Builds the media info from the DVD source.
    /// </summary>
    /// <returns></returns>
    private MediaInfo BuildMediaInfo()
    {
        var duration = _dvdTitleInfo.Pgc. PlaybackTime.AsTimeSpan();
        var vts = _dvdTitleInfo.TitleSet;
        var baseName = _dvdTitleInfo.Name;
        ushort streamId = 1;

        var videoStreams = new[]
        {
            new VideoInfo
            {
                Id = streamId++,
                Name = baseName,
                IsDefault = true
            }
        };
        
        var audioStreams = vts.VtsAudios.Select((a, n) => new AudioInfo()
        {
            Id = streamId++,
            Name = a.LangCode,
            LanguageCode = a.LangCode,
        }).ToArray();

        var subtitleStreams = vts.VtsSubPictures.Select((s, n) => new SubtitleInfo()
        {
            Id = streamId++,
            Name = s.LangCode,
            LanguageCode = s.LangCode
        }).ToArray();
        
        
        return new MediaInfo
        {
            Id = _dvdTitleInfo.Index,
            Name = baseName,
            Duration = _dvdTitleInfo.Pgc.PlaybackTime.AsTimeSpan(),
            Segments = _dvdTitleInfo.Ptts.Select(ptt => new SegmentInfo
            {
                Id = ptt.Pgn,
                Name = baseName,
                Duration = _dvdTitleInfo.Pgc.CellPlayback[ptt.Pgn - 1].PlaybackTime.AsTimeSpan(),
                VideoStreams = videoStreams,
                AudioStreams = audioStreams,
                SubtitleStreams = subtitleStreams,
            }).ToArray(),
            Chapters = GetChapterInfos().ToArray()
        };
    }
    
    #endregion Media info
    
    #region Output
    
    /// <inheritdoc />
    public OutputDefinition CreateDefaultOutputDefinition(CodecOptions codec, MediaFormat containerFormat)
    {
        var baseName = _dvdTitleInfo.Name;
        var duration = _dvdTitleInfo.Pgc.PlaybackTime.AsTimeSpan();
        var vts = _dvdTitleInfo.TitleSet;
        
        var streams = new List<OutputStream>
        {
            new()
            {
                Enabled = true,
                Id = 0x1E0,
                Type = OutputStreamType.Video
            }
        };

        ushort audioId = 0x80; // TODO: Filter by type
        foreach (var audio in vts.VtsAudios)
        {
            streams.Add(new OutputStream
            {
                Enabled = true,
                Id = audioId++,
                LanguageCode = audio.LangCode,
                Type = OutputStreamType.Audio
            });
        }

        ushort subPictureId = 0x20;
        foreach (var subPicture in vts.VtsSubPictures)
        {
            streams.Add(new OutputStream
            {
                Enabled = true,
                Id = subPictureId++,
                LanguageCode = subPicture.LangCode,
                Type = OutputStreamType.Subtitle,
                Format = "dvd_subtitle"
            });
        }
        
        var files = OutputHelper.GetFilesByStreams(baseName, streams, codec, containerFormat);
        
        return new OutputDefinition
        {
            Identifier = Identifier,
            MediaInfo = new OutputMediaInfo
            {
                Name = baseName,
            },
            Duration = duration,
            Codec = codec,
            Files = files.ToArray(),
            Chapters = OutputChapter.FromChapterInfos(GetChapterInfos()).ToArray()
        };
    }
    
    #endregion Output
}