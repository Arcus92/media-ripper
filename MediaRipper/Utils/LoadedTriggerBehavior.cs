using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace MediaRipper.Utils;

/// <summary>
///     A trigger behavior to attach a custom <see cref="OnLoaded" /> event.
/// </summary>
public class LoadedTriggerBehavior : Behavior<Control>
{
    /// <summary>
    ///     The command property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<LoadedTriggerBehavior, ICommand?>(nameof(Command));

    /// <summary>
    ///     Gets and sets the command to invoke when the control is loaded.
    /// </summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Command?.Execute(sender);
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        if (AssociatedObject is not null) AssociatedObject.Loaded += OnLoaded;
        base.OnAttached();
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        if (AssociatedObject is not null) AssociatedObject.Loaded -= OnLoaded;
        base.OnDetaching();
    }
}