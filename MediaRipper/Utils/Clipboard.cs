using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;

namespace MediaRipper.Utils;

public static class Clipboard
{
    /// <summary>
    /// Gets the clipboard api for the current platform.
    /// </summary>
    /// <returns>Returns the clipboard api if available.</returns>
    public static IClipboard? GetClipboard()
    {
        switch (Application.Current?.ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime classicDesktopApp:
                return classicDesktopApp.MainWindow?.Clipboard;
            case ISingleViewApplicationLifetime singleViewApp:
            {
                var visualRoot = singleViewApp.MainView?.GetVisualRoot();
                if (visualRoot is TopLevel topLevel) 
                {
                    return topLevel.Clipboard;
                }

                break;
            }
        }

        return null;
    }
}