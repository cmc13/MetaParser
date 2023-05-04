using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace MetaParser.Models;

public abstract class MetaAction
{
    public abstract ActionType Type { get; protected init; }

    protected MetaAction() { }

    public static MetaAction CreateMetaAction(ActionType actionType) => actionType switch
    {
        ActionType.ExpressionAct => new ExpressionMetaAction(actionType),
        ActionType.Multiple => new AllMetaAction(),
        ActionType.SetState => new MetaAction<string>(actionType),
        ActionType.ReturnFromCall => new MetaAction<int>(actionType),
        ActionType.SetVTOption => new SetVTOptionMetaAction(),
        ActionType.ChatWithExpression => new ExpressionMetaAction(actionType),
        ActionType.CallState => new CallStateMetaAction(),
        ActionType.ChatCommand => new MetaAction<string>(actionType),
        ActionType.WatchdogSet => new WatchdogSetMetaAction(),
        ActionType.EmbeddedNavRoute => new EmbeddedNavRouteMetaAction(),
        ActionType.None => new MetaAction<int>(actionType),
        ActionType.WatchdogClear => new TableMetaAction(actionType),
        ActionType.GetVTOption => new GetVTOptionMetaAction(),
        ActionType.DestroyView => new DestroyViewMetaAction(),
        ActionType.DestroyAllViews => new TableMetaAction(actionType),
        ActionType.CreateView => new CreateViewMetaAction(),
        _ => throw new Exception($"Invalid action type ({actionType})"),
    };
}

public class MetaAction<T> : MetaAction
{
    public override ActionType Type { get; protected init; }

    public T Data { get; set; }

    public MetaAction(ActionType type)
    {
        Type = type;
    }

    protected MetaAction() : base() { }

    public override string ToString() => Type switch
    {
        ActionType.SetState => $"Set State: {Data}",
        ActionType.ChatCommand => $"Chat: {Data}",
        _ => Type.GetDescription()
    };
}

public class TableMetaAction : MetaAction<OrderedDictionary>
{
    public TableMetaAction(ActionType type) : base(type)
    {
        Data = new();
    }
}

public class DestroyViewMetaAction : TableMetaAction
{
    public DestroyViewMetaAction() : base(ActionType.DestroyView)
    { }

    public string ViewName
    {
        get => (string)Data["n"];
        set => Data["n"] = value;
    }

    public override string ToString() => $"Destroy View: {ViewName}";
}

public class CreateViewMetaAction : TableMetaAction
{
    public CreateViewMetaAction() : base(ActionType.CreateView)
    { }

    public string ViewName
    {
        get => (string)Data["n"] ?? "";
        set => Data["n"] = value;
    }

    public string ViewDefinition
    {
        get => ((ViewString)Data["x"])?.String ?? "";
        set => Data["x"] = (ViewString)value;
    }

    public override string ToString() => $"Create View: {ViewName}";
}

public class ExpressionMetaAction : TableMetaAction
{
    public ExpressionMetaAction(ActionType type) : base(type)
    { }

    public string Expression
    {
        get => (string)Data["e"];
        set => Data["e"] = value;
    }

    public override string ToString() => Type switch
    {
        ActionType.ChatWithExpression => $"Chat Expr: {Expression}",
        _ => $"Expr: {Expression}"
    };
}

public class AllMetaAction : MetaAction<List<MetaAction>>
{
    public AllMetaAction() : base(ActionType.Multiple)
    {
        Data = new();
    }

    public override string ToString() => $"Do All: {{ {string.Join("; ", Data)} }}";
}

public class SetVTOptionMetaAction : TableMetaAction
{
    public SetVTOptionMetaAction() : base(ActionType.SetVTOption)
    { }

    public string Option
    {
        get => (string)Data["o"] ?? "";
        set => Data["o"] = value;
    }

    public string Value
    {
        get => (string)Data["v"] ?? "";
        set => Data["v"] = value;
    }

    public override string ToString() => $"Set {Option} => {Value}";
}

public class GetVTOptionMetaAction : TableMetaAction
{
    public GetVTOptionMetaAction() : base(ActionType.GetVTOption)
    { }

    public string Option
    {
        get => (string)Data["o"] ?? "";
        set => Data["o"] = value;
    }

    public string Variable
    {
        get => (string)Data["v"] ?? "";
        set => Data["v"] = value;
    }

    public override string ToString() => $"Get {Option} => {Variable}";
}

public class CallStateMetaAction : TableMetaAction
{
    public CallStateMetaAction() : base(ActionType.CallState)
    { }

    public string CallState
    {
        get => (string)Data["st"] ?? "";
        set => Data["st"] = value;
    }

    public string ReturnState
    {
        get => (string)Data["ret"] ?? "";
        set => Data["ret"] = value;
    }

    public override string ToString() => $"Call: {CallState} (Ret: {ReturnState})";
}

public class WatchdogSetMetaAction : TableMetaAction
{
    public WatchdogSetMetaAction() : base(ActionType.WatchdogSet)
    { }

    public string State
    {
        get => (string)Data["s"] ?? "";
        set => Data["s"] = value;
    }

    public double Range
    {
        get => Data != null && Data["r"] != null ? (double)Data["r"] : default;
        set => Data["r"] = value;
    }

    public double Time
    {
        get => Data != null && Data["t"] != null ? (double)Data["t"] : default;
        set => Data["t"] = value;
    }

    public override string ToString() => $"Set Watchdog: {Range}m {Time}s => {State}";
}

public class EmbeddedNavRouteMetaAction : MetaAction<(string name, NavRoute nav)>
{
    public EmbeddedNavRouteMetaAction() : base(ActionType.EmbeddedNavRoute)
    { }

    public override string ToString() => "Load Embedded Nav Route";
}