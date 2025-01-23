using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Schema;

namespace MetaParser.WPF.ValidationRules
{
    public class IsValidViewDefinitionValidationRule : ValidationRule
    {
        private static readonly XmlSchema schema = LoadSchema();

        private static XmlSchema LoadSchema()
        {
            using var str = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("MetaParser.WPF.Assets.VTankView.xsd");
            var schema = XmlSchema.Read(str, null);
            return schema;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(value.ToString());
                doc.Schemas.Add(schema);

                doc.Validate(null);
            }
            catch (XmlSchemaValidationException)
            {
                return new ValidationResult(false, $"Input does not match schema");
            }
            catch (XmlException)
            {
                return new ValidationResult(false, $"Input is not a valid XML document");
            }

            return ValidationResult.ValidResult;
        }
    }
}
