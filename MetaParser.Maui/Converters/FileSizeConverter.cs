using System.Globalization;

namespace MetaParser.Maui.Converters;

public class FileSizeConverter
    : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long size && targetType == typeof(string))
        {
            var sizes = new[] { "B", "KB", "MB", "GB", "TB" };
            var order = 0;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                size /= 1024;
                order++;
            }

            return $"{size} {sizes[order]}";
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
