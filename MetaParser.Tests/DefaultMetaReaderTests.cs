using AutoFixture;
using MetaParser.Formatting;
using MetaParser.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaParser.Tests
{
    [TestClass]
    public class DefaultMetaReaderTests
    {
        private readonly Fixture fixture = new();

        private static StringBuilder DEFAULT_HEADER => new StringBuilder()
            .AppendLine("1")
            .AppendLine("CondAct")
            .AppendLine("5")
            .AppendLine("CType")
            .AppendLine("AType")
            .AppendLine("CData")
            .AppendLine("AData")
            .AppendLine("State")
            .AppendLine("n")
            .AppendLine("n")
            .AppendLine("n")
            .AppendLine("n")
            .AppendLine("n");

        private static StringBuilder DEFAULT_TABLE_HEADER => new StringBuilder()
            .AppendLine("TABLE")
            .AppendLine("2")
            .AppendLine("k")
            .AppendLine("v")
            .AppendLine("n")
            .AppendLine("n");

        private static StringBuilder DEFAULT_LIST_HEADER => new StringBuilder()
            .AppendLine("TABLE")
            .AppendLine("2")
            .AppendLine("K")
            .AppendLine("V")
            .AppendLine("n")
            .AppendLine("n");

        [TestMethod]
        public async Task ReadMetaAsync_FileWithNoRules_ReadsSuccessfully()
        {
            var file = DEFAULT_HEADER
                .AppendLine("0")
                .ToString()
                .ToStream();

            var f = new DefaultMetaReader(new DefaultNavReader());
            var m = await f.ReadMetaAsync(file);

            Assert.IsNotNull(m);
            Assert.AreEqual(0, m.Rules.Count);
        }

        #region ReadActionAsync Tests

        [TestMethod]
        public async Task ReadActionAsync_StringAction_ReadsSuccessfully()
        {
            var expectedData = fixture.Create<string>();
            var sb = new StringBuilder()
                .AppendLine("s")
                .AppendLine(expectedData)
                .ToString();
            using var reader = new StringReader(sb);
            var action = MetaAction.CreateMetaAction(ActionType.ChatCommand) as MetaAction<string>;

            var f = new DefaultMetaReader(new DefaultNavReader());

            await f.ReadActionAsync(reader, action);
            Assert.AreEqual(expectedData, action.Data);
        }

        [TestMethod]
        public async Task ReadActionAsync_StringActionButInvalidStringDesignator_ThrowsException()
        {
            var r = new Random();
            var chars = Enumerable.Range('a', 'z' - 'a' + 1).Select(i => (char)i).Except(new[] { 's' }).ToList();
            var sb = new StringBuilder()
                .AppendLine(chars.Skip(r.Next(0, chars.Count)).First().ToString())
                .AppendLine("asdf")
                .ToString();
            using var reader = new StringReader(sb);
            var action = MetaAction.CreateMetaAction(ActionType.ChatCommand) as MetaAction<string>;

            var f = new DefaultMetaReader(new DefaultNavReader());

            await Assert.ThrowsExceptionAsync<Exception>(() => f.ReadActionAsync(reader, action));
        }

        [TestMethod]
        public async Task ReadActionAsync_IntAction_ReadsSuccessfully()
        {
            var expectedData = fixture.Create<int>();
            var sb = new StringBuilder()
                .AppendLine("i")
                .AppendLine(expectedData.ToString())
                .ToString();
            using var reader = new StringReader(sb);
            var action = MetaAction.CreateMetaAction(ActionType.ReturnFromCall) as MetaAction<int>;

            var f = new DefaultMetaReader(new DefaultNavReader());

            await f.ReadActionAsync(reader, action);
            Assert.AreEqual(expectedData, action.Data);
        }

        [TestMethod]
        public async Task ReadActionAsync_IntActionButInvalidIntDesignator_ThrowsException()
        {
            var r = new Random();
            var chars = Enumerable.Range('a', 'z' - 'a' + 1).Select(i => (char)i).Except(new[] { 'i' }).ToList();
            var sb = new StringBuilder()
                .AppendLine(chars.Skip(r.Next(0, chars.Count)).First().ToString())
                .AppendLine("0")
                .ToString();
            using var reader = new StringReader(sb);
            var action = MetaAction.CreateMetaAction(ActionType.ReturnFromCall) as MetaAction<int>;

            var f = new DefaultMetaReader(new DefaultNavReader());

            await Assert.ThrowsExceptionAsync<Exception>(() => f.ReadActionAsync(reader, action));
        }

        [TestMethod]
        public async Task ReadActionAsync_IntActionButInvalidIntValue_ThrowsException()
        {
            var sb = new StringBuilder()
                .AppendLine("i")
                .AppendLine("asdf")
                .ToString();
            using var reader = new StringReader(sb);
            var action = MetaAction.CreateMetaAction(ActionType.ReturnFromCall) as MetaAction<int>;

            var f = new DefaultMetaReader(new DefaultNavReader());

            await Assert.ThrowsExceptionAsync<Exception>(() => f.ReadActionAsync(reader, action));
        }

        [TestMethod]
        public async Task ReadActionAsync_TableAction_ReadsSuccessfully()
        {
            var sb = DEFAULT_TABLE_HEADER
                .AppendLine("0")
                .ToString();
            using var reader = new StringReader(sb);
            var action = MetaAction.CreateMetaAction(ActionType.DestroyAllViews) as TableMetaAction;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await f.ReadActionAsync(reader, action);

            Assert.AreEqual(0, action.Data.Count);
        }

        [TestMethod]
        public async Task ReadActionAsync_TableActionWithNonEmptyTable_ReadsSuccessfully()
        {
            object GenerateValue(string type) => type switch
            {
                "s" => fixture.Create<string>(),
                "i" => fixture.Create<int>(),
                "d" => fixture.Create<double>(),
                _ => null
            };

            var r = new Random();
            var d = fixture.CreateMany<string>().ToDictionary(s => s, s =>
            {
                var type = "sid"[r.Next(0, 3)].ToString();
                return (type, GenerateValue(type));
            });
            var sb = DEFAULT_TABLE_HEADER
                .AppendLine(d.Count.ToString());

            foreach (var kv in d)
            {
                sb.AppendLine("s").AppendLine(kv.Key);
                sb.AppendLine(kv.Value.type).AppendLine(kv.Value.Item2.ToString());
            }

            using var reader = new StringReader(sb.ToString());
            var action = MetaAction.CreateMetaAction(ActionType.DestroyAllViews) as TableMetaAction;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await f.ReadActionAsync(reader, action);

            Assert.AreEqual(d.Count, action.Data.Count);

            foreach (var kv in d)
                Assert.AreEqual(kv.Value.Item2, action.Data[kv.Key]);
        }

        [TestMethod]
        public async Task ReadActionAsync_TableActionWithNonEmptyTableButInvalidKeyDesignator_ThrowsMetaParserException()
        {
            object GenerateValue(string type) => type switch
            {
                "s" => fixture.Create<string>(),
                "i" => fixture.Create<int>(),
                "d" => fixture.Create<double>(),
                _ => null
            };

            var r = new Random();
            var d = fixture.CreateMany<string>().ToDictionary(s => s, s =>
            {
                var type = "sid"[r.Next(0, 3)].ToString();
                return (type, GenerateValue(type));
            });
            var sb = DEFAULT_TABLE_HEADER
                .AppendLine(d.Count.ToString());

            foreach (var kv in d)
            {
                sb.AppendLine("r").AppendLine(kv.Key);
                sb.AppendLine(kv.Value.type).AppendLine(kv.Value.Item2.ToString());
            }

            using var reader = new StringReader(sb.ToString());
            var action = MetaAction.CreateMetaAction(ActionType.DestroyAllViews) as TableMetaAction;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await Assert.ThrowsExceptionAsync<MetaParserException>(() => f.ReadActionAsync(reader, action));
        }

        [TestMethod]
        public async Task ReadActionAsync_TableActionWithNonEmptyTableButInvalidValueDesignator_ThrowsMetaParserException()
        {
            object GenerateValue(string type) => type switch
            {
                "q" => fixture.Create<string>(),
                "w" => fixture.Create<int>(),
                "e" => fixture.Create<double>(),
                _ => null
            };

            var r = new Random();
            var d = fixture.CreateMany<string>().ToDictionary(s => s, s =>
            {
                var type = "qwe"[r.Next(0, 3)].ToString();
                return (type, GenerateValue(type));
            });
            var sb = DEFAULT_TABLE_HEADER
                .AppendLine(d.Count.ToString());

            foreach (var kv in d)
            {
                sb.AppendLine("s").AppendLine(kv.Key);
                sb.AppendLine(kv.Value.type).AppendLine(kv.Value.Item2.ToString());
            }

            using var reader = new StringReader(sb.ToString());
            var action = MetaAction.CreateMetaAction(ActionType.DestroyAllViews) as TableMetaAction;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await Assert.ThrowsExceptionAsync<MetaParserException>(() => f.ReadActionAsync(reader, action));
        }

        [TestMethod]
        public async Task ReadActionAsync_TableActionWithNonEmptyTableButInvalidNumericTypes_ThrowsMetaParserException()
        {
            object GenerateValue(string type) => type switch
            {
                "s" => fixture.Create<string>(),
                "i" => fixture.Create<string>(),
                "d" => fixture.Create<string>(),
                _ => null
            };

            var r = new Random();
            var d = fixture.CreateMany<string>().ToDictionary(s => s, s =>
            {
                var type = "sid"[r.Next(0, 3)].ToString();
                return (type, GenerateValue(type));
            });
            while (!d.Any(kv => kv.Value.type != "s"))
            {
                d = fixture.CreateMany<string>().ToDictionary(s => s, s =>
                {
                    var type = "sid"[r.Next(0, 3)].ToString();
                    return (type, GenerateValue(type));
                });
            }

            var sb = DEFAULT_TABLE_HEADER
                .AppendLine(d.Count.ToString());

            foreach (var kv in d)
            {
                sb.AppendLine("s").AppendLine(kv.Key);
                sb.AppendLine(kv.Value.type).AppendLine(kv.Value.Item2.ToString());
            }

            using var reader = new StringReader(sb.ToString());
            var action = MetaAction.CreateMetaAction(ActionType.DestroyAllViews) as TableMetaAction;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await Assert.ThrowsExceptionAsync<MetaParserException>(() => f.ReadActionAsync(reader, action));
        }

        [TestMethod]
        public async Task ReadActionAsync_AllMetaActionWithEmptyConditionList_ReadsSuccessfully()
        {
            var sb = DEFAULT_LIST_HEADER
                .AppendLine("0");
            using var reader = new StringReader(sb.ToString());
            var action = MetaAction.CreateMetaAction(ActionType.Multiple) as AllMetaAction;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await f.ReadActionAsync(reader, action);

            Assert.AreEqual(0, action.Data.Count);
        }

        [TestMethod]
        public async Task ReadActionAsync_AllMetaActionWithNonEmptyConditionList_ReadsSuccessfully()
        {
            var count = fixture.Create<int>();
            var sb = DEFAULT_LIST_HEADER
                .AppendLine(count.ToString());

            for (var i = 0; i < count; i++)
            {
                sb.AppendLine("i").AppendLine(((int)ActionType.None).ToString());
                sb.AppendLine("i").AppendLine("0");
            }

            using var reader = new StringReader(sb.ToString());
            var action = MetaAction.CreateMetaAction(ActionType.Multiple) as AllMetaAction;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await f.ReadActionAsync(reader, action);

            Assert.AreEqual(count, action.Data.Count);
            foreach (var act in action.Data)
            {
                Assert.IsInstanceOfType(act, typeof(MetaAction<int>));
            }
        }

        [TestMethod]
        public async Task ReadActionAsync_AllMetaActionWithInvalidActionTypeDesignator_ReadsSuccessfully()
        {
            var count = fixture.Create<int>();
            var sb = DEFAULT_LIST_HEADER
                .AppendLine(count.ToString());

            for (var i = 0; i < count; i++)
            {
                sb.AppendLine("j").AppendLine(((int)ActionType.None).ToString());
                sb.AppendLine("i").AppendLine("0");
            }

            using var reader = new StringReader(sb.ToString());
            var action = MetaAction.CreateMetaAction(ActionType.Multiple) as AllMetaAction;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await Assert.ThrowsExceptionAsync<Exception>(() => f.ReadActionAsync(reader, action));
        }

        [TestMethod]
        public async Task ReadActionAsync_EmbeddedNavRouteMetaActionWithEmptyNavRoute_ReadsSuccessfully()
        {
            var sb = new StringBuilder()
                .AppendLine("ba")
                .AppendLine("5")
                .AppendLine("[None]")
                .AppendLine("0");

            using var reader = new StringReader(sb.ToString());
            var action = MetaAction.CreateMetaAction(ActionType.EmbeddedNavRoute) as EmbeddedNavRouteMetaAction;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await f.ReadActionAsync(reader, action);

            Assert.IsNull(action.Data.name);
        }

        [TestMethod]
        public async Task ReadActionAsync_EmbeddedNavRouteMetaActionWithName_NavIsNamedCorrectly()
        {
            var expectedName = fixture.Create<string>();
            var sb = new StringBuilder()
                .AppendLine("ba")
                .AppendLine("5")
                .AppendLine(expectedName)
                .AppendLine("0");

            using var reader = new StringReader(sb.ToString());
            var action = MetaAction.CreateMetaAction(ActionType.EmbeddedNavRoute) as EmbeddedNavRouteMetaAction;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await f.ReadActionAsync(reader, action);

            Assert.AreEqual(expectedName, action.Data.name);
        }

        [TestMethod]
        public async Task ReadActionAsync_EmbeddedNavRouteMetaActionWithEmptyNav_ReadsNavCorrectly()
        {
            var sb = new StringBuilder()
                .AppendLine("ba")
                .AppendLine("6")
                .AppendLine("[None]")
                .AppendLine("0")
                .AppendLine("uTank2 NAV 1.2")
                .AppendLine("1")
                .AppendLine("0");

            using var reader = new StringReader(sb.ToString());
            var action = MetaAction.CreateMetaAction(ActionType.EmbeddedNavRoute) as EmbeddedNavRouteMetaAction;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await f.ReadActionAsync(reader, action);

            Assert.IsInstanceOfType(action.Data.nav.Data, typeof(List<NavNode>));
            var list = action.Data.nav.Data as List<NavNode>;
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public async Task ReadActionAsync_EmbeddedNavRouteMetaActionWithFollowNav_ReadsNavCorrectly()
        {
            var expectedName = fixture.Create<string>();
            var expectedId = fixture.Create<int>();
            var sb = new StringBuilder()
                .AppendLine("ba")
                .AppendLine("6")
                .AppendLine("[None]")
                .AppendLine("0")
                .AppendLine("uTank2 NAV 1.2")
                .AppendLine("3")
                .AppendLine(expectedName)
                .AppendLine(expectedId.ToString());

            using var reader = new StringReader(sb.ToString());
            var action = MetaAction.CreateMetaAction(ActionType.EmbeddedNavRoute) as EmbeddedNavRouteMetaAction;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await f.ReadActionAsync(reader, action);

            Assert.IsInstanceOfType(action.Data.nav.Data, typeof(NavFollow));
            var follow = action.Data.nav.Data as NavFollow;
            Assert.AreEqual(expectedName, follow.TargetName);
            Assert.AreEqual(expectedId, follow.TargetId);
        }

        #endregion

        #region ReadConditionAsync Tests

        [TestMethod]
        public async Task ReadConditionAsync_ConditionInt_ReadsSuccessfully()
        {
            var expectedData = fixture.Create<int>();
            var sb = new StringBuilder()
                .AppendLine("i")
                .AppendLine(expectedData.ToString());

            using var reader = new StringReader(sb.ToString());
            var condition = Condition.CreateCondition(ConditionType.Never) as Condition<int>;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await f.ReadConditionAsync(reader, condition);

            Assert.AreEqual(expectedData, condition.Data);
        }

        [TestMethod]
        public async Task ReadConditionAsync_ConditionIntWithInvalidIntDesignator_Throws()
        {
            var expectedData = fixture.Create<int>();
            var sb = new StringBuilder()
                .AppendLine("j")
                .AppendLine(expectedData.ToString());

            using var reader = new StringReader(sb.ToString());
            var condition = Condition.CreateCondition(ConditionType.Never) as Condition<int>;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await Assert.ThrowsExceptionAsync<MetaParserException>(() => f.ReadConditionAsync(reader, condition));
        }

        [TestMethod]
        public async Task ReadConditionAsync_ConditionIntWithInvalidIntValue_Throws()
        {
            var sb = new StringBuilder()
                .AppendLine("i")
                .AppendLine(fixture.Create<string>());

            using var reader = new StringReader(sb.ToString());
            var condition = Condition.CreateCondition(ConditionType.Never) as Condition<int>;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await Assert.ThrowsExceptionAsync<MetaParserException>(() => f.ReadConditionAsync(reader, condition));
        }

        [TestMethod]
        public async Task ReadConditionAsync_ConditionString_ReadsSuccessfully()
        {
            var expectedData = fixture.Create<string>();
            var sb = new StringBuilder()
                .AppendLine("s")
                .AppendLine(expectedData.ToString());

            using var reader = new StringReader(sb.ToString());
            var condition = Condition.CreateCondition(ConditionType.ChatMessage) as Condition<string>;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await f.ReadConditionAsync(reader, condition);

            Assert.AreEqual(expectedData, condition.Data);
        }

        [TestMethod]
        public async Task ReadConditionAsync_ConditionStringWithInvalidStringDesignator_Throws()
        {
            var sb = new StringBuilder()
                .AppendLine("b")
                .AppendLine(fixture.Create<string>());

            using var reader = new StringReader(sb.ToString());
            var condition = Condition.CreateCondition(ConditionType.Never) as Condition<int>;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await Assert.ThrowsExceptionAsync<MetaParserException>(() => f.ReadConditionAsync(reader, condition));
        }

        [TestMethod]
        public async Task ReadConditionAsync_NotCondition_ReadsSuccessfully()
        {
            var sb = DEFAULT_LIST_HEADER
                .AppendLine("1")
                .AppendLine("i")
                .AppendLine("0")
                .AppendLine("i")
                .AppendLine("0");

            using var reader = new StringReader(sb.ToString());
            var condition = Condition.CreateCondition(ConditionType.Not) as NotCondition;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await f.ReadConditionAsync(reader, condition);

            Assert.IsNotNull(condition.Data);
            Assert.IsInstanceOfType(condition.Data, typeof(Condition<int>));
            Assert.AreEqual(ConditionType.Never, condition.Data.Type);
        }

        [TestMethod]
        public async Task ReadConditionAsync_NotConditionWithMultipleConditionsInConditionList_Throws()
        {
            var sb = DEFAULT_LIST_HEADER
                .AppendLine("3")
                .AppendLine("i")
                .AppendLine("0")
                .AppendLine("i")
                .AppendLine("0")
                .AppendLine("i")
                .AppendLine("0")
                .AppendLine("i")
                .AppendLine("0")
                .AppendLine("i")
                .AppendLine("0")
                .AppendLine("i")
                .AppendLine("0");

            using var reader = new StringReader(sb.ToString());
            var condition = Condition.CreateCondition(ConditionType.Not) as NotCondition;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await Assert.ThrowsExceptionAsync<MetaParserException>(() => f.ReadConditionAsync(reader, condition));
        }

        [TestMethod]
        public async Task ReadConditionAsync_NotConditionWithInvalidConditionCount_Throws()
        {
            var sb = DEFAULT_LIST_HEADER
                .AppendLine("asdf")
                .AppendLine("i")
                .AppendLine("0")
                .AppendLine("i")
                .AppendLine("0")
                .AppendLine("i")
                .AppendLine("0")
                .AppendLine("i")
                .AppendLine("0")
                .AppendLine("i")
                .AppendLine("0")
                .AppendLine("i")
                .AppendLine("0");

            using var reader = new StringReader(sb.ToString());
            var condition = Condition.CreateCondition(ConditionType.Not) as NotCondition;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await Assert.ThrowsExceptionAsync<MetaParserException>(() => f.ReadConditionAsync(reader, condition));
        }

        [TestMethod]
        public async Task ReadConditionAsync_NotConditionWithEmptyConditionList_ReadsSuccessfully()
        {
            var sb = DEFAULT_LIST_HEADER
                .AppendLine("0");

            using var reader = new StringReader(sb.ToString());
            var condition = Condition.CreateCondition(ConditionType.Not) as NotCondition;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await f.ReadConditionAsync(reader, condition);

            Assert.IsNull(condition.Data);
        }

        [TestMethod]
        public async Task ReadConditionAsync_MultipleCondition_ReadsSuccessfully()
        {
            var count = fixture.Create<int>();
            var sb = DEFAULT_LIST_HEADER
                .AppendLine(count.ToString());

            for (var i = 0; i < count; i++)
            {
                sb.AppendLine("i").AppendLine("0");
                sb.AppendLine("i").AppendLine("0");
            }

            using var reader = new StringReader(sb.ToString());
            var condition = Condition.CreateCondition(ConditionType.Any) as MultipleCondition;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await f.ReadConditionAsync(reader, condition);

            Assert.AreEqual(count, condition.Data.Count);
        }

        [TestMethod]
        public async Task ReadConditionAsync_MultipleConditionWithEmptyConditionList_ReadsSuccessfully()
        {
            var count = fixture.Create<int>();
            var sb = DEFAULT_LIST_HEADER
                .AppendLine("0");

            using var reader = new StringReader(sb.ToString());
            var condition = Condition.CreateCondition(ConditionType.Any) as MultipleCondition;

            var f = new DefaultMetaReader(new DefaultNavReader());
            await f.ReadConditionAsync(reader, condition);

            Assert.AreEqual(0, condition.Data.Count);
        }

        #endregion
    }
}
