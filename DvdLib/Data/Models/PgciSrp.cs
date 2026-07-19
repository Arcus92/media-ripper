using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// Program Chain Information Search Pointer
/// </summary>
public class PgciSrp
{ 
    public byte EntryId { get; private set; } = 0;
    public byte BlockMode { get; private set; } = 0;
    public byte BlockType { get; private set; } = 0;
    public ushort PtlIdMask { get; private set; } = 0;
    public uint PgcStartByte { get; private set; } = 0;
    public Pgc? Pgc { get; set; } = null;

    private void Read(BigEndianBinaryReader reader)
    {
        EntryId = reader.ReadByte();
        var b = reader.ReadBits8();
        BlockMode = b.ReadBits(2);
        BlockType = b.ReadBits(2);
        b.Skip(4);
        PtlIdMask = reader.ReadUInt16();
        PgcStartByte = reader.ReadUInt32();
    }
    
    public static PgciSrp FromReader(BigEndianBinaryReader reader)
    {
        var data = new PgciSrp();
        data.Read(reader);
        return data;
    }
    
    public static PgciSrp[] FromReader(BigEndianBinaryReader reader, int count)
    {
        var array = new PgciSrp[count];
        for (var i = 0; i < count; i++)
        {
            array[i] = FromReader(reader);
        }
        return array;
    }
}