using System.Text;
using MediaLib.Formats;
using MediaLib.Models;

namespace MediaLib.Output;

public static class OutputHelper
{
    /// <summary>
    /// Creates a default output definition for this media info.
    /// </summary>
    /// <param name="baseName">The output file basename.</param>
    /// <param name="mediaInfo">The media info to extract the output from.</param>
    /// <param name="codec">The codec options to use.</param>
    /// <param name="containerFormat">The container format to use.</param>
    /// <returns>Returns the output definition.</returns>
    public static OutputDefinition CreateDefaultOutputDefinition(string baseName, MediaInfo mediaInfo,
        CodecOptions codec, MediaFormat containerFormat)
    {
        var files = GetFilesByStreams(baseName, mediaInfo.Streams, codec, containerFormat);
        
        return new OutputDefinition
        {
            Identifier = mediaInfo.Identifier,
            MediaInfo = new MediaMetadata
            {
                Name = baseName,
            },
            Duration = mediaInfo.Duration,
            Codec = codec,
            Files = files.ToArray(),
            Chapters = mediaInfo.Chapters
        };
    }
    
    /// <summary>
    /// Returns all output files from the given set of streams when converting to the given output videoFormat.
    /// This will 
    /// </summary>
    /// <param name="baseName">The base name of the title.</param>
    /// <param name="streams">The different streams of the title.</param>
    /// <param name="codec">The codec options.</param>
    /// <param name="containerFormat">The output container format.</param>
    /// <returns>Returns a list of output files.</returns>
    public static List<OutputFile> GetFilesByStreams(string baseName, IEnumerable<StreamInfo> streams, CodecOptions codec, MediaFormat containerFormat)
    {
        var files = new List<OutputFile>();

        var mainFileStreams = new List<StreamInfo>();

        // Guessing the subtitle type by order.
        var filenameCounter = new Dictionary<string, int>();
        var filenameBuilder = new StringBuilder();
        
        foreach (var stream in streams)
        {
            if (IsStreamSupported(containerFormat, stream))
            {
                mainFileStreams.Add(stream);
            }
            else
            {
                // The format must be exported into its own file.
                if (!TryGetFormatByStream(stream, out var format))
                    continue;
                
                // Building the filename
                filenameBuilder.Clear();
                filenameBuilder.Append(baseName);

                if (!string.IsNullOrEmpty(stream.LanguageCode))
                {
                    filenameBuilder.Append('.');
                    filenameBuilder.Append(stream.LanguageCode);
                }

                if ((stream.Flags & StreamFlags.Forced) != 0)
                {
                    filenameBuilder.Append(".forced");
                }
            
                // Count how often the language code was encountered.
                var filenameBase = filenameBuilder.ToString();
                filenameCounter.TryGetValue(filenameBase, out var counter);
                
                if (counter >= 1) 
                {
                    filenameBuilder.Append($".extra{counter}");
                }
                filenameBuilder.Append(format.Extension);
            
                // Increment counter
                filenameCounter[filenameBase] = ++counter;
            
                files.Add(new OutputFile()
                {
                    Filename = filenameBuilder.ToString(),
                    Format = format.FFmpegFormat,
                    Streams = [stream]
                });
            }
            
        }
        
        // Create the main video file.
        var mainFile = new OutputFile()
        {
            Filename = $"{baseName}{containerFormat.Extension}",
            Format = containerFormat.FFmpegFormat,
            Streams = mainFileStreams.ToArray()
        };
        files.Insert(0, mainFile);

        return files;
    }

    /// <summary>
    /// Returns if the stream is supported by the container format.
    /// </summary>
    /// <param name="containerFormat">The container format.</param>
    /// <param name="stream">The stream to check.</param>
    /// <returns>Returns true, if the stream is supported by the container.</returns>
    public static bool IsStreamSupported(MediaFormat containerFormat, StreamInfo stream)
    {
        return stream.Type switch
        {
            StreamType.Video or StreamType.Audio => true, // Assume it is supported.
            StreamType.Subtitle => ContainerFormats.SupportSubtitle(containerFormat.FFmpegFormat, stream.Format ?? ""),
            _ => false
        };
    }

    
    /// <summary>
    /// Returns the media format of the given stream.
    /// </summary>
    /// <param name="stream">The output stream to check.</param>
    /// <param name="result">Returns the media format.</param>
    /// <returns>Returns true, if the format was found.</returns>
    public static bool TryGetFormatByStream(StreamInfo stream, out MediaFormat result)
    {
        switch (stream.Type)
        {
            case StreamType.Video:
                return ContainerFormats.All.TryGetFromFormat(stream.Format ?? "", out result);
            case StreamType.Subtitle:
                return SubtitleFormats.All.TryGetFromFormat(stream.Format ?? "", out result);
            case StreamType.Audio:
                // Not supported yet
            default:
                result = default;
                return false;
        }
    }
}