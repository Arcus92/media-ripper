using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// Multi Channel Attributes
/// </summary>
public struct MultiChannelAttributes
{
    public MultiChannelAttributes()
    {
    }

    public bool Ach0Gme { get; set; } = false;
    public bool Ach1Gme { get; set; } = false;

    private void Read(BigEndianBinaryReader reader)
    {
        var b = reader.ReadBits8();
        b.Skip(7);
        Ach0Gme = b.ReadBit();
        
        b = reader.ReadBits8();
        b.Skip(7);
        Ach1Gme = b.ReadBit();
        
        b = reader.ReadBits8();
        
        b = reader.ReadBits8();
        
        b = reader.ReadBits8();
        
        reader.Skip(19);
    }
    
    public static MultiChannelAttributes FromReader(BigEndianBinaryReader reader)
    {
        var attr = new MultiChannelAttributes();
        attr.Read(reader);
        return attr;
    }
    
    public static MultiChannelAttributes[] FromReader(BigEndianBinaryReader reader, int count)
    {
        var array = new MultiChannelAttributes[count];
        for (var i = 0; i < count; i++)
        {
            array[i] = FromReader(reader);
        }
        return array;
    }
}