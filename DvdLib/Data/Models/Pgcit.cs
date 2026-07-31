using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
///     Program Chain Information Table
/// </summary>
public class Pgcit : IBigEndianBinaryReadable
{
    public PgciSrp[] PgciSrp { get; private set; } = [];

    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
        var start = reader.Position;

        var nrOfPgciSrpt = reader.ReadUInt16();
        reader.ReadZero(2);
        var lastByte = reader.ReadUInt32();

        PgciSrp = reader.Read<PgciSrp>(nrOfPgciSrpt);

        for (var i = 0; i < nrOfPgciSrpt; i++)
        {
            reader.SeekTo(start + PgciSrp[i].PgcStartByte);
            PgciSrp[i].Pgc = reader.Read<Pgc>();
        }
    }
}