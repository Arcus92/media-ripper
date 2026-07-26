using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// Cell position.
/// </summary>
public struct CellPosition : IBigEndianBinaryReadable
{
    public CellPosition()
    {
    }
    
    public ushort VobIdNr { get; private set; } = 0;
    public byte CellNr { get; private set; } = 0;
    
    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
        VobIdNr = reader.ReadUInt16();
        reader.ReadZero();
        CellNr = reader.ReadByte();
    }
}