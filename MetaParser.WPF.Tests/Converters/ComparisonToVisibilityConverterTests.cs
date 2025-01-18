using MetaParser.WPF.Converters;
using System.Globalization;
using System.Text;
using System.Windows;

namespace MetaParser.WPF.Tests.Converters;

[TestClass]
public class ComparisonToVisibilityConverterTests
{
    [TestMethod]
    [DataRow(1.0, ">0", Visibility.Visible)]
    [DataRow(0.0, ">0", Visibility.Collapsed)]
    [DataRow(-1.0, "<0", Visibility.Visible)]
    [DataRow(3.0, "<0", Visibility.Collapsed)]
    [DataRow(10.0, ">=5", Visibility.Visible)]
    [DataRow(4.0, ">=5", Visibility.Collapsed)]
    [DataRow(2.0, "<=5", Visibility.Visible)]
    [DataRow(6.0, "<=5", Visibility.Collapsed)]
    [DataRow(5.0, "=5", Visibility.Visible)]
    [DataRow(9.0, "=5", Visibility.Collapsed)]
    [DataRow(7.0, "!=5", Visibility.Visible)]
    [DataRow(5.0, "!=5", Visibility.Collapsed)]
    public void Convert_HappyPath_PerformsComparisonCorrectly(double value, string comparison, Visibility expected)
    {
        var cvt = new ComparisonToVisibilityConverter();

        var result = cvt.Convert(value, typeof(Visibility), comparison, CultureInfo.InvariantCulture);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<Visibility>(result);
        Assert.AreEqual(expected, (Visibility)result);
    }
}
