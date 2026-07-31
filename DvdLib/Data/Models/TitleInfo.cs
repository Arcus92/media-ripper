using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
///     Title Information
/// </summary>
public class TitleInfo : IBigEndianBinaryReadable
{
    public PlaybackType PlaybackType { get; private set; }
    public byte NrOfAngles { get; private set; }
    public ushort NrOfPtts { get; private set; }
    public ushort ParentalId { get; private set; }
    public byte TitleSetNr { get; private set; }
    public byte VtsTtn { get; private set; }
    public uint TitleSetSector { get; private set; }

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