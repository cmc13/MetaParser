using MetaParser.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MetaParser.WPF.Converters
{
    public class VTankOptionToDisplayDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string optionStr)
            {
                if (optionStr != null && VTankOptionsExtensions.TryParse(optionStr, out var opt))
                {
                    return opt.GetDescription();
                }

                return null;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
