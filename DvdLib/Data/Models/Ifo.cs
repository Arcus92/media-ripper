using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
///     DVD Information
/// </summary>
public class Ifo : IBigEndianBinaryReadable
{
    // https://dvds.beandog.org/doku.php?id=libdvdread
    // https://code.videolan.org/videolan/libdvdread/-/blob/master/src/ifo_read.c?ref_type=heads
    // https://code.videolan.org/videolan/libdvdread/-/blob/master/src/dvdread/ifo_types.h?ref_type=heads

    public VtsiMat? Vts { get; private set; }
    public TtSrpt? TtSrpt { get; private set; }
    public VmgiMat? Vmg { get; private set; }
    public VtsPttSrpt? VtsPttSrpt { get; private set; }
    public Pgcit? VtsPgcit { get; private set; }
    public CAdtT? CAdtT { get; private set; }

    void IBigEndianBinaryReadable.Read(BigEndianBinaryReader reader)
    {
        var header = reader.ReadString(12);
        switch (header)
        {
            case "DVDVIDEO-VMG":
                ReadVmg(reader);
                break;
            case "DVDVIDEO-VTS":
                ReadVts(reader);
                break;
            default:
                throw new InvalidDataException("Invalid IFO magic number!");
        }
    }

    /// <summary>
    ///     Reads the IFO file.
    /// </summary>
    /// <param name="path">The path to the IFO file.</param>
    public void Read(string path)
    {
        using var fileStream = File.OpenRead(path);
        Read(fileStream);
    }

    /// <summary>
    ///     Reads the IFO file from stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public void Read(Stream stream)
    {
        var reader = new BigEndianBinaryReader(stream);
        ((IBigEndianBinaryReadable)this).Read(reader);
    }

    private void ReadVmg(BigEndianBinaryReader reader)
    {
        var vmg = reader.Read<VmgiMat>();
        if (vmg.TtSrpt > 0)
        {
            reader.SkipTo(vmg.TtSrpt * Dvd.BlockSize);
            TtSrpt = reader.Read<TtSrpt>();
        }

        Vmg = vmg;
    }

    private void ReadVts(BigEndianBinaryReader reader)
    {
        var vts = reader.Read<VtsiMat>();
        if (vts.VtsPttSrpt > 0)
        {
            reader.SeekTo(vts.VtsPttSrpt * Dvd.BlockSize);
            VtsPttSrpt = reader.Read<VtsPttSrpt>();
        }

        if (vts.VtsPgcit > 0)
        {
            reader.SeekTo(vts.VtsPgcit * Dvd.BlockSize);
            VtsPgcit = reader.Read<Pgcit>();
        }

        if (vts.VtsCAdt > 0)
        {
            reader.SeekTo(vts.VtsCAdt * Dvd.BlockSize);
            CAdtT = reader.Read<CAdtT>();
        }

        Vts = vts;
    }
}