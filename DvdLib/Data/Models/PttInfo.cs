using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// PartOfTitle Unit Information
/// </summary>
public class PttInfo : IBigEndianBinaryReadable
{
    public ushort Pgcn { get; private set; } = 0;
    public ushort Pgn { get; private set; } = 0;

    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
        Pgcn = reader.ReadUInt16();
        Pgn = reader.ReadUInt16();
    }
}