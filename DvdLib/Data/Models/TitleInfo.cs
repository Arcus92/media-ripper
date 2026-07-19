using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// Title Information
/// </summary>
public class TitleInfo
{
    public PlaybackType PlaybackType { get; private set; } = default;
    public byte NrOfAngles { get; private set; } = 0;
    public ushort NrOfPtts { get; private set; } = 0;
    public ushort ParentalId { get; private set; } = 0;
    public byte TitleSetNr { get; private set; } = 0;
    public byte VtsTtn { get; private set; } = 0;
    public uint TitleSetSector { get; private set; } = 0;
    
    private void Read(BigEndianBinaryReader reader)
    {
        PlaybackType = PlaybackType.FromReader(reader);
        NrOfAngles = reader.ReadByte();
        NrOfPtts = reader.ReadUInt16();
        ParentalId = reader.ReadUInt16();
        TitleSetNr = reader.ReadByte();
        VtsTtn = reader.ReadByte();
        TitleSetSector = reader.ReadUInt32();
    }
    
    public static TitleInfo FromReader(BigEndianBinaryReader reader)
    {
        var data = new TitleInfo();
        data.Read(reader);
        return data;
    }
    
    public static TitleInfo[] FromReader(BigEndianBinaryReader reader, int count)
    {
        var array = new TitleInfo[count];
        for (var i = 0; i < count; i++)
        {
            array[i] = FromReader(reader);
        }
        return array;
    }
}