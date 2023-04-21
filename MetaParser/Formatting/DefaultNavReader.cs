using MetaParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MetaParser.Formatting;

public class DefaultNavReader : INavReader
{
    public async Task<NavRoute> ReadNavAsync(TextReader reader)
    {
        NavRoute route = new();

        var line = await reader.ReadLineAsync().ConfigureAwait(false);
        if (!line.Equals("uTank2 NAV 1.2"))
            throw new MetaParserException("Invalid nav route", "uTank2 NAV 1.2", line);

        line = await reader.ReadLineAsync().ConfigureAwait(false);
        if (!int.TryParse(line, out var navTypeInt) || !Enum.IsDefined(typeof(NavType), navTypeInt))
            throw new MetaParserException("Invalid nav type", string.Join('|', Enum.GetValues<NavType>().Cast<int>()), line);

        route.Type = (NavType)navTypeInt;

        if (route.Type == NavType.Follow)
        {
            var follow = new NavFollow();
            await ReadNavFollowAsync(reader, follow).ConfigureAwait(false);

            route.Data = follow;
        }
        else
        {
            var list = new List<NavNode>();
            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!int.TryParse(line, out var nodeCount))
                throw new MetaParserException("Invalid nav node count", typeof(int), line);

            for (var i = 0; i < nodeCount; ++i)
            {
                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!int.TryParse(line, out var navNodeTypeInt) || !Enum.IsDefined(typeof(NavNodeType), navNodeTypeInt))
                    throw new MetaParserException("Invalid nav node type", typeof(int), line);

                var navNode = NavNode.Create((NavNodeType)navNodeTypeInt);
                list.Add(navNode);

                await ReadNavNodeAsync(reader, navNode).ConfigureAwait(false);
            }

            route.Data = list;
        }

        return route;
    }

    public async Task ReadNavFollowAsync(TextReader reader, NavFollow follow)
    {
        var line = await reader.ReadLineAsync().ConfigureAwait(false);
        follow.TargetName = line;

        line = await reader.ReadLineAsync().ConfigureAwait(false);
        if (!int.TryParse(line, out var targetId))
            throw new MetaParserException("Invalid nav follow target id", typeof(int), line);

        follow.TargetId = targetId;
    }

    private async Task<(double x, double y, double z)> ReadPointAsync(TextReader reader)
    {
        var d = new double[3];
        for (var i = 0; i < d.Length; ++i)
        {
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!double.TryParse(line, out d[i]))
                throw new MetaParserException($"Invalid point specification -- {"xyz"[i]} coordinate", typeof(double), line);
        }

        return (d[0], d[1], d[2]);
    }

    public async Task ReadNavNodeAsync(TextReader reader, NavNode node)
    {
        node.Point = await ReadPointAsync(reader).ConfigureAwait(false);

        var line = await reader.ReadLineAsync().ConfigureAwait(false);
        if (!line.Equals("0"))
            throw new MetaParserException("Invalid nav route node", "0", line);

        if (node is NavNodePortalObs portalObs)
        {
            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!int.TryParse(line, out var portalId))
                throw new MetaParserException("Invalid portal nav route node (portal id)", typeof(int), line);

            portalObs.Data = portalId;
        }
        else if (node is NavNodeRecall recall)
        {
            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!int.TryParse(line, out var recallSpellIdInt) || !Enum.IsDefined(typeof(RecallSpellId), recallSpellIdInt))
                throw new MetaParserException("Invalid recall nav route node (spell id)", typeof(int), line);

            recall.Data = (RecallSpellId)recallSpellIdInt;
        }
        else if (node is NavNodePause pause)
        {
            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!double.TryParse(line, out var pauseTime))
                throw new MetaParserException("Invalid pause nav route node (pause time)", typeof(double), line);

            pause.Data = pauseTime;
        }
        else if (node is NavNodeChat chat)
        {
            line = await reader.ReadLineAsync().ConfigureAwait(false);

            chat.Data = line;
        }
        else if (node is NavNodeOpenVendor openVendor)
        {
            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!int.TryParse(line, out var vendorId))
                throw new MetaParserException("Invalid open vendor nav route node (vendor id)", typeof(int), line);

            line = await reader.ReadLineAsync().ConfigureAwait(false);

            openVendor.Data = (vendorId, line);
        }
        else if (node is NavNodePortal portal)
        {
            line = await reader.ReadLineAsync().ConfigureAwait(false);
            var objectName = line;

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!int.TryParse(line, out var objectClassInt) || !Enum.IsDefined(typeof(ObjectClass), objectClassInt))
                throw new MetaParserException("Invalid portal nav route node", typeof(int), line);

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!line.Equals(bool.TrueString))
                throw new MetaParserException("Invalid portal nav route node", bool.TrueString, line);

            var (tx, ty, tz) = await ReadPointAsync(reader).ConfigureAwait(false);

            portal.Data = (objectName, (ObjectClass)objectClassInt, tx, ty, tz);
        }
        else if (node is NavNodeNPCChat npcChat)
        {
            line = await reader.ReadLineAsync().ConfigureAwait(false);
            var objectName = line;

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!int.TryParse(line, out var objectClassInt) || !Enum.IsDefined(typeof(ObjectClass), objectClassInt) || objectClassInt != (int)ObjectClass.NPC)
                throw new MetaParserException("Invalid npc nav route node", ((int)ObjectClass.NPC).ToString(), line);

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!line.Equals(bool.TrueString))
                throw new MetaParserException("Invalid npc nav route node", bool.TrueString, line);

            var (tx, ty, tz) = await ReadPointAsync(reader).ConfigureAwait(false);

            npcChat.Data = (objectName, (ObjectClass)objectClassInt, tx, ty, tz);
        }
        else if (node is NavNodeJump jump)
        {
            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!double.TryParse(line, out var heading))
                throw new MetaParserException("Invalid jump nav route node", typeof(double), line);

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            bool shift;
            if (line.Equals(bool.TrueString))
                shift = true;
            else if (line.Equals(bool.FalseString))
                shift = false;
            else
                throw new MetaParserException("Invalid jump nav route node", $"{bool.TrueString}|{bool.FalseString}", line);

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!double.TryParse(line, out var delay))
                throw new MetaParserException("Invalid jump nav route node", typeof(double), line);

            jump.Data = (heading, shift, delay);
        }
    }
}
