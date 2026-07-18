using MediaRipper.Models;

namespace MediaRipper.Services.Interfaces;

public interface ILanguageService
{
    /// <summary>
    /// Gets the currently loaded language.
    /// </summary>
    public AppLanguage CurrentLanguage { get; }
    
    /// <summary>
    /// Sets the current application language and loads the translation resource file.
    /// </summary>
    /// <param name="language">The language to load.</param>
    public void SetLanguage(AppLanguage language);
    
    /// <summary>
    /// Gets the translation text.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <returns>Returns the translated text.</returns>
    public string? Translate(string key);
}