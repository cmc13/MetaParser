using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace MetaParser.WPF.ValidationRules
{
    public class RegexValidationRule : ValidationRule
    {
        public string Regex { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is string s))
            {
                return new ValidationResult(false, "Input is an invalid type");
            }

            var r = new Regex(Regex);
            if (!r.IsMatch(s))
                return new ValidationResult(false, "Input does not match expected pattern");

            return ValidationResult.ValidResult;
        }
    }
}
