using AutoFixture;
using MetaParser.Formatting;
using MetaParser.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MetaParser.Tests;

[TestClass]
public class MetafReaderTests
{
    private readonly Fixture fixture = new();

    [TestMethod]
    public async Task ReadMetaAsync_EmptyFile_ReadsEmptyMeta()
    {
        var sb = new StringBuilder();
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(0, meta.Rules.Count);
    }
    [TestMethod]
    public async Task ReadMetaAsync_FileWithComments_ReadsEmptyMeta()
    {
        var sb = new StringBuilder()
            .AppendLine()
            .AppendLine("~~ asdfdsafdsafassda")
            .AppendLine("                  ~~ asdfsafdsafas")
            .AppendLine()
            .AppendLine(" ~~ asdfdasfdass ~~ asdfsafdsafdsa ~~ asdfdsafsa")
            .AppendLine("                                ")
            .AppendLine()
            .AppendLine("~~~~~~~~~~~~~~~~~~~~")
            .AppendLine();
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(0, meta.Rules.Count);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsMeta()
    {
        var expectedState = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine($"STATE: {{{expectedState}}}")
            .AppendLine("   IF: Never")
            .AppendLine("       DO: None");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);

        Assert.IsNotNull(meta.Rules[0].Condition);
        Assert.IsNotNull(meta.Rules[0].Action);
        Assert.AreEqual(ConditionType.Never, meta.Rules[0].Condition.Type);
        Assert.AreEqual(ActionType.None, meta.Rules[0].Action.Type);
        Assert.AreEqual(expectedState, meta.Rules[0].State);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsExpressionCondition()
    {
        var expectedExpression = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine($"   IF: Expr {{{expectedExpression}}}")
            .AppendLine("       DO: None");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ConditionType.Expression, meta.Rules[0].Condition.Type);
        Assert.IsInstanceOfType<ExpressionCondition>(meta.Rules[0].Condition);
        var ec = meta.Rules[0].Condition as ExpressionCondition;

        Assert.AreEqual(expectedExpression, ec.Expression);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsNoMobsInDistCondition()
    {
        var expectedDistance = fixture.Create<double>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine($"   IF: NoMobsInDist {expectedDistance}")
            .AppendLine("       DO: None");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ConditionType.NoMonstersWithinDistance, meta.Rules[0].Condition.Type);
        Assert.IsInstanceOfType<NoMonstersInDistanceCondition>(meta.Rules[0].Condition);
        var ec = meta.Rules[0].Condition as NoMonstersInDistanceCondition;

        Assert.AreEqual(expectedDistance, ec.Distance);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsChatCaptureCondition()
    {
        var expectedChat = fixture.Create<string>();
        var expectedColor = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine($"   IF: ChatCapture {{{expectedChat}}} {{{expectedColor}}}")
            .AppendLine("       DO: None");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ConditionType.ChatMessageCapture, meta.Rules[0].Condition.Type);
        Assert.IsInstanceOfType<ChatMessageCaptureCondition>(meta.Rules[0].Condition);
        var ec = meta.Rules[0].Condition as ChatMessageCaptureCondition;

        Assert.AreEqual(expectedChat, ec.Pattern);
        Assert.AreEqual(expectedColor, ec.Color);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsChatMatchCondition()
    {
        var expectedChat = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine($"   IF: ChatMatch {{{expectedChat}}}")
            .AppendLine("       DO: None");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ConditionType.ChatMessage, meta.Rules[0].Condition.Type);
        Assert.IsInstanceOfType<Condition<string>>(meta.Rules[0].Condition);
        var ec = meta.Rules[0].Condition as Condition<string>;

        Assert.AreEqual(expectedChat, ec.Data);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsMobsInDistPriorityCondition()
    {
        var expectedCount = fixture.Create<int>();
        var expectedDistance = fixture.Create<double>();
        var expectedPriority = fixture.Create<int>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine($"   IF: MobsInDist_Priority {expectedCount} {expectedDistance} {expectedPriority}")
            .AppendLine("       DO: None");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ConditionType.MonstersWithPriorityWithinDistance, meta.Rules[0].Condition.Type);
        Assert.IsInstanceOfType<MonstersWithPriorityWithinDistanceCondition>(meta.Rules[0].Condition);
        var ec = meta.Rules[0].Condition as MonstersWithPriorityWithinDistanceCondition;

        Assert.AreEqual(expectedCount, ec.Count);
        Assert.AreEqual(expectedDistance, ec.Distance);
        Assert.AreEqual(expectedPriority, ec.Priority);
    }

    [TestMethod]
    [DataRow("Death", ConditionType.Died)]
    [DataRow("Never", ConditionType.Never)]
    [DataRow("Always", ConditionType.Always)]
    [DataRow("NavEmpty", ConditionType.NavrouteEmpty)]
    [DataRow("NeedToBuff", ConditionType.NeedToBuff)]
    [DataRow("VendorOpen", ConditionType.VendorOpen)]
    [DataRow("VendorClosed", ConditionType.VendorClosed)]
    [DataRow("IntoPortal", ConditionType.PortalspaceEnter)]
    [DataRow("ExitPortal", ConditionType.PortalspaceExit)]
    public async Task ReadMetaAsync_HappyPath_ReadsEmptyCondition(string pattern, ConditionType expectedConditionType)
    {
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine($"   IF: {pattern}")
            .AppendLine("       DO: None");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(expectedConditionType, meta.Rules[0].Condition.Type);
        Assert.IsInstanceOfType<Condition<int>>(meta.Rules[0].Condition);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsDistToRteGECondition()
    {
        var expectedDistance = fixture.Create<double>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine($"   IF: DistToRteGE {expectedDistance}")
            .AppendLine("       DO: None");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ConditionType.DistanceToAnyRoutePointGE, meta.Rules[0].Condition.Type);
        Assert.IsInstanceOfType<DistanceToAnyRoutePointGECondition>(meta.Rules[0].Condition);
        var ec = meta.Rules[0].Condition as DistanceToAnyRoutePointGECondition;

        Assert.AreEqual(expectedDistance, ec.Distance);
    }

    [TestMethod]
    [DataRow("PSecsInStateGE", ConditionType.SecondsInStatePersistGE)]
    [DataRow("SecsInStateGE", ConditionType.SecondsInStateGE)]
    [DataRow("BuPercentGE", ConditionType.BurdenPercentGE)]
    [DataRow("MainSlotsLE", ConditionType.MainPackSlotsLE)]
    public async Task ReadMetaAsync_HappyPath_ReadsIntCondition(string pattern, ConditionType expectedConditionType)
    {
        var expectedValue = fixture.Create<int>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine($"   IF: {pattern} {expectedValue}")
            .AppendLine("       DO: None");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(expectedConditionType, meta.Rules[0].Condition.Type);
        Assert.IsInstanceOfType<Condition<int>>(meta.Rules[0].Condition);
        var ec = meta.Rules[0].Condition as Condition<int>;

        Assert.AreEqual(expectedValue, ec.Data);
    }

    [TestMethod]
    [DataRow("ItemCountGE", ConditionType.ItemCountGE)]
    [DataRow("ItemCountLE", ConditionType.ItemCountLE)]
    public async Task ReadMetaAsync_HappyPath_ReadsItemCountCondition(string pattern, ConditionType expectedConditionType)
    {
        var expectedCount = fixture.Create<int>();
        var expectedItem = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine($"   IF: {pattern} {expectedCount} {{{expectedItem}}}")
            .AppendLine("       DO: None");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(expectedConditionType, meta.Rules[0].Condition.Type);
        Assert.IsInstanceOfType<ItemCountCondition>(meta.Rules[0].Condition);
        var ec = meta.Rules[0].Condition as ItemCountCondition;

        Assert.AreEqual(expectedCount, ec.Count);
        Assert.AreEqual(expectedItem, ec.ItemName);
    }

    [TestMethod]
    [DataRow("CellE", ConditionType.LandCellE)]
    [DataRow("BlockE", ConditionType.LandBlockE)]
    public async Task ReadMetaAsync_HappyPath_ReadsLandCellCondition(string pattern, ConditionType expectedConditionType)
    {
        var expectedLandCell = fixture.Create<int>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine($"   IF: {pattern} {expectedLandCell:X}")
            .AppendLine("       DO: None");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(expectedConditionType, meta.Rules[0].Condition.Type);
        Assert.IsInstanceOfType<Condition<int>>(meta.Rules[0].Condition);
        var ec = meta.Rules[0].Condition as Condition<int>;

        Assert.AreEqual(expectedLandCell, ec.Data);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsMobsInDistNameCondition()
    {
        var expectedCount = fixture.Create<int>();
        var expectedDistance = fixture.Create<double>();
        var expectedName = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine($"   IF: MobsInDist_Name {expectedCount} {expectedDistance} {{{expectedName}}}")
            .AppendLine("       DO: None");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ConditionType.MonsterCountWithinDistance, meta.Rules[0].Condition.Type);
        Assert.IsInstanceOfType<MonsterCountWithinDistanceCondition>(meta.Rules[0].Condition);
        var ec = meta.Rules[0].Condition as MonsterCountWithinDistanceCondition;

        Assert.AreEqual(expectedCount, ec.Count);
        Assert.AreEqual(expectedDistance, ec.Distance);
        Assert.AreEqual(expectedName, ec.MonsterNameRx);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsSecsOnSpellGECondition()
    {
        var expectedSeconds = fixture.Create<int>();
        var expectedSpellId = fixture.Create<int>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine($"   IF: SecsOnSpellGE {expectedSeconds} {expectedSpellId}")
            .AppendLine("       DO: None");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ConditionType.TimeLeftOnSpellGE, meta.Rules[0].Condition.Type);
        Assert.IsInstanceOfType<TimeLeftOnSpellGECondition>(meta.Rules[0].Condition);
        var ec = meta.Rules[0].Condition as TimeLeftOnSpellGECondition;

        Assert.AreEqual(expectedSeconds, ec.Seconds);
        Assert.AreEqual(expectedSpellId, ec.SpellId);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsNotCondition()
    {
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine($"   IF: Not Always")
            .AppendLine("       DO: None");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ConditionType.Not, meta.Rules[0].Condition.Type);
        Assert.IsInstanceOfType<NotCondition>(meta.Rules[0].Condition);
        var ec = meta.Rules[0].Condition as NotCondition;

        Assert.IsNotNull(ec.Data);
        Assert.AreEqual(ConditionType.Always, ec.Data.Type);
        Assert.IsInstanceOfType<Condition<int>>(ec.Data);
    }

    [TestMethod]
    [DataRow("Any", ConditionType.Any)]
    [DataRow("All", ConditionType.All)]
    public async Task ReadMetaAsync_HappyPath_ReadsMultipleCondition(string pattern, ConditionType expectedConditionType)
    {
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine($"   IF: {pattern}")
            .AppendLine("           Always")
            .AppendLine("           Always")
            .AppendLine("           Always")
            .AppendLine("       DO: None");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(expectedConditionType, meta.Rules[0].Condition.Type);
        Assert.IsInstanceOfType<MultipleCondition>(meta.Rules[0].Condition);
        var ec = meta.Rules[0].Condition as MultipleCondition;

        Assert.IsNotNull(ec.Data);
        Assert.AreEqual(3, ec.Data.Count);
    }

    [TestMethod]
    [DataRow("None", ActionType.None, typeof(MetaAction<int>))]
    [DataRow("Return", ActionType.ReturnFromCall, typeof(MetaAction<int>))]
    [DataRow("DestroyAllViews", ActionType.DestroyAllViews, typeof(TableMetaAction))]
    public async Task ReadMetaAsync_HappyPath_ReadsEmptyAction(string pattern, ActionType expectedActionType, Type expectedType)
    {
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine("   IF: Always")
            .AppendLine($"       DO: {pattern}");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(expectedActionType, meta.Rules[0].Action.Type);
        Assert.IsInstanceOfType(meta.Rules[0].Action, expectedType);
    }

    [TestMethod]
    [DataRow("Chat", ActionType.ChatCommand)]
    [DataRow("SetState", ActionType.SetState)]
    public async Task ReadMetaAsync_HappyPath_ReadsStringAction(string pattern, ActionType expectedActionType)
    {
        var expectedData = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine("   IF: Always")
            .AppendLine($"       DO: {pattern} {{{expectedData}}}");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(expectedActionType, meta.Rules[0].Action.Type);
        Assert.IsInstanceOfType<MetaAction<string>>(meta.Rules[0].Action);
        var ec = meta.Rules[0].Action as MetaAction<string>;

        Assert.AreEqual(expectedData, ec.Data);
    }

    [TestMethod]
    [DataRow("ChatExpr", ActionType.ChatWithExpression)]
    [DataRow("DoExpr", ActionType.ExpressionAct)]
    public async Task ReadMetaAsync_HappyPath_ReadsExpressionAction(string pattern, ActionType expectedActionType)
    {
        var expectedData = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine("   IF: Always")
            .AppendLine($"       DO: {pattern} {{{expectedData}}}");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(expectedActionType, meta.Rules[0].Action.Type);
        Assert.IsInstanceOfType<ExpressionMetaAction>(meta.Rules[0].Action);
        var ec = meta.Rules[0].Action as ExpressionMetaAction;

        Assert.AreEqual(expectedData, ec.Expression);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsGetOptionAction()
    {
        var expectedOption = fixture.Create<string>();
        var expectedVariable = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine("   IF: Always")
            .AppendLine($"       DO: GetOpt {{{expectedOption}}} {{{expectedVariable}}}");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ActionType.GetVTOption, meta.Rules[0].Action.Type);
        Assert.IsInstanceOfType<GetVTOptionMetaAction>(meta.Rules[0].Action);
        var ec = meta.Rules[0].Action as GetVTOptionMetaAction;

        Assert.AreEqual(expectedOption, ec.Option);
        Assert.AreEqual(expectedVariable, ec.Variable);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsSetOptionAction()
    {
        var expectedOption = fixture.Create<string>();
        var expectedValue = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine("   IF: Always")
            .AppendLine($"       DO: SetOpt {{{expectedOption}}} {{{expectedValue}}}");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ActionType.SetVTOption, meta.Rules[0].Action.Type);
        Assert.IsInstanceOfType<SetVTOptionMetaAction>(meta.Rules[0].Action);
        var ec = meta.Rules[0].Action as SetVTOptionMetaAction;

        Assert.AreEqual(expectedOption, ec.Option);
        Assert.AreEqual(expectedValue, ec.Value);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsCallStateAction()
    {
        var expectedCallState = fixture.Create<string>();
        var expectedReturnState = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine("   IF: Always")
            .AppendLine($"       DO: CallState {{{expectedCallState}}} {{{expectedReturnState}}}");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ActionType.CallState, meta.Rules[0].Action.Type);
        Assert.IsInstanceOfType<CallStateMetaAction>(meta.Rules[0].Action);
        var ec = meta.Rules[0].Action as CallStateMetaAction;

        Assert.AreEqual(expectedCallState, ec.CallState);
        Assert.AreEqual(expectedReturnState, ec.ReturnState);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsDestroyViewAction()
    {
        var expectedViewName = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine("   IF: Always")
            .AppendLine($"       DO: DestroyView {{{expectedViewName}}}");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ActionType.DestroyView, meta.Rules[0].Action.Type);
        Assert.IsInstanceOfType<DestroyViewMetaAction>(meta.Rules[0].Action);
        var ec = meta.Rules[0].Action as DestroyViewMetaAction;

        Assert.AreEqual(expectedViewName, ec.ViewName);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsCreateViewAction()
    {
        var expectedViewName = fixture.Create<string>();
        var expectedViewDefinition = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine("   IF: Always")
            .AppendLine($"       DO: CreateView {{{expectedViewName}}} {{{expectedViewDefinition}}}");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ActionType.CreateView, meta.Rules[0].Action.Type);
        Assert.IsInstanceOfType<CreateViewMetaAction>(meta.Rules[0].Action);
        var ec = meta.Rules[0].Action as CreateViewMetaAction;

        Assert.AreEqual(expectedViewName, ec.ViewName);
        Assert.AreEqual(expectedViewDefinition, ec.ViewDefinition);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsSetWatchdogAction()
    {
        var expectedRange = fixture.Create<double>();
        var expectedTime = fixture.Create<int>();
        var expectedState = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine("   IF: Always")
            .AppendLine($"       DO: SetWatchdog {expectedRange} {expectedTime} {{{expectedState}}}");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ActionType.WatchdogSet, meta.Rules[0].Action.Type);
        Assert.IsInstanceOfType<WatchdogSetMetaAction>(meta.Rules[0].Action);
        var ec = meta.Rules[0].Action as WatchdogSetMetaAction;

        Assert.AreEqual(expectedTime, ec.Time);
        Assert.AreEqual(expectedRange, ec.Range);
        Assert.AreEqual(expectedState, ec.State);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsMultipleAction()
    {
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine("   IF: Always")
            .AppendLine("       DO: DoAll")
            .AppendLine("           None")
            .AppendLine("           None")
            .AppendLine("           None");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ActionType.Multiple, meta.Rules[0].Action.Type);
        Assert.IsInstanceOfType<AllMetaAction>(meta.Rules[0].Action);
        var ec = meta.Rules[0].Action as AllMetaAction;

        Assert.IsNotNull(ec.Data);
        Assert.AreEqual(3, ec.Data.Count);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPath_ReadsEmbeddedNavAction()
    {
        var expectedNavRef = "_" + Guid.NewGuid().ToString().Replace("-", "");
        var expectedNavName = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("STATE: {Default}")
            .AppendLine("   IF: Always")
            .AppendLine($"       DO: EmbedNav {expectedNavRef} {{{expectedNavName}}}")
            .AppendLine($"NAV: {expectedNavRef} once")
            .AppendLine($"pnt 0.1 0.2 0.3");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ActionType.EmbeddedNavRoute, meta.Rules[0].Action.Type);
        Assert.IsInstanceOfType<EmbeddedNavRouteMetaAction>(meta.Rules[0].Action);
        var ec = meta.Rules[0].Action as EmbeddedNavRouteMetaAction;

        Assert.AreEqual(expectedNavName, ec.Data.name);
        Assert.IsNotNull(ec.Data.nav);
        Assert.IsInstanceOfType<List<NavNode>>(ec.Data.nav.Data);
        var navNodeList = ec.Data.nav.Data as List<NavNode>;

        Assert.AreEqual(1, navNodeList.Count);
        Assert.IsInstanceOfType<NavNodePoint>(navNodeList[0]);
        Assert.AreEqual(0.1, navNodeList[0].Point.x);
        Assert.AreEqual(0.2, navNodeList[0].Point.y);
        Assert.AreEqual(0.3, navNodeList[0].Point.z);
    }

    [TestMethod]
    public async Task ReadMetaAsync_HappyPathAndNavDeclaredFirst_ReadsEmbeddedNavAction()
    {
        var expectedNavRef = "_" + Guid.NewGuid().ToString().Replace("-", "");
        var expectedNavName = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine($"NAV: {expectedNavRef} once")
            .AppendLine($"pnt 0.1 0.2 0.3")
            .AppendLine("STATE: {Default}")
            .AppendLine("   IF: Always")
            .AppendLine($"       DO: EmbedNav {expectedNavRef} {{{expectedNavName}}}");
        using var stream = sb.ToString().ToStream();

        var metaReader = new MetafReader(new MetafNavReader());

        var meta = await metaReader.ReadMetaAsync(stream);

        Assert.IsNotNull(meta);
        Assert.AreEqual(1, meta.Rules.Count);
        Assert.AreEqual(ActionType.EmbeddedNavRoute, meta.Rules[0].Action.Type);
        Assert.IsInstanceOfType<EmbeddedNavRouteMetaAction>(meta.Rules[0].Action);
        var ec = meta.Rules[0].Action as EmbeddedNavRouteMetaAction;

        Assert.AreEqual(expectedNavName, ec.Data.name);
        Assert.IsNotNull(ec.Data.nav);
        Assert.IsInstanceOfType<List<NavNode>>(ec.Data.nav.Data);
        var navNodeList = ec.Data.nav.Data as List<NavNode>;

        Assert.AreEqual(1, navNodeList.Count);
        Assert.IsInstanceOfType<NavNodePoint>(navNodeList[0]);
        Assert.AreEqual(0.1, navNodeList[0].Point.x);
        Assert.AreEqual(0.2, navNodeList[0].Point.y);
        Assert.AreEqual(0.3, navNodeList[0].Point.z);
    }
}