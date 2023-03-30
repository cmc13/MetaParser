using MetaParser.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MetaParser.Formatting
{
    static class TextReaderExtensions
    {
        public static string ReadUntil(this TextReader reader, int ch)
        {
            var builder = new StringBuilder();

            for (var cTest = reader.Peek();
                cTest != -1 && cTest != ch;
                cTest = reader.Peek())
            {
                builder.Append((char)reader.Read());
            }

            return builder.ToString();
        }
    }

    public class XMLMetaReader : IMetaReader
    {
        private readonly INavReader navReader;

        public XMLMetaReader(INavReader navReader)
        {
            this.navReader = navReader;
        }

        private async Task ReadAction(TextReader reader, MetaAction action)
        {
            Match m = null;

            switch (action)
            {
                case MetaAction<string> cStr:
                    cStr.Data = reader.ReadUntil('}');
                    break;

                case MetaAction<int> cInt:
                    var actionString = reader.ReadUntil('}');
                    if (!int.TryParse(actionString, out var actionData))
                        throw new Exception($"Invalid return meta action: {actionString}");

                    cInt.Data = actionData;
                    break;

                case AllMetaAction cAll:
                    for (var ch2 = reader.Peek(); ch2 != -1 && ch2 != '}'; ch2 = reader.Peek())
                    {
                        var aType = reader.ReadUntil('{');

                        // consume beginning bracket
                        if ((ch2 = reader.Read()) != '{')
                            throw new MetaParserException("Invalid action syntax", "{", ch2 > 0 ? ch2.ToString() : "<EOF>");

                        if (!Enum.TryParse<ActionType>(aType, out var actionType))
                            throw new MetaParserException("Invalid meta action type");

                        var innerAction = MetaAction.CreateMetaAction(actionType);
                        await ReadAction(reader, innerAction).ConfigureAwait(false);

                        // consume end bracket
                        if ((ch2 = reader.Read()) != '}')
                            throw new MetaParserException("Invalid action syntax", "}", ch2 > 0 ? ch2.ToString() : "<EOF>");

                        cAll.Data.Add(innerAction);
                    }
                    break;

                case EmbeddedNavRouteMetaAction cNav:
                    var navName = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (!int.TryParse(await reader.ReadLineAsync().ConfigureAwait(false), out _))
                        throw new Exception("Invalid embedded nav route action");
                    var navRoute = await navReader.ReadNavAsync(reader).ConfigureAwait(false);
                    cNav.Data = (navName, navRoute);
                    break;

                case DestroyViewMetaAction dva:
                    dva.ViewName = reader.ReadUntil('}');
                    break;

                case CreateViewMetaAction cva:
                    var viewName = reader.ReadUntil(';');
                    cva.ViewName = viewName;
                    if (reader.Read() != ';')
                        throw new MetaParserException("Invalid CreateViewMetaAction syntax");
                    var viewData = reader.ReadUntil('}');
                    cva.ViewDefinition = viewData;
                    break;

                case ExpressionMetaAction ea:
                    ea.Expression = reader.ReadUntil('}');
                    break;

                case SetVTOptionMetaAction soa:
                    m = Regex.Match(reader.ReadUntil('}'), @"^(?<option>.*?);(?<value>.*)$");
                    if (!m.Success)
                        throw new MetaParserException("Invalid set VT option meta action");
                    soa.Option = m.Groups["option"].Value;
                    soa.Value = m.Groups["value"].Value;
                    break;

                case GetVTOptionMetaAction goa:
                    m = Regex.Match(reader.ReadUntil('}'), @"^(?<option>.*?);(?<variable>.*)$");
                    if (!m.Success)
                        throw new MetaParserException("Invalid get VT option meta action");
                    goa.Option = m.Groups["option"].Value;
                    goa.Variable = m.Groups["variable"].Value;
                    break;

                case CallStateMetaAction csa:
                    m = Regex.Match(reader.ReadUntil('}'), @"^(?<call>.*);(?<return>.*)$");
                    if (!m.Success)
                        throw new MetaParserException("Invalid get VT option meta action");
                    csa.CallState = m.Groups["call"].Value;
                    csa.ReturnState = m.Groups["return"].Value;
                    break;

                case WatchdogSetMetaAction wsa:
                    m = Regex.Match(reader.ReadUntil('}'), @"^(?<state>.*);(?<range>\d+([.]\d+)?);(?<time>\d+([.]\d+)?)$");
                    if (!m.Success)
                        throw new MetaParserException("Invalid watchdog set meta action");
                    wsa.State = m.Groups["state"].Value;
                    if (!double.TryParse(m.Groups["range"].ValueSpan, out var range))
                        throw new MetaParserException($"Invalid range string: {m.Groups["range"].Value}");
                    wsa.Range = range;
                    if (!double.TryParse(m.Groups["time"].ValueSpan, out var time))
                        throw new MetaParserException($"Invalid time string: {m.Groups["time"].Value}");
                    wsa.Time = time;
                    break;

                default:
                    throw new InvalidOperationException($"Invalid action type, {nameof(ReadAction)} should be implemented elsewhere.");
            }
        }

        private void ReadCondition(TextReader reader, Condition condition)
        {
            Match m = null;

            switch (condition)
            {
                case Condition<string> cStr:
                    cStr.Data = reader.ReadUntil('}');
                    break;

                case Condition<int> cInt:
                    var style = (condition.Type == ConditionType.LandBlockE || condition.Type == ConditionType.LandCellE) ?
                        System.Globalization.NumberStyles.HexNumber :
                        System.Globalization.NumberStyles.Integer;
                    if (!int.TryParse(reader.ReadUntil('}'), style, null, out var conditionData))
                        throw new MetaParserException("Invalid meta condition value");

                    cInt.Data = conditionData;
                    break;

                case NotCondition cNot:
                    var conditionList = ReadConditionList(reader).ToList();
                    if (conditionList.Count > 1)
                        throw new MetaParserException("Not condition can only contain 1 sub-condition");
                    cNot.Data = conditionList.SingleOrDefault();
                    break;

                case MultipleCondition cMult:
                    cMult.Data = ReadConditionList(reader).ToList();
                    break;

                case ExpressionCondition ec:
                    ec.Expression = reader.ReadUntil('}');
                    break;

                case DistanceToAnyRoutePointGECondition dc:
                    if (!double.TryParse(reader.ReadUntil('}'), out var distanceToAnyRoutePointGE))
                        throw new MetaParserException("Invalid distance string");
                    dc.Distance = distanceToAnyRoutePointGE;
                    break;

                case NoMonstersInDistanceCondition nmc:
                    if (!double.TryParse(reader.ReadUntil('}'), out var distance))
                        throw new MetaParserException("Invalid distance string");
                    nmc.Distance = distance;
                    break;

                case MonsterCountWithinDistanceCondition mcc:
                    ParseMonsterCountWithinDistance(reader.ReadUntil('}'), mcc);
                    break;

                case MonstersWithPriorityWithinDistanceCondition mpc:
                    ParseMonsterPriorityWithinDistance(reader.ReadUntil('}'), mpc);
                    break;

                case ItemCountCondition ic:
                    m = Regex.Match(reader.ReadUntil('}'), @"^(?<item>.*);(?<count>\d+)$");
                    if (!m.Success)
                        throw new MetaParserException("Invalid condition string");
                    ic.ItemName = m.Groups["item"].Value;
                    if (!int.TryParse(m.Groups["count"].ValueSpan, out var count))
                        throw new MetaParserException($"Invalid count string: {m.Groups["count"].ValueSpan}");
                    ic.Count = count;
                    break;

                case ChatMessageCaptureCondition cmc:
                    m = Regex.Match(reader.ReadUntil('}'), @"^(?<message>.*);(?<color>.*)$");
                    if (!m.Success)
                        throw new MetaParserException("Invalid condition string");
                    cmc.Pattern = m.Groups["message"].Value;
                    cmc.Color = m.Groups["color"].Value;
                    break;

                case TimeLeftOnSpellGECondition tsc:
                    m = Regex.Match(reader.ReadUntil('}'), @"^(?<spell>\d+);(?<time>\d+)$");
                    if (!m.Success)
                        throw new MetaParserException("Invalid condition string");
                    if (!int.TryParse(m.Groups["spell"].ValueSpan, out var spellId))
                        throw new MetaParserException($"Invalid spell id string: {m.Groups["spell"].ValueSpan}");
                    tsc.SpellId = spellId;
                    if (!int.TryParse(m.Groups["time"].ValueSpan, out var time))
                        throw new MetaParserException($"Invalid time string: {m.Groups["time"].ValueSpan}");
                    tsc.Seconds = time;
                    break;

                default:
                    throw new InvalidOperationException($"Invalid condition type, {nameof(ReadCondition)} should be implemented elsewhere.");
            }
        }

        private IEnumerable<Condition> ReadConditionList(TextReader reader)
        {
            for (var ch = reader.Peek(); ch != -1 && ch != '}'; ch = reader.Peek())
            {
                var cType = reader.ReadUntil('{');

                if ((ch = reader.Read()) != '{')
                    throw new MetaParserException("Invalid condition syntax", "{", ch > 0 ? ch.ToString() : "<EOF>");

                if (!Enum.TryParse<ConditionType>(cType.Trim(' ', ':'), out var conditionType))
                    throw new MetaParserException($"Invalid meta condition type: {cType}");
                var innerCondition = Condition.CreateCondition(conditionType);
                ReadCondition(reader, innerCondition);

                if ((ch = reader.Read()) != '}')
                    throw new MetaParserException("Invalid condition syntax", "}", ch > 0 ? ch.ToString() : "<EOF>");

                yield return innerCondition;
            }
        }

        private static void ParseMonsterPriorityWithinDistance(string conditionText, MonstersWithPriorityWithinDistanceCondition mpc)
        {
            var m = Regex.Match(conditionText, @"^(?<priority>\d+);(?<count>\d+);(?<distance>\d+([.]\d+)?)$");
            if (!m.Success)
                throw new MetaParserException($"Invalid condition string: {conditionText}");
            if (!int.TryParse(m.Groups["priority"].ValueSpan, out var priority))
                throw new MetaParserException($"Invalid priority string: {priority}");
            mpc.Priority = priority;
            if (!int.TryParse(m.Groups["count"].ValueSpan, out var count))
                throw new MetaParserException($"Invalid count string: {count}");
            mpc.Count = count;
            if (!double.TryParse(m.Groups["distance"].ValueSpan, out var distance))
                throw new MetaParserException($"Invalid distance string: {distance}");
            mpc.Distance = distance;
        }

        private static void ParseMonsterCountWithinDistance(string conditionText, MonsterCountWithinDistanceCondition mcc)
        {
            var m = Regex.Match(conditionText, @"^(?<name>.*);(?<count>\d+);(?<distance>\d+([.]\d+)?)$");
            if (!m.Success)
                throw new MetaParserException($"Invalid condition string: {conditionText}");
            mcc.MonsterNameRx = m.Groups["name"].Value;
            if (!int.TryParse(m.Groups["count"].ValueSpan, out var count))
                throw new MetaParserException($"Invalid count string: {count}");
            mcc.Count = count;
            if (!double.TryParse(m.Groups["distance"].ValueSpan, out var distance))
                throw new MetaParserException($"Invalid distance string: {distance}");
            mcc.Distance = distance;
        }

        public async Task<Meta> ReadMetaAsync(Stream stream)
        {
            var m = new Meta();
            DataTable dt = new();
            dt.ReadXml(stream);

            foreach (DataRow row in dt.Rows)
            {
                var rule = await ReadRuleAsync(row).ConfigureAwait(false);
                m.Rules.Add(rule);
            }

            return m;
        }

        private async Task<Models.Rule> ReadRuleAsync(DataRow row)
        {
            if (!Enum.TryParse<ConditionType>((string)row[0], out var conditionType))
                throw new MetaParserException("Invalid meta condition type");

            if (!Enum.TryParse<ActionType>((string)row[1], out var actionType))
                throw new MetaParserException("Invalid meta action type");

            var state = (string)row[4];

            var condition = Condition.CreateCondition(conditionType);
            var action = MetaAction.CreateMetaAction(actionType);

            using var conditionReader = new StringReader((string)row[2]);
            using var actionReader = new StringReader((string)row[3]);
            ReadCondition(conditionReader, condition);
            await ReadAction(actionReader, action).ConfigureAwait(false);

            return new()
            {
                State = state,
                Condition = condition,
                Action = action
            };
        }
    }
}
