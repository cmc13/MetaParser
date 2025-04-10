using MetaParser.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MetaParser.Formatting;

using static MetafRegex;

public sealed partial class MetafReader : IMetaReader
{
    private class EmbeddedNavRouteMetaActionWithTransform
        : EmbeddedNavRouteMetaAction
    {
        public EmbeddedNavRouteMetaActionWithTransform(EmbeddedNavRouteMetaAction a)
        {
            Data = a.Data;
        }

        public double[] Transform { get; set; }

        public void ApplyTransform()
        {
            Data = (Data.name, Data.nav.ApplyTransform(Transform));
        }
    }

    private static readonly Dictionary<string, Regex> ConditionListRegex = new(16)
    {
        { "NoMobsInDist",           DistanceRegex() },
        { "Expr",                   SingleStringRegex() },
        { "ChatCapture",            DoubleStringRegex() },
        { "ChatMatch",              SingleStringRegex() },
        { "MobsInDist_Priority",    MobsInDist_PriorityRegex() },
        { "DistToRteGE",            DistanceRegex() },
        { "PSecsInStateGE",         SingleIntRegex() },
        { "SecsInStateGE",          SingleIntRegex() },
        { "BuPercentGE",            SingleIntRegex() },
        { "MainSlotsLE",            SingleIntRegex() },
        { "ItemCountLE",            ItemCountRegex() },
        { "ItemCountGE",            ItemCountRegex() },
        { "CellE",                  LandCellRegex() },
        { "BlockE",                 LandCellRegex() },
        { "MobsInDist_Name",        MobsInDist_NameRegex() },
        { "SecsOnSpellGE",          SecsOnSpellGERegex() }
    };
    private static readonly Dictionary<string, Regex> ActionListRegex = new(11)
    {
        { "EmbedNav",       EmbedNavRegex() },
        { "Chat",           SingleStringRegex() },
        { "SetState",       SingleStringRegex() },
        { "ChatExpr",       SingleStringRegex() },
        { "DoExpr",         SingleStringRegex() },
        { "SetOpt",         DoubleStringRegex() },
        { "GetOpt",         DoubleStringRegex() },
        { "CallState",      DoubleStringRegex() },
        { "DestroyView",    SingleStringRegex() },
        { "CreateView",     DoubleStringRegex() },
        { "SetWatchdog",    SetWatchdogRegex() }
    };

    private static readonly Dictionary<string, ConditionType> ConditionList = new(25)
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

    private static readonly Dictionary<string, ActionType> ActionList = new(15)
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
    private readonly IFileSystemService fileSystemService;

    public MetafReader(MetafNavReader navReader, IFileSystemService fileSystemService)
    {
        this.navReader = navReader;
        this.fileSystemService = fileSystemService;
    }

    public MetafReader(MetafNavReader navReader)
        : this(navReader, new FileSystemService())
    { }

    public async Task<Meta> ReadMetaAsync(Stream stream)
    {
        var meta = new Meta();
        using var reader = new StreamReader(stream);
        var navReferences = new Dictionary<string, NavRoute>();
        string line = await reader.ReadLineAsync().ConfigureAwait(false);

        do
        {
            while (line != null && EmptyLineRegex().IsMatch(line))
                line = await reader.ReadLineAsync().ConfigureAwait(false);

            if (line != null)
            {
                var m = StateNavRegex().Match(line);
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

        ApplyTransforms(meta);

        return meta;
    }

    private static void ApplyTransforms(Meta meta)
    {
        IEnumerable<EmbeddedNavRouteMetaActionWithTransform> GetTransforms(MetaAction action) => action switch
        {
            EmbeddedNavRouteMetaActionWithTransform a => new[] { a },
            AllMetaAction ama => ama.Data.SelectMany(GetTransforms),
            _ => Enumerable.Empty<EmbeddedNavRouteMetaActionWithTransform>()
        };

        // apply transforms
        foreach (var rule in meta.Rules)
        {
            foreach (var a in GetTransforms(rule.Action))
                a.ApplyTransform();
        }
    }

    private async Task<string> ParseStateAsync(string line, TextReader reader, Meta meta, Dictionary<string, NavRoute> navReferences)
    {
        Match m = StateRegex().Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid state definition", "{<state name>}", line);

        var state = m.Groups["state"].Value;

        line = line.Substring(m.Index + m.Length);

        // Parse Rules
        while (line != null && !StateNavRegex().IsMatch(line))
        {
            while (line != null && EmptyLineRegex().IsMatch(line))
                line = await reader.ReadLineAsync().ConfigureAwait(false);

            if (IfRegex().IsMatch(line))
            {
                (line, var rule) = await ParseRuleAsync(line, reader, state, navReferences);
                if (rule != null)
                {
                    meta.Rules.Add(rule);
                }
            }
            else
                throw new MetaParserException("Missing condition specifier", "IF:", line);
        }

        return line;
    }

    private async Task<(string, Rule)> ParseRuleAsync(string line, TextReader reader, string state, Dictionary<string, NavRoute> navReferences)
    {
        var m = IfRegex().Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid condition definition", "IF:", line);
        else if (m.Groups["tabs"].Length != 1)
            throw new MetaParserException("Condition definition must be indented once");

        (line, var cond) = await ParseConditionAsync(line.Substring(m.Index + m.Length), reader, 0);

        m = DoRegex().Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid action definition", "DO:", line);
        else if (m.Groups["tabs"].Length != 2)
            throw new MetaParserException("Action definition must be indented twice");

        (line, var action) = await ParseActionAsync(line.Substring(m.Index + m.Length), reader, 0, navReferences);

        return (line, new Rule()
        {
            Condition = cond,
            Action = action,
            State = state,
        });
    }

    private async Task<(string, MetaAction)> ParseActionAsync(string line, TextReader reader, int indentLevel, Dictionary<string, NavRoute> navReferences)
    {
        // skip whitespace
        while (line != null && EmptyLineRegex().IsMatch(line))
            line = await reader.ReadLineAsync().ConfigureAwait(false);

        if (line == null || StateNavRegex().IsMatch(line) || IfRegex().IsMatch(line))
            return (line, null);

        var m = ActionRegex().Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid action definition");
        else if (indentLevel > 0 && m.Groups["tabs"].Length > indentLevel)
            throw new MetaParserException("Too many tabs in action definition", indentLevel.ToString(), m.Groups["tabs"].Length.ToString());
        else if (m.Groups["tabs"].Length < indentLevel)
            return (line, null);

        MetaAction action;
        if (m.Groups["action"].Value == "DoAll")
        {
            action = MetaAction.CreateMetaAction(ActionType.Multiple);

            line = line.Substring(m.Groups["action"].Index + m.Groups["action"].Length);
            while (line != null && EmptyLineRegex().IsMatch(line))
                line = await reader.ReadLineAsync().ConfigureAwait(false);

            while (line != null && !StateNavRegex().IsMatch(line) && !IfRegex().IsMatch(line))
            {
                (line, var c) = await ParseActionAsync(line, reader, indentLevel != 0 ? indentLevel + 1 : 4, navReferences).ConfigureAwait(false);
                if (c != null)
                    ((AllMetaAction)action).Data.Add(c);
                else
                    break;
            }
        }
        else if (ActionList.ContainsKey(m.Groups["action"].Value))
        {
            action = await ParseActionLineAsync(m, line.Substring(m.Groups["action"].Index + m.Groups["action"].Length), navReferences).ConfigureAwait(false);

            do { line = await reader.ReadLineAsync().ConfigureAwait(false); } while (line != null && EmptyLineRegex().IsMatch(line));
        }
        else
            throw new MetaParserException($"Invalid action type: {m.Groups["action"].Value}");

        return (line, action);
    }

    private async Task<MetaAction> ParseActionLineAsync(Match m, string line, Dictionary<string, NavRoute> navReferences)
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
            case EmbeddedNavRouteMetaAction a:
                if (!navReferences.TryGetValue(m.Groups["navRef"].Value, out var nav))
                {
                    nav = navReferences[m.Groups["navRef"].Value] = new();
                }

                a.Data = (m.Groups["navName"].Value, nav);

                // transform
                if (m.Groups["xf"].Success)
                {
                    var m2 = NavTransformRegex().Match(m.Groups["xf"].Value);
                    if (!m2.Success)
                        throw new MetaParserException("Invalid nav transform");

                    action = new EmbeddedNavRouteMetaActionWithTransform(a)
                    {
                        Transform = m2.Groups["d"].Captures.Select(c => double.Parse(c.Value)).ToArray()
                    };
                }
                break;

            case MetaAction<string> a:
                a.Data = m.Groups["arg"].Value.UnescapeString();
                break;

            case ExpressionMetaAction a:
                a.Expression = m.Groups["arg"].Value.UnescapeString();
                break;

            case SetVTOptionMetaAction a:
                a.Option = m.Groups["arg1"].Value.UnescapeString();
                a.Value = m.Groups["arg2"].Value.UnescapeString();
                break;

            case GetVTOptionMetaAction a:
                a.Option = m.Groups["arg1"].Value.UnescapeString();
                a.Variable = m.Groups["arg2"].Value.UnescapeString();
                break;

            case CallStateMetaAction a:
                a.CallState = m.Groups["arg1"].Value.UnescapeString();
                a.ReturnState = m.Groups["arg2"].Value.UnescapeString();
                break;

            case DestroyViewMetaAction a:
                a.ViewName = m.Groups["arg"].Value.UnescapeString();
                break;

            case CreateViewMetaAction a:
                a.ViewName = m.Groups["arg1"].Value.UnescapeString();
                a.ViewDefinition = m.Groups["arg2"].Value.UnescapeString();

                // Check for XML file
                if (a.ViewDefinition.StartsWith(':'))
                {
                    var fileName = a.ViewDefinition[1..].Trim();
                    if (fileSystemService.FileExists(fileName))
                    {
                        using var fs = fileSystemService.OpenFileForReadAccess(fileName);
                        using var reader = new StreamReader(fs);
                        a.ViewDefinition = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }
                    else
                        throw new MetaParserException($"External file not found: {fileName}");
                }
                break;

            case WatchdogSetMetaAction a:
                a.Range = double.TryParse(m.Groups["range"].Value, out var range) ? range : throw new MetaParserException("Invalid set watchdog definition");
                a.Time = int.TryParse(m.Groups["time"].Value, out var time) ? time : throw new MetaParserException("Invalid set watchdog definition");
                a.State = m.Groups["state"].Value.UnescapeString();
                break;
        }

        return action;
    }

    private async Task<(string, Condition)> ParseConditionAsync(string line, TextReader reader, int indentLevel)
    {
        // skip whitespace
        while (line != null && EmptyLineRegex().IsMatch(line))
            line = await reader.ReadLineAsync().ConfigureAwait(false);

        if (line == null || StateNavRegex().IsMatch(line) || DoRegex().IsMatch(line))
            return (line, null);

        var m = ConditionRegex().Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid condition definition");
        else if (indentLevel > 0 && m.Groups["tabs"].Length > indentLevel)
            throw new MetaParserException("Too many tabs in condition definition", indentLevel.ToString(), m.Groups["tabs"].Length.ToString());
        else if (m.Groups["tabs"].Length < indentLevel)
            return (line, null);

        Condition cond;
        switch (m.Groups["cond"].Value)
        {
            case "Not":
                cond = Condition.CreateCondition(ConditionType.Not);
                (line, ((NotCondition)cond).Data) = await ParseConditionAsync(line.Substring(m.Index + m.Length), reader, 0).ConfigureAwait(false);
                break;

            case "All":
            case "Any":
                cond = Condition.CreateCondition(m.Groups["cond"].Value == "All" ? ConditionType.All : ConditionType.Any);

                line = line.Substring(m.Groups["cond"].Index + m.Groups["cond"].Length);
                while (line != null && EmptyLineRegex().IsMatch(line))
                    line = await reader.ReadLineAsync().ConfigureAwait(false);

                while (line != null && !StateNavRegex().IsMatch(line) && !DoRegex().IsMatch(line))
                {
                    (line, var c) = await ParseConditionAsync(line, reader, indentLevel != 0 ? indentLevel + 1 : 3);
                    if (c != null)
                        ((MultipleCondition)cond).Data.Add(c);
                    else
                        break;
                }
                break;

            case string s when ConditionList.ContainsKey(s):
                cond = ParseConditionLine(s, line.Substring(m.Groups["cond"].Index + m.Groups["cond"].Length));

                do { line = await reader.ReadLineAsync().ConfigureAwait(false); } while (line != null && EmptyLineRegex().IsMatch(line));
                break;

            default:
                throw new MetaParserException($"Invalid condition type: {m.Groups["cond"].Value}");
        };

        return (line, cond);
    }

    private Condition ParseConditionLine(string condString, string line)
    {
        var cond = Condition.CreateCondition(ConditionList[condString]);

        if (ConditionListRegex.ContainsKey(condString))
        {
            var m = ConditionListRegex[condString].Match(line);
            if (!m.Success)
                throw new MetaParserException($"Invalid {cond.Type} condition definition");

            switch (cond)
            {
                case Condition<int> c when c.Type == ConditionType.LandBlockE || c.Type == ConditionType.LandCellE:
                    c.Data = int.Parse(m.Groups["cell"].Value, System.Globalization.NumberStyles.HexNumber);
                    break;

                case Condition<int> c when c.Type == ConditionType.SecondsInStateGE || c.Type == ConditionType.SecondsInStatePersistGE || c.Type == ConditionType.BurdenPercentGE || c.Type == ConditionType.MainPackSlotsLE:
                    c.Data = int.Parse(m.Groups["arg"].Value);
                    break;

                case NoMonstersInDistanceCondition c:
                    c.Distance = double.Parse(m.Groups["distance"].Value);
                    break;

                case ExpressionCondition c:
                    c.Expression = m.Groups["arg"].Value.UnescapeString();
                    break;

                case ChatMessageCaptureCondition c:
                    c.Pattern = m.Groups["arg1"].Value.UnescapeString();
                    c.Color = m.Groups["arg2"].Value.UnescapeString();
                    break;

                case Condition<string> c:
                    c.Data = m.Groups["arg"].Value.UnescapeString();
                    break;

                case MonstersWithPriorityWithinDistanceCondition c:
                    c.Count = int.Parse(m.Groups["count"].Value);
                    c.Distance = double.Parse(m.Groups["distance"].Value);
                    c.Priority = int.Parse(m.Groups["priority"].Value);
                    break;

                case DistanceToAnyRoutePointGECondition c:
                    c.Distance = double.Parse(m.Groups["distance"].Value);
                    break;

                case ItemCountCondition c:
                    c.Count = int.Parse(m.Groups["count"].Value);
                    c.ItemName = m.Groups["item"].Value.UnescapeString();
                    break;

                case MonsterCountWithinDistanceCondition c:
                    c.Count = int.Parse(m.Groups["count"].Value);
                    c.Distance = double.Parse(m.Groups["distance"].Value);
                    c.MonsterNameRx = m.Groups["name"].Value.UnescapeString();
                    break;

                case TimeLeftOnSpellGECondition c:
                    c.Seconds = int.Parse(m.Groups["seconds"].Value);
                    c.SpellId = int.Parse(m.Groups["spell"].Value);
                    break;
            }
        }

        return cond;
    }
}
