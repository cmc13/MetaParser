using MetaParser.Models;

namespace MetaParser.WPF.MetaValidation
{
    public record MetaValidationResult(Meta Meta, Rule Rule, string Message);
}
