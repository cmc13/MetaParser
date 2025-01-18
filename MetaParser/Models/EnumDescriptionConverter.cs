using System;
using System.ComponentModel;
using System.Reflection;

namespace MetaParser.Models;

public sealed class EnumDescriptionConverter
    : EnumConverter
{
    public EnumDescriptionConverter(Type type)
        : base(type)
    {
    }

    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
    {
        if (destinationType == typeof(string))
        {
            if (value != null)
            {
                var fi = value.GetType().GetField(value.ToString());
                if (fi != null)
                {
                    var attribute = fi.GetCustomAttribute<DescriptionAttribute>();
                    return attribute?.Description ?? value.ToString();
                }
            }

            return string.Empty;
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}
