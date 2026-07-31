using System.Buffers.Binary;
using MediaLib.Utils.IO;

namespace DvdLib.VolumeDescriptor;

public class PrimaryVolumeDescriptor : IVolumeDescriptor, IBigEndianBinaryReadable
{
    // https://wiki.osdev.org/ISO_9660

    public string SystemIdentifier { get; set; } = "";
    public string VolumeIdentifier { get; set; } = "";
    public uint VolumeSpaceSize { get; set; }
    public ushort VolumeSetSize { get; set; }
    public ushort VolumeSequenceNumber { get; set; }
    public ushort LogicalBlockSize { get; set; }
    public uint PathTableSize { get; set; }
    public int PathTableLLocation { get; set; }
    public int PathTableLOptionalLocation { get; set; }
    public int PathTableMLocation { get; set; }
    public int PathTableMOptionalLocation { get; set; }
    public DirectoryEntry RootDirectoryEntry { get; set; } = new();
    public string VolumeSetIdentifier { get; set; } = "";
    public string PublisherIdentifier { get; set; } = "";

    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
        reader.ReadZero();
        SystemIdentifier = reader.ReadString(32).TrimEnd();
        VolumeIdentifier = reader.ReadString(32).TrimEnd();
        reader.ReadZero(8);
        reader.Skip(4);
        VolumeSpaceSize = reader.ReadUInt32();
        reader.ReadZero(32);
        reader.Skip(2);
        VolumeSetSize = reader.ReadUInt16();
        reader.Skip(2);
        VolumeSequenceNumber = reader.ReadUInt16();
        reader.Skip(2);
        LogicalBlockSize = reader.ReadUInt16();
        reader.Skip(4);
        PathTableSize = reader.ReadUInt32();
        PathTableLLocation = BinaryPrimitives.ReverseEndianness(reader.ReadInt32());
        PathTableLOptionalLocation = BinaryPrimitives.ReverseEndianness(reader.ReadInt32());
        PathTableMLocation = reader.ReadInt32();
        PathTableMOptionalLocation = reader.ReadInt32();
        RootDirectoryEntry = reader.Read<DirectoryEntry>();
        VolumeSetIdentifier = reader.ReadString(128).TrimEnd();
        PublisherIdentifier = reader.ReadString(128).TrimEnd();
        var x = 0;
    }
}