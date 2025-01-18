using MetaParser.Models;
using MetaParser.WPF.Converters;
using System.Globalization;

namespace MetaParser.WPF.Tests.Converters;

[TestClass]
public class ConditionTypeToMaximumValueConverterTests
{

    [TestMethod]
    [DataRow(ConditionType.MainPackSlotsLE, 102)]
    [DataRow(ConditionType.SecondsInStateGE, int.MaxValue)]
    [DataRow(ConditionType.ItemCountLE, int.MaxValue)]
    [DataRow(ConditionType.ItemCountGE, int.MaxValue)]
    [DataRow(ConditionType.LandBlockE, int.MaxValue)]
    [DataRow(ConditionType.LandCellE, int.MaxValue)]
    [DataRow(ConditionType.SecondsInStatePersistGE, int.MaxValue)]
    [DataRow(ConditionType.TimeLeftOnSpellGE, 14400)]
    [DataRow(ConditionType.BurdenPercentGE, 300)]
    [DataRow(ConditionType.DistanceToAnyRoutePointGE, 10000)]
    public void Convert_HappyPath_ReturnsExpectedValue(ConditionType type, int expectedValue)
    {
        var cvt = new ConditionTypeToMaximumValueConverter();

        var result = cvt.Convert(type, typeof(int), null, CultureInfo.InvariantCulture);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<int>(result);
        Assert.AreEqual(expectedValue, (int)result);
    }
}
