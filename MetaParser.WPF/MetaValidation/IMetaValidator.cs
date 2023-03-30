using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MetaParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
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
        bool ContainsState(MetaAction action, string state)
        {
            if (action is CallStateMetaAction cs && (cs.CallState == state || cs.ReturnState == state))
                return true;
            if (action.Type == ActionType.SetState && action is MetaAction<string> s && s.Data == state)
                return true;
            if (action is WatchdogSetMetaAction ws && ws.State == state)
                return true;
            if (action is AllMetaAction aa && aa.Data.Any(d => ContainsState(d, state)))
                return true;
            if (action is CreateViewMetaAction vwa && vwa.ViewDefinition.Contains("setstate=\"" + state + "\""))
                return true;
            return false;
        }

        foreach (var state in meta.Rules.Select(r => r.State).Distinct().Except(new[] { "Default" }))
        {
            if (meta.Rules.All(r => !ContainsState(r.Action, state)))
                yield return new MetaValidationResult(meta, null, $"Unreachable state detected: {state}");
        }
    }
}

public class UndefinedStateMetaValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        IEnumerable<string> HasUnreachableState(IEnumerable<string> states, MetaAction action)
        {
            if (action is CallStateMetaAction cs && !states.Contains(cs.CallState))
                yield return cs.CallState;
            if (action is CallStateMetaAction cs2 && !states.Contains(cs2.ReturnState))
                yield return cs2.ReturnState;
            if (action.Type == ActionType.SetState && action is MetaAction<string> s && !states.Contains(s.Data ?? ""))
                yield return s.Data ?? "";
            if (action is WatchdogSetMetaAction ws && !states.Contains(ws.State))
                yield return  ws.State;
            if (action is AllMetaAction aa && aa.Data.Any(d => HasUnreachableState(states, d) != null))
            {
                foreach (var state in aa.Data.SelectMany(d => HasUnreachableState(states, d)))
                    yield return state;
            }
            if (action is CreateViewMetaAction vwa)
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
                        .Where(v => v != null && !states.Contains(v))
                        .Distinct())
                    {
                        yield return state;
                    }
                }
            }
            if (action.Type == ActionType.ChatCommand && action is MetaAction<string> cca)
            {
                var match = Regex.Match(cca.Data, @"^/vt\s*setmetastate\s*(.*)$");
                if (match != null && match.Success && !states.Contains(match.Groups[1].Value.TrimEnd()))
                    yield return match.Groups[1].Value.TrimEnd();
            }
            if (action is EmbeddedNavRouteMetaAction na)
            {
                if (na.Data.nav.Data is IEnumerable<NavNode> navList)
                {
                    foreach (var chatNav in navList.OfType<NavNodeChat>())
                    {
                        var match = Regex.Match(chatNav.Data, @"^/vt\s*setmetastate\s*(.*)$");
                        if (match != null && match.Success && !states.Contains(match.Groups[1].Value.TrimEnd()))
                            yield return match.Groups[1].Value.TrimEnd();
                    }
                }
            }
        }

        var states = meta.Rules.Select(r => r.State).Distinct();
        foreach (var r in meta.Rules.Select(r => (r, HasUnreachableState(states, r.Action).Distinct())))
        {
            foreach (var state in r.Item2)
                yield return new MetaValidationResult(meta, r.r, $"Undefined state detected: {state}");
        }
    }
}

public class EmptyMultipleConditionValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        bool HasEmptyCondition(Condition c)
        {
            if (c is MultipleCondition mc)
            {
                if (mc.Data.Count == 0)
                    return true;
                else if (mc.Data.Any(HasEmptyCondition))
                    return true;
            }

            return false;
        }

        foreach (var r in meta.Rules.Where(r => HasEmptyCondition(r.Condition)))
        {
            yield return new MetaValidationResult(meta, r, $"Empty multiple condition detected");
        }
    }
}

public class EmptyMultipleActionValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        bool HasEmptyAction(MetaAction c)
        {
            if (c is AllMetaAction mc)
            {
                if (mc.Data.Count == 0)
                    return true;
                else if (mc.Data.Any(HasEmptyAction))
                    return true;
            }

            return false;
        }

        foreach (var r in meta.Rules.Where(r => HasEmptyAction(r.Action)))
        {
            yield return new MetaValidationResult(meta, r, $"Empty multiple action detected");
        }
    }
}

public class VTankOptionValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        string InvalidVTankOption(MetaAction c)
        {
            if (c is AllMetaAction mc)
            {
                foreach (var a in mc.Data)
                {
                    var op = InvalidVTankOption(a);
                    if (op != null)
                        return op;
                }
            }
            else if (c is SetVTOptionMetaAction sc)
            {
                if (!VTankOptionsExtensions.TryParse(sc.Option, out _))
                {
                    return sc.Option;
                }
            }
            else if (c is GetVTOptionMetaAction gc)
            {
                if (!VTankOptionsExtensions.TryParse(gc.Option, out _))
                {
                    return gc.Option;
                }
            }

            return null;
        }

        foreach (var rule in meta.Rules)
        {
            var opt = InvalidVTankOption(rule.Action);
            if (opt != null)
            {
                if (rule.Action is AllMetaAction)
                {
                    yield return new(meta, rule, $"Rule action contains an invalid/unknown VTank option name: {opt}");
                }
                else
                {
                    yield return new(meta, rule, $"Invalid/unknown VTank option name: {opt}");
                }
            }
        }
    }
}

public class VTankOptionValueValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        IEnumerable<(string op, string vv)> GetInvalidOptionValues(MetaAction action)
        {
            if (action is AllMetaAction am)
            {
                foreach (var ac in am.Data)
                {
                    foreach (var result in GetInvalidOptionValues(ac))
                        yield return result;
                }
            }
            else if (action is SetVTOptionMetaAction so)
            {
                if (VTankOptionsExtensions.TryParse(so.Option, out var opt))
                {
                    if (!opt.IsValidValue(so.Value))
                        yield return (so.Option, so.Value);
                }
            }
        }

        foreach (var rule in meta.Rules)
        {
            foreach (var result in GetInvalidOptionValues(rule.Action))
            {
                yield return new MetaValidationResult(meta, rule, $"Invalid value for {result.op} option: {result.vv}");
            }
        }
    }
}

public class VacuouslyTrueConditionValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        bool IsVacuouslyTrue(Condition c)
        {
            if (c.Type == ConditionType.BurdenPercentGE &&  c is Condition<int> cc && cc.Data <= 0)
            {
                return true;
            }
            else if (c is DistanceToAnyRoutePointGECondition drc && drc.Distance <= 0)
            {
                return true;
            }
            else if (c.Type == ConditionType.ItemCountGE && c is ItemCountCondition ic && ic.Count <= 0)
            {
                return true;
            }
            else if (c.Type == ConditionType.MainPackSlotsLE && c is Condition<int> cc2 && cc2.Data >= 102)
            {
                return true;
            }
            else if ((c.Type == ConditionType.SecondsInStateGE || c.Type == ConditionType.SecondsInStatePersistGE) && c is Condition<int> cc3 && cc3.Data <= 0)
            {
                return true;
            }
            else if (c is MultipleCondition mc && mc.Data.Any(IsVacuouslyTrue))
            {
                return true;
            }

            return false;
        }

        foreach (var rule in meta.Rules)
        {
            if (IsVacuouslyTrue(rule.Condition))
            {
                if (rule.Condition is MultipleCondition)
                {
                    yield return new MetaValidationResult(meta, rule, $"Rule contains a vacuously true condition");
                }
                else
                {
                    yield return new MetaValidationResult(meta, rule, "Rule condition is vacuously true");
                }
            }
        }
    }
}

public class InvalidRegexValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        bool HasInvalidRegex(Condition c)
        {
            if (c is MonsterCountWithinDistanceCondition mcc)
            {
                if (!IsValidRegex(mcc.MonsterNameRx))
                    return true;
            }
            else if (c.Type == ConditionType.ChatMessage && c is Condition<string> sc)
            {
                if (!IsValidRegex(sc.Data))
                    return true;
            }
            else if (c is ChatMessageCaptureCondition cmc)
            {
                if (!IsValidRegex(cmc.Pattern))
                    return true;
            }
            else if (c is MultipleCondition mc && mc.Data.Any(HasInvalidRegex))
            {
                return true;
            }

            return false;
        }

        foreach (var rule in meta.Rules.Where(r => HasInvalidRegex(r.Condition)))
        {
            yield return new MetaValidationResult(meta, rule, "Condition has invalid regular expression syntax");
        }
    }

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
        var dict = new Dictionary<string, int>();
        void GetViewNames(MetaAction action)
        {
            if (action is CreateViewMetaAction cma)
            {
                if (dict.ContainsKey(cma.ViewName))
                    dict[cma.ViewName]++;
                else
                    dict[cma.ViewName] = 1;
            }
            else if (action is AllMetaAction ama)
            {
                foreach (var a in ama.Data)
                    GetViewNames(a);
            }
        }

        foreach (var rule in meta.Rules)
        {
            GetViewNames(rule.Action);
        }

        foreach (var kv in dict.Where(v => v.Value > 1))
        {
            yield return new MetaValidationResult(meta, null, $"View name declared multiple times: {kv.Key}");
        }
    }
}

public class UndefinedViewNameValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        IEnumerable<string> GetViewNames(MetaAction action)
        {
            if (action is CreateViewMetaAction cma)
            {
                yield return cma.ViewName;
            }
            else if (action is AllMetaAction ama)
            {
                foreach (var a in ama.Data.SelectMany(GetViewNames))
                    yield return a;
            }
        }

        var viewNames = meta.Rules.SelectMany(r => GetViewNames(r.Action)).Distinct().ToList();

        IEnumerable<string> GetUndefinedViewNames(MetaAction action)
        {
            if (action is DestroyViewMetaAction dma)
            {
                if (!viewNames.Contains(dma.ViewName))
                    yield return dma.ViewName;
            }
            else if (action is AllMetaAction ama)
            {
                foreach (var vw in ama.Data.SelectMany(GetUndefinedViewNames))
                    yield return vw;
            }
        }

        foreach (var rule in meta.Rules)
        {
            foreach (var view in GetUndefinedViewNames(rule.Action).Distinct())
                yield return new MetaValidationResult(meta, rule, $"Undefined view name: {view}");
        }
    }
}

public class ValidXMLViewValidator : IMetaValidator
{
    public IEnumerable<MetaValidationResult> ValidateMeta(Meta meta)
    {
        using var str = File.OpenRead("Assets/VTankView.xsd");
        var schema = XmlSchema.Read(str, null);

        bool IsValidXMLView(MetaAction action)
        {
            if (action is CreateViewMetaAction cva)
            {
                try
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(cva.ViewDefinition);
                    doc.Schemas.Add(schema);

                    doc.Validate(null);
                }
                catch (XmlSchemaValidationException)
                {
                    return false;
                }
                catch (XmlException)
                {
                    return false;
                }
            }
            else if (action is AllMetaAction ama)
            {
                if (ama.Data.Any(a => !IsValidXMLView(a)))
                    return false;
            }

            return true;
        }

        foreach (var rule in meta.Rules)
        {
            if (!IsValidXMLView(rule.Action))
                yield return new MetaValidationResult(meta, rule, @"Invalid View XML format");
        }
    }
}

public class ValidMetaExpressionFunctionValidator : IMetaValidator
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
        IEnumerable<string> GetExpressionFunctionNamesInCondition(Condition c)
        {
            if (c is ExpressionCondition ec)
            {
                var stream = new AntlrInputStream(ec.Expression);
                var lexer = new MetaExpressionsLexer(stream);
                var tokens = new CommonTokenStream(lexer);
                var parser = new MetaExpressionsParser(tokens);
                var ctx = parser.parse();
                var visitor = new FunctionNameVisitor();
                foreach (var fn in visitor.Visit(ctx))
                    yield return fn;
            }
            else if (c is MultipleCondition mc)
            {
                foreach (var f in mc.Data.SelectMany(GetExpressionFunctionNamesInCondition))
                    yield return f;
            }
        }

        IEnumerable<string> GetExpressionFunctionNamesInAction(MetaAction a)
        {
            if (a is ExpressionMetaAction ea)
            {
                var stream = new AntlrInputStream(ea.Expression);
                var lexer = new MetaExpressionsLexer(stream);
                var tokens = new CommonTokenStream(lexer);
                var parser = new MetaExpressionsParser(tokens);
                var ctx = parser.parse();
                var visitor = new FunctionNameVisitor();
                foreach (var fn in visitor.Visit(ctx))
                    yield return fn;
            }
            else if (a is AllMetaAction aa)
            {
                foreach (var f in aa.Data.SelectMany(GetExpressionFunctionNamesInAction))
                    yield return f;
            }
        }

        var rules = new List<MetaValidationResult>();
        foreach (var rule in meta.Rules)
        {
            try
            {
                foreach (var f in GetExpressionFunctionNamesInCondition(rule.Condition).Where(c => !VTANK_EXPRESSION_FUNCTIONS.Contains(c)))
                    rules.Add(new(meta, rule, $"Invalid expression function in condition: {f}"));
                foreach (var f in GetExpressionFunctionNamesInAction(rule.Action).Where(a => !VTANK_EXPRESSION_FUNCTIONS.Contains(a)))
                    rules.Add(new(meta, rule, $"Invalid expression function in action: {f}"));
            }
            catch (NullReferenceException) { }
        }

        return rules;
    }
}