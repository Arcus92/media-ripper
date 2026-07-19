using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// PartOfTitle Search Pointer Table
/// </summary>
public class TtSrpt
{
    public TitleInfo[] Titles { get; private set; } = [];

    private void Read(BigEndianBinaryReader reader)
    {
        var nrOfSrpts = reader.ReadUInt16();
        reader.ReadZero(2);
        var lastByte = reader.ReadUInt32();
        Titles = TitleInfo.FromReader(reader, nrOfSrpts);
    }
    
    public static TtSrpt FromReader(BigEndianBinaryReader reader)
    {
        var data = new TtSrpt();
        data.Read(reader);
        return data;
    }
}