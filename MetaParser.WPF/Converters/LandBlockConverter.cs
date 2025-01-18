using System;
using System.Globalization;
using System.Windows.Data;

namespace MetaParser.WPF.Converters
{
    public sealed class LandBlockConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value is int block || (value is string s && int.TryParse(s, out block))))
            {
                unchecked
                {
                    var u = (uint)block;
                    return u.ToString("X");
                }
            }
            else if (value is uint uBlock)
            {
                return uBlock.ToString("X");
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                try
                {
                    var u = System.Convert.ToUInt32(s.TrimStart('0'), 16);
                    var i = (int)u;
                    return i;
                }
                catch (OverflowException) { }
                catch (FormatException)
                {
                    if (NamedLandcellExtensions.TryParse(s, out var landcell))
                    {
                        return (int)landcell;
                    }
                    else
                        throw;
                }
                catch (ArgumentOutOfRangeException) { }
            }

            return value;
        }
    }
}
