using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace MetaParser.WPF.Extensions
{
    public class EnumBindingSourceExtension : MarkupExtension
    {
        private class EnumComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                var converter = TypeDescriptor.GetConverter(x.GetType());
                return StringComparer.OrdinalIgnoreCase.Compare(converter.ConvertToInvariantString(x), converter.ConvertToInvariantString(y));
            }

        }
        private Type _enumType;
        public Type EnumType
        {
            get { return this._enumType; }
            set
            {
                if (value != this._enumType)
                {
                    if (null != value)
                    {
                        Type enumType = Nullable.GetUnderlyingType(value) ?? value;

                        if (!enumType.IsEnum)
                            throw new ArgumentException("Type must be for an Enum.");
                    }

                    this._enumType = value;
                }
            }
        }

        public EnumBindingSourceExtension() { }

        public EnumBindingSourceExtension(Type enumType)
        {
            this.EnumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (null == this._enumType)
                throw new InvalidOperationException("The EnumType must be specified.");

            var actualEnumType = Nullable.GetUnderlyingType(this._enumType) ?? this._enumType;
            var enumValues = Enum.GetValues(actualEnumType)
                .OfType<object>()
                .Where(o =>
                {
                    var memInfo = actualEnumType.GetMember(o.ToString()).FirstOrDefault();
                    return !memInfo.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any();
                }).ToArray();

            if (actualEnumType == this._enumType)
            {
                Array.Sort(enumValues, new EnumComparer());
                return enumValues;
            }

            Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            Array.Sort(tempArray, new EnumComparer());
            return tempArray;
        }
    }
}
