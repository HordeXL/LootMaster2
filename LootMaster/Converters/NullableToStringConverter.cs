using System.Globalization;
using System.Windows.Data;

namespace LootMaster.Converters;

/// <summary>Converts null → "" and any other value → its ToString().
/// Doubles are formatted with "0.######" to avoid scientific notation.</summary>
[ValueConversion(typeof(object), typeof(string))]
public sealed class NullableToStringConverter : IValueConverter
{
    public static readonly NullableToStringConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d) return d.ToString("0.######", culture);
        return value?.ToString() ?? "";
    }

    public object? ConvertBack(string? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
