using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using MediaRipper.Models.Outputs;
using MediaRipper.Services.Interfaces;
using MediaRipper.Utils;
using MediaRipper.Views;

namespace MediaRipper.ViewModels;

public class SourceSelectorViewModel : ViewModelBase
{
    private readonly ISettingService _settingService;
    private readonly IMediaProviderService _mediaProviderService;
    private readonly IStorageProviderAccessor _storageProviderAccessor;
    private readonly IOutputQueueService _outputQueueService;

    public SourceSelectorViewModel(ISettingService settingService, IMediaProviderService mediaProviderService, 
        IStorageProviderAccessor storageProviderAccessor, IOutputQueueService outputQueueService)
    {
        _settingService = settingService;
        _mediaProviderService = mediaProviderService;
        _storageProviderAccessor = storageProviderAccessor;
        _outputQueueService = outputQueueService;
        
        SourcePath = _settingService.Data.SourcePath ?? "";
        _outputQueueService.StatusChanged += OnOutputQueueServiceStatusChanged;
    }
    
    /// <summary>
    /// The view was loaded.
    /// </summary>
    public async Task OnLoaded()
    {
        await OpenAsync();
    }

    /// <summary>
    /// Gets and sets the disk input path.
    /// </summary>
    public string SourcePath
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets and sets the disk content hash.
    /// </summary>
    public string ContentHash
    {
        get;
        set => SetProperty(ref field, value);
    } = "";

    /// <summary>
    /// Gets if this element is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get;
        private set => SetProperty(ref field, value);
    } = true;

    private void OnOutputQueueServiceStatusChanged(object? sender, EventArgs e)
    {
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        IsEnabled = _outputQueueService.Status != OutputQueueStatus.Running;
    }

    private void UpdateDiskInfo()
    {
        var diskInfo = _mediaProviderService.GetDiskInfo();
        ContentHash = diskInfo?.ContentHash ?? "";
    }
    
    /// <summary>
    /// Opens and loads the current source path.
    /// </summary>
    public async Task OpenAsync()
    {
        if (!IsEnabled) return;
        await _mediaProviderService.OpenAsync(SourcePath);
        UpdateDiskInfo();
        
        _settingService.Data.SourcePath = SourcePath;
        _settingService.NotifyChange();
    }

    /// <summary>
    /// Refresh the current source path.
    /// </summary>
    public async Task RefreshAsync()
    {
        if (!IsEnabled) return;
        await _mediaProviderService.OpenAsync(SourcePath);
        UpdateDiskInfo();
    }
    
    /// <summary>
    /// Opens a folder picker to select the source file.
    /// </summary>
    public async Task OpenFolderPickerAsync()
    {
        if (!IsEnabled) return;
        var storageProvider = _storageProviderAccessor.StorageProvider;
        if (storageProvider is null)
        {
            return;
        }
        
        var paths = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(SourcePath)
        });

        if (paths.Count >= 1)
        {
            SourcePath = paths[0].Path.LocalPath;
            await OpenAsync();
        }
    }

    /// <summary>
    /// Copies the content hash into the clipboard.
    /// </summary>
    public async Task CopyContentHashAsync()
    {
        if (string.IsNullOrEmpty(ContentHash)) return;
        var clipboard = Clipboard.GetClipboard();
        if (clipboard is null) return;
        await clipboard.SetTextAsync(ContentHash);
    }
    
    /// <inheritdoc />
    public override Control CreateView()
    {
        return new SourceSelectorView();
    }
}