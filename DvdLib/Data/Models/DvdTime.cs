using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// DVD Time
/// </summary>
public struct DvdTime
{
    public byte Hour { get; set; }
    public byte Minute { get; set; }
    public byte Second { get; set; }
    public byte Frame { get; set; }

    private void Read(BigEndianBinaryReader reader)
    {
        Hour = reader.ReadByte();
        Minute = reader.ReadByte();
        Second = reader.ReadByte();
        Frame = reader.ReadByte();
    }
    
    public static DvdTime FromReader(BigEndianBinaryReader reader)
    {
        var time = new DvdTime();
        time.Read(reader);
        return time;
    }
}