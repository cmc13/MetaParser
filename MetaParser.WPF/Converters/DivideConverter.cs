using System;
using System.Globalization;
using System.Windows.Data;

namespace MetaParser.WPF.Converters
{
    public class DivideConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                double p;
                if (parameter is string s)
                    p = double.Parse(s);
                else
                    p = (double)parameter;

                return d / p;
            }
            else if (value is int i)
            {
                int p;
                if (parameter is string s)
                    p = int.Parse(s);
                else
                    p = (int)i;

                return i / p;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
