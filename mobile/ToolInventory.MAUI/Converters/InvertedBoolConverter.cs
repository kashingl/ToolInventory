using System.Globalization;

namespace ToolInventory.MAUI.Converters;

public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool booleanValue && !booleanValue;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool booleanValue && !booleanValue;
}
