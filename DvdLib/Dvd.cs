using System.Text.RegularExpressions;
using DvdLib.Data.Models;
using DvdLib.VolumeDescriptor;
using MediaLib.Utils;
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
    private Dictionary<ushort, DvdDomainInfo> TitleSetInfo { get; } = new();
    
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
    
    private uint _videoTsSectorOffset;

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
            var path = Path.Combine(DiskPath, "VIDEO_TS");
            
            var rootDirectoryEntry = ReadDirectoryFromVolume();
            var videoDirectoryEntry = rootDirectoryEntry.GetEntry("VIDEO_TS");
            if (videoDirectoryEntry is not null)
            {
                foreach (var entry in videoDirectoryEntry.Entries.Where(e => !e.IsDirectory))
                {
                    if (entry.IsDirectory || !entry.Filename.EndsWith(".IFO")) continue;

                    var filePath = Path.Combine(path, entry.Filename);
                    
                    var filename = Path.GetFileNameWithoutExtension(entry.Filename);
                    var titleSetIndex = GetTitleSetIndexByFilename(filename);
                    
                    var ifo = new Ifo();
                    ifo.Read(filePath);
                    
                    var domainInfo = new DvdDomainInfo(titleSetIndex, ifo, entry);
                    TitleSetInfo.Add(titleSetIndex, domainInfo);
                }
            }
            
            // Reading the VIDEO_TS.IFO file with all titles
            if (!TitleSetInfo.TryGetValue(0, out var info) || info.Ifo.TtSrpt is null)
            {
                return;
            }
            _videoTsSectorOffset = info.DirectoryEntry.DataLocation;

            for (ushort titleIndex = 0; titleIndex < info.Ifo.TtSrpt.Titles.Length; titleIndex++)
            {
                var title = info.Ifo.TtSrpt.Titles[titleIndex];
                // Reading title set .IFO file
                if (!TitleSetInfo.TryGetValue(title.TitleSetNr, out var titleSet) || 
                    titleSet.Ifo.Vts is null ||
                    titleSet.Ifo.VtsPttSrpt is null ||
                    titleSet.Ifo.VtsPgcit is null)
                {
                    continue;
                }

                var vtsTitle = titleSet.Ifo.VtsPttSrpt.Titles[title.VtsTtn - 1];
                var pgciSrp = titleSet.Ifo.VtsPgcit.PgciSrp[vtsTitle.Ptts[0].Pgcn - 1];
                var pgc = pgciSrp.Pgc!;

                var titleInfo = new DvdTitleInfo(titleIndex, title, titleSet, vtsTitle.Ptts, pgc, pgciSrp);
                TitleInfo.Add(titleIndex, titleInfo);
            }
            
            // Load the streams and build the content hash
            var fileInfos = new List<ContentHash.HashFileInfo>();
            foreach (var file in Directory.EnumerateFiles(path))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                
                // Collect infos for the content hash calculation
                var fileInfo = new FileInfo(file);
                fileInfos.Add(new ContentHash.HashFileInfo()
                {
                    Name = name,
                    CreationTime = fileInfo.CreationTime,
                    Size = fileInfo.Length,
                });
            }

            // Order the playlist files before calculating the hash.
            ContentHash = fileInfos.OrderBy(i => i.Name).CalculateHash();
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
    
    #endregion Info
    
    #region File system

    /// <summary>
    /// Reads the root directory from the volume descriptor.
    /// </summary>
    /// <returns>Returns the root dictionary entry with all sub-entries loaded.</returns>
    private DirectoryEntry ReadDirectoryFromVolume()
    {
        using var stream = GetRawDeviceStream();
        using var reader = new BigEndianBinaryReader(stream);
        
        // Start of Volume Descriptor
        reader.SeekTo(16 * BlockSize);
        
        var volumeDescriptor = reader.Read<VolumeDescriptorSet>();
        if (volumeDescriptor.Descriptor is PrimaryVolumeDescriptor primaryDescriptor)
        {
            var root = primaryDescriptor.RootDirectoryEntry;
            root.ReadEntries(reader, true);
            return root;
        }

        throw new IOException($"Expected PrimaryVolumeDescriptor. Found {volumeDescriptor.Type} instead.");
    }
    
    #endregion File system
    
    #region Streams
    
    /// <summary>
    /// Opens a stream for the given title and program.
    /// </summary>
    /// <param name="titleId">The title id.</param>
    /// <param name="programId">The program id.</param>
    /// <returns>Returns the stream.</returns>
    public Stream GetProgramStream(ushort titleId, ushort programId)
    {
        var title = TitleInfo[titleId];
        var cellId = title.Pgc.ProgramMap[programId - 1];
        var cell = title.Pgc.CellPlayback[cellId - 1];
        
        var titleSetSector = _videoTsSectorOffset + title.TitleInfo.TitleSetSector + title.TitleSetInfo.VtsTtVobs;
        var cellStartSector = titleSetSector + cell.FirstSector;
        var cellEndSector = titleSetSector + cell.LastSector;
        
        // Handle decryption
        if (VobDecryptionHandler is not null)
        {
            return VobDecryptionHandler.Invoke(this, titleSetSector, cellStartSector, cellEndSector);
        }

        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns the raw stream of the disk device.
    /// </summary>
    /// <returns>Returns the Stream.</returns>
    private Stream GetRawDeviceStream()
    {
        // TODO: Add support for Windows and macOS
        return new FileStream(DiskMountSource, FileMode.Open, FileAccess.Read, FileShare.Read);
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