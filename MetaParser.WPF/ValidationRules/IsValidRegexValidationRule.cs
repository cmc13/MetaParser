using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace MetaParser.WPF.ValidationRules
{
    public class IsValidRegexValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var strValue = (string)value;
            try
            {
                Regex.Match("", strValue);
            }
            catch (ArgumentException)
            {
                return new ValidationResult(false, $"Input is not a valid regular expression");
            }

            return ValidationResult.ValidResult;
        }
    }
}
