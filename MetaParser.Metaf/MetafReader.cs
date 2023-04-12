using Antlr4.Runtime;
using MetaParser.Formatting;
using MetaParser.Models;

namespace MetaParser.Metaf;

public class MetafReader
    : IMetaReader
{
    public Task<Meta> ReadMetaAsync(Stream stream)
    {
        try
        {
            var antlrStream = new AntlrInputStream(stream);
            var lexer = new metafLexer(antlrStream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new metafParser(tokens);
            var ctx = parser.prog();
            var visitor = new metafVisitor();

            return Task.FromResult((Meta)visitor.Visit(ctx));
        }
        catch (Exception e)
        {
            return Task.FromException<Meta>(e);
        }
    }
}
