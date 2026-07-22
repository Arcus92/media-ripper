using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// DVD Time
/// </summary>
public struct DvdTime
{
    /// <summary>
    /// Gets the hours BCD encoded.
    /// </summary>
    public byte Hour { get; set; }
    
    /// <summary>
    /// Gets the minutes BCD encoded.
    /// </summary>
    public byte Minute { get; set; }
    
    /// <summary>
    /// Gets the seconds BCD encoded.
    /// </summary>
    public byte Second { get; set; }
    
    /// <summary>
    /// Gets the frame number. The last two bytes define the frame rate.
    /// </summary>
    public byte Frame { get; set; }

    public TimeSpan AsTimeSpan()
    {
        // Time is BCD encoded
        return new TimeSpan(
            DecodeBcd(Hour), 
            DecodeBcd(Minute), 
            DecodeBcd(Second));
    }

    private void Read(BigEndianBinaryReader reader)
    {
        Hour = reader.ReadByte();
        Minute = reader.ReadByte();
        Second = reader.ReadByte();
        Frame = reader.ReadByte();
    }
    
    private static int DecodeBcd(byte b)
    {
        return ((b >> 4) & 0x0F) * 10 + (b & 0x0F);
    }
    
    public static DvdTime FromReader(BigEndianBinaryReader reader)
    {
        var time = new DvdTime();
        time.Read(reader);
        return time;
    }
}