using MetaParser.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace MetaParser.WPF.ValidationRules
{
    public class BindingProxy : System.Windows.Freezable
    {
        protected override Freezable CreateInstanceCore() => new BindingProxy();

        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy), new PropertyMetadata(null));
    }

    public class OptionWrapper : DependencyObject
    {
        public static readonly DependencyProperty OptionProperty = DependencyProperty.Register(nameof(Option), typeof(string), typeof(OptionWrapper), new PropertyMetadata(null));

        public string Option
        {
            get => (string)GetValue(OptionProperty);
            set => SetValue(OptionProperty, value);
        }
    }

    public class ValidOptionValueValidationRule : ValidationRule
    {
        public OptionWrapper Option { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string str && VTankOptionsExtensions.TryParse(Option.Option, out var opt))
            {
                if (!opt.IsValidValue(str))
                    return new ValidationResult(false, $"Invalid value for {Option.Option} option: '{value}'");
            }

            return ValidationResult.ValidResult;
        }
    }
}
