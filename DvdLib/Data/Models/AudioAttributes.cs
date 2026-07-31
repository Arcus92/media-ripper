using DvdLib.Data.Enums;
using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
///     Audio Attributes
/// </summary>
public struct AudioAttributes : IBigEndianBinaryReadable
{
    public AudioAttributes()
    {
    }

    public AudioFormat AudioFormat { get; set; } = default;
    public bool MultichannelExtension { get; set; } = false;
    public byte LandType { get; set; } = 0;
    public ApplicationMode ApplicationMode { get; set; } = default;

    public byte Quantization { get; set; } = 0;
    public byte SampleFrequency { get; set; } = 0;
    public byte Channels { get; set; } = 0;


    public string LangCode { get; set; } = "";
    public byte LangExtension { get; set; } = 0;
    public AudioCodeExtension CodeExtension { get; set; } = default;

    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
        var b = reader.ReadBits8();
        AudioFormat = (AudioFormat)b.ReadBits(3);
        MultichannelExtension = b.ReadBit();
        LandType = b.ReadBits(2);
        ApplicationMode = (ApplicationMode)b.ReadBits(2);

        b = reader.ReadBits8();
        Quantization = b.ReadBits(2);
        SampleFrequency = b.ReadBits(2);
        b.Skip(1);
        Channels = b.ReadBits(3);

        LangCode = reader.ReadString(2);
        LangExtension = reader.ReadByte();
        CodeExtension = (AudioCodeExtension)reader.ReadByte();
        reader.Skip(1);

        b = reader.ReadBits8();
    }
}