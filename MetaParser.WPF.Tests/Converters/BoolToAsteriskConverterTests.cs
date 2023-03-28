using AutoFixture;
using MetaParser.WPF.Converters;
using System.Globalization;

namespace MetaParser.WPF.Tests.Converters;

[TestClass]
public class BoolToAsteriskConverterTests
{
    private readonly Fixture fixture = new();

    [TestMethod]
    public void Convert_IsBoolAndTrue_ReturnsAsterisk()
    {
        var cvt = new BoolToAsteriskConverter();

        var result = cvt.Convert(true, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.IsInstanceOfType<string>(result);
        Assert.AreEqual("*", result);
    }

    [TestMethod]
    public void Convert_IsBoolAndFalse_ReturnsEmptyString()
    {
        var cvt = new BoolToAsteriskConverter();

        var result = cvt.Convert(false, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.IsInstanceOfType<string>(result);
        Assert.AreEqual("", result);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(5)]
    [DataRow(5.0)]
    public void Convert_IsNotBool_ReturnsEmptyString(object value)
    {
        var cvt = new BoolToAsteriskConverter();
        var result = cvt.Convert(value, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.IsInstanceOfType<string>(result);
        Assert.AreEqual("", result);
    }
}
