using System.Text.RegularExpressions;
using DvdLib.Data.Models;
using MediaLib.Utils.IO;

namespace DvdLib;

public partial class Dvd
{
    public const int BlockSize = 2048;
    
    /// <summary>
    /// Gets the DVD path.
    /// </summary>
    public string DiskPath { get; }

    /// <summary>
    /// Gets the mount source of the DVD. For example /dev/sr0.
    /// </summary>
    public string DiskMountSource { get; private set; }

    /// <summary>
    /// Gets the Dvd disk name.
    /// </summary>
    public string DiskName { get; }
    
    /// <summary>
    /// Gets all title set information by title set index.
    /// </summary>
    public Dictionary<ushort, DvdTitleSetInfo> TitleSetInfo { get; } = new();
    
    /// <summary>
    /// Gets all loaded video streams from VIDEO_TS.
    /// </summary>
    public Dictionary<ushort, DvdTitleInfo> TitleInfo { get; } = new();
    
    public Dvd(string diskPath)
    {
        diskPath = Path.GetFullPath(diskPath).TrimEnd('/', '\\'); // Sanitize
        DiskPath = diskPath;
        DiskMountSource = diskPath;
        DiskName = Path.GetFileName(diskPath);
    }
    
    #region Info
    
    /// <summary>
    /// Gets the content hash of the disc. This content hash is compatible with TheDiscDb.
    /// </summary>
    public string ContentHash { get; private set; } = "";

    /// <summary>
    /// Loads the DVD content and populates <see cref="TitleInfo"/>.
    /// </summary>
    public async Task LoadAsync()
    {
        TitleSetInfo.Clear();
        TitleInfo.Clear();
        DiskMountSource = await MountUtils.GetMountSourceAsync(DiskPath);
        await Task.Run(() =>
        {
            var videoStreamPath = Path.Combine(DiskPath, "VIDEO_TS");
            
            var ifoFiles = Directory.EnumerateFiles(videoStreamPath, "*.IFO");
            foreach (var ifoFile in ifoFiles)
            {
                var filename = Path.GetFileNameWithoutExtension(ifoFile);
                var titleSetIndex = GetTitleSetIndexByFilename(filename);
                
                var ifo = new Ifo();
                ifo.Read(ifoFile);

                var fileLengths = GetVobFileLengths(titleSetIndex).ToArray();
                
                TitleSetInfo.Add(titleSetIndex, new DvdTitleSetInfo(titleSetIndex, ifo, fileLengths));
            }
            
            // Reading the main info file with all titles
            if (!TitleSetInfo.TryGetValue(0, out var vmg) || vmg.Information.TtSrpt is null)
            {
                return;
            }

            for (ushort index = 0; index < vmg.Information.TtSrpt.Titles.Length; index++)
            {
                var title = vmg.Information.TtSrpt.Titles[index];
                if (!TitleSetInfo.TryGetValue(title.TitleSetNr, out var titleSet) || 
                    titleSet.Information.Vts is null ||
                    titleSet.Information.VtsPttSrpt is null ||
                    titleSet.Information.VtsPgcit is null)
                {
                    continue;
                }

                var vtsTitle = titleSet.Information.VtsPttSrpt.Titles[title.VtsTtn - 1];
                var pgc = titleSet.Information.VtsPgcit.PgciSrp[vtsTitle.Ptts[0].Pgcn - 1].Pgc!;

                var stream = new DvdTitleInfo(index, title, titleSet.Information.Vts, vtsTitle.Ptts, pgc);
                TitleInfo.Add(index, stream);
            }
        });
    }
    
    [GeneratedRegex(@"VTS_(\d\d)_(\d)")]
    private static partial Regex TitleSetFilenameRegex();
    
    private static byte GetTitleSetIndexByFilename(string filename)
    {
        if (filename == "VIDEO_TS")
        {
            return 0;
        }
        
        var match = TitleSetFilenameRegex().Match(filename);
        if (!match.Success)
            throw new ArgumentException($"Unknown filename: {filename}!");
        
        var titleSet = byte.Parse(match.Groups[1].Value);
        return titleSet;
    }

    private IEnumerable<long> GetVobFileLengths(byte titleSetIndex)
    {
        for (var i = 0; i <= 9; i++)
        {
            var filename = titleSetIndex == 0 ? 
                "VIDEO_TS.VOB" : 
                $"VTS_{titleSetIndex:00}_{i}.VOB";

            var path = Path.Combine(DiskPath, "VIDEO_TS", filename);
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists) break;
            yield return fileInfo.Length;

            if (titleSetIndex == 0) break;
        }
    }
    
    #endregion Info
    
    #region Streams
    
    /// <summary>
    /// Opens a stream for the given title and cell.
    /// </summary>
    /// <param name="titleId">The title id.</param>
    /// <param name="cellId">The cell id.</param>
    /// <returns>Returns the stream.</returns>
    public Stream GetCellStream(ushort titleId, ushort cellId)
    {
        var title = TitleInfo[titleId];
        var cell = title.Pgc.CellPlayback[cellId - 1];
        
        var titleSetSector = title.TitleInfo.TitleSetSector;
        var titleSetDataSector = titleSetSector + title.TitleSet.VtsTtVobs;
        var cellStartSector = titleSetDataSector + cell.FirstSector;
        var cellEndSector = titleSetDataSector + cell.LastSector;
        
        // Handle decryption
        if (VobDecryptionHandler is not null)
        {
            return VobDecryptionHandler.Invoke(this, titleSetDataSector, cellStartSector, cellEndSector);
        }

        throw new NotImplementedException();
    }
    
    #endregion Streams
    
    #region Decryption
    
    /// <summary>
    /// The decryption handler method.
    /// </summary>
    public delegate Stream DecryptionHandler(Dvd dvd, uint titleSetSector, uint cellSectorStart, uint cellSectorEnd);

    /// <summary>
    /// Gets and sets the VOB decryption stream.
    /// </summary>
    public static DecryptionHandler? VobDecryptionHandler { get; set; }

    #endregion Decryption
}