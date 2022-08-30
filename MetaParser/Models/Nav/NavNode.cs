using System;
using System.Runtime.Serialization;

namespace MetaParser.Models
{
    public abstract class NavNode : ISerializable
    {
        public (double x, double y, double z) Point { get; set; }

        protected NavNode(NavNodeType type)
        {
            Type = type;
        }

        protected NavNode(SerializationInfo info, StreamingContext context)
        {
            Point = (
                    info.GetDouble("X"),
                    info.GetDouble("Y"),
                    info.GetDouble("Z")
                );
            Type = (NavNodeType)info.GetValue(nameof(Type), typeof(NavNodeType));
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

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("X", Point.x);
            info.AddValue("Y", Point.y);
            info.AddValue("Z", Point.z);
            info.AddValue(nameof(Type), Type, typeof(NavNodeType));
        }
    }

    public abstract class NavNode<T>
        : NavNode
    {
        public NavNode(NavNodeType type) : base(type) { }

        protected NavNode(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public T Data { get; set; } = default;

        public override string ToString() => $"{base.ToString()} {Data?.ToString()}".TrimEnd();
    }

    public class NavNodePoint
        : NavNode
    {
        public NavNodePoint() : base(NavNodeType.Point) { }

        protected NavNodePoint(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    public class NavNodePortalObs
        : NavNode<int>
    {
        public NavNodePortalObs() : base(NavNodeType.PortalObs) { }

        protected NavNodePortalObs(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Data = info.GetInt32(nameof(Data));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Data), Data);
        }
    }

    public class NavNodeRecall
        : NavNode<RecallSpellId>
    {
        public NavNodeRecall() : base(NavNodeType.Recall) { }

        protected NavNodeRecall(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Data = (RecallSpellId)info.GetValue(nameof(Data), typeof(RecallSpellId));
        }

        public override string ToString() => $"Recall: {Data.GetDescription()}";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Data), Data, typeof(RecallSpellId));
        }
    }

    public class NavNodePause
        : NavNode<double>
    {
        public NavNodePause() : base(NavNodeType.Pause) { }

        protected NavNodePause(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Data = info.GetDouble(nameof(Data));
        }

        public override string ToString() => $"Pause: {Data}ms";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Data), Data);
        }
    }

    public class NavNodeChat
        : NavNode<string>
    {
        public NavNodeChat() : base(NavNodeType.Chat) { }

        protected NavNodeChat(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Data = info.GetString(nameof(Data));
        }

        public override string ToString() => $"Chat: {Data}";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Data), Data, typeof(string));
        }
    }

    public class NavNodeOpenVendor
        : NavNode<(int vendorId, string vendorName)>
    {
        public NavNodeOpenVendor() : base(NavNodeType.OpenVendor) { }

        protected NavNodeOpenVendor(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Data = (
                    info.GetInt32("VendorId"),
                    info.GetString("VendorName")
                );
        }

        public override string ToString() => $"Open Vendor: {Data.vendorName ?? Data.vendorId.ToString()}";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("VendorId", Data.vendorId);
            info.AddValue("VendorName", Data.vendorName, typeof(string));
        }
    }

    public class NavNodePortal
        : NavNode<(string objectName, ObjectClass objectClass, double x, double y, double z)>
    {
        public NavNodePortal() : base(NavNodeType.Portal) { }

        protected NavNodePortal(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Data = (
                    info.GetString("ObjectName"),
                    (ObjectClass)info.GetValue("ObjectClass", typeof(ObjectClass)),
                    info.GetDouble("TargetX"),
                    info.GetDouble("TargetY"),
                    info.GetDouble("TargetZ")
                );
        }

        public override string ToString() => $"Portal: {Data.objectName} ({Math.Abs(Data.y):F3}{(Data.y > 0 ? "N" : "S")}, {Math.Abs(Data.x):F3}{(Data.x > 0 ? "E" : "W")})";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ObjectName", Data.objectName, typeof(string));
            info.AddValue("ObjectClass", Data.objectClass, typeof(ObjectClass));
            info.AddValue("TargetX", Data.x);
            info.AddValue("TargetY", Data.y);
            info.AddValue("TargetZ", Data.z);
        }
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

        protected NavNodeNPCChat(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Data = (
                    info.GetString("ObjectName"),
                    (ObjectClass)info.GetValue("ObjectClass", typeof(ObjectClass)),
                    info.GetDouble("TargetX"),
                    info.GetDouble("TargetY"),
                    info.GetDouble("TargetZ")
                );
        }

        public override string ToString() => $"Use NPC: {Data.objectName}";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ObjectName", Data.objectName, typeof(string));
            info.AddValue("ObjectClass", Data.objectClass, typeof(ObjectClass));
            info.AddValue("TargetX", Data.x);
            info.AddValue("TargetY", Data.y);
            info.AddValue("TargetZ", Data.z);
        }
    }

    public class NavNodeCheckpoint
        : NavNode
    {
        public NavNodeCheckpoint() : base(NavNodeType.Checkpoint) { }

        protected NavNodeCheckpoint(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    public class NavNodeJump
        : NavNode<(double heading, bool shift, double delay)>
    {
        public NavNodeJump() : base(NavNodeType.Jump) { }

        protected NavNodeJump(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Data = (
                info.GetDouble("Heading"),
                info.GetBoolean("Shift"),
                info.GetDouble("Delay")
                );
        }

        public override string ToString() => $"Jump: {Data.delay}ms {Data.heading} {(Data.shift ? "(shift)" : "")}";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Heading", Data.heading);
            info.AddValue("Shift", Data.shift);
            info.AddValue("Delay", Data.delay);
        }
    }
}