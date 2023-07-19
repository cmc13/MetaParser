using MetaParser.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MetaParser.Formatting;

static class TextReaderExtensions
{
    public static string ReadUntil(this TextReader reader, int ch)
    {
        var builder = new StringBuilder();

        for (var cTest = reader.Peek();
            cTest != -1 && cTest != ch;
            cTest = reader.Peek())
        {
            builder.Append((char)reader.Read());
        }

        return builder.ToString();
    }

    public static async Task<double> ReadDoubleAsync(this TextReader reader)
    {
        var line = await reader.ReadLineAsync().ConfigureAwait(false);
        if (!double.TryParse(line, out var heading))
            throw new MetaParserException("Invalid double", typeof(double), line);

        return heading;
    }

    public static async Task<int> ReadIntAsync(this TextReader reader)
    {
        var line = await reader.ReadLineAsync().ConfigureAwait(false);
        if (!int.TryParse(line, out var heading))
            throw new MetaParserException("Invalid int", typeof(int), line);

        return heading;
    }

    public static async Task<TEnum> ReadEnumAsync<TEnum>(this TextReader reader) where TEnum : Enum
    {
        var line = await reader.ReadLineAsync().ConfigureAwait(false);
        if (!int.TryParse(line, out var heading))
            throw new MetaParserException("Invalid int", typeof(int), line);
        else if (!Enum.IsDefined(typeof(TEnum), heading))
            throw new MetaParserException($"Unknown {typeof(TEnum).Name}: {heading}");

        return (TEnum)(object)heading;
    }
}
