using MetaParser.Models;
using MetaParser.WPF.Converters;
using System.Globalization;

namespace MetaParser.WPF.Tests.Converters;

[TestClass]
public class ConditionTypeToMinimumValueConverterTests
{
    [TestMethod]
    [DataRow(ConditionType.MainPackSlotsLE, 0)]
    [DataRow(ConditionType.SecondsInStateGE, 0)]
    [DataRow(ConditionType.ItemCountLE, 0)]
    [DataRow(ConditionType.ItemCountGE, 0)]
    [DataRow(ConditionType.LandBlockE, int.MinValue)]
    [DataRow(ConditionType.LandCellE, int.MinValue)]
    [DataRow(ConditionType.SecondsInStatePersistGE, 0)]
    [DataRow(ConditionType.TimeLeftOnSpellGE, 0)]
    [DataRow(ConditionType.BurdenPercentGE, 0)]
    [DataRow(ConditionType.DistanceToAnyRoutePointGE, 0)]
    public void Convert_HappyPath_ReturnsExpectedValue(ConditionType type, int expectedValue)
    {
        var cvt = new ConditionTypeToMinimumValueConverter();

        var result = cvt.Convert(type, typeof(int), null, CultureInfo.InvariantCulture);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<int>(result);
        Assert.AreEqual(expectedValue, (int)result);
    }
}
