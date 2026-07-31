using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
///     PartOfTitle Search Pointer Table
/// </summary>
public class TtSrpt : IBigEndianBinaryReadable
{
    public TitleInfo[] Titles { get; private set; } = [];

    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
        var nrOfSrpts = reader.ReadUInt16();
        reader.ReadZero(2);
        var lastByte = reader.ReadUInt32();
        Titles = reader.Read<TitleInfo>(nrOfSrpts);
    }
}