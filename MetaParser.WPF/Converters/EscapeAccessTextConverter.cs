using System;
using System.Globalization;
using System.Windows.Data;

namespace MetaParser.WPF.Converters
{
    public class EscapeAccessTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
                return str.Replace("_", "__");
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
