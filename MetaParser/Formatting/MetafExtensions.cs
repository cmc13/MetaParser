using MetaParser.Models;
using System.Collections.Generic;
using System.Linq;

namespace MetaParser.Formatting;

static class MetafExtensions
{
    public static string UnescapeString(this string str) => str.Replace("{{", "{").Replace("}}", "}");

    public static NavRoute ApplyTransform(this NavRoute nav, double[] transform)
    {
        if (nav.Data is List<NavNode> nodes)
            return new() { Type = nav.Type, Data = nodes.Select(n => n.ApplyTransform(transform)).ToList() };
        return nav;
    }

    public static NavNode ApplyTransform(this NavNode node, double[] transform)
    {
        var newNode = NavNode.Create(node.Type);

        var dataProperty = newNode.GetType().GetProperty("Data");
        dataProperty?.SetValue(newNode, dataProperty.GetValue(node));

        newNode.Point = node.Point.ApplyTransform(transform);

        return newNode;
    }

    public static (double, double, double) ApplyTransform(this (double x, double y, double z) pt, double[] transform)
    {
        return (
                transform[0] * pt.x + transform[1] * pt.y + transform[4],
                transform[2] * pt.x + transform[3] * pt.y + transform[5],
                transform[6] + pt.z
            );
    }
}
