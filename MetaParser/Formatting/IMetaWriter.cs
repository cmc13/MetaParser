using MetaParser.Models;
using System.IO;
using System.Threading.Tasks;

namespace MetaParser.Formatting;

public interface IMetaWriter
{
    Task WriteActionAsync(TextWriter writer, MetaAction action);
    Task WriteConditionAsync(TextWriter writer, Condition condition);
    Task WriteMetaAsync(Stream stream, Meta meta);
    Task WriteRuleAsync(TextWriter writer, Rule rule);
}
