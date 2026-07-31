using DvdLib.Data.Models;

namespace DvdLib;

public class DvdTitleInfo
{
    public DvdTitleInfo(ushort titleIndex, TitleInfo titleInfo, DvdDomainInfo domainInfo, PttInfo[] ptts, Pgc pgc,
        PgciSrp pgciSrp)
    {
        TitleIndex = titleIndex;
        TitleInfo = titleInfo;
        DomainInfo = domainInfo;
        TitleSetInfo = DomainInfo.Ifo.Vts!;
        Pgc = pgc;
        PgciSrp = pgciSrp;
        Ptts = ptts;
    }

    /// <summary>
    ///     Gets the title index.
    /// </summary>
    public ushort TitleIndex { get; }

    /// <summary>
    ///     Gets the internal title info.
    /// </summary>
    public TitleInfo TitleInfo { get; }

    /// <summary>
    ///     The domain information of the title.
    /// </summary>
    public DvdDomainInfo DomainInfo { get; }

    /// <summary>
    ///     The title set information of the title.
    /// </summary>
    public VtsiMat TitleSetInfo { get; }

    /// <summary>
    ///     Gets the title set index.
    /// </summary>
    public byte TitleSetIndex => DomainInfo.TitleSetIndex;

    /// <summary>
    ///     The program chain info.
    /// </summary>
    public Pgc Pgc { get; }

    /// <summary>
    ///     The program chain search pointer.
    /// </summary>
    public PgciSrp PgciSrp { get; }

    /// <summary>
    ///     The title parts.
    /// </summary>
    public PttInfo[] Ptts { get; }

    /// <summary>
    ///     Gets the name of this title.
    /// </summary>
    public string Name => $"Title {TitleIndex:00}";
}