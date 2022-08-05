using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MetaParser.WPF.Converters
{
    class CoordinateConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[0] == DependencyProperty.UnsetValue)
                return 0;

            unchecked
            {
                var data = (int)values[0];
                var ellipseWidth = (double)values[1];
                var canvasWidth = (double)values[2];

                uint xCoordinate;
                if ((string)parameter == "x")
                    xCoordinate = ((uint)data) / 0x1000000;
                else
                    xCoordinate = (((uint)data) / 0x10000) % 0x100;
                var leftOffset = xCoordinate * canvasWidth / 256 - ellipseWidth / 2;
                return leftOffset;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
