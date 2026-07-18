using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace MediaRipper.Utils;

/// <summary>
/// 
/// </summary>
public class DynamicResourceKeyConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (Application.Current is null || value is not string resourceKey) 
            return value;

        if (Application.Current.TryFindResource(resourceKey, out var resource) && resource is string text)
        {
            return text;
        }

        return value;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}