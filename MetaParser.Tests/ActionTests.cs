using AutoFixture;
using MetaParser.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MetaParser.Tests
{
    [TestClass]
    public class ActionTests
    {
        private readonly Fixture fixture = new();

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
        [DataRow(ActionType.WatchdogClear, typeof(MetaAction<int>))]
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
    }
}
