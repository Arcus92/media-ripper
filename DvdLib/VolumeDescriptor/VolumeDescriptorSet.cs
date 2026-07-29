using MediaLib.Utils.IO;

namespace DvdLib.VolumeDescriptor;

/// <summary>
/// Volume file-system header (ISO 9660).
/// </summary>
public class VolumeDescriptorSet : IBigEndianBinaryReadable
{
    public VolumeDescriptorType Type { get; set; }
    public string Identifier { get; set; } = "";
    public byte Version { get; set; }
    public IVolumeDescriptor? Descriptor { get; set; }
    
    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
        Type = (VolumeDescriptorType)reader.ReadByte();
        Identifier = reader.ReadString(5);
        Version = reader.ReadByte();

        switch (Type)
        {
            case VolumeDescriptorType.BootRecord:
                Descriptor = reader.Read<BootRecord>();
                break;
            case VolumeDescriptorType.PrimaryVolumeDescriptor:
                Descriptor = reader.Read<PrimaryVolumeDescriptor>();
                break;
            case VolumeDescriptorType.VolumeDescriptorSetTerminator:
                Descriptor = reader.Read<VolumeDescriptorSetTerminator>();
                break;
            default:
                throw new NotImplementedException($"Unknown volume descriptor type: {Type}");
        }
    }
}