using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MetaParser.Models
{
    public class NavRoute : ISerializable
    {
        private NavType type;

        public NavType Type
        {
            get => type;
            set
            {
                if (Type != value)
                {
                    if (type == NavType.Follow || (value != NavType.Follow && Data == null))
                    {
                        Data = new List<NavNode>();
                    }
                    else if (value == NavType.Follow)
                    {
                        Data = new NavFollow();
                    }

                    type = value;
                }
            }
        }

        public object Data { get; set; }

        public NavRoute() { }

        protected NavRoute(SerializationInfo info, StreamingContext context)
        {
            Type = (NavType)info.GetValue(nameof(Type), typeof(NavType));
            if (Type == NavType.Follow)
            {
                Data = info.GetValue(nameof(Data), typeof(NavFollow));
            }
            else
            {
                Data = info.GetValue(nameof(Data), typeof(List<NavNode>)) ?? new List<NavNode>();
            }
        }

        public override string ToString()
        {
            if (Type == NavType.Follow)
            {
                return Data.ToString();
            }
            else if (Data is List<NavNode> nodes)
            {
                return $"{Type} ({nodes.Count})";
            }

            throw new InvalidOperationException("Invalid nav spec");
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(Type), Type, typeof(NavType));
            info.AddValue(nameof(Data), Data, Data.GetType());
        }
    }
}
