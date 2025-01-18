using System;
using System.Globalization;
using System.Windows.Data;

namespace MetaParser.WPF.Converters;

public sealed class ExponentialConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int i)
        {
            return Math.Sign(i) * Math.Log2(Math.Abs(i) + 1);
        }
        else if (value is double d)
        {
            return Math.Sign(d) * Math.Log2(Math.Abs(d) + 1);
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var dValue = (double)value;
        var sign = Math.Sign(dValue);
        if (sign == 0)
            sign = 1;
        if (targetType == typeof(int))
        {
            return sign * (int)Math.Pow(2.0, dValue) - 1;
        }
        else if (targetType == typeof(double))
        {
            return sign * Math.Pow(2.0, dValue) - 1;
        }

        return value;
    }
}
