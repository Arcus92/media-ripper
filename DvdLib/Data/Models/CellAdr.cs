using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// Cell address.
/// </summary>
public struct CellAdr : IBigEndianBinaryReadable
{
    public CellAdr()
    {
    }

    public ushort VobId { get; private set; } = 0;
    public byte CellId { get; private set; } = 0;
    public uint FirstSector { get; private set; } = 0;
    public uint LastSector { get; private set; } = 0;
    
    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
        VobId = reader.ReadUInt16();
        CellId = reader.ReadByte();
        reader.ReadZero();
        FirstSector = reader.ReadUInt32();
        LastSector = reader.ReadUInt32();
    }
}