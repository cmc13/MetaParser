using MetaParser.WPF.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace MetaParser.WPF.Tests.Converters;

[TestClass]
public class CompactFileConverterTests
{
    [TestMethod]
    public void Convert_IsLongPath_ConvertsCorrectly()
    {
        var longPath = @"C:\this\is\a\really\long\path\that\exceeds\forty\characters\and\will\be\shortened.jpg";
        var expected = @"C:...\shortened.jpg";

        var cvt = new CompactFilePathConverter();

        var obj = cvt.Convert(longPath, typeof(string), 20, CultureInfo.CurrentCulture);

        Assert.IsNotNull(obj);
        Assert.IsInstanceOfType<string>(obj);

        var str = obj as string;
        Assert.AreEqual(expected, str);
    }

    [TestMethod]
    public void Convert_IsShortPath_DoesntChange()
    {
        var shortPath = @"C:\short.jpg";
        var expected = shortPath;

        var cvt = new CompactFilePathConverter();

        var obj = cvt.Convert(shortPath, typeof(string), 20, CultureInfo.CurrentCulture);

        Assert.IsNotNull(obj);
        Assert.IsInstanceOfType<string>(obj);

        var str = obj as string;
        Assert.AreEqual(expected, str);
    }
}
