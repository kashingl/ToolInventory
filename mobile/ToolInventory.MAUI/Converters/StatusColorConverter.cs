using System.Globalization;

namespace ToolInventory.MAUI.Converters;

public class StatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value?.ToString() switch
        {
            "Available" => Colors.Green,
            "CheckedOut" => Colors.OrangeRed,
            "UnderMaintenance" => Colors.DodgerBlue,
            "Retired" => Colors.Gray,
            _ => Colors.Black
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
