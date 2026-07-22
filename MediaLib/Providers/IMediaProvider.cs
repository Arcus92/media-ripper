using MediaLib.Models;
using MediaLib.Sources;

namespace MediaLib.Providers;

/// <summary>
/// Defined a provider of media files. This can be a disk or a folder.
/// </summary>
public interface IMediaProvider : IDisposable
{
    /// <summary>
    /// Returns the disk info from this provider.
    /// </summary>
    /// <returns></returns>
    DiskInfo GetDiskInfo();
    
    /// <summary>
    /// Loads the media provider.
    /// </summary>
    /// <returns></returns>
    Task LoadAsync();
    
    /// <summary>
    /// Returns all possible media sources from this provider.
    /// </summary>
    /// <returns>The list of all definitions from this provider.</returns>
    IAsyncEnumerable<IMediaSource> GetSourcesAsync();
    
    /// <summary>
    /// Creates a media exporter for the given source.
    /// </summary>
    /// <param name="parameter">The media convert parameter containing the output definition and output path.</param>
    /// <returns>The created exporter.</returns>
    IMediaConverter CreateConverter(MediaConverterParameter parameter);

    /// <summary>
    /// Returns the raw media stream from the given source.
    /// </summary>
    /// <param name="source">The source to access the raw stream from.</param>
    /// <param name="segmentId">The segemnt id to access.</param>
    /// <returns></returns>
    Stream GetRawStream(IMediaSource source, ushort segmentId);
    
    /// <summary>
    /// Returns if the given provider contains the given media by its identifier.
    /// </summary>
    /// <param name="identifier">The media identifier.</param>
    /// <returns>Returns true, if the media is contained in this provider.</returns>
    bool Contains(MediaIdentifier identifier);
}