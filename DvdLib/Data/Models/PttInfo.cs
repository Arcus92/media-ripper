using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// PartOfTitle Unit Information
/// </summary>
public class PttInfo
{

    public ushort Pgcn { get; private set; } = 0;
    public ushort Pgn { get; private set; } = 0;


    private void Read(BigEndianBinaryReader reader)
    {
        Pgcn = reader.ReadUInt16();
        Pgn = reader.ReadUInt16();
    }
    
    public static PttInfo FromReader(BigEndianBinaryReader reader)
    {
        var data = new PttInfo();
        data.Read(reader);
        return data;
    }
    
    public static PttInfo[] FromReader(BigEndianBinaryReader reader, int count)
    {
        var array = new PttInfo[count];
        for (var i = 0; i < count; i++)
        {
            array[i] = FromReader(reader);
        }
        return array;
    }
}