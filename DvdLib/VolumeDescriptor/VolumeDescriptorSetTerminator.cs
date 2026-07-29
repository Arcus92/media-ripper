using MediaLib.Utils.IO;

namespace DvdLib.VolumeDescriptor;

public class VolumeDescriptorSetTerminator : IBigEndianBinaryReadable, IVolumeDescriptor
{
    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
    }
}