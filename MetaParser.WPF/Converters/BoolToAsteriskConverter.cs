using System;
using System.Globalization;
using System.Windows.Data;

namespace MetaParser.WPF.Converters
{
    public class BoolToAsteriskConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return "*";
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
