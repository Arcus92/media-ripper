using Avalonia;
using Avalonia.Styling;
using MediaRipper.Models;
using MediaRipper.Services.Interfaces;

namespace MediaRipper.Services;

public class ThemeService : IThemeService
{
    /// <inheritdoc />
    public AppTheme CurrentTheme { get; private set; }
    
    /// <inheritdoc />
    public void SetTheme(AppTheme theme)
    {
        if (theme == CurrentTheme) return;

        if (Application.Current is null) return;
        Application.Current.RequestedThemeVariant = theme switch
        {
            AppTheme.Light => ThemeVariant.Light,
            AppTheme.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
        
        CurrentTheme = theme;
    }
}