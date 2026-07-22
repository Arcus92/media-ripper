using DvdLib.Data.Models;

namespace DvdLib;

public class DvdTitleSetInfo
{
    /// <summary>
    /// Gets the title set index. An index of 0 is the VIDEO_TS file.
    /// </summary>
    public ushort TitleSetIndex { get; }
    
    /// <summary>
    /// Gets the IFO file.
    /// </summary>
    public Ifo Information { get; }
    
    /// <summary>
    /// Gets the lengths of the VOB files.
    /// </summary>
    public long[] FileLengths { get; }
    
    public DvdTitleSetInfo(ushort titleSetIndex, Ifo information, long[] fileLengths)
    {
        TitleSetIndex = titleSetIndex;
        Information = information;
        FileLengths = fileLengths;
    }
}