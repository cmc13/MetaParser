using AutoFixture;
using AutoFixture.Kernel;
using MetaParser.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MetaParser.Tests;

[TestClass]
public class ConditionTests
{
    private readonly Fixture fixture = new();

    [TestMethod]
    [DataRow(ConditionType.All, typeof(MultipleCondition))]
    [DataRow(ConditionType.Never, typeof(Condition<int>))]
    [DataRow(ConditionType.Always, typeof(Condition<int>))]
    [DataRow(ConditionType.Died, typeof(Condition<int>))]
    [DataRow(ConditionType.NeedToBuff, typeof(Condition<int>))]
    [DataRow(ConditionType.NavrouteEmpty, typeof(Condition<int>))]
    [DataRow(ConditionType.VendorClosed, typeof(Condition<int>))]
    [DataRow(ConditionType.VendorOpen, typeof(Condition<int>))]
    [DataRow(ConditionType.PortalspaceEnter, typeof(Condition<int>))]
    [DataRow(ConditionType.PortalspaceExit, typeof(Condition<int>))]
    [DataRow(ConditionType.SecondsInStateGE, typeof(Condition<int>))]
    [DataRow(ConditionType.SecondsInStatePersistGE, typeof(Condition<int>))]
    [DataRow(ConditionType.MainPackSlotsLE, typeof(Condition<int>))]
    [DataRow(ConditionType.BurdenPercentGE, typeof(Condition<int>))]
    [DataRow(ConditionType.LandBlockE, typeof(Condition<int>))]
    [DataRow(ConditionType.LandCellE, typeof(Condition<int>))]
    [DataRow(ConditionType.ChatMessage, typeof(Condition<string>))]
    [DataRow(ConditionType.Expression, typeof(ExpressionCondition))]
    [DataRow(ConditionType.Not, typeof(NotCondition))]
    [DataRow(ConditionType.Any, typeof(MultipleCondition))]
    [DataRow(ConditionType.ItemCountGE, typeof(ItemCountCondition))]
    [DataRow(ConditionType.ItemCountLE, typeof(ItemCountCondition))]
    [DataRow(ConditionType.NoMonstersWithinDistance, typeof(NoMonstersInDistanceCondition))]
    [DataRow(ConditionType.ChatMessageCapture, typeof(ChatMessageCaptureCondition))]
    [DataRow(ConditionType.MonsterCountWithinDistance, typeof(MonsterCountWithinDistanceCondition))]
    [DataRow(ConditionType.MonstersWithPriorityWithinDistance, typeof(MonstersWithPriorityWithinDistanceCondition))]
    [DataRow(ConditionType.TimeLeftOnSpellGE, typeof(TimeLeftOnSpellGECondition))]
    public void Condition_CreateCondition_CreatesCorrectConditionType(ConditionType type, Type expectedConditionType)
    {
        var condition = Condition.CreateCondition(type);
        Assert.IsInstanceOfType(condition, expectedConditionType);
        Assert.AreEqual(type, condition.Type);
    }

    [TestMethod]
    public void Condition_CreateCondition_AllConditionTypesCanCreateCondition()
    {
        foreach (var conditionType in Enum.GetValues<ConditionType>())
        {
            var condition = Condition.CreateCondition(conditionType);
            Assert.IsNotNull(condition);
            Assert.AreEqual(conditionType, condition.Type);
        }
    }

    [TestMethod]
    [DataRow(ConditionType.SecondsInStateGE, 5, "Seconds in State ≥ 5")]
    [DataRow(ConditionType.BurdenPercentGE, 123, "Burden Percent ≥ 123")]
    [DataRow(ConditionType.SecondsInStatePersistGE, 999, "Seconds in State (Persistent) ≥ 999")]
    [DataRow(ConditionType.MainPackSlotsLE, 3456, "Main Pack Slots ≤ 3456")]
    [DataRow(ConditionType.LandBlockE, 12345678, "LandBlock = 00BC614E")]
    [DataRow(ConditionType.LandCellE, 98765432, "LandCell = 05E30A78")]
    [DataRow(ConditionType.ChatMessage, "asdf", "Chat Message: asdf")]
    [DataRow(ConditionType.Never, 0, "Never")]
    [DataRow(ConditionType.Always, 0, "Always")]
    [DataRow(ConditionType.Died, 0, "Character Died")]
    [DataRow(ConditionType.NeedToBuff, 0, "Need to Buff")]
    [DataRow(ConditionType.VendorClosed, 0, "Vendor Closed")]
    [DataRow(ConditionType.VendorOpen, 0, "Vendor Open")]
    [DataRow(ConditionType.PortalspaceEnter, 0, "Portalspace Enter")]
    [DataRow(ConditionType.PortalspaceExit, 0, "Portalspace Exit")]
    [DataRow(ConditionType.NavrouteEmpty, 0, "Navroute Empty")]
    public void Condition_ToString_ProducedCorrectStringRepresentation(ConditionType type, object data, string expectedToString)
    {
        var condition = Condition.CreateCondition(type);
        var dataProp = condition.GetType().GetProperty("Data");
        dataProp.SetValue(condition, data);

        Assert.AreEqual(expectedToString, condition.ToString());
    }

    [TestMethod]
    public void NotCondition_ToString_ProducedCorrectStringRepresentation()
    {
        var testCond = Condition.CreateCondition(ConditionType.MainPackSlotsLE) as Condition<int>;
        testCond.Data = fixture.Create<int>();
        var notCond = Condition.CreateCondition(ConditionType.Not) as NotCondition;
        notCond.Data = testCond;

        Assert.AreEqual($"Not {testCond}", notCond.ToString());
    }
    
    [TestMethod]
    [DataRow(ConditionType.All)]
    [DataRow(ConditionType.Any)]
    public void MultipleCondition_ToString_ProducesCorrectStringRepresentation(ConditionType multType)
    {
        var list = Enum.GetValues<ConditionType>().Select(e => Condition.CreateCondition(e));
        var multCond = Condition.CreateCondition(multType) as MultipleCondition;
        multCond.Data.AddRange(list);

        Assert.AreEqual($"{multType}: {{ {string.Join("; ", list)} }}", multCond.ToString());
    }

    [TestMethod]
    public void ExpressionCondition_ToString_ProducesCorrectStringRepresentation()
    {
        var expr = fixture.Create<string>();
        var cond = Condition.CreateCondition(ConditionType.Expression) as ExpressionCondition;
        cond.Expression = expr;

        Assert.AreEqual($"Expr: {expr}", cond.ToString());
    }

    [TestMethod]
    public void DistanceToAnyRoutePointGECondition_ToString_ProducedCorrectStringRepresentation()
    {
        var dist = fixture.Create<double>();
        var cond = Condition.CreateCondition(ConditionType.DistanceToAnyRoutePointGE) as DistanceToAnyRoutePointGECondition;
        cond.Distance = dist;

        Assert.AreEqual($"Distance to Any Route Pt ≥ {dist}", cond.ToString());
    }

    [TestMethod]
    public void NoMonstersInDistanceCondition_ToString_ProducedCorrectStringRepresentation()
    {
        var dist = fixture.Create<double>();
        var cond = Condition.CreateCondition(ConditionType.NoMonstersWithinDistance) as NoMonstersInDistanceCondition;
        cond.Distance = dist;

        Assert.AreEqual($"No Monsters Within {dist}m", cond.ToString());
    }

    [TestMethod]
    public void MonsterCountWithinDistanceCondition_ToString_ProducesCorrectStringRepresentation()
    {
        var dist = fixture.Create<double>();
        var count = fixture.Create<int>();
        var rx = fixture.Create<string>();
        var cond = Condition.CreateCondition(ConditionType.MonsterCountWithinDistance) as MonsterCountWithinDistanceCondition;
        cond.Distance = dist;
        cond.Count = count;
        cond.MonsterNameRx = rx;

        Assert.AreEqual($"{count} monsters matching '{rx}' within {dist}m", cond.ToString());
    }

    [TestMethod]
    public void MonstersWithPriorityWithinDistanceCondition_ToString_ProducesCorrectStringRepresentation()
    {
        var dist = fixture.Create<double>();
        var count = fixture.Create<int>();
        var priority = fixture.Create<int>();
        var cond = Condition.CreateCondition(ConditionType.MonstersWithPriorityWithinDistance) as MonstersWithPriorityWithinDistanceCondition;
        cond.Distance = dist;
        cond.Count = count;
        cond.Priority = priority;

        Assert.AreEqual($"{count} monsters with {priority} priority within {dist}m", cond.ToString());
    }

    [TestMethod]
    [DataRow(ConditionType.ItemCountGE)]
    [DataRow(ConditionType.ItemCountLE)]
    public void ItemCountCondition_ToString_ProducesCorrectStringRepresentation(ConditionType type)
    {
        var item = fixture.Create<string>();
        var count = fixture.Create<int>();
        var cond = Condition.CreateCondition(type) as ItemCountCondition;
        cond.ItemName = item;
        cond.Count = count;

        Assert.AreEqual($"{item} Count {(type == ConditionType.ItemCountGE ? "≥" : "≤")} {count}", cond.ToString());
    }

    [TestMethod]
    [DataRow("", "Any")]
    [DataRow((string)null, "Any")]
    [DataRow("Red", "Red")]
    [DataRow("1", "1")]
    [DataRow("     ", "     ")]
    public void ChatMessageCaptureCondition_ToString_ProducesCorrectStringRepresentation(string color, string expected)
    {
        var pattern = fixture.Create<string>();
        var cond = Condition.CreateCondition(ConditionType.ChatMessageCapture) as ChatMessageCaptureCondition;
        cond.Pattern = pattern;
        cond.Color = color;

        Assert.AreEqual($"Chat Message matching '{pattern}' ({expected})", cond.ToString());
    }

    [TestMethod]
    [DataRow(1, "Strength Other I")]
    [DataRow(10, "10")]
    public void TimeLeftOnSpellGECondition_ToString_ProducesCorrectStringRepresentation(int spellId, string spellName)
    {
        var time = fixture.Create<int>();
        var cond = Condition.CreateCondition(ConditionType.TimeLeftOnSpellGE) as TimeLeftOnSpellGECondition;
        cond.Seconds = time;
        cond.SpellId = spellId;

        Assert.AreEqual($"{time}s left on spell ({spellName})", cond.ToString());
    }

    [TestMethod]
    [DataRow(ConditionType.Expression, "e", typeof(string), nameof(ExpressionCondition.Expression))]
    [DataRow(ConditionType.DistanceToAnyRoutePointGE, "dist", typeof(double), nameof(DistanceToAnyRoutePointGECondition.Distance))]
    [DataRow(ConditionType.NoMonstersWithinDistance, "r", typeof(double), nameof(NoMonstersInDistanceCondition.Distance))]
    [DataRow(ConditionType.MonsterCountWithinDistance, "n", typeof(string), nameof(MonsterCountWithinDistanceCondition.MonsterNameRx))]
    [DataRow(ConditionType.MonsterCountWithinDistance, "c", typeof(int), nameof(MonsterCountWithinDistanceCondition.Count))]
    [DataRow(ConditionType.MonsterCountWithinDistance, "r", typeof(double), nameof(MonsterCountWithinDistanceCondition.Distance))]
    [DataRow(ConditionType.MonstersWithPriorityWithinDistance, "p", typeof(int), nameof(MonstersWithPriorityWithinDistanceCondition.Priority))]
    [DataRow(ConditionType.MonstersWithPriorityWithinDistance, "c", typeof(int), nameof(MonstersWithPriorityWithinDistanceCondition.Count))]
    [DataRow(ConditionType.MonstersWithPriorityWithinDistance, "r", typeof(double), nameof(MonstersWithPriorityWithinDistanceCondition.Distance))]
    [DataRow(ConditionType.ItemCountGE, "n", typeof(string), nameof(ItemCountCondition.ItemName))]
    [DataRow(ConditionType.ItemCountGE, "c", typeof(int), nameof(ItemCountCondition.Count))]
    [DataRow(ConditionType.ChatMessageCapture, "p", typeof(string), nameof(ChatMessageCaptureCondition.Pattern))]
    [DataRow(ConditionType.ChatMessageCapture, "c", typeof(string), nameof(ChatMessageCaptureCondition.Color))]
    [DataRow(ConditionType.TimeLeftOnSpellGE, "sid", typeof(int), nameof(TimeLeftOnSpellGECondition.SpellId))]
    [DataRow(ConditionType.TimeLeftOnSpellGE, "sec", typeof(int), nameof(TimeLeftOnSpellGECondition.Seconds))]
    public void TableCondition_HasData_PutsDataInCorrectKeyInDictionary(ConditionType type, string key, Type valueType, string prop)
    {
        var m = typeof(SpecimenFactory).GetMethod(nameof(SpecimenFactory.Create), 1, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new[] { typeof(ISpecimenBuilder) }, null);
        m = m.MakeGenericMethod(new[] { valueType });
        var value = m.Invoke(null, new[] { fixture });
        var a = Condition.CreateCondition(type) as TableCondition;
        a.GetType().GetProperty(prop).SetValue(a, value);

        Assert.IsTrue(a.Data.Contains(key));
        Assert.AreEqual(value, a.Data[key]);
    }

    [TestMethod]
    [DataRow(ConditionType.Expression, "e", typeof(string), nameof(ExpressionCondition.Expression))]
    [DataRow(ConditionType.DistanceToAnyRoutePointGE, "dist", typeof(double), nameof(DistanceToAnyRoutePointGECondition.Distance))]
    [DataRow(ConditionType.NoMonstersWithinDistance, "r", typeof(double), nameof(NoMonstersInDistanceCondition.Distance))]
    [DataRow(ConditionType.MonsterCountWithinDistance, "n", typeof(string), nameof(MonsterCountWithinDistanceCondition.MonsterNameRx))]
    [DataRow(ConditionType.MonsterCountWithinDistance, "c", typeof(int), nameof(MonsterCountWithinDistanceCondition.Count))]
    [DataRow(ConditionType.MonsterCountWithinDistance, "r", typeof(double), nameof(MonsterCountWithinDistanceCondition.Distance))]
    [DataRow(ConditionType.MonstersWithPriorityWithinDistance, "p", typeof(int), nameof(MonstersWithPriorityWithinDistanceCondition.Priority))]
    [DataRow(ConditionType.MonstersWithPriorityWithinDistance, "c", typeof(int), nameof(MonstersWithPriorityWithinDistanceCondition.Count))]
    [DataRow(ConditionType.MonstersWithPriorityWithinDistance, "r", typeof(double), nameof(MonstersWithPriorityWithinDistanceCondition.Distance))]
    [DataRow(ConditionType.ItemCountGE, "n", typeof(string), nameof(ItemCountCondition.ItemName))]
    [DataRow(ConditionType.ItemCountGE, "c", typeof(int), nameof(ItemCountCondition.Count))]
    [DataRow(ConditionType.ChatMessageCapture, "p", typeof(string), nameof(ChatMessageCaptureCondition.Pattern))]
    [DataRow(ConditionType.ChatMessageCapture, "c", typeof(string), nameof(ChatMessageCaptureCondition.Color))]
    [DataRow(ConditionType.TimeLeftOnSpellGE, "sid", typeof(int), nameof(TimeLeftOnSpellGECondition.SpellId))]
    [DataRow(ConditionType.TimeLeftOnSpellGE, "sec", typeof(int), nameof(TimeLeftOnSpellGECondition.Seconds))]
    public void TableCondition_HasDataInDictionary_ReturnsDataFromProperty(ConditionType type, string key, Type valueType, string prop)
    {
        var m = typeof(SpecimenFactory).GetMethod(nameof(SpecimenFactory.Create), 1, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new[] { typeof(ISpecimenBuilder) }, null);
        m = m.MakeGenericMethod(new[] { valueType });
        var value = m.Invoke(null, new[] { fixture });
        var a = Condition.CreateCondition(type) as TableCondition;
        a.Data[key] = value;

        Assert.AreEqual(value, a.GetType().GetProperty(prop).GetValue(a, null));
    }
}
