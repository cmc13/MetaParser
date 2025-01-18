using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace MetaParser.WPF.Converters
{
    public sealed class AllConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var desiredValue = System.Convert.ChangeType(parameter, targetType);
            if (values.All(v => v.Equals(desiredValue)))
                return true;
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
