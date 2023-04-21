using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace MetaParser.Models;

public abstract class Condition
{
    public ConditionType Type { get; protected init; }

    protected Condition() { }

    public static Condition CreateCondition(ConditionType conditionType) => conditionType switch
    {
        ConditionType.Never or
        ConditionType.Always or
        ConditionType.Died or
        ConditionType.NeedToBuff or
        ConditionType.NavrouteEmpty or
        ConditionType.VendorOpen or
        ConditionType.VendorClosed or
        ConditionType.PortalspaceEnter or
        ConditionType.PortalspaceExit or
        ConditionType.SecondsInStateGE or
        ConditionType.SecondsInStatePersistGE or
        ConditionType.MainPackSlotsLE or
        ConditionType.BurdenPercentGE or
        ConditionType.LandBlockE or
        ConditionType.LandCellE => new Condition<int>(conditionType),
        ConditionType.ChatMessage => new Condition<string>(conditionType),
        ConditionType.Expression => new ExpressionCondition(),
        ConditionType.Not => new NotCondition(),
        ConditionType.DistanceToAnyRoutePointGE => new DistanceToAnyRoutePointGECondition(),
        ConditionType.Any => new MultipleCondition(ConditionType.Any),
        ConditionType.All => new MultipleCondition(ConditionType.All),
        ConditionType.ItemCountLE => new ItemCountCondition(ConditionType.ItemCountLE),
        ConditionType.ItemCountGE => new ItemCountCondition(ConditionType.ItemCountGE),
        ConditionType.NoMonstersWithinDistance => new NoMonstersInDistanceCondition(),
        ConditionType.ChatMessageCapture => new ChatMessageCaptureCondition(),
        ConditionType.MonsterCountWithinDistance => new MonsterCountWithinDistanceCondition(),
        ConditionType.MonstersWithPriorityWithinDistance => new MonstersWithPriorityWithinDistanceCondition(),
        ConditionType.TimeLeftOnSpellGE => new TimeLeftOnSpellGECondition(),
        _ => throw new Exception($"Invalid Meta condition: {conditionType}"),
    };
}

public class Condition<T>
    : Condition
{
    private T data;

    public T Data
    {
        get => data;
        set
        {
            data = value;
        }
    }

    public Condition(ConditionType type)
    {
        Type = type;
    }

    public override string ToString() => Type switch
    {
        ConditionType.SecondsInStateGE or
        ConditionType.BurdenPercentGE or
        ConditionType.SecondsInStatePersistGE or
        ConditionType.MainPackSlotsLE => $"{Type.GetDescription()} {Data}",
        ConditionType.LandBlockE or
        ConditionType.LandCellE => $"{Type.GetDescription()} {ConvertToUint((int)Convert.ChangeType(Data, typeof(int))):X8}",
        ConditionType.ChatMessage => $"{Type.GetDescription()}: {Data}",
        _ => Type.GetDescription()
    };

    private static uint ConvertToUint(int num)
    {
        unchecked
        {
            return (uint)num;
        }
    }
}

public class TableCondition
    : Condition<OrderedDictionary>
{
    public TableCondition(ConditionType type) : base(type)
    {
        Data = new();
    }
}

public class MultipleCondition
    : Condition<List<Condition>>
{
    public MultipleCondition(ConditionType type) : base(type)
    {
        Data = new();
    }

    public override string ToString() => $"{Type}: {{ {string.Join("; ", Data)} }}";
}

public class NotCondition
    : Condition<Condition>
{
    public NotCondition() : base(ConditionType.Not)
    {
        Data = Condition.CreateCondition(ConditionType.Never);
    }

    public override string ToString() => $"Not {Data}";
}

public class ExpressionCondition
    : TableCondition
{
    public ExpressionCondition() : base(ConditionType.Expression)
    { }

    public string Expression
    {
        get => (string)Data["e"] ?? "";
        set => Data["e"] = value;
    }

    public override string ToString() => $"Expr: {Expression}";
}

public class DistanceToAnyRoutePointGECondition
    : TableCondition
{
    public DistanceToAnyRoutePointGECondition() : base(ConditionType.DistanceToAnyRoutePointGE)
    { }

    public double Distance
    {
        get => Data != null && Data["dist"] != null ? (double)Data["dist"] : default;
        set => Data["dist"] = value;
    }

    public override string ToString() => $"Distance to Any Route Pt ≥ {Distance}"; 
}

public class NoMonstersInDistanceCondition
    : TableCondition
{
    public NoMonstersInDistanceCondition() : base(ConditionType.NoMonstersWithinDistance)
    { }

    public double Distance
    {
        get => Data != null && Data["r"] != null ? (double)Data["r"] : default;
        set => Data["r"] = value;
    }

    public override string ToString() => $"No Monsters Within {Distance}m";
}

public class MonsterCountWithinDistanceCondition
    : TableCondition
{
    public MonsterCountWithinDistanceCondition() : base(ConditionType.MonsterCountWithinDistance)
    { }

    public string MonsterNameRx
    {
        get => (string)Data["n"];
        set => Data["n"] = value;
    }

    public int Count
    {
        get => Data != null && Data["c"] != null ? (int)Data["c"] : default(int);
        set => Data["c"] = value;
    }

    public double Distance
    {
        get => Data != null && Data["r"] != null ? (double)Data["r"] : default(double);
        set => Data["r"] = value;
    }

    public override string ToString() => $"{Count} monsters matching '{MonsterNameRx}' within {Distance}m";
}

public class MonstersWithPriorityWithinDistanceCondition
    : TableCondition
{
    public MonstersWithPriorityWithinDistanceCondition() : base(ConditionType.MonstersWithPriorityWithinDistance)
    { }

    public int Priority
    {
        get => Data != null && Data["p"] != null ? (int)Data["p"] : default(int);
        set => Data["p"] = value;
    }

    public int Count
    {
        get => Data != null && Data["c"] != null ? (int)Data["c"] : default(int);
        set => Data["c"] = value;
    }

    public double Distance
    {
        get => Data != null && Data["r"] != null ? (double)Data["r"] : default(double);
        set => Data["r"] = value;
    }

    public override string ToString() => $"{Count} monsters with {Priority} priority within {Distance}m";
}

public class ItemCountCondition
    : TableCondition
{
    public ItemCountCondition(ConditionType type) : base(type)
    { }

    public string ItemName
    {
        get => (string)Data["n"] ?? "";
        set => Data["n"] = value;
    }

    public int Count
    {
        get => Data != null && Data["c"] != null ? (int)Data["c"] : default(int);
        set => Data["c"] = value;
    }

    public override string ToString() => $"{ItemName} Count {(Type == ConditionType.ItemCountGE ? "≥" : "≤")} {Count}";
}

public class ChatMessageCaptureCondition
    : TableCondition
{
    public ChatMessageCaptureCondition() : base(ConditionType.ChatMessageCapture)
    { }

    public string Pattern
    {
        get => (string)Data["p"];
        set => Data["p"] = value;
    }

    public string Color
    {
        get => (string)Data["c"];
        set => Data["c"] = value;
    }

    public override string ToString() => $"Chat Message matching '{Pattern}' ({(string.IsNullOrEmpty(Color) ? "Any" : Color)})";
}

public class TimeLeftOnSpellGECondition
    : TableCondition
{
    public TimeLeftOnSpellGECondition() : base(ConditionType.TimeLeftOnSpellGE)
    { }

    public int SpellId
    {
        get => Data != null && Data["sid"] != null ? (int)Data["sid"] : default(int);
        set => Data["sid"] = value;
    }

    public int Seconds
    {
        get => Data != null && Data["sec"] != null ? (int)Data["sec"] : default(int);
        set => Data["sec"] = value;
    }

    public override string ToString() => $"{Seconds}s left on spell ({(Spells.SpellList.TryGetValue(SpellId, out var s) ? s : SpellId)})";
}