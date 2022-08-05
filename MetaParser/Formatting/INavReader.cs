using MetaParser.Models;
using System.IO;
using System.Threading.Tasks;

namespace MetaParser.Formatting
{
    public interface INavReader
    {
        Task ReadNavAsync(TextReader reader, NavRoute route);
        Task ReadNavFollowAsync(TextReader reader, NavFollow follow);
        Task ReadNavNodeAsync(TextReader reader, NavNode node);
    }
}
