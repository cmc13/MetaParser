using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Data;

namespace MetaParser.WPF.Converters
{
    public class CompactFilePathConverter : IValueConverter
    {
        private static readonly int DEFAULT_COMPACT_LENGTH = 40;

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        static extern bool PathCompactPathEx([Out] StringBuilder pszOut, [In] string szPath, int cchMax, int dwFlags);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                if (parameter is not int length && (parameter is not string lengthStr || !int.TryParse(lengthStr, out length)))
                    length = DEFAULT_COMPACT_LENGTH;

                if (str.Length >= length)
                {
                    var sb = new StringBuilder();
                    PathCompactPathEx(sb, str, length, 0);
                    return sb.ToString();
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
