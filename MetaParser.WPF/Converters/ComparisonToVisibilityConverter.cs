using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace MetaParser.WPF.Converters
{
    public class ComparisonToVisibilityConverter : IValueConverter
    {
        private static readonly Regex r = new(@"^([><!]?=?)(\d+(?:\.\d+)?)$", RegexOptions.Compiled);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = true;
            if (value is IComparable c && parameter is string s)
            {
                var match = r.Match(s);
                if (match != null && match.Success)
                {
                    object p = (match.Groups[2].Value.Contains('.') || value.GetType() == typeof(double)) ?
                        (object)double.Parse(match.Groups[2].Value) :
                        (object)int.Parse(match.Groups[2].Value);

                    result = match.Groups[1].Value switch
                    {
                        ">=" => c.CompareTo(p) >= 0,
                        ">" => c.CompareTo(p) > 0,
                        "<=" => c.CompareTo(p) <= 0,
                        "<" => c.CompareTo(p) < 0,
                        "=" => c.CompareTo(p) == 0,
                        "!=" => c.CompareTo(p) != 0,
                        _ => throw new ArgumentException("Unexpected comparison operator")
                    };
                }
                else
                    throw new ArgumentException("Unable to parse comparison");
            }

            if (targetType == typeof(Visibility))
                return result ? Visibility.Visible : Visibility.Collapsed;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
