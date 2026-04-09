using System.Windows;

namespace LootMaster.Helpers;

/// <summary>
/// A Freezable that holds a DataContext reference so DataGridColumn bindings
/// (which are not in the visual tree) can reach the ViewModel via Source={StaticResource}.
/// </summary>
public sealed class BindingProxy : Freezable
{
    protected override Freezable CreateInstanceCore() => new BindingProxy();

    public object? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy));
}
