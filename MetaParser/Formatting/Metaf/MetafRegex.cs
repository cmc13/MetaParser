using System.Text.RegularExpressions;

namespace MetaParser.Formatting;

internal static partial class MetafRegex
{
    private const string DOUBLE_REGEX = @"[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))";

    // meta
    [GeneratedRegex(@"^\s*(~~.*)?$")]
    public static partial Regex EmptyLineRegex();
    [GeneratedRegex(@"^\s*(?<op>STATE|NAV):")]
    public static partial Regex StateNavRegex();
    [GeneratedRegex(@"^\s*{(?<state>([^{}]|{{|}})*)}\s*(~~.*)?")]
    public static partial Regex StateRegex();
    [GeneratedRegex(@"^(?<tabs>\t*)(?<op>IF:)")]
    public static partial Regex IfRegex();
    [GeneratedRegex(@"^(?<tabs>\t*)(?<op>DO:)")]
    public static partial Regex DoRegex();
    [GeneratedRegex(@"^(?<tabs>\t*)\s*(?<cond>\S*)")]
    public static partial Regex ConditionRegex();
    [GeneratedRegex(@"^(?<tabs>\t*)\s*(?<action>\S*)")]
    public static partial Regex ActionRegex();
    [GeneratedRegex(@"^\s*(?<arg>\d+)\s*(~~.*)?")]
    public static partial Regex SingleIntRegex();
    [GeneratedRegex(@"^\s*{(?<arg>([^{}]|{{|}})*)}\s*(~~.*)?")]
    public static partial Regex SingleStringRegex();
    [GeneratedRegex(@"^\s*(?<count>\d+)\s*{(?<item>([^{}]|{{|}})*)}\s*(~~.*)?")]
    public static partial Regex ItemCountRegex();
    [GeneratedRegex(@"^\s*(?<cell>[0-9a-fA-F]+)\s*(~~.*)?")]
    public static partial Regex LandCellRegex();
    [GeneratedRegex(@"^\s*{(?<arg1>([^{}]|{{|}})*)}\s*{(?<arg2>([^{}]|{{|}})*)}\s*(~~.*)?")]
    public static partial Regex DoubleStringRegex();
    [GeneratedRegex(@"^\s*" + @"(?<d>" + DOUBLE_REGEX + @")(\s+(?<d>" + DOUBLE_REGEX + @")){6}\s*$")]
    public static partial Regex NavTransformRegex();
    [GeneratedRegex(@"^\s*(?<distance>" + DOUBLE_REGEX + @")\s*(~~.*)?")]
    public static partial Regex DistanceRegex();
    [GeneratedRegex(@"^\s*(?<count>\d+)\s*(?<distance>" + DOUBLE_REGEX + @")\s*(?<priority>\d+)\s*(~~.*)?")]
    public static partial Regex MobsInDist_PriorityRegex();
    [GeneratedRegex(@"^\s*(?<count>\d+)\s*(?<distance>" + DOUBLE_REGEX + @")\s*{(?<name>([^{}]|{{|}})*)}\s*(~~.*)?")]
    public static partial Regex MobsInDist_NameRegex();
    [GeneratedRegex(@"\s*(?<seconds>\d+)\s*(?<spell>\d+)\s*(~~.*)?")]
    public static partial Regex SecsOnSpellGERegex();
    [GeneratedRegex(@"\s*(?<navRef>[a-zA-Z_][a-zA-Z0-9_]*)\s*{(?<navName>([^{}]|{{|}})*)}(\s+{(?<xf>([^{}]|{{|}})*)})?\s*(~~.*)?")]
    public static partial Regex EmbedNavRegex();
    [GeneratedRegex(@"^\s*(?<range>" + DOUBLE_REGEX + @")\s*(?<time>\d+)\s*{(?<state>([^{}]|{{|}})*)}\s*(~~.*)?")]
    public static partial Regex SetWatchdogRegex();

    // nav
    [GeneratedRegex(@"^\s*NAV:")]
    public static partial Regex NavLineRegex();
    [GeneratedRegex(@"^\s*(?<navRef>[a-zA-Z_][a-zA-Z0-9_]*)\s*(?<navType>circular|linear|once|follow)\s*(~~.*)?")]
    public static partial Regex NavRegex();
    [GeneratedRegex(@"^\s*(?<nodeType>pnt|prt|rcl|pau|cht|vnd|ptl|tlk|jmp|chk)")]
    public static partial Regex NavTypeRegex();
    [GeneratedRegex($@"^\s*(?<x>{DOUBLE_REGEX})\s*(?<y>{DOUBLE_REGEX})\s*(?<z>{DOUBLE_REGEX})")]
    public static partial Regex PointRegex();
    [GeneratedRegex(@"^\s*pnt|prt|rcl|pau|cht|vnd|ptl|tlk|chk|jmp")]
    public static partial Regex PointDefRegex();
    [GeneratedRegex(@"^\s*flw\s*(?<id>[0-9a-fA-F]+)\s*{(?<name>([^{}]|{{|}})*)}\s*(~~.*)?$")]
    public static partial Regex FollowRegex();
    [GeneratedRegex(@"^\s*(?<id>[0-9a-fA-F]+)\s*(~~.*)?$")]
    public static partial Regex PortalObsRegex();
    [GeneratedRegex(@"^\s*{(?<spell>([^{}]|{{|}})*)}\s*(~~.*)?$")]
    public static partial Regex RecallRegex();
    [GeneratedRegex(@"^\s*(?<time>" + DOUBLE_REGEX + @")\s*(~~.*)?$")]
    public static partial Regex PauseRegex();
    [GeneratedRegex(@"^\s*{(?<chat>([^{}]|{{|}})*)}\s*(~~.*)?$")]
    public static partial Regex ChatRegex();
    [GeneratedRegex(@"^\s*(?<id>[a-fA-F0-9]+)\s*{(?<name>([^{}]|{{|}})*)}\s*(~~.*)?$")]
    public static partial Regex VendorRegex();
    [GeneratedRegex(@"^\s*(?<oc>\d+)\s*{(?<name>([^{}]|{{|}})*)}\s*(~~.*)?$")]
    public static partial Regex PortalRegex();
    [GeneratedRegex(@"^\s*(?<heading>" + DOUBLE_REGEX + @")\s*{(?<tf>True|False)}\s*(?<time>" + DOUBLE_REGEX + @")\s*(~~.*)?$")]
    public static partial Regex JumpRegex();
}
