using MetaParser.Models;
using System.IO;
using System.Threading.Tasks;

namespace MetaParser.Formatting
{
    public interface INavWriter
    {
        Task WriteNavAsync(TextWriter writer, NavRoute route);
        Task WriteNavFollowAsync(TextWriter writer, NavFollow follow);
        Task WriteNavNodeAsync(TextWriter writer, NavNode node);
    }
}