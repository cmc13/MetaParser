using System;

namespace MetaParser.Models;

public abstract class NavNode
{
    public (double x, double y, double z) Point { get; set; }

    protected NavNode(NavNodeType type)
    {
        Type = type;
    }

    public NavNodeType Type { get; }

    public static NavNode Create(NavNodeType type) => type switch
    {
        NavNodeType.Chat => new NavNodeChat(),
        NavNodeType.Checkpoint => new NavNodeCheckpoint(),
        NavNodeType.Point => new NavNodePoint(),
        NavNodeType.PortalObs => new NavNodePortalObs(),
        NavNodeType.Recall => new NavNodeRecall(),
        NavNodeType.Pause => new NavNodePause(),
        NavNodeType.OpenVendor => new NavNodeOpenVendor(),
        NavNodeType.Portal => new NavNodePortal(),
        NavNodeType.NPCChat => new NavNodeNPCChat(),
        NavNodeType.Jump => new NavNodeJump(),
        _ => throw new Exception("Invalid nav node type")
    };

    public override string ToString() => $"{Type}: ({Math.Abs(Point.y):F3}{(Point.y > 0 ? "N" : "S")}, {Math.Abs(Point.x):F3}{(Point.x > 0 ? "E" : "W")})";
}

public abstract class NavNode<T>
    : NavNode
{
    public NavNode(NavNodeType type) : base(type) { }

    public T Data { get; set; } = default;

    public override string ToString() => $"{base.ToString()} {Data?.ToString()}".TrimEnd();
}

public class NavNodePoint
    : NavNode
{
    public NavNodePoint() : base(NavNodeType.Point) { }
}

public class NavNodePortalObs
    : NavNode<int>
{
    public NavNodePortalObs() : base(NavNodeType.PortalObs) { }
}

public class NavNodeRecall
    : NavNode<RecallSpellId>
{
    public NavNodeRecall() : base(NavNodeType.Recall) { }

    public override string ToString() => $"Recall: {Data.GetDescription()}";
}

public class NavNodePause
    : NavNode<double>
{
    public NavNodePause() : base(NavNodeType.Pause) { }

    public override string ToString() => $"Pause: {Data}ms";
}

public class NavNodeChat
    : NavNode<string>
{
    public NavNodeChat() : base(NavNodeType.Chat) { }

    public override string ToString() => $"Chat: {Data}";
}

public class NavNodeOpenVendor
    : NavNode<(int vendorId, string vendorName)>
{
    public NavNodeOpenVendor() : base(NavNodeType.OpenVendor) { }

    public override string ToString() => $"Open Vendor: {Data.vendorName ?? Data.vendorId.ToString()}";
}

public class NavNodePortal
    : NavNode<(string objectName, ObjectClass objectClass, double x, double y, double z)>
{
    public NavNodePortal() : base(NavNodeType.Portal) { }

    public override string ToString() => $"Portal: {Data.objectName} ({Math.Abs(Data.y):F3}{(Data.y > 0 ? "N" : "S")}, {Math.Abs(Data.x):F3}{(Data.x > 0 ? "E" : "W")})";
}

public class NavNodeNPCChat
    : NavNode<(string objectName, ObjectClass objectClass, double x, double y, double z)>
{
    public NavNodeNPCChat() : base(NavNodeType.NPCChat)
    {
        Data = (
                null,
                ObjectClass.NPC,
                0, 0, 0
            );
    }

    public override string ToString() => $"Use NPC: {Data.objectName}";
}

public class NavNodeCheckpoint
    : NavNode
{
    public NavNodeCheckpoint() : base(NavNodeType.Checkpoint) { }
}

public class NavNodeJump
    : NavNode<(double heading, bool shift, double delay)>
{
    public NavNodeJump() : base(NavNodeType.Jump) { }

    public override string ToString() => $"Jump: {Data.delay}ms {Data.heading} {(Data.shift ? "(shift)" : "")}";
}