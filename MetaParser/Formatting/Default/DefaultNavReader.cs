using MetaParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
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

        route.Type = await reader.ReadEnumAsync<NavType>().ConfigureAwait(false);

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
                var navNodeType = await reader.ReadEnumAsync<NavNodeType>().ConfigureAwait(false);

                var navNode = NavNode.Create(navNodeType);
                list.Add(navNode);

                await ReadNavNodeAsync(reader, navNode).ConfigureAwait(false);
            }

            route.Data = list;
        }

        return route;
    }

    public async Task ReadNavFollowAsync(TextReader reader, NavFollow follow)
    {
        follow.TargetName = await reader.ReadLineAsync().ConfigureAwait(false);
        follow.TargetId = await reader.ReadIntAsync().ConfigureAwait(false);
    }

    private async Task<(double x, double y, double z)> ReadPointAsync(TextReader reader)
    {
        var d = new double[3];
        for (var i = 0; i < d.Length; ++i)
        {
            d[i] = await reader.ReadDoubleAsync().ConfigureAwait(false);
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
            portalObs.Data = await reader.ReadIntAsync().ConfigureAwait(false);
        }
        else if (node is NavNodeRecall recall)
        {
            recall.Data = await reader.ReadEnumAsync<RecallSpellId>().ConfigureAwait(false);
        }
        else if (node is NavNodePause pause)
        {
            pause.Data = await reader.ReadDoubleAsync().ConfigureAwait(false);
        }
        else if (node is NavNodeChat chat)
        {
            chat.Data = await reader.ReadLineAsync().ConfigureAwait(false);
        }
        else if (node is NavNodeOpenVendor openVendor)
        {
            var vendorId = await reader.ReadIntAsync().ConfigureAwait(false);

            line = await reader.ReadLineAsync().ConfigureAwait(false);

            openVendor.Data = (vendorId, line);
        }
        else if (node is NavNodePortal portal)
        {
            line = await reader.ReadLineAsync().ConfigureAwait(false);
            var objectName = line;

            var objectClass = await reader.ReadEnumAsync<ObjectClass>().ConfigureAwait(false);

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!line.Equals(bool.TrueString))
                throw new MetaParserException("Invalid portal nav route node", bool.TrueString, line);

            var (tx, ty, tz) = await ReadPointAsync(reader).ConfigureAwait(false);

            portal.Data = (objectName, objectClass, tx, ty, tz);
        }
        else if (node is NavNodeNPCChat npcChat)
        {
            line = await reader.ReadLineAsync().ConfigureAwait(false);
            var objectName = line;

            var objectClass = await reader.ReadEnumAsync<ObjectClass>().ConfigureAwait(false);
            if (objectClass != ObjectClass.NPC)
                throw new MetaParserException("Object class must be NPC type for npc chat node", ((int)ObjectClass.NPC).ToString(), ((int)objectClass).ToString());

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!line.Equals(bool.TrueString))
                throw new MetaParserException("Invalid npc nav route node", bool.TrueString, line);

            var (tx, ty, tz) = await ReadPointAsync(reader).ConfigureAwait(false);

            npcChat.Data = (objectName, objectClass, tx, ty, tz);
        }
        else if (node is NavNodeJump jump)
        {
            var heading = await reader.ReadDoubleAsync().ConfigureAwait(false);

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            bool shift;
            if (line.Equals(bool.TrueString))
                shift = true;
            else if (line.Equals(bool.FalseString))
                shift = false;
            else
                throw new MetaParserException("Invalid jump nav route node", $"{bool.TrueString}|{bool.FalseString}", line);

            var delay = await reader.ReadDoubleAsync().ConfigureAwait(false);

            jump.Data = (heading, shift, delay);
        }
    }
}
