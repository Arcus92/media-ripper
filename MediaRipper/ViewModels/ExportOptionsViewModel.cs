using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using MediaLib;
using MediaLib.Formats;
using MediaRipper.Services.Interfaces;
using MediaRipper.Views;

namespace MediaRipper.ViewModels;

public class ExportSettingsViewModel : ViewModelBase
{
    private readonly IOutputService _outputService;
    private readonly IMediaProviderService _mediaProviderService;
    private readonly ISettingService _settingService;
    private readonly OutputSelectorViewModel _outputSelector;
    private readonly SourceTreeViewModel _sourceTree;
    private readonly MediaLookupViewModel _mediaLookup;
    
    public ExportSettingsViewModel(IOutputService outputService, IMediaProviderService mediaProviderService, 
        ISettingService settingService, OutputSelectorViewModel outputSelector, SourceTreeViewModel sourceTree, 
        MediaLookupViewModel mediaLookup)
    {
        _outputService = outputService;
        _mediaProviderService = mediaProviderService;
        _settingService = settingService;
        _outputSelector = outputSelector;
        _sourceTree = sourceTree;
        _mediaLookup = mediaLookup;
        
        _sourceTree.PropertyChanged += OnSourceTreePropertyChanged;
    }

    #region Selection

    /// <summary>
    /// Gets if the selected item can be queued.
    /// </summary>
    public bool CanQueueSelection
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets if the selected item can be played.
    /// </summary>
    public bool CanPlaySelection
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets if the selected item can be saved.
    /// </summary>
    public bool CanSaveSelection
    {
        get;
        set => SetProperty(ref field, value);
    }

    private void OnSourceTreePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SourceTreeViewModel.SelectedItem):
                UpdateSelection();
                break;
        }
    }

    private void UpdateSelection()
    {
        var canPlayPreview = !string.IsNullOrEmpty(_settingService.Data.FFplayPath);
        
        if (_sourceTree.TryGetSelectedTitleNode(out var titleNode))
        {
            var output = _outputService.GetByIdentifier(titleNode.Source.Identifier);
            CanQueueSelection = output is null;
            CanPlaySelection = canPlayPreview;
            CanSaveSelection = true;
        }
        else
        {
            CanQueueSelection = false;
            CanPlaySelection = false;
            CanSaveSelection = false;
        }
    }
    
    #endregion Selection

    #region Format settings

    /// <summary>
    /// Convert video to web-friendly format.
    /// </summary>
    private static readonly CodecOptions DefaultCodecOptions = new()
    {
        VideoCodec = "libx264",
        ConstantRateFactor = 16,
        MaxRate = 20000,
        BufferSize = 25000
    };
    
    /// <summary>
    /// Gets the list of all output formats.
    /// </summary>
    public MediaFormat[] AllOutputFormats => ContainerFormats.All;

    /// <summary>
    /// Gets and sets the output format.
    /// </summary>
    public MediaFormat OutputFormat
    {
        get;
        set => SetProperty(ref field, value);
    } = ContainerFormats.Mp4;

    #endregion Format settings
    
    #region Commands
    
    /// <summary>
    /// Adds the selected title to the output list.
    /// </summary>
    public async Task QueueSelectionAsync()
    {
        if (!_sourceTree.TryGetSelectedTitleNode(out var titleNode))
            return;
        
        var outputDefinition = titleNode.Source.CreateDefaultOutputDefinition(DefaultCodecOptions, OutputFormat);

        if (_mediaLookup.TryGetMediaInfo(out var mediaInfo))
        {
            var basename = mediaInfo.GetBasename();

            foreach (var file in outputDefinition.Files)
            {
                file.Filename = file.Filename[outputDefinition.MediaInfo.Name.Length..].Insert(0, basename);
            }
            
            outputDefinition.MediaInfo = mediaInfo;
            _mediaLookup.IncreaseEpisodeNumber();
        }
        
        await _outputService.AddAsync(outputDefinition);
        UpdateSelection();
    }

    public async Task PlayPreviewAsync()
    {
        if (!_sourceTree.TryGetSelectedTitleNode(out var titleNode))
        {
            return;
        }

        // Pipe-ing the decrypted segment stream into your player. You won't be able to seek properly.
        // You can skip a few seconds ahead, but not backwards. Changing audio or subtitle tracks will force you to 
        // jump ahead to the end of the current playback buffer.
        // Despite all of this, this is a usable preview to determine the content.
        var process = new Process();
        process.StartInfo.FileName = _settingService.Data.FFplayPath;
        process.StartInfo.Arguments = "-";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;

        process.Start();
        
        await Task.Run(async () =>
        {
            try
            {
                foreach (var segment in titleNode.Source.Info.Segments)
                {
                    await using var stream = _mediaProviderService.GetRawStream(titleNode.Source, segment.Id);
                    await stream.CopyToAsync(process.StandardInput.BaseStream);
                }
            }
            catch (IOException)
            {
                // Broken pipe exception is expected when consuming player is closed...
            }
        });
        
        await process.WaitForExitAsync();
    }

    public async Task SaveSegmentAsync()
    {
        if (!_sourceTree.TryGetSelectedTitleNode(out var titleNode))
        {
            return;
        }

        var identifier = titleNode.Source.Identifier;
        var path = Path.Combine(_outputSelector.OutputPath,
            $"{identifier.DiskName}_{identifier.Id}.m2ts");
        
        await using var output = File.Create(path);
        await using var stream = _mediaProviderService.GetRawStream(titleNode.Source);
        await stream.CopyToAsync(output);
    }
    
    #endregion Commands
    
    /// <inheritdoc />
    public override Control CreateView()
    {
        return new ExportSettingsView();
    }
}