using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// Title Information
/// </summary>
public class TitleInfo : IBigEndianBinaryReadable
{
    public PlaybackType PlaybackType { get; private set; } = default;
    public byte NrOfAngles { get; private set; } = 0;
    public ushort NrOfPtts { get; private set; } = 0;
    public ushort ParentalId { get; private set; } = 0;
    public byte TitleSetNr { get; private set; } = 0;
    public byte VtsTtn { get; private set; } = 0;
    public uint TitleSetSector { get; private set; } = 0;
    
    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
        PlaybackType = reader.Read<PlaybackType>();
        NrOfAngles = reader.ReadByte();
        NrOfPtts = reader.ReadUInt16();
        ParentalId = reader.ReadUInt16();
        TitleSetNr = reader.ReadByte();
        VtsTtn = reader.ReadByte();
        TitleSetSector = reader.ReadUInt32();
    }
}