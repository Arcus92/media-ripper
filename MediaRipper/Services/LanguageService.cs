using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MediaRipper.Models;
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
    
    /// <inheritdoc />
    public void SetLanguage(AppLanguage language)
    {
        if (CurrentLanguage == language && _currentLanguageDictionary is not null) return;
        if (Application.Current is null) return;
        var resources = Application.Current.Resources;
        
        var uri = new Uri($"avares://MediaRipper/Resources/Strings/{language}.axaml");
        if (AvaloniaXamlLoader.Load(uri) is not ResourceDictionary langaugeDictionary) return;
        
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