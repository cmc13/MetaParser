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
    private static readonly Regex StateRegex = new(@"^\s*{(?<state>([^{}]|{{|}})*)}\s*(~~.*)?", RegexOptions.Compiled);
    private static readonly Regex IfRegex = new(@"^\s*(?<op>IF:)", RegexOptions.Compiled);
    private static readonly Regex DoRegex = new(@"^\s*(?<op>DO:)", RegexOptions.Compiled);
    private static readonly Regex ConditionRegex = new(@"^\s*(?<cond>\S*)", RegexOptions.Compiled);
    private static readonly Regex ActionRegex = new(@"^\s*(?<action>\S*)", RegexOptions.Compiled);
    private static readonly Regex ConditionIntRegex = new(@"^\s*(?<arg>\d+)\s*(~~.*)?", RegexOptions.Compiled);
    private static readonly Regex ConditionStringRegex = new(@"^\s*{(?<arg>([^{}]|{{|}})*)}\s*(~~.*)?", RegexOptions.Compiled);
    private static readonly Regex ItemCountRegex = new(@"^\s*(?<arg1>\d+)\s*{(?<arg2>([^{}]|{{|}})*)}\s*(~~.*)?");
    private static readonly Regex LandCellRegex = new(@"^\s*(?<arg>[0-9a-fA-F]+)\s*(~~.*)?", RegexOptions.Compiled);
    private static readonly Regex OptionRegex = new(@"^\s*{(?<arg1>([^{}]|{{|}})*)}\s*{(?<arg2>([^{}]|{{|}})*)}\s*(~~.*)?", RegexOptions.Compiled);
    private static readonly Dictionary<string, Regex> ConditionListRegex = new()
    {
        { "NoMobsInDist",           new(@"^\s*(?<arg>" + DOUBLE_REGEX + @")", RegexOptions.Compiled) },
        { "Expr",                   ConditionStringRegex },
        { "ChatCapture",            new(@"^\s*{(?<arg1>([^{}]|{{|}})*)}\s*{(?<arg2>([^{}]|{{|}})*)}\s*(~~.*)?", RegexOptions.Compiled) },
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
        { "MobsInDist_Name",        new(@"^\s*(?<arg1>\d+)\s*(?<arg2>" + DOUBLE_REGEX + @")\s*{(?<arg3>([^{}]|{{|}})*)}\s*(~~.*)?", RegexOptions.Compiled) },
        { "SecsOnSpellGE",          new(@"\s*(?<arg1>\d+)\s*(?<arg2>\d+)\s*(~~.*)?", RegexOptions.Compiled) }
    };
    private static readonly Dictionary<string, Regex> ActionListRegex = new()
    {
        { "EmbedNav",       new(@"\s*(?<navRef>[a-zA-Z_][a-zA-Z0-9_]*)\s*{(?<navName>([^{}]|{{|}})*)}\s*(~~.*)?", RegexOptions.Compiled) },
        { "Chat",           ConditionStringRegex },
        { "SetState",       ConditionStringRegex },
        { "ChatExpr",       ConditionStringRegex },
        { "DoExpr",         ConditionStringRegex },
        { "SetOpt",         OptionRegex },
        { "GetOpt",         OptionRegex },
        { "CallState",      OptionRegex },
        { "DestroyView",    ConditionStringRegex },
        { "CreateView",     OptionRegex },
        { "SetWatchdog",    new(@"^\s*(?<arg1>" + DOUBLE_REGEX + @")\s*(?<arg2>\d+)\s*{(?<arg3>([^{}]|{{|}})*)}\s*(~~.*)?", RegexOptions.Compiled) }
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
        string line = await reader.ReadLineAsync().ConfigureAwait(false);

        do
        {
            while (line != null && EmptyLineRegex.IsMatch(line))
                line = await reader.ReadLineAsync().ConfigureAwait(false);

            if (line != null)
            {
                var m = StateNavRegex.Match(line);
                if (m.Groups["op"].Value == "STATE")
                {
                    line = await ParseStateAsync(line.Substring(m.Index + m.Length), reader, meta, navReferences);
                }
                else if (m.Groups["op"].Value == "NAV")
                {
                    (line, _) = await navReader.ReadNavAsync(line.Substring(m.Index + m.Length), reader, navReferences);
                }
                else
                    throw new MetaParserException("Invalid state/nav declaration", "STATE|NAV", line);
            }
        } while (line != null);

        return meta;
    }

    private async Task<string> ParseStateAsync(string line, TextReader reader, Meta meta, Dictionary<string, NavRoute> navReferences)
    {
        Match m = StateRegex.Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid state definition");

        var state = m.Groups["state"].Value;

        line = line.Substring(m.Index + m.Length);

        // Parse Rules
        while (line != null && !StateNavRegex.IsMatch(line))
        {
            while (line != null && EmptyLineRegex.IsMatch(line))
                line = await reader.ReadLineAsync().ConfigureAwait(false);

            if (IfRegex.IsMatch(line))
            {
                (line, var rule) = await ParseRuleAsync(line, reader, state, navReferences);
                if (rule != null)
                {
                    meta.Rules.Add(rule);
                }
            }
        }

        return line;
    }

    private async Task<(string, Rule)> ParseRuleAsync(string line, TextReader reader, string state, Dictionary<string, NavRoute> navReferences)
    {
        Match m = IfRegex.Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid condition definition");

        (line, var cond) = await ParseConditionAsync(line.Substring(m.Index + m.Length), reader);

        m = DoRegex.Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid action definition");

        (line, var action) = await ParseActionAsync(line.Substring(m.Index + m.Length), reader, navReferences);

        return (line, new Rule()
        {
            Condition = cond,
            Action = action,
            State = state,
        });
    }

    private async Task<(string, MetaAction)> ParseActionAsync(string line, TextReader reader, Dictionary<string, NavRoute> navReferences)
    {
        // skip whitespace
        while (line != null && EmptyLineRegex.IsMatch(line))
            line = await reader.ReadLineAsync().ConfigureAwait(false);

        if (line == null || StateNavRegex.IsMatch(line) || IfRegex.IsMatch(line))
            return (line, null);

        var m = ActionRegex.Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid action definition");

        MetaAction action;
        if (m.Groups["action"].Value == "DoAll")
        {
            action = MetaAction.CreateMetaAction(ActionType.Multiple);

            line = line.Substring(m.Groups["action"].Index + m.Groups["action"].Length);
            while (line != null && EmptyLineRegex.IsMatch(line))
                line = await reader.ReadLineAsync().ConfigureAwait(false);

            while (line != null && !StateNavRegex.IsMatch(line) && !IfRegex.IsMatch(line))
            {
                (line, var c) = await ParseActionAsync(line, reader, navReferences).ConfigureAwait(false);
                if (c != null)
                    ((AllMetaAction)action).Data.Add(c);
            }
        }
        else if (ActionList.ContainsKey(m.Groups["action"].Value))
        {
            action = ParseActionLine(m, line.Substring(m.Groups["action"].Index + m.Groups["action"].Length), navReferences);

            do { line = await reader.ReadLineAsync().ConfigureAwait(false); } while (line != null && EmptyLineRegex.IsMatch(line));
        }
        else
            throw new MetaParserException($"Invalid action type: {m.Groups["action"].Value}");

        return (line, action);
    }

    private MetaAction ParseActionLine(Match m, string line, Dictionary<string, NavRoute> navReferences)
    {
        var action = MetaAction.CreateMetaAction(ActionList[m.Groups["action"].Value]);

        if (ActionListRegex.ContainsKey(m.Groups["action"].Value))
        {
            m = ActionListRegex[m.Groups["action"].Value].Match(line);
            if (!m.Success)
                throw new MetaParserException($"Invalid {action.Type} action definition");
        }

        switch (action)
        {
            case MetaAction a when a.Type == ActionType.None || a.Type == ActionType.DestroyAllViews || a.Type == ActionType.WatchdogClear || a.Type == ActionType.ReturnFromCall:
                break;

            case EmbeddedNavRouteMetaAction a:
                if (!navReferences.TryGetValue(m.Groups["navRef"].Value, out var nav))
                {
                    nav = new();
                    navReferences.Add(m.Groups["navRef"].Value, nav);
                }
                a.Data = (m.Groups["navName"].Value, nav);
                break;

            case MetaAction<string> a:
                a.Data = m.Groups["arg"].Value.Replace("{{", "{").Replace("}}", "}");
                break;

            case ExpressionMetaAction a:
                a.Expression = m.Groups["arg"].Value.Replace("{{", "{").Replace("}}", "}");
                break;

            case SetVTOptionMetaAction a:
                a.Option = m.Groups["arg1"].Value.Replace("{{", "{").Replace("}}", "}");
                a.Value = m.Groups["arg2"].Value.Replace("{{", "{").Replace("}}", "}");
                break;

            case GetVTOptionMetaAction a:
                a.Option = m.Groups["arg1"].Value.Replace("{{", "{").Replace("}}", "}");
                a.Variable = m.Groups["arg2"].Value.Replace("{{", "{").Replace("}}", "}");
                break;

            case CallStateMetaAction a:
                a.CallState = m.Groups["arg1"].Value.Replace("{{", "{").Replace("}}", "}");
                a.ReturnState = m.Groups["arg2"].Value.Replace("{{", "{").Replace("}}", "}");
                break;

            case DestroyViewMetaAction a:
                a.ViewName = m.Groups["arg"].Value.Replace("{{", "{").Replace("}}", "}");
                break;

            case CreateViewMetaAction a:
                a.ViewName = m.Groups["arg1"].Value.Replace("{{", "{").Replace("}}", "}");
                a.ViewDefinition = m.Groups["arg2"].Value.Replace("{{", "{").Replace("}}", "}");
                break;

            case WatchdogSetMetaAction a:
                a.Range = double.TryParse(m.Groups["arg1"].Value, out var range) ? range : throw new MetaParserException("Invalid set watchdog definition");
                a.Time = int.TryParse(m.Groups["arg2"].Value, out var time) ? time : throw new MetaParserException("Invalid set watchdog definition");
                a.State = m.Groups["arg3"].Value.Replace("{{", "{").Replace("}}", "}");
                break;
        }

        return action;
    }

    private async Task<(string, Condition)> ParseConditionAsync(string line, TextReader reader)
    {
        // skip whitespace
        while (line != null && EmptyLineRegex.IsMatch(line))
            line = await reader.ReadLineAsync().ConfigureAwait(false);

        if (line == null || StateNavRegex.IsMatch(line) || DoRegex.IsMatch(line))
            return (line, null);

        var m = ConditionRegex.Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid condition definition");

        Condition cond;
        if (m.Groups["cond"].Value == "Not")
        {
            cond = Condition.CreateCondition(ConditionType.Not);
            (line, ((NotCondition)cond).Data) = await ParseConditionAsync(line.Substring(m.Index + m.Length), reader).ConfigureAwait(false);
        }
        else if (m.Groups["cond"].Value == "All" || m.Groups["cond"].Value == "Any")
        {
            cond = Condition.CreateCondition(m.Groups["cond"].Value == "All" ? ConditionType.All : ConditionType.Any);

            line = line.Substring(m.Groups["cond"].Index + m.Groups["cond"].Length);
            while (line != null && EmptyLineRegex.IsMatch(line))
                line = await reader.ReadLineAsync().ConfigureAwait(false);

            while (line != null && !StateNavRegex.IsMatch(line) && !DoRegex.IsMatch(line))
            {
                (line, var c) = await ParseConditionAsync(line, reader);
                if (c != null)
                    ((MultipleCondition)cond).Data.Add(c);
            }
        }
        else if (ConditionList.ContainsKey(m.Groups["cond"].Value))
        {
            cond = ParseConditionLine(m, line.Substring(m.Groups["cond"].Index + m.Groups["cond"].Length));

            do { line = await reader.ReadLineAsync().ConfigureAwait(false); } while (line != null && EmptyLineRegex.IsMatch(line));
        }
        else
            throw new MetaParserException($"Invalid condition type: {m.Groups["cond"].Value}");

        return (line, cond);
    }

    private Condition ParseConditionLine(Match m, string line)
    {
        var cond = Condition.CreateCondition(ConditionList[m.Groups["cond"].Value]);

        if (ConditionListRegex.ContainsKey(m.Groups["cond"].Value))
        {
            m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
            if (!m.Success)
                throw new MetaParserException($"Invalid {cond.Type} condition definition");
        }

        switch (cond)
        {
            case Condition<int> c when c.Type == ConditionType.LandBlockE || c.Type == ConditionType.LandCellE:
                c.Data = int.Parse(m.Groups["arg"].Value, System.Globalization.NumberStyles.HexNumber);
                break;

            case Condition<int> c when c.Type == ConditionType.SecondsInStateGE || c.Type == ConditionType.SecondsInStatePersistGE || c.Type == ConditionType.BurdenPercentGE || c.Type == ConditionType.MainPackSlotsLE:
                c.Data = int.Parse(m.Groups["arg"].Value);
                break;

            case Condition<int> _:
                break;

            case NoMonstersInDistanceCondition c:
                c.Distance = double.Parse(m.Groups["arg"].Value);
                break;

            case ExpressionCondition c:
                c.Expression = m.Groups["arg"].Value.Replace("{{", "{").Replace("}}", "}");
                break;

            case ChatMessageCaptureCondition c:
                c.Pattern = m.Groups["arg1"].Value.Replace("{{", "{").Replace("}}", "}");
                c.Color = m.Groups["arg2"].Value.Replace("{{", "{").Replace("}}", "}");
                break;

            case Condition<string> c:
                c.Data = m.Groups["arg"].Value.Replace("{{", "{").Replace("}}", "}");
                break;

            case MonstersWithPriorityWithinDistanceCondition c:
                c.Count = int.Parse(m.Groups["arg1"].Value);
                c.Distance = double.Parse(m.Groups["arg2"].Value);
                c.Priority = int.Parse(m.Groups["arg3"].Value);
                break;

            case DistanceToAnyRoutePointGECondition c:
                c.Distance = double.Parse(m.Groups["arg"].Value);
                break;

            case ItemCountCondition c:
                c.Count = int.Parse(m.Groups["arg1"].Value);
                c.ItemName = m.Groups["arg2"].Value.Replace("{{", "{").Replace("}}", "}");
                break;

            case MonsterCountWithinDistanceCondition c:
                c.Count = int.Parse(m.Groups["arg1"].Value);
                c.Distance = double.Parse(m.Groups["arg2"].Value);
                c.MonsterNameRx = m.Groups["arg3"].Value.Replace("{{", "{").Replace("}}", "}");
                break;

            case TimeLeftOnSpellGECondition c:
                c.Seconds = int.Parse(m.Groups["arg1"].Value);
                c.SpellId = int.Parse(m.Groups["arg2"].Value);
                break;
        }

        return cond;
    }
}
