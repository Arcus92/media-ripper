using MediaLib.Utils.IO;

namespace DvdLib.VolumeDescriptor;

public class BootRecord : IBigEndianBinaryReadable, IVolumeDescriptor
{
    public string SystemIdentifier { get; set; } = "";
    public string BootIdentifier { get; set; } = "";
    public byte[] SystemUse { get; set; } = [];
    
    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
        SystemIdentifier = reader.ReadString(32);
        BootIdentifier = reader.ReadString(32);
        SystemUse = reader.ReadBytes(1977);
    }
}