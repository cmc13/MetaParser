using MetaParser.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace MetaParser.WPF.Converters
{
    public class ConditionTypeToMinimumValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ConditionType c)
            {
                var fi = typeof(ConditionType).GetField(c.ToString());
                if (fi != null)
                {
                    var range = fi.GetCustomAttribute<RangeAttribute>(false);
                    return (int)range.Minimum;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
