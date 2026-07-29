using DvdLib;
using DvdLib.Data.Enums;
using MediaLib.Formats;
using MediaLib.Models;
using MediaLib.Output;
using MediaLib.Sources;
using MediaLib.Utils.Data;

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
        var baseName = _dvdTitleInfo.Name;
        var vts = _dvdTitleInfo.TitleSetInfo;
        
        
        // DVD uses fixed PIDs:
        // Video          0xE0
        // AC-3	          0x80-0x87
        // DTS	          0x88-0x8F
        // LPCM	          0xA0-0xA7
        // MPEG-1/2 audio 0xC0-0xC7
        // Subpicture     0x20-0x3F
        
        var streams = new List<StreamInfo>
        {
            new()
            {
                Id = 0x1E0,
                Type = StreamType.Video,
                Name = vts.VtsVideo.MpegVersion.ToString()
            }
        };

        ushort audioIdAc3 = 0x80;
        ushort audioIdDts = 0x88;
        ushort audioIdLpcm = 0xA0;
        ushort audioIdMpeg = 0xC0;
        foreach (var audio in vts.VtsAudios)
        {
            ushort audioId = audio.AudioFormat switch
            {
                AudioFormat.Ac3 => audioIdAc3++,
                AudioFormat.Dts => audioIdDts++,
                AudioFormat.Lpcm => audioIdLpcm++,
                AudioFormat.Mpeg1 or AudioFormat.Mpeg2Ext => audioIdMpeg++,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            streams.Add(new StreamInfo
            {
                Id = audioId,
                LanguageCode = MapLanguageCode(audio.LangCode),
                Type = StreamType.Audio
            });
        }

        ushort subPictureId = 0x20;
        foreach (var subPicture in vts.VtsSubPictures)
        {
            streams.Add(new StreamInfo
            {
                Id = subPictureId++,
                LanguageCode = MapLanguageCode(subPicture.LangCode),
                Type = StreamType.Subtitle,
                Format = "dvd_subtitle"
            });
        }
        
        return new MediaInfo
        {
            Identifier = Identifier,
            Name = baseName,
            Duration = _dvdTitleInfo.Pgc.PlaybackTime.AsTimeSpan(),
            Segments = _dvdTitleInfo.Ptts.Select(ptt =>
            {
                var cellId = _dvdTitleInfo.Pgc.ProgramMap[ptt.Pgn - 1];
                return new SegmentInfo
                {
                    Id = ptt.Pgn,
                    Name = baseName,
                    Duration = _dvdTitleInfo.Pgc.CellPlayback[cellId - 1].PlaybackTime.AsTimeSpan(),
                };
            }).ToArray(),
            Streams = streams.ToArray(),
            Chapters = GetChapterInfos().ToArray()
        };
    }

    private string MapLanguageCode(string languageCode)
    {
        // DVDs use two-digit (Set 1) language code, FFmpeg requires three-digit (Set2 T) codes.
        return Iso639.Set1ToSet2T.GetValueOrDefault(languageCode, languageCode);
    }
    
    #endregion Media info
    
    #region Output
    
    /// <inheritdoc />
    public OutputDefinition CreateDefaultOutputDefinition(CodecOptions codec, MediaFormat containerFormat)
    {
        var baseName = $"{Identifier.DiskName}_{_dvdTitleInfo.TitleIndex}";
        return OutputHelper.CreateDefaultOutputDefinition(baseName, Info, codec, containerFormat);
    }
    
    #endregion Output
}