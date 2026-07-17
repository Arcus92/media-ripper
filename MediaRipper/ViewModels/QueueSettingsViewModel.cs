using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using MediaRipper.Models.Outputs;
using MediaRipper.Services.Interfaces;
using MediaRipper.Views;

namespace MediaRipper.ViewModels;

public class QueueSettingsViewModel : ViewModelBase
{
    private readonly IApplicationService _applicationService;
    private readonly IOutputQueueService _outputQueueService;
    private readonly IOutputService _outputService;
    private readonly IMediaProviderService _mediaProviderService;
    private readonly OutputTreeViewModel _outputTreeViewModel;

    public QueueSettingsViewModel(IApplicationService applicationService, IOutputQueueService outputQueueService,
        IOutputService outputService, IMediaProviderService mediaProviderService, 
        OutputTreeViewModel outputTreeViewModel)
    {
        _applicationService = applicationService;
        _outputQueueService = outputQueueService;
        _outputService = outputService;
        _mediaProviderService = mediaProviderService;
        _outputTreeViewModel = outputTreeViewModel;
        
        _outputQueueService.StatusChanged += OnOutputQueueServiceStatusChanged;
        _mediaProviderService.Changed += OnMediaProviderServiceChanged;
        _outputTreeViewModel.PropertyChanged += OnOutputTreeViewModelOnPropertyChanged;
    }

    #region Queue

    /// <summary>
    /// Gets if the queue is started.
    /// </summary>
    public bool IsRunning
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets if the queue can be started.
    /// </summary>
    public bool CanStartQueue
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets if the selected item can be dequeued.
    /// </summary>
    public bool CanDequeueSelection
    {
        get;
        set => SetProperty(ref field, value);
    }
    
    private void OnOutputTreeViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(OutputTreeViewModel.SelectedItem):
                UpdateSelection();
                break;
        }
    }
    
    private void OnOutputQueueServiceStatusChanged(object? sender, EventArgs e)
    {
        UpdateQueue();
        UpdateSelection();
    }

    private void OnMediaProviderServiceChanged(object? sender, EventArgs e)
    {
        UpdateQueue();
    }

    private void UpdateQueue()
    {
        IsRunning = _outputQueueService.Status == OutputQueueStatus.Running;
        CanStartQueue = _mediaProviderService.IsLoaded;
    }

    private void UpdateSelection()
    {
        var output = _outputTreeViewModel.SelectedItem?.Model;
        CanDequeueSelection = output is not null && output.Status != OutputStatus.Completed && output.Status != OutputStatus.Processing;
    }
    
    #endregion Queue
    
    #region Commands
    
    /// <inheritdoc cref="IOutputQueueService.Start"/>
    public void StartQueue()
    {
        _outputQueueService.Start();
    }
    
    /// <inheritdoc cref="IOutputQueueService.Stop"/>
    public void StopQueue()
    {
        _outputQueueService.Stop();
    }

    /// <summary>
    /// Opens the application settings.
    /// </summary>
    public void OpenSettings()
    {
        _applicationService.ShowWindow<SettingsWindowViewModel>();
    }
    
    public async Task DequeueSelectionAsync()
    {
        var output = _outputTreeViewModel.SelectedItem?.Model;
        if (output is null) return;
        if (output.Status is OutputStatus.Completed or OutputStatus.Processing) return; // Do not remove completed outputs!
        await _outputService.RemoveAsync(output);
        UpdateSelection();
    }
    
    #endregion Commands
    
    /// <inheritdoc />
    public override Control CreateView()
    {
        return new QueueSettingsView();
    }
}