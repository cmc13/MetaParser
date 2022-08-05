using MetaParser.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MetaParser.Formatting
{
    public class DefaultMetaWriter : IMetaWriter
    {
        private static readonly string[] headerLines = { "1", "CondAct", "5", "CType", "AType", "CData", "AData", "State", "n", "n", "n", "n", "n" };
        private readonly INavWriter navFormatter;

        public DefaultMetaWriter(INavWriter navWriter)
        {
            this.navFormatter = navWriter;
        }

        public async Task WriteMetaAsync(Stream stream, Meta meta)
        {
            using var writer = new StreamWriter(stream);

            foreach (var headerLine in headerLines)
            {
                writer.WriteLine(headerLine);
            }

            // Append all together, preserve order
            var bigList = new List<Rule>();
            foreach (var kv in meta.States.OrderBy(m => m.Key))
                bigList.AddRange(kv.Value);

            await writer.WriteLineAsync(bigList.Count.ToString()).ConfigureAwait(false);

            foreach (var rule in bigList)
                await WriteRuleAsync(writer, rule).ConfigureAwait(false);
        }

        public async Task WriteRuleAsync(TextWriter writer, Rule rule)
        {
            await writer.WriteLineAsync("i").ConfigureAwait(false);
            await writer.WriteLineAsync(((int)rule.Condition.Type).ToString()).ConfigureAwait(false);
            await writer.WriteLineAsync("i").ConfigureAwait(false);
            await writer.WriteLineAsync(((int)rule.Action.Type).ToString()).ConfigureAwait(false);
            await WriteConditionAsync(writer, rule.Condition).ConfigureAwait(false);
            await WriteActionAsync(writer, rule.Action).ConfigureAwait(false);
            await writer.WriteLineAsync("s").ConfigureAwait(false);
            await writer.WriteLineAsync(rule.State).ConfigureAwait(false);
        }

        public async Task WriteActionAsync(TextWriter writer, MetaAction action)
        {
            if (action is MetaAction<string> cStr)
            {
                await writer.WriteLineAsync("s").ConfigureAwait(false);
                await writer.WriteLineAsync(cStr.Data).ConfigureAwait(false);
            }
            else if (action is MetaAction<int> cInt)
            {
                await writer.WriteLineAsync("i").ConfigureAwait(false);
                await writer.WriteLineAsync(cInt.Data.ToString()).ConfigureAwait(false);
            }
            else if (action is TableMetaAction cTbl)
            {
                await WriteDataListAsync(writer, cTbl.Data).ConfigureAwait(false);
            }
            else if (action is AllMetaAction cAll)
            {
                await writer.WriteLineAsync("TABLE").ConfigureAwait(false);
                await writer.WriteLineAsync("2").ConfigureAwait(false);
                await writer.WriteLineAsync("K").ConfigureAwait(false);
                await writer.WriteLineAsync("V").ConfigureAwait(false);
                await writer.WriteLineAsync("n").ConfigureAwait(false);
                await writer.WriteLineAsync("n").ConfigureAwait(false);
                await writer.WriteLineAsync(cAll.Data.Count.ToString()).ConfigureAwait(false);

                foreach (var subAction in cAll.Data)
                {
                    await writer.WriteLineAsync("i").ConfigureAwait(false);
                    await writer.WriteLineAsync(((int)subAction.Type).ToString()).ConfigureAwait(false);
                    await WriteActionAsync(writer, subAction).ConfigureAwait(false);
                }
            }
            else if (action is EmbeddedNavRouteMetaAction cNav)
            {
                await writer.WriteLineAsync("ba");

                // Use string writer to get char count of nav
                using var sWriter = new StringWriter();
                await sWriter.WriteLineAsync(cNav.Data.name ?? "[None]").ConfigureAwait(false);
                if (cNav.Data.nav.Data is List<NavNode> list && list != null)
                    await sWriter.WriteLineAsync(list.Count.ToString()).ConfigureAwait(false);
                else if (cNav.Data.nav.Data is NavFollow)
                    await sWriter.WriteLineAsync('1').ConfigureAwait(false);
                await navFormatter.WriteNavAsync(sWriter, cNav.Data.nav).ConfigureAwait(false);

                var navStr = sWriter.ToString();
                await writer.WriteLineAsync(navStr.Length.ToString()).ConfigureAwait(false);
                await writer.WriteAsync(navStr).ConfigureAwait(false);
            }
            else
                throw new NotImplementedException();
        }

        public async Task WriteConditionAsync(TextWriter writer, Condition condition)
        {
            if (condition is Condition<int> cInt)
            {
                await writer.WriteLineAsync("i").ConfigureAwait(false);
                await writer.WriteLineAsync(cInt.Data.ToString()).ConfigureAwait(false);
            }
            else if (condition is Condition<string> cStr)
            {
                await writer.WriteLineAsync("s").ConfigureAwait(false);
                await writer.WriteLineAsync(cStr.Data).ConfigureAwait(false);
            }
            else if (condition is TableCondition cTbl)
            {
                await WriteDataListAsync(writer, cTbl.Data).ConfigureAwait(false);
            }
            else if (condition is NotCondition cNot)
            {
                await WriteConditionListAsync(writer, new() { cNot.Data }).ConfigureAwait(false);
            }
            else if (condition is MultipleCondition cMlt)
            {
                await WriteConditionListAsync(writer, cMlt.Data).ConfigureAwait(false);
            }
        }

        private static async Task WriteDataListAsync(TextWriter writer, OrderedDictionary data)
        {
            Dictionary<Type, string> typeDict = new()
            {
                { typeof(int), "i" },
                { typeof(string), "s" },
                { typeof(double), "d" },
                { typeof(ViewString), "ba" }
            };

            await writer.WriteLineAsync("TABLE").ConfigureAwait(false);
            await writer.WriteLineAsync("2").ConfigureAwait(false);
            await writer.WriteLineAsync("k").ConfigureAwait(false);
            await writer.WriteLineAsync("v").ConfigureAwait(false);
            await writer.WriteLineAsync("n").ConfigureAwait(false);
            await writer.WriteLineAsync("n").ConfigureAwait(false);
            await writer.WriteLineAsync(data.Count.ToString()).ConfigureAwait(false);
            for (var i = 0; i < data.Count; ++i)
            {
                var keyList = data.Keys.Cast<string>().ToArray();
                var valueList = data.Values.Cast<object>().ToArray();
                await writer.WriteLineAsync("s").ConfigureAwait(false);
                await writer.WriteLineAsync(keyList[i]).ConfigureAwait(false);
                if (!typeDict.ContainsKey(valueList[i].GetType()))
                    throw new Exception("Invalid value type");
                await writer.WriteLineAsync(typeDict[valueList[i].GetType()]).ConfigureAwait(false);
                if (valueList[i] is ViewString vs)
                {
                    await writer.WriteLineAsync(vs.String.Length.ToString()).ConfigureAwait(false);
                    await writer.WriteAsync(vs.ToString()).ConfigureAwait(false);
                }
                else
                    await writer.WriteLineAsync(valueList[i].ToString()).ConfigureAwait(false);
            }
        }

        private async Task WriteConditionListAsync(TextWriter writer, List<Condition> conditions)
        {
            foreach (var headerLine in new[] { "TABLE", "2", "K", "V", "n", "n" })
            {
                await writer.WriteLineAsync(headerLine).ConfigureAwait(false);
            }

            await writer.WriteLineAsync(conditions.Count.ToString()).ConfigureAwait(false);

            foreach (var condition in conditions)
            {
                await writer.WriteLineAsync("i").ConfigureAwait(false);
                await writer.WriteLineAsync(((int)condition.Type).ToString()).ConfigureAwait(false);
                await WriteConditionAsync(writer, condition).ConfigureAwait(false);
            }
        }
    }
}
