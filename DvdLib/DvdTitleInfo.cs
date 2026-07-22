using DvdLib.Data.Models;

namespace DvdLib;

public class DvdTitleInfo
{
    /// <summary>
    /// Gets the title index.
    /// </summary>
    public ushort Index { get; }
    
    /// <summary>
    /// The title set.
    /// </summary>
    public VtsiMat TitleSet { get; }
    
    /// <summary>
    /// Gets the DVD internal title info.
    /// </summary>
    public TitleInfo TitleInfo { get; }

    /// <summary>
    /// The program chain info.
    /// </summary>
    public Pgc Pgc { get; }
    
    /// <summary>
    /// The title parts.
    /// </summary>
    public PttInfo[] Ptts { get; }

    /// <summary>
    /// Gets the title set index.
    /// </summary>
    public byte TitleSetIndex => TitleInfo.TitleSetNr;
    
    /// <summary>
    /// Gets the name of this title.
    /// </summary>
    public string Name => $"Title {Index:00}";

    public DvdTitleInfo(ushort index, TitleInfo titleInfo, VtsiMat titleSet, PttInfo[] ptts, Pgc pgc)
    {
        Index = index;
        TitleInfo = titleInfo;
        TitleSet = titleSet;
        Pgc = pgc;
        Ptts = ptts;
    }
}