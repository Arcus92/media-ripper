using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// Cell address table.
/// </summary>
public class CAdtT : IBigEndianBinaryReadable
{
    public CellAdr[] CellAdrs { get; private set; } = [];
    
    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
        var nrOfVobs = reader.ReadUInt16();
        reader.ReadZero(2);
        var lastByte = reader.ReadUInt32();

        CellAdrs = reader.Read<CellAdr>(nrOfVobs);
    }
}