using MetaParser.Models;
using System.IO;
using System.Threading.Tasks;

namespace MetaParser.Formatting;

public interface IMetaReader
{
    Task<Meta> ReadMetaAsync(Stream stream);
}
