using DvdLib.Data.Models;
using DvdLib.VolumeDescriptor;

namespace DvdLib;

/// <summary>
/// Information for a domain. If <see cref="TitleSetIndex"/> is 0, it is the VMG, otherwise a VTS.
/// </summary>
public class DvdDomainInfo
{
    /// <summary>
    /// Gets the title set index. An index of 0 is the VIDEO_TS file.
    /// </summary>
    public byte TitleSetIndex { get; }
    
    /// <summary>
    /// Gets the IFO file.
    /// </summary>
    public Ifo Ifo { get; }
    
    /// <summary>
    /// Gets the directory entry of the IFO file.
    /// </summary>
    public DirectoryEntry DirectoryEntry { get; }
    
    public DvdDomainInfo(byte titleSetIndex, Ifo ifo, DirectoryEntry directoryEntry)
    {
        TitleSetIndex = titleSetIndex;
        Ifo = ifo;
        DirectoryEntry = directoryEntry;
    }
}