using MetaParser.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MetaParser.Formatting;

public class DefaultMetaWriter : IMetaWriter
{
    private static readonly string headerString = $"1{Environment.NewLine}CondAct{Environment.NewLine}5{Environment.NewLine}CType{Environment.NewLine}AType{Environment.NewLine}CData{Environment.NewLine}AData{Environment.NewLine}State{Environment.NewLine}n{Environment.NewLine}n{Environment.NewLine}n{Environment.NewLine}n{Environment.NewLine}n";
    private static readonly string tableHeaderString = $"TABLE{Environment.NewLine}2{Environment.NewLine}k{Environment.NewLine}v{Environment.NewLine}n{Environment.NewLine}n";
    private static readonly string listHeaderString = $"TABLE{Environment.NewLine}2{Environment.NewLine}K{Environment.NewLine}V{Environment.NewLine}n{Environment.NewLine}n";
    private readonly INavWriter navFormatter;

    public DefaultMetaWriter(INavWriter navWriter)
    {
        this.navFormatter = navWriter;
    }

    public async Task WriteMetaAsync(Stream stream, Meta meta)
    {
        using var writer = new StreamWriter(stream, leaveOpen: true);

        await writer.WriteLineAsync(headerString).ConfigureAwait(false);

        await writer.WriteLineAsync(meta.Rules.Count.ToString()).ConfigureAwait(false);

        foreach (var rule in meta.Rules.OrderBy(r => r.State))
        {
            await WriteRuleAsync(writer, rule).ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
        }
    }

    public async Task WriteRuleAsync(TextWriter writer, Rule rule)
    {
        await WriteDataAsync(writer, (int)rule.Condition.Type).ConfigureAwait(false);
        await WriteDataAsync(writer, (int)rule.Action.Type).ConfigureAwait(false);
        await WriteConditionAsync(writer, rule.Condition).ConfigureAwait(false);
        await WriteActionAsync(writer, rule.Action).ConfigureAwait(false);
        await WriteDataAsync(writer, rule.State).ConfigureAwait(false);
    }

    public Task WriteActionAsync(TextWriter writer, MetaAction action) => action switch
    {
        MetaAction<string> cStr => WriteDataAsync(writer, cStr.Data),
        MetaAction<int> cInt => WriteDataAsync(writer, cInt.Data),
        TableMetaAction cTbl => WriteTableData(writer, cTbl.Data),
        AllMetaAction cAll => WriteMultipleActionAsync(writer, cAll),
        EmbeddedNavRouteMetaAction cNav => WriteNavActionAsync(writer, cNav),
        _ => throw new InvalidCastException("Invalid action type")
    };

    private async Task WriteNavActionAsync(TextWriter writer, EmbeddedNavRouteMetaAction cNav)
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

    private async Task WriteMultipleActionAsync(TextWriter writer, AllMetaAction cAll)
    {
        await writer.WriteLineAsync(listHeaderString).ConfigureAwait(false);
        await writer.WriteLineAsync(cAll.Data.Count.ToString()).ConfigureAwait(false);

        foreach (var subAction in cAll.Data)
        {
            await WriteDataAsync(writer, (int)subAction.Type).ConfigureAwait(false);
            await WriteActionAsync(writer, subAction).ConfigureAwait(false);
        }
    }

    public Task WriteConditionAsync(TextWriter writer, Condition condition) => condition switch
    {
        Condition<int> cInt => WriteDataAsync(writer, cInt.Data),
        Condition<string> cStr => WriteDataAsync(writer, cStr.Data),
        TableCondition cTbl => WriteTableData(writer, cTbl.Data),
        NotCondition cNot => WriteConditionListAsync(writer, new() { cNot.Data }),
        MultipleCondition cMlt => WriteConditionListAsync(writer, cMlt.Data),
        _ => throw new InvalidCastException("Invalid condition type")
    };

    private static async Task WriteTableData(TextWriter writer, OrderedDictionary data)
    {
        await writer.WriteLineAsync(tableHeaderString).ConfigureAwait(false);
        await writer.WriteLineAsync(data.Count.ToString()).ConfigureAwait(false);

        var keyList = data.Keys.Cast<string>().ToArray();
        for (var i = 0; i < data.Count; ++i)
        {
            await WriteDataAsync(writer, keyList[i]);
            await WriteDataAsync(writer, data[keyList[i]]);
        }
    }

    private static Task WriteDataAsync<T>(TextWriter writer, T data) => data switch
    {
        int i => writer.WriteLineAsync("i").ContinueWith(_ => writer.WriteLineAsync(i.ToString())),
        string s => writer.WriteLineAsync("s").ContinueWith(_ => writer.WriteLineAsync(s)),
        double d => writer.WriteLineAsync("d").ContinueWith(_ => writer.WriteLineAsync(d.ToString())),
        ViewString vs => writer.WriteLineAsync("ba")
                                .ContinueWith(_ => writer.WriteLineAsync(vs.Length.ToString()))
                                .ContinueWith(_ => writer.WriteAsync(vs.String)),
        _ => throw new InvalidCastException("Invalid value type -- must be either int, string, double, or ViewString")
    };

    private async Task WriteConditionListAsync(TextWriter writer, List<Condition> conditions)
    {
        await writer.WriteLineAsync(listHeaderString).ConfigureAwait(false);
        await writer.WriteLineAsync(conditions.Count.ToString()).ConfigureAwait(false);

        foreach (var condition in conditions)
        {
            await WriteDataAsync(writer, (int)condition.Type).ConfigureAwait(false);
            await WriteConditionAsync(writer, condition).ConfigureAwait(false);
        }
    }
}
