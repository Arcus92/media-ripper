using MediaLib.Utils.IO;

namespace DvdLib.VolumeDescriptor;

public struct VolumeDateTime : IBigEndianBinaryReadable
{
    public byte Year { get; set; }
    public byte Month { get; set; }
    public byte Day { get; set; }
    public byte Hour { get; set; }
    public byte Minute { get; set; }
    public byte Second { get; set; }
    public sbyte Offset { get; set; }

    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
        Year = reader.ReadByte();
        Month = reader.ReadByte();
        Day = reader.ReadByte();
        Hour = reader.ReadByte();
        Minute = reader.ReadByte();
        Second = reader.ReadByte();
        Offset = reader.ReadSByte();
    }

    public DateTimeOffset AsDateTime()
    {
        return new DateTimeOffset(1900 + Year, Month, Day, Hour, Minute, Second, 0,
            TimeSpan.FromMinutes(Offset * 15));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return AsDateTime().ToString();
    }
}