using MetaParser.Models;

namespace MetaParser.WPF.MetaValidation;

public sealed record MetaValidationResult(Meta Meta, Rule Rule, string Message);
