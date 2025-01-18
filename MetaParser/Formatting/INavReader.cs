using MetaParser.Models;
using System.IO;
using System.Threading.Tasks;

namespace MetaParser.Formatting;

public interface INavReader
{
    Task<NavRoute> ReadNavAsync(TextReader reader);
    Task ReadNavFollowAsync(TextReader reader, NavFollow follow);
    Task ReadNavNodeAsync(TextReader reader, NavNode node);
}
