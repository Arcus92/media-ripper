using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// PartOfTitle Search Pointer Table
/// </summary>
public class VtsPttSrpt : IBigEndianBinaryReadable
{
    public Ttu[] Titles { get; private set; } = [];

    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
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
            var ptt = reader.Read<PttInfo>((int)n);
            Titles[i] = new Ttu
            {
                Ptts = ptt
            };
        }
    }
}