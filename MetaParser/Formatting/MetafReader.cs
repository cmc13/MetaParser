using MetaParser.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MetaParser.Formatting;

static class ReaderExtensions
{
    public static async IAsyncEnumerable<string> StreamLines(this StreamReader reader)
    {
        while (!reader.EndOfStream)
            yield return await reader.ReadLineAsync().ConfigureAwait(false);
    }
}

public class MetafReader : IMetaReader
{
    private static readonly string DOUBLE_REGEX = @"[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))";
    private static readonly Regex EmptyLineRegex = new(@"^\s*(~~.*)?$", RegexOptions.Compiled);
    private static readonly Regex StateNavRegex = new(@"^\s*(?<op>STATE|NAV):", RegexOptions.Compiled);
    private static readonly Regex StateRegex = new(@"^\s*{(?<state>[^}]*)}\s*(~~.*)?", RegexOptions.Compiled);
    private static readonly Regex IfRegex = new(@"^\s*(?<op>IF:)", RegexOptions.Compiled);
    private static readonly Regex DoRegex = new(@"^\s*(?<op>DO:)", RegexOptions.Compiled);
    private static readonly Regex ConditionRegex = new(@"^\s*(?<cond>\S*)", RegexOptions.Compiled);
    private static readonly Regex ActionRegex = new(@"^\s*(?<action>\S*)", RegexOptions.Compiled);
    private static readonly Regex ConditionIntRegex = new(@"^\s*(?<arg>\d+)\s*(~~.*)?", RegexOptions.Compiled);
    private static readonly Regex ConditionStringRegex = new(@"^\s*{(?<arg>[^}]*)}\s*(~~.*)?", RegexOptions.Compiled);
    private static readonly Regex ItemCountRegex = new(@"^\s*(?<arg1>\d+)\s*{(?<arg2>[^}]*)}\s*(~~.*)?");
    private static readonly Regex LandCellRegex = new(@"^\s*(?<arg>[0-9a-fA-F]+)\s*(~~.*)?", RegexOptions.Compiled);
    private static readonly Regex OptionRegex = new(@"^\s*{(?<arg1>[^}]*)}\s*{(?<arg2>[^}]*)}\s*(~~.*)?", RegexOptions.Compiled);
    private static readonly Dictionary<string, Regex> ConditionListRegex = new()
    {
        { "NoMobsInDist",           new(@"^\s*(?<arg>" + DOUBLE_REGEX + @")", RegexOptions.Compiled) },
        { "Expr",                   ConditionStringRegex },
        { "ChatCapture",            new(@"^\s*{(?<arg1>[^}]*)}\s*{(?<arg2>[^}]*)}\s*(~~.*)?", RegexOptions.Compiled) },
        { "ChatMatch",              ConditionStringRegex },
        { "MobsInDist_Priority",    new(@"^\s*(?<arg1>\d+)\s*(?<arg2>" + DOUBLE_REGEX + @")\s*(?<arg3>\d+)\s*(~~.*)?", RegexOptions.Compiled) },
        { "DistToRteGE",            new(@"^\s*(?<arg>" + DOUBLE_REGEX + @")\s*(~~.*)?", RegexOptions.Compiled) },
        { "PSecsInStateGE",         ConditionIntRegex },
        { "SecsInStateGE",          ConditionIntRegex },
        { "BuPercentGE",            ConditionIntRegex },
        { "MainSlotsLE",            ConditionIntRegex },
        { "ItemCountLE",            ItemCountRegex },
        { "ItemCountGE",            ItemCountRegex },
        { "CellE",                  LandCellRegex },
        { "BlockE",                 LandCellRegex },
        { "MobsInDist_Name",        new(@"^\s*(?<arg1>\d+)\s*(?<arg2>" + DOUBLE_REGEX + @")\s*{(?<arg3>[^}]*)}\s*(~~.*)?", RegexOptions.Compiled) },
        { "SecsOnSpellGE",          new(@"\s*(?<arg1>\d+)\s*(?<arg2>\d+)\s*(~~.*)?", RegexOptions.Compiled) }
    };
    private static readonly Dictionary<string, Regex> ActionListRegex = new()
    {
        { "EmbedNav",       new(@"\s*(?<navRef>\S*)\s*{(?<navName>)[^}]*}\s*(~~.*)?", RegexOptions.Compiled) },
        { "Chat",           ConditionStringRegex },
        { "SetState",       ConditionStringRegex },
        { "ChatExpr",       ConditionStringRegex },
        { "DoExpr",         ConditionStringRegex },
        { "SetOpt",         OptionRegex },
        { "GetOpt",         OptionRegex },
        { "CallState",      OptionRegex },
        { "DestroyView",    ConditionStringRegex },
        { "CreateView",     OptionRegex },
        { "SetWatchdog",    new(@"^\s*(?<arg1>" + DOUBLE_REGEX + @")\s*(?<arg2>\d+)\s*{(?<arg3>[^}]*)}\s*(~~.*)?", RegexOptions.Compiled) }
    };

    private static readonly Dictionary<string, ConditionType> ConditionList = new()
    {
        { "Expr",                   ConditionType.Expression },
        { "ChatCapture",            ConditionType.ChatMessageCapture },
        { "ChatMatch",              ConditionType.ChatMessage },
        { "Never",                  ConditionType.Never },
        { "Always",                 ConditionType.Always },
        { "NavEmpty",               ConditionType.NavrouteEmpty },
        { "MobsInDist_Priority",    ConditionType.MonstersWithPriorityWithinDistance },
        { "Death",                  ConditionType.Died },
        { "NeedToBuff",             ConditionType.NeedToBuff },
        { "PSecsInStateGE",         ConditionType.SecondsInStatePersistGE },
        { "SecsInStateGE",          ConditionType.SecondsInStateGE },
        { "VendorOpen",             ConditionType.VendorOpen },
        { "VendorClosed",           ConditionType.VendorClosed },
        { "NoMobsInDist",           ConditionType.NoMonstersWithinDistance },
        { "SecsOnSpellGE",          ConditionType.TimeLeftOnSpellGE },
        { "ItemCountLE",            ConditionType.ItemCountLE },
        { "ItemCountGE",            ConditionType.ItemCountGE },
        { "CellE",                  ConditionType.LandCellE },
        { "DistToRteGE",            ConditionType.DistanceToAnyRoutePointGE },
        { "MainSlotsLE",            ConditionType.MainPackSlotsLE },
        { "IntoPortal",             ConditionType.PortalspaceEnter },
        { "MobsInDist_Name",        ConditionType.MonsterCountWithinDistance },
        { "ExitPortal",             ConditionType.PortalspaceExit },
        { "BlockE",                 ConditionType.LandBlockE },
        { "BuPercentGE",            ConditionType.BurdenPercentGE },
    };

    private static readonly Dictionary<string, ActionType> ActionList = new()
    {
        { "None",               ActionType.None },
        { "EmbedNav",           ActionType.EmbeddedNavRoute },
        { "ChatExpr",           ActionType.ChatWithExpression },
        { "SetOpt",             ActionType.SetVTOption },
        { "SetState",           ActionType.SetState },
        { "CallState",          ActionType.CallState },
        { "SetWatchdog",        ActionType.WatchdogSet },
        { "CreateView",         ActionType.CreateView },
        { "Chat",               ActionType.ChatCommand },
        { "Return",             ActionType.ReturnFromCall },
        { "ClearWatchdog",      ActionType.WatchdogClear },
        { "DestroyView",        ActionType.DestroyView },
        { "DoExpr",             ActionType.ExpressionAct },
        { "GetOpt",             ActionType.GetVTOption },
        { "DestroyAllViews",    ActionType.DestroyAllViews },
    };

    private readonly MetafNavReader navReader;

    public MetafReader(MetafNavReader navReader)
    {
        this.navReader = navReader;
    }

    public async Task<Meta> ReadMetaAsync(Stream stream)
    {
        var meta = new Meta();
        using var reader = new StreamReader(stream);
        var navReferences = new Dictionary<string, NavRoute>();

        var enumerator = reader.StreamLines().GetAsyncEnumerator();

        if (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            while (enumerator.Current != null)
            {
                // skip whitespace
                while (EmptyLineRegex.IsMatch(enumerator.Current) && await enumerator.MoveNextAsync().ConfigureAwait(false)) { }

                if (enumerator.Current == null)
                    break;

                var m = StateNavRegex.Match(enumerator.Current);
                if (!m.Success)
                    throw new MetaParserException("Invalid top level Metaf operator", "STATE|NAV", enumerator.Current);

                if (m.Groups["op"].Value == "STATE")
                {
                    m = StateRegex.Match(enumerator.Current.Substring(m.Index + m.Length));
                    if (!m.Success)
                        throw new MetaParserException("Invalid state declaration -- missing state name");

                    if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                        break;

                    await foreach (var rule in ParseStateAsync(enumerator, m.Groups["state"].Value, navReferences).ConfigureAwait(false))
                    {
                        meta.Rules.Add(rule);
                    }
                }
                else // NAV
                {
                    await navReader.ReadNavAsync(reader, navReferences).ConfigureAwait(false);
                }
            }
        }

        return meta;
    }

    private async IAsyncEnumerable<Rule> ParseStateAsync(IAsyncEnumerator<string> enumerator, string state, Dictionary<string, NavRoute> navReferences)
    {
        // Parse Rules
        while (enumerator.Current != null && !StateNavRegex.IsMatch(enumerator.Current))
        {
            // skip whitespace
            while (EmptyLineRegex.IsMatch(enumerator.Current) && await enumerator.MoveNextAsync().ConfigureAwait(false)) { }

            if (enumerator.Current == null || StateNavRegex.IsMatch(enumerator.Current))
                break;

            var m = IfRegex.Match(enumerator.Current);
            if (!m.Success)
                throw new MetaParserException("Invalid state definition");

            // Parse Condition
            var c = await ParseConditionAsync(enumerator.Current.Substring(m.Index + m.Length), enumerator);
            if (c == null)
                throw new MetaParserException("Invalid condition definition");

            // skip whitespace
            while (EmptyLineRegex.IsMatch(enumerator.Current) && await enumerator.MoveNextAsync().ConfigureAwait(false)) { }

            if (enumerator.Current == null || StateNavRegex.IsMatch(enumerator.Current))
                break;

            m = DoRegex.Match(enumerator.Current);
            if (!m.Success)
                throw new MetaParserException("Invalid state definition");

            // Parse Action
            var a = await ParseActionAsync(enumerator.Current.Substring(m.Index + m.Length), enumerator, navReferences);
            if (a == null)
                throw new MetaParserException("Invalid action definition");

            yield return new()
            {
                Condition = c,
                Action = a,
                State = state
            };
        }
    }

    private async Task<MetaAction> ParseActionAsync(string line, IAsyncEnumerator<string> enumerator, Dictionary<string, NavRoute> navReferences)
    {
        // skip whitespace
        while (EmptyLineRegex.IsMatch(line) && await enumerator.MoveNextAsync().ConfigureAwait(false))
            line = enumerator.Current;

        if (line == null || StateNavRegex.IsMatch(line) || IfRegex.IsMatch(line))
            return null;

        var m = ActionRegex.Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid action definition");

        MetaAction action;
        if (m.Groups["action"].Value == "DoAll")
        {
            action = MetaAction.CreateMetaAction(ActionType.Multiple);

            for (var c = await ParseActionAsync(line.Substring(m.Groups["action"].Index + m.Groups["action"].Length), enumerator, navReferences);
                c != null;
                c = await ParseActionAsync(enumerator.Current, enumerator, navReferences))
            {
                ((AllMetaAction)action).Data.Add(c);
            }
        }
        else if (ActionList.ContainsKey(m.Groups["action"].Value))
        {
            line = line.Substring(m.Groups["action"].Index + m.Groups["action"].Length);
            action = MetaAction.CreateMetaAction(ActionList[m.Groups["action"].Value]);
            switch (action)
            {
                case MetaAction a when a.Type == ActionType.None || a.Type == ActionType.DestroyAllViews || a.Type == ActionType.WatchdogClear || a.Type == ActionType.ReturnFromCall:
                    break;

                case EmbeddedNavRouteMetaAction a:
                    m = ActionListRegex[m.Groups["action"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid embedded nav definition");
                    var nav = new NavRoute();
                    navReferences.Add(m.Groups["navRef"].Value, nav);
                    a.Data = (m.Groups["navName"].Value, nav);
                    break;

                case MetaAction<string> a:
                    m = ActionListRegex[m.Groups["action"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException($"Invalid chat/set state definition");
                    a.Data = m.Groups["arg"].Value;
                    break;

                case ExpressionMetaAction a:
                    m = ActionListRegex[m.Groups["action"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid expression action definition");
                    a.Expression = m.Groups["arg"].Value;
                    break;

                case SetVTOptionMetaAction a:
                    m = ActionListRegex[m.Groups["action"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid set option definition");
                    a.Option = m.Groups["arg1"].Value;
                    a.Value = m.Groups["arg2"].Value;
                    break;

                case GetVTOptionMetaAction a:
                    m = ActionListRegex[m.Groups["action"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid get option definition");
                    a.Option = m.Groups["arg1"].Value;
                    a.Variable = m.Groups["arg2"].Value;
                    break;

                case CallStateMetaAction a:
                    m = ActionListRegex[m.Groups["action"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid call state definition");
                    a.CallState = m.Groups["arg1"].Value;
                    a.ReturnState = m.Groups["arg2"].Value;
                    break;

                case DestroyViewMetaAction a:
                    m = ActionListRegex[m.Groups["action"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid destroy view definition");
                    a.ViewName = m.Groups["arg"].Value;
                    break;

                case CreateViewMetaAction a:
                    m = ActionListRegex[m.Groups["action"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid create view definition");
                    a.ViewName = m.Groups["arg1"].Value;
                    a.ViewDefinition = m.Groups["arg2"].Value;
                    break;

                case WatchdogSetMetaAction a:
                    m = ActionListRegex[m.Groups["action"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid set watchdog definition");
                    a.Range = double.TryParse(m.Groups["arg1"].Value, out var range) ? range : throw new MetaParserException("Invalid set watchdog definition");
                    a.Time = int.TryParse(m.Groups["arg2"].Value, out var time) ? time : throw new MetaParserException("Invalid set watchdog definition");
                    a.State = m.Groups["arg3"].Value;
                    break;
            }

            await enumerator.MoveNextAsync().ConfigureAwait(false);
        }
        else
            throw new MetaParserException($"Invalid action type: {m.Groups["action"].Value}");

        return action;
    }

    private async Task<Condition> ParseConditionAsync(string line, IAsyncEnumerator<string> enumerator)
    {
        // skip whitespace
        while (EmptyLineRegex.IsMatch(line) && await enumerator.MoveNextAsync().ConfigureAwait(false))
            line = enumerator.Current;

        if (line == null || StateNavRegex.IsMatch(line) || DoRegex.IsMatch(line))
            return null;

        var m = ConditionRegex.Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid condition definition");

        Condition cond;
        if (m.Groups["cond"].Value == "Not")
        {
            cond = Condition.CreateCondition(ConditionType.Not);
            ((NotCondition)cond).Data = await ParseConditionAsync(line.Substring(m.Index + m.Length), enumerator).ConfigureAwait(false);
        }
        else if (m.Groups["cond"].Value == "All" || m.Groups["cond"].Value == "Any")
        {
            cond = Condition.CreateCondition(m.Groups["cond"].Value == "All" ? ConditionType.All : ConditionType.Any);

            for (var c = await ParseConditionAsync(line.Substring(m.Groups["cond"].Index + m.Groups["cond"].Length), enumerator);
                c != null;
                c = await ParseConditionAsync(enumerator.Current, enumerator))
            {
                ((MultipleCondition)cond).Data.Add(c);
            }
        }
        else if (ConditionList.ContainsKey(m.Groups["cond"].Value))
        {
            line = line.Substring(m.Groups["cond"].Index + m.Groups["cond"].Length);
            cond = Condition.CreateCondition(ConditionList[m.Groups["cond"].Value]);
            switch (cond)
            {
                case Condition<int> c when c.Type == ConditionType.LandBlockE || c.Type == ConditionType.LandCellE:
                    m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid landcell/landblock condition definition");
                    c.Data = int.Parse(m.Groups["arg"].Value, System.Globalization.NumberStyles.HexNumber);
                    break;

                case Condition<int> c when c.Type == ConditionType.SecondsInStateGE || c.Type == ConditionType.SecondsInStatePersistGE || c.Type == ConditionType.BurdenPercentGE || c.Type == ConditionType.MainPackSlotsLE:
                    m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException($"Invalid integer condition definition");
                    c.Data = int.Parse(m.Groups["arg"].Value);
                    break;

                case Condition<int> _:
                    break;

                case NoMonstersInDistanceCondition c:
                    m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid no mobs in distance definition");
                    c.Distance = double.Parse(m.Groups["arg"].Value);
                    break;

                case ExpressionCondition c:
                    m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid expression condition definition");
                    c.Expression = m.Groups["arg"].Value;
                    break;

                case ChatMessageCaptureCondition c:
                    m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid chat capture definition");
                    c.Pattern = m.Groups["arg1"].Value;
                    c.Color = m.Groups["arg2"].Value;
                    break;

                case Condition<string> c:
                    m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid chat match definition");
                    c.Data = m.Groups["arg"].Value;
                    break;

                case MonstersWithPriorityWithinDistanceCondition c:
                    m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid priority monsters in distance definition");
                    c.Count = int.Parse(m.Groups["arg1"].Value);
                    c.Distance = double.Parse(m.Groups["arg2"].Value);
                    c.Priority = int.Parse(m.Groups["arg3"].Value);
                    break;

                case DistanceToAnyRoutePointGECondition c:
                    m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid distance to any route point definition");
                    c.Distance = double.Parse(m.Groups["arg"].Value);
                    break;

                case ItemCountCondition c:
                    m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid item count definition");
                    c.Count = int.Parse(m.Groups["arg1"].Value);
                    c.ItemName = m.Groups["arg2"].Value;
                    break;

                case MonsterCountWithinDistanceCondition c:
                    m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid mobs in distance by name definition");
                    c.Count = int.Parse(m.Groups["arg1"].Value);
                    c.Distance = double.Parse(m.Groups["arg2"].Value);
                    c.MonsterNameRx = m.Groups["arg3"].Value;
                    break;

                case TimeLeftOnSpellGECondition c:
                    m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                    if (!m.Success)
                        throw new MetaParserException("Invalid time left on spell definition");
                    c.Seconds = int.Parse(m.Groups["arg1"].Value);
                    c.SpellId = int.Parse(m.Groups["arg2"].Value);
                    break;
            }

            await enumerator.MoveNextAsync().ConfigureAwait(false);
        }
        else
            throw new MetaParserException($"Invalid condition type: {m.Groups["cond"].Value}");

        return cond;
    }
}
