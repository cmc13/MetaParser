using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MetaParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace MetaParser.WPF.MetaValidation;

public interface IMetaValidator
{
    IEnumerable<MetaValidationResult> ValidateMeta(Meta meta);
}

public class EntryPointMetaValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        if (!meta.Rules.Any(r => r.State == "Default"))
            yield return new MetaValidationResult(meta, null, "Missing Default state");
    }
}

public class AggregateMetaValidator : IMetaValidator
{
    private static readonly IEnumerable<IMetaValidator> validators = typeof(AggregateMetaValidator).Assembly.GetTypes()
            .Where(t => t.IsClass && t.GetInterfaces().Contains(typeof(IMetaValidator)))
            .Except(new[] { typeof(AggregateMetaValidator) })
            .Select(t => (IMetaValidator)Activator.CreateInstance(t));

    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        var list = new List<MetaValidationResult>();
        foreach (var v in validators)
        {
            var results = v.ValidateMeta(meta);
            list.AddRange(results);
        }
        return list.AsReadOnly();
    }
}

public class UnreachableStateMetaValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        return meta.Rules.Select(r => r.State).Distinct().Except(new[] { "Default" })
            .Where(s => meta.Rules.All(r => !ContainsState(r.Action, s)))
            .Select(s => new MetaValidationResult(meta, null, $"Unreachable state detected: {s}"));
    }

    private static bool ContainsState(MetaAction action, string state) => action switch
    {
        CallStateMetaAction cs when (cs.CallState == state || cs.ReturnState == state) => true,
        MetaAction<string> s when s.Type == ActionType.SetState && s.Data == state => true,
        WatchdogSetMetaAction ws when ws.State == state => true,
        CreateViewMetaAction vwa when vwa.ViewDefinition.Contains("setstate=\"" + state + "\"") => true,
        AllMetaAction ama when ama.Data.Any(d => ContainsState(d, state)) => true,
        _ => false
    };
}

public partial class UndefinedStateMetaValidator : IMetaValidator
{
    [GeneratedRegex(@"^/vt\s*setmetastate\s*(.*)$")]
    private static partial Regex SMSRegex();

    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        var states = meta.Rules.Select(r => r.State).Distinct();
        foreach (var r in meta.Rules.Select(r => (r, HasUnreachableState(states, r.Action).Distinct())))
        {
            foreach (var state in r.Item2)
                yield return new MetaValidationResult(meta, r.r, $"Undefined state detected: {state}");
        }
    }

    private static IEnumerable<string> HasUnreachableState(IEnumerable<string> states, MetaAction action) => action switch
    {
        CallStateMetaAction cs when !states.Contains(cs.CallState) => new[] { cs.CallState },
        CallStateMetaAction cs when !states.Contains(cs.ReturnState) => new[] { cs.ReturnState },
        MetaAction<string> s when s.Type == ActionType.SetState && !states.Contains(s.Data ?? "") => new[] { s.Data ?? "" },
        WatchdogSetMetaAction ws when !states.Contains(ws.State) => new[] { ws.State },
        AllMetaAction ama => ama.Data.SelectMany(a => HasUnreachableState(states, a)),
        CreateViewMetaAction vwa => GetCreateViewStates(vwa).Where(state => !states.Contains(state)),
        MetaAction<string> s when s.Type == ActionType.ChatCommand => GetChatCommandStates(s).Where(state => !states.Contains(state)),
        EmbeddedNavRouteMetaAction na => GetNavStateChanges(na).Where(state => !states.Contains(state)),
        _ => Enumerable.Empty<string>()
    };

    private static IEnumerable<string> GetNavStateChanges(EmbeddedNavRouteMetaAction na)
    {
        if (na.Data.nav.Data is IEnumerable<NavNode> navList)
        {
            foreach (var chatNav in navList.OfType<NavNodeChat>())
            {
                var match = SMSRegex().Match(chatNav.Data);
                if (match != null && match.Success)
                    yield return match.Groups[1].Value.TrimEnd();
            }
        }
    }

    private static IEnumerable<string> GetChatCommandStates(MetaAction<string> cca)
    {
        var match = SMSRegex().Match(cca.Data);
        if (match != null && match.Success)
            yield return match.Groups[1].Value.TrimEnd();
    }

    private static IEnumerable<string> GetCreateViewStates(CreateViewMetaAction vwa)
    {
        XElement xml = null;
        try
        {
            xml = XElement.Parse(vwa.ViewDefinition);
        }
        catch { }
        if (xml != null)
        {
            foreach (var state in xml
                .Descendants("control")
                .Where(x => x.Attribute("type").Value == "button")
                .Select(x => x.Attribute("setstate")?.Value)
                .Where(v => v != null)
                .Distinct())
            {
                yield return state;
            }
        }
    }
}

public class EmptyMultipleConditionValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        return meta.Rules.Where(r => HasEmptyCondition(r.Condition))
            .Select(r => new MetaValidationResult(meta, r, "Empty multiple condition detected"));
    }

    private static bool HasEmptyCondition(Condition c) =>
        c is MultipleCondition mc && (mc.Data.Count == 0 || mc.Data.Any(HasEmptyCondition));
}

public class EmptyMultipleActionValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        return meta.Rules.Where(r => HasEmptyAction(r.Action))
            .Select(r => new MetaValidationResult(meta, r, "Empty multiple action detected"));
    }

    private static bool HasEmptyAction(MetaAction c) =>
        c is AllMetaAction mc && (mc.Data.Count == 0 || mc.Data.Any(HasEmptyAction));
}

public class VTankOptionValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        foreach (var rule in meta.Rules)
        {
            var opt = InvalidVTankOption(rule.Action).ToArray();
            if (opt.Length > 0)
            {
                yield return new(meta, rule, rule.Action is AllMetaAction ? $"Rule action contains invalid/unknown VTank option name(s): {string.Join(", ", opt)}" : $"Invalid/unknown VTank option name(s): {string.Join(", ", opt)}");
            }
        }
    }

    private static IEnumerable<string> InvalidVTankOption(MetaAction c) => c switch
    {
        GetVTOptionMetaAction gc when !VTankOptionsExtensions.TryParse(gc.Option, out _) => new[] { gc.Option },
        SetVTOptionMetaAction sc when !VTankOptionsExtensions.TryParse(sc.Option, out _) => new[] { sc.Option },
        AllMetaAction ama => ama.Data.SelectMany(InvalidVTankOption),
        _ => Enumerable.Empty<string>()
    };
}

public class VTankOptionValueValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        return meta.Rules
            .SelectMany(r => GetInvalidOptionValues(r.Action)
                .Select(t => new MetaValidationResult(meta, r, $"Invalid value for {t.op} option: {t.vv}")));
    }

    private static IEnumerable<(string op, string vv)> GetInvalidOptionValues(MetaAction action) => action switch
    {
        SetVTOptionMetaAction so when VTankOptionsExtensions.TryParse(so.Option, out var opt) && !opt.IsValidValue(so.Value) => new[] { (so.Option, so.Value) },
        AllMetaAction ama => ama.Data.SelectMany(GetInvalidOptionValues),
        _ => Enumerable.Empty<(string, string)>()
    };
}

public class VacuouslyTrueConditionValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        return meta.Rules
            .Where(r => IsVacuouslyTrue(r.Condition))
            .Select(r => new MetaValidationResult(meta, r, r.Condition is MultipleCondition ? "Rule contains a vacuously true condition" : "Rule conditions vacuously true"));
    }

    private static bool IsVacuouslyTrue(Condition c) => c switch
    {
        Condition<int> cc when cc.Type == ConditionType.BurdenPercentGE && cc.Data <= 0 => true,
        DistanceToAnyRoutePointGECondition drc when drc.Distance <= 0 => true,
        ItemCountCondition ic when ic.Type == ConditionType.ItemCountGE && ic.Count <= 0 => true,
        Condition<int> cc when cc.Type == ConditionType.MainPackSlotsLE && cc.Data >= 102 => true,
        Condition<int> cc when (cc.Type == ConditionType.SecondsInStateGE || cc.Type == ConditionType.SecondsInStatePersistGE) && cc.Data <= 0 => true,
        MultipleCondition mc when mc.Data.Any(IsVacuouslyTrue) => true,
        _ => false
    };
}

public class InvalidRegexValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        return meta.Rules
            .Where(r => HasInvalidRegex(r.Condition))
            .Select(r => new MetaValidationResult(meta, r, "Condition has invalid regular expression syntax"));
    }

    private static bool HasInvalidRegex(Condition c) => c switch
    {
        MonsterCountWithinDistanceCondition mcc when !IsValidRegex(mcc.MonsterNameRx) => true,
        Condition<string> sc when sc.Type == ConditionType.ChatMessage && !IsValidRegex(sc.Data) => true,
        ChatMessageCaptureCondition cmc when !IsValidRegex(cmc.Pattern) => true,
        MultipleCondition mc when mc.Data.Any(HasInvalidRegex) => true,
        _ => false
    };

    private static bool IsValidRegex(string pattern)
    {
        try
        {
            _ = new Regex(pattern);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class DuplicateViewNameValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        var viewNames = new Dictionary<string, int>();
        var viewRules = new Dictionary<Rule, List<string>>();

        foreach (var rule in meta.Rules)
        {
            var views = new List<string>();
            foreach (var t in GetViewNames(rule.Action).GroupBy(v => v).Select(g => (g.Key,g.Count())))
            {
                if (!viewNames.TryGetValue(t.Key, out var count))
                    count = 0;

                viewNames[t.Key] = count + t.Item2;

                if (!views.Contains(t.Key))
                    views.Add(t.Key);
            }

            viewRules[rule] = views;
        }

        

        foreach (var kv in viewNames.Where(v => v.Value > 1))
        {
            foreach (var rule in viewRules.Where(kv2 => kv2.Value.Contains(kv.Key)).Select(kv2 => kv2.Key))
                yield return new MetaValidationResult(meta, rule, $"View name declared multiple times: {kv.Key}");
        }
    }

    private static IEnumerable<string> GetViewNames(MetaAction action) => action switch
    {
        CreateViewMetaAction cma => new[] { cma.ViewName },
        AllMetaAction ama => ama.Data.SelectMany(GetViewNames),
        _ => Enumerable.Empty<string>()
    };
}

public class UndefinedViewNameValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        var viewNames = meta.Rules.SelectMany(r => GetViewNames(r.Action)).Distinct().ToList();

        foreach (var rule in meta.Rules)
        {
            foreach (var view in GetUndefinedViewNames(rule.Action, viewNames).Distinct())
                yield return new MetaValidationResult(meta, rule, $"Undefined view name: {view}");
        }
    }

    private static IEnumerable<string> GetUndefinedViewNames(MetaAction action, IEnumerable<string> viewNames) => action switch
    {
        DestroyViewMetaAction dma when !viewNames.Contains(dma.ViewName) => new[] { dma.ViewName },
        AllMetaAction ama => ama.Data.SelectMany(a => GetUndefinedViewNames(a, viewNames)),
        _ => Enumerable.Empty<string>()
    };

    private static IEnumerable<string> GetViewNames(MetaAction action) => action switch
    {
        CreateViewMetaAction cma => new[] { cma.ViewName },
        AllMetaAction ama => ama.Data.SelectMany(GetViewNames),
        _ => Enumerable.Empty<string>()
    };
}

public class ValidXMLViewValidator : IMetaValidator
{
    private static readonly XmlSchema schema;

    static ValidXMLViewValidator()
    {
        using var str = File.OpenRead("Assets/VTankView.xsd");
        schema = XmlSchema.Read(str, null);
    }

    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        return meta.Rules
            .Where(r => !IsValidXMLView(r.Action))
            .Select(r => new MetaValidationResult(meta, r, "Invalid View XML format"));
    }

    private static bool ValidateXML(string xml)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            doc.Schemas.Add(schema);

            doc.Validate(null);
        }
        catch (Exception ex) when (ex is XmlSchemaValidationException || ex is XmlException)
        {
            return false;
        }

        return true;
    }

    private static bool IsValidXMLView(MetaAction action) => action switch
    {
        CreateViewMetaAction cva => ValidateXML(cva.ViewDefinition),
        AllMetaAction ama => ama.Data.All(IsValidXMLView),
        _ => true
    };
}

public class MetaExpressionValidator
    : IMetaValidator
{
    private class FunctionNameVisitor : MetaExpressionsBaseVisitor<List<string>>
    {
        public override List<string> VisitFunctionCall([NotNull] MetaExpressionsParser.FunctionCallContext context)
        {
            var node = context.STRING();
            var fn = node.GetText();
            return new() { fn };
        }

        protected override List<string> AggregateResult(List<string> aggregate, List<string> nextResult)
        {
            if (nextResult != null)
                aggregate?.AddRange(nextResult);

            return aggregate ?? nextResult;
        }
    }

    private static readonly string[] VTANK_EXPRESSION_FUNCTIONS = new string[]
    {
        //VTank base expressions
        "testvar",
        "getvar",
        "setvar",
        "touchvar",
        "clearallvars",
        "clearvar",
        "getcharintprop",
        "getchardoubleprop",
        "getcharquadprop",
        "getcharboolprop",
        "getcharstringprop",
        "getisspellknown",
        "getcancastspell_hunt",
        "getcancastspell_buff",
        "getcharvital_base",
        "getcharvital_current",
        "getcharvital_buffedmax",
        "getcharskill_traininglevel",
        "getcharskill_base",
        "getcharskill_buffed",
        "getplayerlandcell",
        "getplayercoordinates",
        "coordinategetns",
        "coordinategetwe",
        "coordinategetz",
        "coordinatetostring",
        "coordinateparse",
        "coordinatedistancewithz",
        "coordinatedistanceflat",
        "wobjectgetphysicscoordinates",
        "wobjectgetname",
        "wobjectgetobjectclass",
        "wobjectgettemplatetype",
        "wobjectgetisdooropen",
        "wobjectfindnearestmonster",
        "wobjectfindnearestdoor",
        "wobjectfindnearestbyobjectclass",
        "wobjectfindininventorybytemplatetype",
        "wobjectfindininventorybyname",
        "wobjectfindininventorybynamerx",
        "wobjectgetselection",
        "wobjectgetplayer",
        "wobjectfindnearestbynameandobjectclass",
        "actiontryselect",
        "actiontryuseitem",
        "actiontryapplyitem",
        "actiontrygiveitem",
        "actiontryequipanywand",
        "actiontrycastbyid",
        "actiontrycastbyidontarget",
        "chatbox",
        "chatboxpaste",
        "statushud",
        "statushudcolored",
        "uigetcontrol",
        "uisetlabel",
        "isfalse",
        "istrue",
        "iif",
        "randint",
        "cstr",
        "strlen",
        "getobjectinternaltype",
        "cstrf",
        "stopwatchcreate",
        "stopwatchstart",
        "stopwatchstop",
        "stopwatchelapsedseconds",
        "cnumber",
        "floor",
        "ceiling",
        "round",
        "abs",
        "vtsetmetastate",
        "vtgetmetastate",
        "vtsetsetting",
        "vtgetsetting",
        "vtmacroenabled",
        "ord",
        "chr",
        "uisetvisible",
        "uiviewexists",
        "uiviewvisible",

        // UB Expressions
        "acos",
        "asin",
        "atan",
        "atan2",
        "cos",
        "cosh",
        "delayexec",
        "dictadditem",
        "dictclear",
        "dictcopy",
        "dictcreate",
        "dictgetitem",
        "dicthaskey",
        "dictkeys",
        "dictremovekey",
        "dictsize",
        "dictvalues",
        "exec",
        "getaccounthash",
        "getbusystate",
        "getcombatstate",
        "getdatetimelocal",
        "getdatetimeutc",
        "getequippedweapontype",
        "getheading",
        "getheadingto",
        "getregexmatch",
        "getunixtime",
        "getworldname",
        "hexstr",
        "ifthen",
        "isportaling",
        "listadd",
        "listclear",
        "listcontains",
        "listcopy",
        "listcount",
        "listcreate",
        "listfilter",
        "listfromrange",
        "listgetitem",
        "listindexof",
        "listinsert",
        "listlastindexof",
        "listmap",
        "listpop",
        "listreduce",
        "listremove",
        "listremoveat",
        "listreverse",
        "listsort",
        "setcombatstate",
        "sin",
        "sinh",
        "sqrt",
        "tan",
        "tanh",
        "tostring",
        "uboptget",
        "uboptset",
        "vitae",
        "wobjectfindnearestbytemplatetype",
        "wobjectgetboolprop",
        "wobjectgetdoubleprop",
        "wobjectgetintprop",
        "wobjectgetstringprop",
        "wobjecthasdata",
        "wobjectisvalid",
        "wobjectlastidtime",
        "wobjectrequestdata",

        "testquestflag",
        "getqueststatus",
        "getquestktprogress",
        "getquestktrequired",
        "isrefreshingquests",

        "ustadd",
        "ustopen",
        "ustsalvage",
        "getgameyear",
        "getgamemonth",
        "getgamemonthname",
        "getgameday",
        "getgamehourname",
        "getgamehour",
        "getminutesuntilday",
        "getminutesuntilnight",
        "getgameticks",
        "getisday",
        "getisnight",

        "getfellowshipstatus",
        "getfellowshipname",
        "getfellowshipcount",
        "getfellowshipleaderid",
        "getfellowid",
        "getfellowname",
        "getfellowshiplocked",
        "getfellowshipisleader",
        "getfellowshipisopen",
        "getfellowshipisfull",
        "getfellowshipcanrecruit",
        "getfellownames",
        "getfellowids",

        "wobjectgethealth",
        "wobjectgethealthvalue",
        "wobjectgetstaminavalue",
        "wobjectgetmanavalue",

        "getitemcountininventorybyname",
        "getitemcountininventorybynamerx",
        "wobjectgetspellids",
        "wobjectgetactivespellids",
        "getinventorycountbytemplatetype",
        "actiontrygiveprofile",
        "actiontrymove",
        "actiontrydrop",
        "actiontrysplit",

        "getcharacterindex",
        "setnextlogin",
        "clearnextlogin",

        "setmotion",
        "getmotion",
        "clearmotion",

        "netclients",

        "spellname",
        "componentname",
        "componentdata",
        "getknownspells",
        "spelldata",

        "testpvar",
        "getpvar",
        "setpvar",
        "clearpvar",
        "clearallpvars",
        "touchpvar",
        "testgvar",
        "getgvar",
        "setgvar",
        "cleargvar",
        "clearallgvars",
        "touchgvar",
        "echo",
        "getspellexpiration",
        "getspellexpirationbyname",
        "getcharattribute_buffed",
        "getcharattribute_base",
        "getcharburden",
        "getplayerlandblock",
        "wobjectgetid",
        "wobjectfindbyid",
        "wobjectgetopencontainer",
        "getfreeitemslots",
        "getfreecontainerslots",
        "getcontaineritemcount",
        "wobjectfindall",
        "wobjectfindallinventory",
        "wobjectfindalllandscape",
        "wobjectfindallbyobjectclass",
        "wobjectfindallbytemplatetype",
        "wobjectfindallbynamerx",
        "wobjectfindallinventorybyobjectclass",
        "wobjectfindallinventorybytemplatetype",
        "wobjectfindallinventorybynamerx",
        "wobjectfindalllandscapebyobjectclass",
        "wobjectfindalllandscapebytemplatetype",
        "wobjectfindalllandscapebynamerx",
        "wobjectfindallbycontainer",

        "xpreset",
        "xpmeter",
        "xpduration",
        "xptotal",
        "lumtotal",
        "xpavg",
        "lumavg"
    };

    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        return meta.Rules.SelectMany(r => CheckConditionMetaExpressions(meta, r, r.Condition))
            .Union(meta.Rules.SelectMany(r => CheckActionMetaExpressions(meta, r, r.Action)));
    }

    private static MetaExpressionsParser.ParseContext ParseExpression(string expression)
    {
        var stream = new AntlrInputStream(expression);
        var lexer = new MetaExpressionsLexer(stream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new MetaExpressionsParser(tokens);
        return parser.parse();
    }

    private static IEnumerable<MetaValidationResult> CheckConditionMetaExpressions(Meta m, Rule r, Condition c) => c switch
    {
        ExpressionCondition ec => CheckExpression(m, r, ec.Expression),
        MultipleCondition mc => mc.Data.SelectMany(cc => CheckConditionMetaExpressions(m, r, cc)),
        _ => Enumerable.Empty<MetaValidationResult>()
    };

    private static IEnumerable<MetaValidationResult> CheckActionMetaExpressions(Meta m, Rule r, MetaAction a) => a switch
    {
        ExpressionMetaAction ec when ec.Type == ActionType.ExpressionAct => CheckExpression(m, r, ec.Expression),
        AllMetaAction mc => mc.Data.SelectMany(aa => CheckActionMetaExpressions(m, r, aa)),
        _ => Enumerable.Empty<MetaValidationResult>()
    };

    private static IEnumerable<MetaValidationResult> CheckExpression(Meta m, Rule r, string expression)
    {
        List<MetaValidationResult> results = new();
        try
        {
            var ctx = ParseExpression(expression);

            if (ctx.exception != null)
            {
                results.Add(new MetaValidationResult(m, r, $"Unable to parse expression: {ctx.exception.Message}"));
            }

            var visitor = new FunctionNameVisitor();
            var functionNames = visitor.Visit(ctx)?.Where(f => !VTANK_EXPRESSION_FUNCTIONS.Contains(f));
            if (functionNames != null)
            {
                foreach (var fn in functionNames)
                    results.Add(new MetaValidationResult(m, r, $"Invalid expression function: {fn}"));
            }
        }
        catch (Exception e)
        {
            results.Add(new MetaValidationResult(m, r, $"Unable to parse expression: {e.Message}"));
        }

        return results.AsReadOnly();
    }
}
