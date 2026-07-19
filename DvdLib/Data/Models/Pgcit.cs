using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// Program Chain Information Table
/// </summary>
public class Pgcit
{
    public PgciSrp[] PgciSrp { get; private set; } = [];

    private void Read(BigEndianBinaryReader reader)
    {
        var start = reader.Position;
        
        var nrOfPgciSrpt = reader.ReadUInt16();
        reader.ReadZero(2);
        var lastByte =  reader.ReadUInt32();

        PgciSrp = Models.PgciSrp.FromReader(reader, nrOfPgciSrpt);

        for (var i = 0; i < nrOfPgciSrpt; i++)
        {
            reader.SeekTo(start + PgciSrp[i].PgcStartByte);
            PgciSrp[i].Pgc = Pgc.FromReader(reader);
        }
    }
    
    public static Pgcit FromReader(BigEndianBinaryReader reader)
    {
        var data = new Pgcit();
        data.Read(reader);
        return data;
    }
    
    public static Pgcit[] FromReader(BigEndianBinaryReader reader, int count)
    {
        var array = new Pgcit[count];
        for (var i = 0; i < count; i++)
        {
            array[i] = FromReader(reader);
        }
        return array;
    }
}