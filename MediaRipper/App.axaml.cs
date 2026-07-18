using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using MediaRipper.Services.Interfaces;
using MediaRipper.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MediaRipper;

public partial class App : Application
{
    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public ServiceProvider ServiceProvider { get; private set; } = null!;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        // Register all services for this application
        var collection = new ServiceCollection();
        collection.AddCommonServices();
        
        ServiceProvider = collection.BuildServiceProvider();
        
        var applicationService = ServiceProvider.GetRequiredService<IApplicationService>();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = applicationService.ShowWindow<MainWindowViewModel>();

            var storageProviderAccessor = ServiceProvider.GetRequiredService<IStorageProviderAccessor>();
            storageProviderAccessor.StorageProvider = desktop.MainWindow.StorageProvider;
        }

        base.OnFrameworkInitializationCompleted();
    }
}