using DvdLib.Data.Enums;
using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// Video Attributes
/// </summary>
public struct VideoAttributes
{
    public VideoAttributes()
    {
    }

    public MpegVersion MpegVersion { get; set; } = default;
    public VideoFormat VideoFormat { get; set; } = default;
    public DisplayAspectRatio DisplayAspectRatio { get; set; } = default;
    public byte PermittedDf { get; set; } = 0;

    public bool Line21Cc1 { get; set; } = false;
    public bool Line21Cc2 { get; set; } = false;
    public bool BitRate { get; set; } = false;
    
    public PictureSize PictureSize { get; set; } = default;
    public bool LetterBoxed { get; set; } = false;
    public FilmMode FilmMode { get; set; } = default;

    private void Read(BigEndianBinaryReader reader)
    {
        var b = reader.ReadBits8();
        MpegVersion = (MpegVersion)b.ReadBits(2);
        VideoFormat = (VideoFormat)b.ReadBits(2);
        DisplayAspectRatio = (DisplayAspectRatio)b.ReadBits(2);
        PermittedDf = b.ReadBits(2);
        
        b = reader.ReadBits8();
        Line21Cc1 = b.ReadBit();
        Line21Cc2 = b.ReadBit();
        b.Skip(1);
        BitRate = b.ReadBit();
        
        PictureSize =  (PictureSize)b.ReadBits(2);
        LetterBoxed = b.ReadBit();
        FilmMode = (FilmMode)b.ReadBits(1);
    }
    
    public static VideoAttributes FromReader(BigEndianBinaryReader reader)
    {
        var attr = new VideoAttributes();
        attr.Read(reader);
        return attr;
    }
    
    public static VideoAttributes[] FromReader(BigEndianBinaryReader reader, int count)
    {
        var array = new VideoAttributes[count];
        for (var i = 0; i < count; i++)
        {
            array[i] = FromReader(reader);
        }
        return array;
    }
}