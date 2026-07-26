using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// Multi Channel Attributes
/// </summary>
public struct MultiChannelAttributes : IBigEndianBinaryReadable
{
    public MultiChannelAttributes()
    {
    }

    public bool Ach0Gme { get; set; } = false;
    public bool Ach1Gme { get; set; } = false;

    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
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
}