using System.IO;
using System.Text;

namespace MetaParser.Tests
{
    static class Extensions
    {
        public static Stream ToStream(this string str)
        {
            var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
