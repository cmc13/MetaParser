using MetaParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MetaParser.Formatting
{
    public class DefaultNavReader : INavReader
    {
        public async Task ReadNavAsync(TextReader reader, NavRoute route)
        {
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!line.Equals("uTank2 NAV 1.2"))
                throw new Exception("Invalid nav route");

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!int.TryParse(line, out var navTypeInt))
                throw new Exception("Invalid nav route");

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
                    throw new Exception("Invalid nav route");

                for (var i = 0; i < nodeCount; ++i)
                {
                    line = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (!int.TryParse(line, out var navNodeTypeInt))
                        throw new Exception("Invalid nav route");

                    var navNode = NavNode.Create((NavNodeType)navNodeTypeInt);
                    list.Add(navNode);

                    await ReadNavNodeAsync(reader, navNode).ConfigureAwait(false);
                }

                route.Data = list;
            }
        }

        public async Task ReadNavFollowAsync(TextReader reader, NavFollow follow)
        {
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            follow.TargetName = line;

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!int.TryParse(line, out var targetId))
                throw new Exception("Invalid nav follow definition");

            follow.TargetId = targetId;
        }

        public async Task ReadNavNodeAsync(TextReader reader, NavNode node)
        {
            var line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!double.TryParse(line, out var x))
                throw new Exception("Invalid nav route node");

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!double.TryParse(line, out var y))
                throw new Exception("Invalid nav route node");

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!double.TryParse(line, out var z))
                throw new Exception("Invalid nav route node");

            line = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!line.Equals("0"))
                throw new Exception("Invalid nav route node");

            node.Point = (x, y, z);

            if (node is NavNodePortalObs portalObs)
            {
                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!int.TryParse(line, out var portalId))
                    throw new Exception("Invalid portal nav route node");

                portalObs.Data = portalId;
            }
            else if (node is NavNodeRecall recall)
            {
                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!int.TryParse(line, out var recallSpellIdInt) || !Enum.IsDefined(typeof(RecallSpellId), recallSpellIdInt))
                    throw new Exception("Invalid recall nav route node");

                recall.Data = (RecallSpellId)recallSpellIdInt;
            }
            else if (node is NavNodePause pause)
            {
                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!double.TryParse(line, out var pauseTime))
                    throw new Exception("Invalid pause nav route node");

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
                    throw new Exception("Invalid open vendor nav route node");

                line = await reader.ReadLineAsync().ConfigureAwait(false);

                openVendor.Data = (vendorId, line);
            }
            else if (node is NavNodePortal portal)
            {
                line = await reader.ReadLineAsync().ConfigureAwait(false);
                var objectName = line;

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!int.TryParse(line, out var objectClassInt) || !Enum.IsDefined(typeof(ObjectClass), objectClassInt))
                    throw new Exception("Invalid portal/npc nav route node");

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!line.Equals(bool.TrueString))
                    throw new Exception("Invalid portal/npc nav route node");

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!double.TryParse(line, out var tx))
                    throw new Exception("Invalid portal/npc nav route node");

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!double.TryParse(line, out var ty))
                    throw new Exception("Invalid portal/npc nav route node");

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!double.TryParse(line, out var tz))
                    throw new Exception("Invalid portal/npc nav route node");

                portal.Data = (objectName, (ObjectClass)objectClassInt, tx, ty, tz);
            }
            else if (node is NavNodeNPCChat npcChat)
            {
                line = await reader.ReadLineAsync().ConfigureAwait(false);
                var objectName = line;

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!int.TryParse(line, out var objectClassInt) || !Enum.IsDefined(typeof(ObjectClass), objectClassInt) || objectClassInt != (int)ObjectClass.NPC)
                    throw new Exception("Invalid portal/npc nav route node");

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!line.Equals(bool.TrueString))
                    throw new Exception("Invalid portal/npc nav route node");

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!double.TryParse(line, out var tx))
                    throw new Exception("Invalid portal/npc nav route node");

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!double.TryParse(line, out var ty))
                    throw new Exception("Invalid portal/npc nav route node");

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!double.TryParse(line, out var tz))
                    throw new Exception("Invalid portal/npc nav route node");

                npcChat.Data = (objectName, (ObjectClass)objectClassInt, tx, ty, tz);
            }
            else if (node is NavNodeJump jump)
            {
                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!double.TryParse(line, out var heading))
                    throw new Exception("Invalid jump nav route node");

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                bool shift;
                if (line.Equals(bool.TrueString))
                    shift = true;
                else if (line.Equals(bool.FalseString))
                    shift = false;
                else
                    throw new Exception("Invalid jump nav route node");

                line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (!double.TryParse(line, out var delay))
                    throw new Exception("Invalid jump nav route node");

                jump.Data = (heading, shift, delay);
            }
        }
    }
}
