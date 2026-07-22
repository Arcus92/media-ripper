using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// DVD Information
/// </summary>
public class Ifo
{
    // https://dvds.beandog.org/doku.php?id=libdvdread
    // https://code.videolan.org/videolan/libdvdread/-/blob/master/src/ifo_read.c?ref_type=heads
    // https://code.videolan.org/videolan/libdvdread/-/blob/master/src/dvdread/ifo_types.h?ref_type=heads
    
    public VtsiMat? Vts { get; private set; }
    public TtSrpt? TtSrpt { get; private set; }
    public VmgiMat? Vmg { get; private set; }
    public VtsPttSrpt? VtsPttSrpt { get; private set; }
    public Pgcit? VtsPgcit { get; private set; }

    /// <summary>
    /// Reads the IFO file.
    /// </summary>
    /// <param name="path">The path to the IFO file.</param>
    public void Read(string path)
    {
        using var fileStream = File.OpenRead(path);
        Read(fileStream);
    }
    
    /// <summary>
    /// Reads the IFO file from stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public void Read(Stream stream)
    {
        var reader = new BigEndianBinaryReader(stream);
        Read(reader);
    }

    private void Read(BigEndianBinaryReader reader)
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

    private void ReadVmg(BigEndianBinaryReader reader)
    {
        var vmg = VmgiMat.FromReader(reader);
        if (vmg.TtSrpt > 0)
        {
            reader.SkipTo(vmg.TtSrpt * Dvd.BlockSize);
            TtSrpt = TtSrpt.FromReader(reader);
        }

        Vmg = vmg;
    }
    
    private void ReadVts(BigEndianBinaryReader reader)
    {
        var vts = VtsiMat.FromReader(reader);
        if (vts.VtsPttSrpt > 0)
        {
            reader.SeekTo(vts.VtsPttSrpt * Dvd.BlockSize);
            VtsPttSrpt = VtsPttSrpt.FromReader(reader);
        }
        if (vts.VtsPgcit > 0)
        {
            reader.SeekTo(vts.VtsPgcit * Dvd.BlockSize);
            VtsPgcit = Pgcit.FromReader(reader);
        }

        Vts = vts;
    }
}