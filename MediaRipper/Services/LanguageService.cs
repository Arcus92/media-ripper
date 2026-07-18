using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using MediaRipper.Models;
using MediaRipper.Resources.Languages;
using MediaRipper.Services.Interfaces;

namespace MediaRipper.Services;

public class LanguageService : ILanguageService
{
    /// <inheritdoc />
    public AppLanguage CurrentLanguage { get; private set; }
    
    /// <summary>
    /// The currently loaded language dictionary.
    /// </summary>
    private ResourceDictionary? _currentLanguageDictionary;
    
    /// <summary>
    /// A dictionary of the language classes to make them AOT safe.
    /// </summary>
    private readonly Dictionary<AppLanguage, ResourceDictionary> _dictionaries = new()
    {
        { AppLanguage.English, new EnglishStrings() },
        { AppLanguage.German, new GermanStrings() }
    };
    
    /// <inheritdoc />
    public void SetLanguage(AppLanguage language)
    {
        if (CurrentLanguage == language && _currentLanguageDictionary is not null) return;
        if (Application.Current is null) return;
        var resources = Application.Current.Resources;
        
        if (!_dictionaries.TryGetValue(language, out var langaugeDictionary)) return;
        
        // Overwrite the resources
        foreach (var (key, value) in langaugeDictionary)
        {
            resources[key] = value;
        }
        
        _currentLanguageDictionary = langaugeDictionary;
        CurrentLanguage = language;
    }

    /// <inheritdoc />
    public string? Translate(string key)
    {
        // Load from loaded dictionary 
        if (_currentLanguageDictionary is null) return null;
        if (_currentLanguageDictionary.TryGetResource(key, null, out var resource) && resource is string text)
            return text;
        
        // Fallback to generic resources
        if (Application.Current is null) return null;
        if (Application.Current.TryGetResource(key, null, out resource) && resource is string appText)
            return appText;
        
        return null;
    }
}