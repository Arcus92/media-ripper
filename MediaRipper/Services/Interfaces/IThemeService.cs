using MediaRipper.Models;

namespace MediaRipper.Services.Interfaces;

public interface IThemeService
{
    /// <summary>
    /// Gets the current theme.
    /// </summary>
    AppTheme CurrentTheme { get; }
    
    /// <summary>
    /// Sets the application theme.
    /// </summary>
    /// <param name="theme">The theme to set.</param>
    void SetTheme(AppTheme theme);
}