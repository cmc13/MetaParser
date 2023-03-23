using MetaParser.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MetaParser.Formatting
{
    public class DefaultMetaReader : IMetaReader
    {
        private static readonly string[] headerLines = { "1", "CondAct", "5", "CType", "AType", "CData", "AData", "State", "n", "n", "n", "n", "n" };
        private readonly INavReader navReader;

        public DefaultMetaReader(INavReader navReader)
        {
            this.navReader = navReader;
        }

        public async Task<Meta> ReadMetaAsync(Stream stream)
        {
            var lineNumber = 0;
            using var reader = new StreamReader(stream);
            foreach (var headerLine in headerLines)
            {
                lineNumber++;
                var readLine = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!readLine.Equals(headerLine))
                    throw new System.Exception($"[line {lineNumber}] Invalid meta header line (expected: {headerLine}; actual: {readLine})");
            }

            var m = new Meta();
            lineNumber++;
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(line) || !int.TryParse(line, out var ruleCount))
                throw new System.Exception($"[line {lineNumber}] Unable to read rule count from meta header");

            for (var i = 0; i < ruleCount; ++i)
            {
                var rule = await ReadRuleAsync(reader);
                m.Rules.Add(rule);
            }

            return m;
        }

        public async Task<Rule> ReadRuleAsync(TextReader reader)
        {
            // Read Condition Type
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (line != "i")
                throw new MetaParserException("Invalid meta condition type identifier", "i", line);

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!int.TryParse(line, out var conditionTypeInt) || !Enum.IsDefined(typeof(ConditionType), conditionTypeInt))
                throw new MetaParserException("Invalid meta condition type", typeof(int), line);

            var condition = Condition.CreateCondition((ConditionType)conditionTypeInt);

            // Read Action Type
            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (line != "i")
                throw new MetaParserException("Invalid meta action type identifier", "i", line);

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!int.TryParse(line, out var actionTypeInt) || !Enum.IsDefined(typeof(ActionType), actionTypeInt))
                throw new MetaParserException("Invalid meta action type", typeof(int), line);

            var action = MetaAction.CreateMetaAction((ActionType)actionTypeInt);

            await ReadConditionAsync(reader, condition).ConfigureAwait(false);
            await ReadActionAsync(reader, action).ConfigureAwait(false);

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!line.Equals("s", StringComparison.OrdinalIgnoreCase))
                throw new MetaParserException("Unable to parse state type identifier from meta file", "s", line);

            line = await reader.ReadLineAsync().ConfigureAwait(false);

            return new Rule()
            {
                State = line,
                Condition = condition,
                Action = action
            };
        }

        public async Task ReadActionAsync(TextReader reader, MetaAction action)
        {
            if (action is MetaAction<string> cStr)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!line.Equals("s"))
                    throw new Exception("Invalid set state meta action");

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                cStr.Data = line;
            }
            else if (action is MetaAction<int> cInt)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!line.Equals("i"))
                    throw new Exception("Invalid return meta action");

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!int.TryParse(line, out var data))
                    throw new Exception("Invalid return meta action");

                cInt.Data = data;
            }
            else if (action is TableMetaAction cTbl)
            {
                cTbl.Data = await ReadTableAsync(reader).ConfigureAwait(false);
            }
            else if (action is AllMetaAction cAll)
            {
                foreach (var headerLine in new[] { "TABLE", "2", "K", "V", "n", "n" })
                {
                    var readLine = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (!readLine.Equals(headerLine))
                        throw new Exception($"Invalid Expression action header (expected: {headerLine}; actual {readLine})");
                }

                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!int.TryParse(line, out var actionCount))
                    throw new Exception("Unable to parse action count from meta file");

                for (var i = 0; i < actionCount; ++i)
                {
                    line = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (!line.Equals("i"))
                        throw new Exception("Invalid all meta rule");

                    line = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (!int.TryParse(line, out var actionTypeInt))
                        throw new Exception("Unable to parse action type from meta file");

                    var subAction = MetaAction.CreateMetaAction((ActionType)actionTypeInt);
                    await ReadActionAsync(reader, subAction).ConfigureAwait(false);

                    cAll.Data.Add(subAction);
                }
            }
            else if (action is EmbeddedNavRouteMetaAction cNav)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!line.Equals("ba"))
                    throw new Exception("Invalid embedded nav route action");

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!int.TryParse(line, out var exactCharCount))
                    throw new Exception("Invalid embedded nav route action");

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                var navName = line;
                if (navName.Equals("[None]"))
                    navName = null;

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!int.TryParse(line, out _))
                    throw new Exception("Invalid embedded nav route action");

                var nav = new NavRoute();
                if (exactCharCount > 5)
                    await navReader.ReadNavAsync(reader, nav).ConfigureAwait(false);

                cNav.Data = (navName, nav);
            }
            else
                throw new InvalidOperationException($"Invalid action type, {nameof(ReadActionAsync)} should be implemented elsewhere.");
        }

        public async Task ReadConditionAsync(TextReader reader, Condition condition)
        {
            if (condition is Condition<int> cInt)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line != "i")
                    throw new MetaParserException("Invalid meta condition identifier", "i", line);

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!int.TryParse(line, out var data))
                    throw new MetaParserException("Invalid meta condition value");

                cInt.Data = data;
            }
            else if (condition is Condition<string> cStr)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line != "s")
                    throw new MetaParserException("Invalid meta condition identifier", "s", line);

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                cStr.Data = line;
            }
            else if (condition is TableCondition cTbl)
            {
                cTbl.Data = await ReadTableAsync(reader).ConfigureAwait(false);
            }
            else if (condition is NotCondition cNot)
            {
                var conditions = await ReadConditionListAsync(reader).ToListAsync().ConfigureAwait(false);
                if (conditions.Count > 1)
                    throw new MetaParserException("Not condition can only contain 1 sub-condition");

                cNot.Data = conditions.SingleOrDefault();
            }
            else if (condition is MultipleCondition cMlt)
            {
                await foreach (var cnd in ReadConditionListAsync(reader).ConfigureAwait(false))
                {
                    cMlt.Data.Add(cnd);
                }
            }
            else
                throw new InvalidOperationException($"Invalid condition type, {nameof(ReadConditionAsync)} should be implemented elsewhere.");
        }

        private static async Task<OrderedDictionary> ReadTableAsync(TextReader reader)
        {
            foreach (var headerLine in new[] { "TABLE", "2", "k", "v", "n", "n" })
            {
                var readLine = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!readLine.Equals(headerLine))
                    throw new MetaParserException("Invalid table header", headerLine, readLine);
            }

            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!int.TryParse(line, out var rowCount))
                throw new MetaParserException("Invalid table row count", typeof(int), line);

            var dict = new OrderedDictionary();
            for (var i = 0; i < rowCount; ++i)
            {
                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (line != "s")
                    throw new MetaParserException("Invalid table row key type", "s", line);

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                var key = line;

                var valueType = await reader.ReadLineAsync().ConfigureAwait(false);

                dict.Add(key, await ParseLine(valueType, reader).ConfigureAwait(false));
            }

            return dict;
        }

        private static async Task<object> ParseLine(string type, TextReader reader)
        {
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            return type switch
            {
                "s" => line,
                "i" => int.TryParse(line, out var i) ? i : throw new MetaParserException("Invalid table value", typeof(int), line),
                "d" => double.TryParse(line, out var d) ? d : throw new MetaParserException("Invalid table value", typeof(double), line),
                "ba" => int.TryParse(line, out var charCount) ? await ReadXMLAsync(reader, charCount).ConfigureAwait(false) : throw new MetaParserException("Invalid char count for XML definition", typeof(int), line),
                _ => throw new MetaParserException("Invalid table value type", "s|i|d", type)
            };
        }

        private async IAsyncEnumerable<Condition> ReadConditionListAsync(TextReader reader)
        {
            foreach (var headerLine in new[] { "TABLE", "2", "K", "V", "n", "n" })
            {
                var readLine = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!readLine.Equals(headerLine))
                    throw new MetaParserException("Invalid key/value list header", headerLine, readLine);
            }

            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!int.TryParse(line, out var conditionCount))
                throw new MetaParserException("Invalid condition count", typeof(int), line);

            for (var i = 0; i < conditionCount; ++i)
            {
                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!line.Equals("i"))
                    throw new MetaParserException("Invalid condition type identifier", "i", line);

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!int.TryParse(line, out var conditionTypeInt))
                    throw new MetaParserException("Invalid condition type", typeof(int), line);

                var condition = Condition.CreateCondition((ConditionType)conditionTypeInt);

                await ReadConditionAsync(reader, condition).ConfigureAwait(false);

                yield return condition;
            }
        }

        private static async Task<ViewString> ReadXMLAsync(TextReader reader, int charCount)
        {
            var array = new char[charCount];
            var me = new Memory<char>(array);
            await reader.ReadAsync(me).ConfigureAwait(false);
            return new ViewString { String = new(array) };
        }
    }
}
