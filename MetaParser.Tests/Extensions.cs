using System.IO;
using System.Text;

namespace MetaParser.Tests;

static class Extensions
{
    public static Stream ToStream(this string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        var stream = new MemoryStream();
        stream.Write(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }
}
