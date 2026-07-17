using System.ComponentModel;
using Avalonia.Controls;
using MediaRipper.Services.Interfaces;
using MediaRipper.Views;

namespace MediaRipper.ViewModels;

public class OutputSettingsContainerViewModel : ViewModelBase
{
    private readonly IOutputService _outputService;
    private readonly OutputTreeViewModel _outputTreeViewModel;
    
    public OutputSettingsContainerViewModel(IOutputService outputService, OutputTreeViewModel outputTreeViewModel)
    {
        _outputService = outputService;
        _outputTreeViewModel =  outputTreeViewModel;
        _outputTreeViewModel.PropertyChanged += OnOutputTreeViewModelPropertyChanged;
    }

    private void OnOutputTreeViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(OutputTreeViewModel.SelectedItem):
                var model = _outputTreeViewModel.SelectedItem?.Model;
                SelectedItem = model is null ? null : new OutputViewModel(_outputService, model);
                break;
        }
    }

    /// <summary>
    /// Gets and sets the selected output.
    /// </summary>
    public OutputViewModel? SelectedItem
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc />
    public override Control CreateView()
    {
        return new OutputSettingsContainerView();
    }
}