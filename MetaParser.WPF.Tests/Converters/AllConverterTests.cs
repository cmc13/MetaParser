using AutoFixture;
using MetaParser.WPF.Converters;
using System.Globalization;

namespace MetaParser.WPF.Tests.Converters;

[TestClass]
public class AllConverterTests
{
    private Fixture fixture = new();

    [TestMethod]
    public void Convert_AllValuesAreEqual_ReturnsTrue()
    {
        var value = fixture.Create<Guid>();
        var values = new object[100];
        Array.Fill(values, value);

        var cvt = new AllConverter();

        var result = cvt.Convert(values, typeof(Guid), value, CultureInfo.InvariantCulture);

        Assert.IsInstanceOfType<bool>(result);
        Assert.IsTrue((bool)result);
    }

    [TestMethod]
    public void Convert_NotAllValuesAreEqual_ReturnsFalse()
    {
        var value = fixture.Create<Guid>();
        var values = fixture.CreateMany<Guid>(100).Cast<object>().ToArray();

        var cvt = new AllConverter();

        var result = cvt.Convert(values.ToArray(), typeof(Guid), value, CultureInfo.InvariantCulture);

        Assert.IsInstanceOfType<bool>(result);
        Assert.IsFalse((bool)result);
    }
}
