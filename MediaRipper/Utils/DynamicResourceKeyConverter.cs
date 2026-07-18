using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using MediaRipper.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MediaRipper.Utils;

/// <summary>
/// 
/// </summary>
public class DynamicResourceKeyConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string resourceKey) return value;
        
        var app = (App)Application.Current!;
        var languageService = app.ServiceProvider.GetRequiredService<ILanguageService>();
        return languageService.Translate(resourceKey);
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}