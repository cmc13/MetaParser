using System;
using System.Collections.Generic;

namespace MetaParser.Models;

public class NavRoute
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
}
