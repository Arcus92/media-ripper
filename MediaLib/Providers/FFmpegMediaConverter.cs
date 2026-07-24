using MediaLib.FFmpeg;
using MediaLib.Models;
using MediaLib.Utils.IO;
using Microsoft.Extensions.Logging;
using StreamType = MediaLib.Models.StreamType;

namespace MediaLib.Providers;

/// <summary>
/// A basic converter to convert a media file with <see cref="FFmpeg"/>.
/// </summary>
/// <typeparam name="TProvider">The provider.</typeparam>
public abstract class FFmpegMediaConverter<TProvider> : IMediaConverter where TProvider : IMediaProvider
{
    protected readonly TProvider Provider;
    protected readonly ILogger Logger;
    protected readonly MediaConverterParameter Parameter;
    
    public FFmpegMediaConverter(ILogger logger, TProvider provider, MediaConverterParameter parameter)
    {
        Logger = logger;
        Provider = provider;
        Parameter = parameter;
    }
    
    /// <summary>
    /// Gets and sets the file extension added to the output files while the export is running.
    /// </summary>
    public string? WorkingFileExtension { get; set; } = ".tmp";
    
    /// <summary>
    /// The rename map to store the original and the temporary name.
    /// </summary>
    private readonly Dictionary<string, string> _renameMap = new();

    /// <summary>
    /// Must be called before using <see cref="GetWorkingFilename"/>.
    /// </summary>
    private void InitWorkingFilenames()
    {
        _renameMap.Clear();
    }

    /// <summary>
    /// Returns a filename with the <see cref="WorkingFileExtension"/>.
    /// </summary>
    /// <param name="originalFilename">The original filename.</param>
    /// <returns>Returns a working filename.</returns>
    private string GetWorkingFilename(string originalFilename)
    {
        if (WorkingFileExtension is null) return originalFilename;
        
        // Add working file extension 
        var newFilename = $"{originalFilename}{WorkingFileExtension}";
        _renameMap.Add(originalFilename, newFilename);
        return newFilename;
    }

    /// <summary>
    /// Must be called after all files were converted. This will remove the working extension from all files.
    /// </summary>
    /// <param name="outputPath">The output path to apply the renaming.</param>
    private void ApplyWorkingFilenames(string outputPath)
    {
        // Rename working files
        foreach (var (filename, workingFilename) in _renameMap)
        {
            var path = Path.Combine(outputPath, filename);
            var workingPath = Path.Combine(outputPath, workingFilename);
            File.Move(workingPath, path);
        }
    }

    /// <summary>
    /// Returns the index of the FFmpeg stream used for mapping the source data.
    /// This should either be <see cref="StreamIdType.Index"/> or <see cref="StreamIdType.Pid"/>.
    /// </summary>
    protected virtual StreamId GetStreamId(StreamInfo stream) => StreamId.Index(stream.Id);
    
    /// <summary>
    /// Handles the opening of a segment.
    /// </summary>
    /// <param name="segmentId">The segment id.</param>
    /// <returns>Returns the segment stream.</returns>
    protected abstract Stream OpenSegmentStream(ushort segmentId);
    
    /// <summary>
    /// Returns the raw filesize of the segment.
    /// </summary>
    /// <param name="segmentId">The segment id.</param>
    /// <returns>Returns the segment filesize.</returns>
    protected abstract long GetSegmentFilesize(ushort segmentId);

    /// <summary>
    /// Returns if FFmpeg requires to probe the whole file before converting.
    /// If a stream was not found in the initial probe, FFmpeg cannot use this stream. Sometimes stream packages for
    /// subtitles will only appear really late into the media file. The only way to reliably convert all streams is to
    /// probe the whole file.
    /// However, this requires two passes of file reading and put the whole stream into memory since FFmpeg is not
    /// allowed to seek in the input stream. It also breaks the progress bar.
    /// </summary>
    /// <returns></returns>
    protected virtual bool RequireFullProbeSize() => false;
    
    /// <summary>
    /// Is called when the stream data is passed to FFmpeg and allows the converter to add additinal metadata or codecs
    /// to the converter.
    /// </summary>
    /// <param name="stream">The stream added to FFmepg.</param>
    /// <param name="index">The index of the stream.</param>
    /// <param name="builder">The FFmpeg command builder.</param>
    protected virtual void CustomStreamSettings(StreamInfo stream, int index, CommandBuilder builder) 
    {
    }
    
    /// <summary>
    /// Opens a combined stream with all segments.
    /// </summary>
    /// <returns>Returns the complete stream.</returns>
    public Stream OpenCombinedStream()
    {
        var streamFactories = new List<Func<Stream>>();
        foreach (var segmentId in Parameter.Definition.Identifier.SegmentIds)
        {
            streamFactories.Add(() => OpenSegmentStream(segmentId));
        }

        return new StreamListReader(streamFactories);
    }

    /// <summary>
    /// Returns the total filesize of all segments and the length of <see cref="OpenCombinedStream"/>.
    /// </summary>
    /// <returns>Returns the complete filesize.</returns>
    public long GetCombinedFilesize()
    {
        long length = 0;
        foreach (var segmentId in Parameter.Definition.Identifier.SegmentIds)
        {
            length += GetSegmentFilesize(segmentId);
        }

        return length;
    }

    /// <inheritdoc />
    public virtual async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var definition = Parameter.Definition;
        var outputPath = Parameter.Path;
        var onUpdate = Parameter.OnUpdate;
        
        if (!Provider.Contains(Parameter.Definition.Identifier))
        {
            throw new ArgumentException("Output source is not defined by provider!", nameof(definition));
        }

        // Collecting total input filesize. The FFmpeg time code doesn't work for progress tracking.
        // It will only show the time code for the last stream in our output. This is almost always a subtitle. To be
        // exact, a forced subtitle that is only used a few times in the video.
        // To have a better progress status, we'll track the file position of the virtual input streams.
        var completeInputSize = GetCombinedFilesize();
        var completeStream = OpenCombinedStream();
        
        var ffmpeg = new Engine();
        
        // Build a better update event to calculate the percentage value by consumed bytes.
        Action<ConverterUpdate>? newOnUpdate = null;
        if (onUpdate is not null)
        {
            newOnUpdate = update =>
            {
                update.Percentage = completeStream.Position / (double)completeInputSize;
                onUpdate(update);
            };
        }
        
        Logger.LogInformation("Starting export of {Id} to {OutputPath} as {Basename}", 
            definition.Identifier.Id, outputPath, definition.MediaInfo.Name); 

        // Convert the file
        InitWorkingFilenames();
        await ffmpeg.ConvertAsync(builder =>
        {
            if (RequireFullProbeSize())
            {
                builder.ProbeSize(completeInputSize);
                builder.AnalyzeDuration((long)Parameter.Definition.Duration.TotalMilliseconds * 1000);
            }

            var input = builder.Input(completeStream);
            
            if (definition.ExportChapters)
            {
                // Builds the chapter file in memory
                var chapterStream = new MemoryStream();
                var chapterWriter = new StreamWriter(chapterStream);
                foreach (var chapter in definition.Chapters)
                {
                    var start = (ulong)(chapter.Start.TotalSeconds * 1000);
                    var end = (ulong)(chapter.End.TotalSeconds * 1000);
                    chapterWriter.WriteLine("[CHAPTER]");
                    chapterWriter.WriteLine("TIMEBASE=1/1000");
                    chapterWriter.WriteLine($"START={start}");
                    chapterWriter.WriteLine($"END={end}");
                    chapterWriter.WriteLine($"title={chapter.Name}");
                    chapterWriter.WriteLine();
                }

                chapterWriter.Flush();
                chapterStream.Position = 0;

                // Map the chapter
                builder.Format("ffmetadata");
                var inputChapter = builder.Input(chapterStream);
                builder.MapChapters(inputChapter);
            }

            // FFmpeg supports multiple outputs. We can export the subtitle files in a single run as well.
            // We just need to create a new mapping and then define a new output.
            foreach (var file in definition.Files)
            {
                // Define codec
                builder.Codec(FFmpeg.StreamType.Video, definition.Codec.VideoCodec);
                builder.Codec(FFmpeg.StreamType.Audio, definition.Codec.AudioCodec);
                builder.Codec(FFmpeg.StreamType.Subtitle, definition.Codec.SubtitleCodec);
            
                if (definition.Codec.ConstantRateFactor.HasValue) builder.ConstantRateFactor(definition.Codec.ConstantRateFactor.Value);
                if (definition.Codec.MaxRate.HasValue) builder.MaxRate(definition.Codec.MaxRate.Value);
                if (definition.Codec.BufferSize.HasValue) builder.BufferSize(definition.Codec.BufferSize.Value);
                
                // Map the output streams
                var outputStreamCount = 0;
                foreach (var stream in file.Streams)
                {
                    if (!stream.Enabled) continue;

                    var streamId = GetStreamId(stream);

                    builder.Map(input, streamId);
                    if (!string.IsNullOrEmpty(stream.LanguageCode))
                        builder.Metadata(outputStreamCount, "language", stream.LanguageCode);
                    if ((stream.Flags & StreamFlags.Default) != 0)
                        builder.Disposition(outputStreamCount, "default");

                    CustomStreamSettings(stream, outputStreamCount, builder);
                    
                    outputStreamCount++;
                }
                
                // Video output
                builder.OverwriteOutput();
                builder.Format(file.Format);

                var filename = GetWorkingFilename(file.Filename);
                
                var path = Path.Combine(outputPath, filename);
                builder.Output(path);
            }

        }, newOnUpdate, cancellationToken);

        ApplyWorkingFilenames(outputPath);
        
        Logger.LogInformation("Stream {Id} was exported to {OutputPath}", 
            definition.Identifier.Id, outputPath); 
    }
}