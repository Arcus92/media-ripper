using MediaLib.Utils.IO;

namespace DvdLib.VolumeDescriptor;

public class DirectoryEntry : IBigEndianBinaryReadable
{
    public byte ExtendedAttributeRecordLength { get; set; }
    public uint DataLocation { get; set; }
    public uint DataLength { get; set; }
    public VolumeDateTime DateTime { get; set; }
    public FileFlags FileFlags { get; set; }
    public byte FileUnitSize { get; set; }
    public byte InterleaveGapSize { get; set; }
    public ushort VolumeSequenceNumber { get; set; }
    public string Identifier { get; set; } = "";
    public byte[] SystemUse { get; set; } = [];


    public string Filename { get; private set; } = "";

    public bool IsDirectory => FileFlags.HasFlag(FileFlags.Directory);
    public List<DirectoryEntry> Entries { get; private set; } = [];

    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
        if (!TryRead(reader)) throw new IOException("Could not read directory entry!");
    }

    private bool TryRead(BigEndianBinaryReader reader)
    {
        var start = reader.Position;
        var length = reader.ReadByte();
        if (length == 0) return false;

        ExtendedAttributeRecordLength = reader.ReadByte();
        reader.Skip(4);
        DataLocation = reader.ReadUInt32();
        reader.Skip(4);
        DataLength = reader.ReadUInt32();
        DateTime = reader.Read<VolumeDateTime>();
        FileFlags = (FileFlags)reader.ReadByte();
        FileUnitSize = reader.ReadByte();
        InterleaveGapSize = reader.ReadByte();
        reader.Skip(2);
        VolumeSequenceNumber = reader.ReadUInt16();
        var identifierLength = reader.ReadByte();
        Identifier = reader.ReadString(identifierLength);

        // Padding
        if (reader.Position % 2 == 1) reader.ReadZero();

        SystemUse = reader.ReadBytes((int)(start + length - reader.Position));

        if (Identifier == "\x00")
        {
            Filename = ".";
        }
        else if (Identifier == "\x01")
        {
            Filename = "..";
        }
        else
        {
            var index = Identifier.IndexOf(';');
            if (index >= 0)
                Filename = Identifier.Substring(0, index);
            else
                Filename = Identifier;
        }

        return true;
    }

    /// <summary>
    ///     Reads the sub-entries in this directory.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="recursively"></param>
    /// <returns>Returns the list of sub-entries.</returns>
    public void ReadEntries(BigEndianBinaryReader reader, bool recursively = false)
    {
        var start = DataLocation * Dvd.BlockSize;
        var end = start + DataLength;

        Entries = [];

        reader.SeekTo(start);
        while (reader.Position < end)
        {
            var entry = new DirectoryEntry();
            if (!entry.TryRead(reader)) break;

            // Ignore navigation directories
            if (entry is { IsDirectory: true, Filename: "." or ".." }) continue;

            Entries.Add(entry);
        }

        if (!recursively) return;

        foreach (var entry in Entries)
        {
            if (!entry.IsDirectory) continue;

            entry.ReadEntries(reader, true);
        }
    }

    /// <summary>
    ///     Returns the directory entry with the given name.
    /// </summary>
    /// <param name="fileName">The filename to search for.</param>
    /// <returns>Returns the entry.</returns>
    public DirectoryEntry? GetEntry(string fileName)
    {
        return Entries.FirstOrDefault(x => x.Filename == fileName);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Filename;
    }
}