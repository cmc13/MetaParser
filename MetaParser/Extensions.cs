using System.Collections.Generic;
using System.Threading.Tasks;

namespace MetaParser;

internal static class Extensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> e)
    {
        var list = new List<T>();

        await foreach (var item in e)
            list.Add(item);

        return list;
    }
}