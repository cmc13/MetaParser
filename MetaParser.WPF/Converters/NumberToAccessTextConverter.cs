using System;
using System.Globalization;
using System.Windows.Data;

namespace MetaParser.WPF.Converters
{
    public class NumberToAccessTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
            {
                var str = i.ToString();
                return str.Insert(str.Length - 1, "_");
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
