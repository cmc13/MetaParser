using AutoFixture;
using AutoFixture.Kernel;
using MetaParser.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MetaParser.Tests;

[TestClass]
public class ActionTests
{
    private static readonly Fixture fixture = new();

    [TestMethod]
    [DataRow(ActionType.ExpressionAct, typeof(ExpressionMetaAction))]
    [DataRow(ActionType.Multiple, typeof(AllMetaAction))]
    [DataRow(ActionType.SetState, typeof(MetaAction<string>))]
    [DataRow(ActionType.ReturnFromCall, typeof(MetaAction<int>))]
    [DataRow(ActionType.SetVTOption, typeof(SetVTOptionMetaAction))]
    [DataRow(ActionType.ChatWithExpression, typeof(ExpressionMetaAction))]
    [DataRow(ActionType.CallState, typeof(CallStateMetaAction))]
    [DataRow(ActionType.ChatCommand, typeof(MetaAction<string>))]
    [DataRow(ActionType.WatchdogSet, typeof(WatchdogSetMetaAction))]
    [DataRow(ActionType.EmbeddedNavRoute, typeof(EmbeddedNavRouteMetaAction))]
    [DataRow(ActionType.None, typeof(MetaAction<int>))]
    [DataRow(ActionType.WatchdogClear, typeof(TableMetaAction))]
    [DataRow(ActionType.GetVTOption, typeof(GetVTOptionMetaAction))]
    [DataRow(ActionType.DestroyView, typeof(DestroyViewMetaAction))]
    [DataRow(ActionType.DestroyAllViews, typeof(TableMetaAction))]
    [DataRow(ActionType.CreateView, typeof(CreateViewMetaAction))]
    public void CreateMetaAction_HappyPath_CreatesExpectedActionClass(ActionType type, Type expectedClass)
    {
        var action = MetaAction.CreateMetaAction(type);

        Assert.IsNotNull(action);
        Assert.IsInstanceOfType(action, expectedClass);
    }

    [TestMethod]
    [DataRow(ActionType.SetState, "default", "Set State: default")]
    [DataRow(ActionType.ChatCommand, "asdfdasf", "Chat: asdfdasf")]
    [DataRow(ActionType.ReturnFromCall, null, "Return from Call")]
    [DataRow(ActionType.WatchdogClear, null, "Watchdog Clear")]
    [DataRow(ActionType.DestroyAllViews, null, "Destroy All Views")]
    [DataRow(ActionType.None, null, "None")]
    [DataRow(ActionType.EmbeddedNavRoute, null, "Load Embedded Nav Route")]
    public void MetaAction_ToString_ProducesCorrectStringRepresentation(ActionType type, object data, string expectedResult)
    {

        var action = MetaAction.CreateMetaAction(type);
        if (data != null)
        {
            var dataProp = action.GetType().GetProperty("Data");
            dataProp.SetValue(action, data);
        }

        Assert.AreEqual(expectedResult, action.ToString());
    }

    [TestMethod]
    public void DestroyViewMetaAction_ToString_ProducesCorrectStringRepresentation()
    {
        var viewName = fixture.Create<string>();
        var act = MetaAction.CreateMetaAction(ActionType.DestroyView) as DestroyViewMetaAction;
        act.ViewName = viewName;

        Assert.AreEqual($"Destroy View: {viewName}", act.ToString());
    }

    [TestMethod]
    public void CreateViewMetaAction_ToString_ProducesCorrectStringRepresentation()
    {
        var viewName = fixture.Create<string>();
        var act = MetaAction.CreateMetaAction(ActionType.CreateView) as CreateViewMetaAction;
        act.ViewName = viewName;

        Assert.AreEqual($"Create View: {viewName}", act.ToString());
    }

    [TestMethod]
    public void ExpressionMetaAction_ToString_ProducesCorrectStringRepresentation()
    {
        var expr = fixture.Create<string>();
        var act = MetaAction.CreateMetaAction(ActionType.ExpressionAct) as ExpressionMetaAction;
        act.Expression = expr;

        Assert.AreEqual($"Expr: {expr}", act.ToString());
    }

    [TestMethod]
    public void ChatExprMetaAction_ToString_ProducesCorrectStringRepresentation()
    {
        var expr = fixture.Create<string>();
        var act = MetaAction.CreateMetaAction(ActionType.ChatWithExpression) as ExpressionMetaAction;
        act.Expression = expr;

        Assert.AreEqual($"Chat Expr: {expr}", act.ToString());
    }

    [TestMethod]
    public void SetVTOptionMetaAction_ToString_ProducesCorrectStringRepresentation()
    {
        var opt = fixture.Create<string>();
        var value = fixture.Create<string>();
        var act = MetaAction.CreateMetaAction(ActionType.SetVTOption) as SetVTOptionMetaAction;
        act.Option = opt;
        act.Value = value;

        Assert.AreEqual($"Set {opt} => {value}", act.ToString());
    }

    [TestMethod]
    public void GetVTOptionMetaAction_ToString_ProducesCorrectStringRepresentation()
    {
        var opt = fixture.Create<string>();
        var var = fixture.Create<string>();
        var act = MetaAction.CreateMetaAction(ActionType.GetVTOption) as GetVTOptionMetaAction;
        act.Option = opt;
        act.Variable = var;

        Assert.AreEqual($"Get {opt} => {var}", act.ToString());
    }

    [TestMethod]
    public void CallStateMetaAction_ToString_ProducesCorrectStringRepresentation()
    {
        var call = fixture.Create<string>();
        var ret = fixture.Create<string>();
        var act = MetaAction.CreateMetaAction(ActionType.CallState) as CallStateMetaAction;
        act.CallState = call;
        act.ReturnState = ret;

        Assert.AreEqual($"Call: {call} (Ret: {ret})", act.ToString());
    }

    [TestMethod]
    public void WatchdogSetMetaAction_ToString_ProducesCorrectStringRepresentation()
    {
        var state = fixture.Create<string>();
        var range = fixture.Create<double>();
        var time = fixture.Create<double>();
        var act = MetaAction.CreateMetaAction(ActionType.WatchdogSet) as WatchdogSetMetaAction;
        act.State = state;
        act.Range = range;
        act.Time = time;

        Assert.AreEqual($"Set Watchdog: {range}m {time}s => {state}", act.ToString());
    }

    [TestMethod]
    public void AllMetaAction_ToString_ProducesCorrectStringRepresentation()
    {
        var list = Enum.GetValues<ActionType>().Select(e => MetaAction.CreateMetaAction(e));
        var act = MetaAction.CreateMetaAction(ActionType.Multiple) as AllMetaAction;
        act.Data.AddRange(list);

        Assert.AreEqual($"Do All: {{ {string.Join("; ", list)} }}", act.ToString());
    }

    [TestMethod]
    public void CreateViewMetaAction_HasData_PutsDataInCorrectKeysInDictionary()
    {
        var expectedViewName = fixture.Create<string>();
        var expectedViewData = fixture.Create<string>();
        var a = MetaAction.CreateMetaAction(ActionType.CreateView) as CreateViewMetaAction;
        a.ViewName = expectedViewName;
        a.ViewDefinition = expectedViewData;

        Assert.IsTrue(a.Data.Contains("n"));
        Assert.IsTrue(a.Data.Contains("x"));
        Assert.AreEqual(expectedViewName, a.Data["n"]);
        Assert.AreEqual(expectedViewData, a.Data["x"].ToString());
    }

    [TestMethod]
    public void CreateViewMetaAction_HasDataInDictionary_ReturnsDataInViewNameAndViewData()
    {
        var expectedViewName = fixture.Create<string>();
        var expectedViewData = fixture.Create<string>();
        var a = MetaAction.CreateMetaAction(ActionType.CreateView) as CreateViewMetaAction;
        a.Data["n"] = expectedViewName;
        a.Data["x"] = (ViewString)expectedViewData;

        Assert.AreEqual(expectedViewName, a.ViewName);
        Assert.AreEqual(expectedViewData, a.ViewDefinition);
    }

    [TestMethod]
    [DataRow(ActionType.DestroyView, "n", typeof(string), nameof(DestroyViewMetaAction.ViewName))]
    [DataRow(ActionType.ExpressionAct, "e", typeof(string), nameof(ExpressionMetaAction.Expression))]
    [DataRow(ActionType.SetVTOption, "o", typeof(string), nameof(SetVTOptionMetaAction.Option))]
    [DataRow(ActionType.SetVTOption, "v", typeof(string), nameof(SetVTOptionMetaAction.Value))]
    [DataRow(ActionType.GetVTOption, "o", typeof(string), nameof(GetVTOptionMetaAction.Option))]
    [DataRow(ActionType.GetVTOption, "v", typeof(string), nameof(GetVTOptionMetaAction.Variable))]
    [DataRow(ActionType.CallState, "st", typeof(string), nameof(CallStateMetaAction.CallState))]
    [DataRow(ActionType.CallState, "ret", typeof(string), nameof(CallStateMetaAction.ReturnState))]
    [DataRow(ActionType.WatchdogSet, "s", typeof(string), nameof(WatchdogSetMetaAction.State))]
    [DataRow(ActionType.WatchdogSet, "r", typeof(double), nameof(WatchdogSetMetaAction.Range))]
    [DataRow(ActionType.WatchdogSet, "t", typeof(double), nameof(WatchdogSetMetaAction.Time))]
    public void TableMetaAction_HasData_PutsDataInCorrectKeyInDictionary(ActionType type, string key, Type valueType, string prop)
    {
        var m = typeof(SpecimenFactory).GetMethod(nameof(SpecimenFactory.Create), 1, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new[] { typeof(ISpecimenBuilder) }, null);
        m = m.MakeGenericMethod(new[] { valueType });
        var value = m.Invoke(null, new[] { fixture });
        var a = MetaAction.CreateMetaAction(type) as TableMetaAction;
        a.GetType().GetProperty(prop).SetValue(a, value);

        Assert.IsTrue(a.Data.Contains(key));
        Assert.AreEqual(value, a.Data[key]);
    }

    [TestMethod]
    [DataRow(ActionType.DestroyView, "n", typeof(string), nameof(DestroyViewMetaAction.ViewName))]
    [DataRow(ActionType.ExpressionAct, "e", typeof(string), nameof(ExpressionMetaAction.Expression))]
    [DataRow(ActionType.SetVTOption, "o", typeof(string), nameof(SetVTOptionMetaAction.Option))]
    [DataRow(ActionType.SetVTOption, "v", typeof(string), nameof(SetVTOptionMetaAction.Value))]
    [DataRow(ActionType.GetVTOption, "o", typeof(string), nameof(GetVTOptionMetaAction.Option))]
    [DataRow(ActionType.GetVTOption, "v", typeof(string), nameof(GetVTOptionMetaAction.Variable))]
    [DataRow(ActionType.CallState, "st", typeof(string), nameof(CallStateMetaAction.CallState))]
    [DataRow(ActionType.CallState, "ret", typeof(string), nameof(CallStateMetaAction.ReturnState))]
    [DataRow(ActionType.WatchdogSet, "s", typeof(string), nameof(WatchdogSetMetaAction.State))]
    [DataRow(ActionType.WatchdogSet, "r", typeof(double), nameof(WatchdogSetMetaAction.Range))]
    [DataRow(ActionType.WatchdogSet, "t", typeof(double), nameof(WatchdogSetMetaAction.Time))]
    public void TableMetaAction_HasDataInDictionary_ReturnsDataInProperty(ActionType type, string key, Type valueType, string prop)
    {
        var m = typeof(SpecimenFactory).GetMethod(nameof(SpecimenFactory.Create), 1, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new[] { typeof(ISpecimenBuilder) }, null);
        m = m.MakeGenericMethod(new[] { valueType });
        var value = m.Invoke(null, new[] { fixture });
        var a = MetaAction.CreateMetaAction(type) as TableMetaAction;
        a.Data[key] = value;

        Assert.AreEqual(value, a.GetType().GetProperty(prop).GetValue(a));
    }
}
