using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace MetaParser.WPF.Converters
{
    public sealed class IndexingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable e)
                return IndexedValues(e);

            return value;
        }

        private IEnumerable<KeyValuePair<int, object>> IndexedValues(IEnumerable e)
        {
            var i = 0;
            foreach (var val in e)
                yield return new KeyValuePair<int, object>(++i, val);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
