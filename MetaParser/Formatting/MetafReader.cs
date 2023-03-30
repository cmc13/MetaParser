using MetaParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MetaParser.Formatting
{
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
        private static readonly Regex NavRegex = new(@"^\s*(?<navRef>\S*)\s*(?<navType>\S*)\s*(~~.*)?", RegexOptions.Compiled);
        private static readonly Regex IfRegex = new(@"^\s*(?<op>IF:)", RegexOptions.Compiled);
        private static readonly Regex DoRegex = new(@"^\s*(?<op>DO:)", RegexOptions.Compiled);
        private static readonly Regex ConditionRegex = new(@"^\s*(?<cond>\S*)", RegexOptions.Compiled);
        private static readonly Regex ActionRegex = new(@"^\s*(?<action>\S*)", RegexOptions.Compiled);
        private static readonly Regex PointRegex = new($@"^\s*(?<x>{DOUBLE_REGEX})\s*(?<y>{DOUBLE_REGEX})\s*(?<z>{DOUBLE_REGEX})", RegexOptions.Compiled);
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
            { "Expr", ConditionType.Expression },
            { "ChatCapture", ConditionType.ChatMessageCapture },
            { "ChatMatch", ConditionType.ChatMessage },
            { "Never", ConditionType.Never },
            { "Always", ConditionType.Always },
            { "NavEmpty", ConditionType.NavrouteEmpty },
            { "MobsInDist_Priority", ConditionType.MonstersWithPriorityWithinDistance },
            { "Death", ConditionType.Died },
            { "NeedToBuff", ConditionType.NeedToBuff },
            { "PSecsInStateGE", ConditionType.SecondsInStatePersistGE },
            { "SecsInStateGE", ConditionType.SecondsInStateGE },
            { "VendorOpen", ConditionType.VendorOpen },
            { "VendorClosed", ConditionType.VendorClosed },
            { "NoMobsInDist", ConditionType.NoMonstersWithinDistance },
            { "SecsOnSpellGE", ConditionType.TimeLeftOnSpellGE },
            { "ItemCountLE", ConditionType.ItemCountLE },
            { "ItemCountGE", ConditionType.ItemCountGE },
            { "CellE", ConditionType.LandCellE },
            { "DistToRteGE", ConditionType.DistanceToAnyRoutePointGE },
            { "MainSlotsLE", ConditionType.MainPackSlotsLE },
            { "IntoPortal", ConditionType.PortalspaceEnter },
            { "MobsInDist_Name", ConditionType.MonsterCountWithinDistance },
            { "ExitPortal", ConditionType.PortalspaceExit },
            { "BlockE", ConditionType.LandBlockE },
            { "BuPercentGE", ConditionType.BurdenPercentGE },
        };

        private static readonly Dictionary<string, ActionType> ActionList = new()
        {
            { "None", ActionType.None },
            { "EmbedNav", ActionType.EmbeddedNavRoute },
            { "ChatExpr", ActionType.ChatWithExpression },
            { "SetOpt", ActionType.SetVTOption },
            { "SetState", ActionType.SetState },
            { "CallState", ActionType.CallState },
            { "SetWatchdog", ActionType.WatchdogSet },
            { "CreateView", ActionType.CreateView },
            { "Chat", ActionType.ChatCommand },
            { "Return", ActionType.ReturnFromCall },
            { "ClearWatchdog", ActionType.WatchdogClear },
            { "DestroyView", ActionType.DestroyView },
            { "DoExpr", ActionType.ExpressionAct },
            { "GetOpt", ActionType.GetVTOption },
            { "DestroyAllViews", ActionType.DestroyAllViews },
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
                switch (m.Groups["action"].Value)
                {
                    case "None":
                    case "DestroyAllViews":
                    case "ClearWatchdog":
                    case "Return":
                        break;

                    case "EmbedNav":
                        m = ActionListRegex[m.Groups["action"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid embedded nav definition");
                        var nav = new NavRoute();
                        navReferences.Add(m.Groups["navRef"].Value, nav);
                        ((EmbeddedNavRouteMetaAction)action).Data = (m.Groups["navName"].Value, nav);
                        break;

                    case "Chat":
                    case "SetState":
                        m = ActionListRegex[m.Groups["action"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException($"Invalid chat/set state definition");
                        ((MetaAction<string>)action).Data = m.Groups["arg"].Value;
                        break;

                    case "ChatExpr":
                    case "DoExpr":
                        m = ActionListRegex[m.Groups["action"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid expression action definition");
                        ((ExpressionMetaAction)action).Expression = m.Groups["arg"].Value;
                        break;

                    case "SetOpt":
                        m = ActionListRegex[m.Groups["action"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid set option definition");
                        ((SetVTOptionMetaAction)action).Option = m.Groups["arg1"].Value;
                        ((SetVTOptionMetaAction)action).Value = m.Groups["arg2"].Value;
                        break;

                    case "GetOpt":
                        m = ActionListRegex[m.Groups["action"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid get option definition");
                        ((GetVTOptionMetaAction)action).Option = m.Groups["arg1"].Value;
                        ((GetVTOptionMetaAction)action).Variable = m.Groups["arg2"].Value;
                        break;

                    case "CallState":
                        m = ActionListRegex[m.Groups["action"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid call state definition");
                        ((CallStateMetaAction)action).CallState = m.Groups["arg1"].Value;
                        ((CallStateMetaAction)action).ReturnState = m.Groups["arg2"].Value;
                        break;

                    case "DestroyView":
                        m = ActionListRegex[m.Groups["action"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid destroy view definition");
                        ((DestroyViewMetaAction)action).ViewName = m.Groups["arg"].Value;
                        break;

                    case "CreateView":
                        m = ActionListRegex[m.Groups["action"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid create view definition");
                        ((CreateViewMetaAction)action).ViewName = m.Groups["arg1"].Value;
                        ((CreateViewMetaAction)action).ViewDefinition = m.Groups["arg2"].Value;
                        break;

                    case "SetWatchdog":
                        m = ActionListRegex[m.Groups["action"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid set watchdog definition");
                        ((WatchdogSetMetaAction)action).Range = double.TryParse(m.Groups["arg1"].Value, out var range) ? range : throw new MetaParserException("Invalid set watchdog definition");
                        ((WatchdogSetMetaAction)action).Time = int.TryParse(m.Groups["arg2"].Value, out var time) ? time : throw new MetaParserException("Invalid set watchdog definition");
                        ((WatchdogSetMetaAction)action).State = m.Groups["arg3"].Value;
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
                switch (m.Groups["cond"].Value)
                {
                    case "Never":
                    case "Always":
                    case "NavEmpty":
                    case "Death":
                    case "NeedToBuff":
                    case "VendorOpen":
                    case "VendorClosed":
                    case "IntoPortal":
                    case "ExitPortal":
                        break;

                    case "NoMobsInDist":
                        m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid no mobs in distance definition");
                        ((NoMonstersInDistanceCondition)cond).Distance = double.Parse(m.Groups["arg"].Value);
                        break;

                    case "Expr":
                        m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid expression condition definition");
                        ((ExpressionCondition)cond).Expression = m.Groups["arg"].Value;
                        break;

                    case "ChatCapture":
                        m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid chat capture definition");
                        ((ChatMessageCaptureCondition)cond).Pattern = m.Groups["arg1"].Value;
                        ((ChatMessageCaptureCondition)cond).Color = m.Groups["arg2"].Value;
                        break;

                    case "ChatMatch":
                        m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid chat match definition");
                        ((Condition<string>)cond).Data = m.Groups["arg"].Value;
                        break;

                    case "MobsInDist_Priority":
                        m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid priority monsters in distance definition");
                        ((MonstersWithPriorityWithinDistanceCondition)cond).Count = int.Parse(m.Groups["arg1"].Value);
                        ((MonstersWithPriorityWithinDistanceCondition)cond).Distance = double.Parse(m.Groups["arg2"].Value);
                        ((MonstersWithPriorityWithinDistanceCondition)cond).Priority = int.Parse(m.Groups["arg3"].Value);
                        break;

                    case "DistToRteGE":
                        m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid distance to any route point definition");
                        ((DistanceToAnyRoutePointGECondition)cond).Distance = double.Parse(m.Groups["arg"].Value);
                        break;

                    case "PSecsInStateGE":
                    case "SecsInStateGE":
                    case "BuPercentGE":
                    case "MainSlotsLE":
                        m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException($"Invalid integer condition definition");
                        ((Condition<int>)cond).Data = int.Parse(m.Groups["arg"].Value);
                        break;

                    case "ItemCountLE":
                    case "ItemCountGE":
                        m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid item count definition");
                        ((ItemCountCondition)cond).Count = int.Parse(m.Groups["arg1"].Value);
                        ((ItemCountCondition)cond).ItemName = m.Groups["arg2"].Value;
                        break;

                    case "CellE":
                    case "BlockE":
                        m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid landcell/landblock condition definition");
                        ((Condition<int>)cond).Data = int.Parse(m.Groups["arg"].Value, System.Globalization.NumberStyles.HexNumber);
                        break;

                    case "MobsInDist_Name":
                        m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid mobs in distance by name definition");
                        ((MonsterCountWithinDistanceCondition)cond).Count = int.Parse(m.Groups["arg1"].Value);
                        ((MonsterCountWithinDistanceCondition)cond).Distance = double.Parse(m.Groups["arg2"].Value);
                        ((MonsterCountWithinDistanceCondition)cond).MonsterNameRx = m.Groups["arg3"].Value;
                        break;

                    case "SecsOnSpellGE":
                        m = ConditionListRegex[m.Groups["cond"].Value].Match(line);
                        if (!m.Success)
                            throw new MetaParserException("Invalid time left on spell definition");
                        ((TimeLeftOnSpellGECondition)cond).Seconds = int.Parse(m.Groups["arg1"].Value);
                        ((TimeLeftOnSpellGECondition)cond).SpellId = int.Parse(m.Groups["arg2"].Value);
                        break;
                }

                await enumerator.MoveNextAsync().ConfigureAwait(false);
            }
            else
                throw new MetaParserException($"Invalid condition type: {m.Groups["cond"].Value}");

            return cond;
        }

        private static (double x, double y, double z) ParsePoint(ref string line)
        {
            var m = PointRegex.Match(line);
            if (!m.Success)
                throw new MetaParserException("Invalid nav point definition");

            line = line.Substring(m.Index + m.Length);

            return (
                double.Parse(m.Groups["x"].Value),
                double.Parse(m.Groups["y"].Value),
                double.Parse(m.Groups["z"].Value)
            );
        }
    }
}
