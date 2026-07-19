using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// Playback Type
/// </summary>
public struct PlaybackType
{
    public PlaybackType()
    {
    }

    public bool MultiOrRandomPgcTitle { get; private set; } = false;
    public bool JlcExistsInCellCmd { get; private set; } = false;
    public bool JlcExistsInPrepostCmd { get; private set; } = false;
    public bool JlcExistsInButtonCmd { get; private set; } = false;
    public bool JlcExistsInTtDom { get; private set; } = false;
    public bool ChapterSearchOrPlay { get; private set; } = false;
    public bool TitleOrTimePlay { get; private set; } = false;

    private void Read(BigEndianBinaryReader reader)
    {
        var b = reader.ReadBits8();
        b.Skip(1);
        MultiOrRandomPgcTitle = b.ReadBit();
        JlcExistsInCellCmd = b.ReadBit();
        JlcExistsInPrepostCmd = b.ReadBit();
        JlcExistsInButtonCmd = b.ReadBit();
        JlcExistsInTtDom = b.ReadBit();
        ChapterSearchOrPlay = b.ReadBit();
        TitleOrTimePlay = b.ReadBit();
    }
    
    public static PlaybackType FromReader(BigEndianBinaryReader reader)
    {
        var data = new PlaybackType();
        data.Read(reader);
        return data;
    }
}