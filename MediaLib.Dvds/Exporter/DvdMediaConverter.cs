using DvdLib;
using MediaLib.Dvds.Providers;
using MediaLib.FFmpeg;
using MediaLib.Providers;
using Microsoft.Extensions.Logging;

namespace MediaLib.Dvds.Exporter;

public class DvdMediaConverter : FFmpegMediaConverter<DvdMediaProvider>
{
    public DvdMediaConverter(ILogger logger, DvdMediaProvider provider, MediaConverterParameter parameter) : 
        base(logger, provider, parameter)
    {
    }
    
    /// <inheritdoc />
    protected override ulong GetStreamIndex(StreamMetadata stream) => stream.Pid;

    /// <inheritdoc />
    protected override long GetSegmentFilesize(ushort segmentId)
    {
        var titleId = ushort.Parse(Parameter.Definition.Identifier.Id);
        var title = Provider.Dvd.TitleInfo[titleId];
        var cell = title.Pgc.CellPlayback[segmentId - 1];
        return (cell.LastSector - cell.FirstSector + 1) * Dvd.BlockSize;
    }
    
    /// <inheritdoc />
    protected override Stream OpenSegmentStream(ushort segmentId)
    {
        if (!ushort.TryParse(Parameter.Definition.Identifier.Id, out var titleId))
        {
            throw new ArgumentException("Couldn't parse title id.", nameof(Parameter));
        }
        
        var cellId = segmentId;
        
        var retries = 0;
        const int maxRetries = 5;
        while (true)
        {
            try
            {
                Logger.LogInformation("Opening segment #{cellId}", cellId);
                return Provider.Dvd.GetCellStream(titleId, cellId);
            }
            catch (Exception ex)
            {
                if (retries < maxRetries)
                {
                    retries++;
                    Logger.LogWarning(ex, "Exception while opening segment #{cellId}. Retry {Retry} / {MaxRetry}", cellId, retries, maxRetries);
                }
                else
                {
                    Logger.LogError(ex, "Exception while opening segment #{cellId}!", cellId);
                    throw;
                }
            }
        }
    }

    
}