using MediaLib.Utils.IO;

namespace DvdLib.VolumeDescriptor;

public class PathTable : IBigEndianBinaryReadable
{
    public string Identifier { get; set; } = "";
    
    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
        var identifierLength = reader.ReadByte();
        var extendedAttributeRecordLength = reader.ReadByte();
        var directoryNumber = reader.ReadUInt16();
        Identifier = reader.ReadString(identifierLength).TrimEnd();

        // Padding
        if (reader.Position % 2 == 1)
        {
            reader.ReadZero();
        }
    }
}