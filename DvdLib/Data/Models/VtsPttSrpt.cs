using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// PartOfTitle Search Pointer Table
/// </summary>
public class VtsPttSrpt
{
    public Ttu[] Titles { get; private set; } = [];

    private void Read(BigEndianBinaryReader reader)
    {
        var nrOfSrpts = reader.ReadUInt16();
        reader.ReadZero(2);
        var lastByte = reader.ReadUInt32();

        // Read the ttu offset
        var ttuOffset = new uint[nrOfSrpts];
        for (var i = 0; i < nrOfSrpts; i++)
        {
            ttuOffset[i] = reader.ReadUInt32();
        }
        
        Titles = new Ttu[nrOfSrpts];
        for (var i = 0; i < nrOfSrpts; i++)
        {
            uint size;
            if (i < nrOfSrpts - 1)
            {
                size = ttuOffset[i + 1] -  ttuOffset[i];
            }
            else
            {
                size = lastByte + 1 - ttuOffset[i];
            }
            var n = size / 4;
            var ptt = PttInfo.FromReader(reader, (int)n);
            Titles[i] = new Ttu
            {
                Ptts = ptt
            };
        }
    }
    
    public static VtsPttSrpt FromReader(BigEndianBinaryReader reader)
    {
        var data = new VtsPttSrpt();
        data.Read(reader);
        return data;
    }
}