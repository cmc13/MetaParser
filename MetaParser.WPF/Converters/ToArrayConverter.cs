using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace MetaParser.WPF.Converters
{
    public sealed class ToArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable)
                return value;
            return new[] { value };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
