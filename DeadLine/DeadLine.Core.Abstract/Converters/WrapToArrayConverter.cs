using System.Collections;

namespace DeadLine.Core;

public class WrapToArrayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            IEnumerable enu => enu,
            int i => new[] { i },
            double d => new[] { d },
            _ => value
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}