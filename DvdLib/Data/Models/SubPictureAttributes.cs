using DvdLib.Data.Enums;
using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

public struct SubPictureAttributes : IBigEndianBinaryReadable
{
    public SubPictureAttributes()
    {
    }

    public CodingMode CodingMode { get; set; } = default;
    public LanguageType Type { get; set; } = default;
    public string LangCode { get; set; } = "";
    public byte LangExtension { get; set; } = 0;
    public CodeExtension CodeExtension { get; set; } = default;

    /// <inheritdoc />
    void IBigEndianBinaryReadable.Read(BigEndianBinaryReader reader)
    {
        var b = reader.ReadBits8();
        CodingMode = (CodingMode)b.ReadBits(3);
        b.Skip(3);
        Type = (LanguageType)b.ReadBits(2);

        reader.ReadZero();
        LangCode = reader.ReadString(2);
        LangExtension = reader.ReadByte();
        CodeExtension = (CodeExtension)reader.ReadByte();
    }
}