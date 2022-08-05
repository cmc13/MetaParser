using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MetaParser.WPF.Converters
{
    public class ReverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return Visibility.Collapsed;
            else if (value is Nullable<bool>)
            {
                var bb = (Nullable<bool>)value;
                if (bb.HasValue && bb.Value)
                    return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
                return v == Visibility.Collapsed;
            return true;
        }
    }
}
