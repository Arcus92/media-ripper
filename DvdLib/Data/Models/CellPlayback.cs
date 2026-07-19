using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

public class CellPlayback
{
    public byte BlockMode { get; private set; } = 0;
    public byte BlockType { get; private set; } = 0;
    
    public byte StillTime { get; private set; } = 0;
    public byte CellCmdNr { get; private set; } = 0;
    public DvdTime PlaybackTime { get; private set; } = default;
    public uint FirstSector { get; private set; } = 0;
    public uint FirstIlvuEndSector { get; private set; } = 0;
    public uint LastVobuStartSector { get; private set; } = 0;
    public uint LastSector { get; private set; } = 0;

    private void Read(BigEndianBinaryReader reader)
    {
        var b = reader.ReadBits16();
        BlockMode = (byte)b.ReadBits(2);
        BlockType = (byte)b.ReadBits(2);
        
        // TODO
        StillTime = reader.ReadByte();
        CellCmdNr = reader.ReadByte();
        PlaybackTime = DvdTime.FromReader(reader);
        FirstSector = reader.ReadUInt32();
        FirstIlvuEndSector = reader.ReadUInt32();
        LastVobuStartSector = reader.ReadUInt32();
        LastSector = reader.ReadUInt32();
    }

    public static CellPlayback FromReader(BigEndianBinaryReader reader)
    {
        var data = new CellPlayback();
        data.Read(reader);
        return data;
    }
    
    public static CellPlayback[] FromReader(BigEndianBinaryReader reader, int count)
    {
        var array = new CellPlayback[count];
        for (var i = 0; i < count; i++)
        {
            array[i] = FromReader(reader);
        }
        return array;
    }
}