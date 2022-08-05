using MetaParser.Formatting;
using MetaParser.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MetaParser.Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var fs = File.OpenRead(args[0]);
            var f = new DefaultMetaFormatter();
            var m = await f.ReadMetaAsync(fs).ConfigureAwait(false);

            //using var writer = new StringWriter();
            //await m.metadata["ChangeLocation"][2].Condition.WriteAsync(writer);
            //var str = writer.ToString();
            //Console.WriteLine(writer.ToString());

            using var outFs = File.OpenWrite("tmp.del");
            await f.WriteMetaAsync(outFs, m).ConfigureAwait(false);

            Console.ReadKey();
        }
    }
}
