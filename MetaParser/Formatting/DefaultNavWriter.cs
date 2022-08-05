using MetaParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MetaParser.Formatting
{
    public class DefaultNavWriter : INavWriter
    {
        public async Task WriteNavAsync(TextWriter writer, NavRoute route)
        {
            if (route.Data != null)
            {
                await writer.WriteLineAsync("uTank2 NAV 1.2").ConfigureAwait(false);
                await writer.WriteLineAsync(((int)route.Type).ToString()).ConfigureAwait(false);
                if (route.Type == NavType.Follow && route.Data is NavFollow follow)
                {
                    await WriteNavFollowAsync(writer, follow).ConfigureAwait(false);
                }
                else if (route.Data is List<NavNode> navNodes)
                {
                    await writer.WriteLineAsync(navNodes.Count.ToString()).ConfigureAwait(false);

                    foreach (var navNode in navNodes)
                    {
                        await writer.WriteLineAsync(((int)navNode.Type).ToString()).ConfigureAwait(false);
                        await WriteNavNodeAsync(writer, navNode).ConfigureAwait(false);
                    }
                }
                else
                    throw new InvalidOperationException("Cannot write nav with unknown type");
            }
        }

        public async Task WriteNavFollowAsync(TextWriter writer, NavFollow follow)
        {
            await writer.WriteLineAsync(follow.TargetName).ConfigureAwait(false);
            await writer.WriteLineAsync(follow.TargetId.ToString()).ConfigureAwait(false);
        }

        public async Task WriteNavNodeAsync(TextWriter writer, NavNode node)
        {
            await writer.WriteLineAsync(node.Point.x.ToString()).ConfigureAwait(false);
            await writer.WriteLineAsync(node.Point.y.ToString()).ConfigureAwait(false);
            await writer.WriteLineAsync(node.Point.z.ToString()).ConfigureAwait(false);
            await writer.WriteLineAsync("0").ConfigureAwait(false);

            if (node is NavNodePortalObs portalObs)
            {
                await writer.WriteLineAsync(portalObs.Data.ToString()).ConfigureAwait(false);
            }
            else if (node is NavNodeRecall recall)
            {
                await writer.WriteLineAsync(((int)recall.Data).ToString()).ConfigureAwait(false);
            }
            else if (node is NavNodePause pause)
            {
                await writer.WriteLineAsync(pause.Data.ToString()).ConfigureAwait(false);
            }
            else if (node is NavNodeChat chat)
            {
                await writer.WriteLineAsync(chat.Data.ToString()).ConfigureAwait(false);
            }
            else if (node is NavNodeOpenVendor openVendor)
            {
                await writer.WriteLineAsync(openVendor.Data.vendorId.ToString()).ConfigureAwait(false);
                await writer.WriteLineAsync(openVendor.Data.vendorName).ConfigureAwait(false);
            }
            else if (node is NavNodePortal portal)
            {
                await writer.WriteLineAsync(portal.Data.objectName).ConfigureAwait(false);
                await writer.WriteLineAsync(((int)portal.Data.objectClass).ToString()).ConfigureAwait(false);
                await writer.WriteLineAsync(bool.TrueString).ConfigureAwait(false);
                await writer.WriteLineAsync(portal.Data.x.ToString()).ConfigureAwait(false);
                await writer.WriteLineAsync(portal.Data.y.ToString()).ConfigureAwait(false);
                await writer.WriteLineAsync(portal.Data.z.ToString()).ConfigureAwait(false);
            }
            else if (node is NavNodeNPCChat npcChat)
            {
                await writer.WriteLineAsync(npcChat.Data.objectName).ConfigureAwait(false);
                await writer.WriteLineAsync(((int)npcChat.Data.objectClass).ToString()).ConfigureAwait(false);
                await writer.WriteLineAsync(bool.TrueString).ConfigureAwait(false);
                await writer.WriteLineAsync(npcChat.Data.x.ToString()).ConfigureAwait(false);
                await writer.WriteLineAsync(npcChat.Data.y.ToString()).ConfigureAwait(false);
                await writer.WriteLineAsync(npcChat.Data.z.ToString()).ConfigureAwait(false);
            }
            else if (node is NavNodeJump jump)
            {
                await writer.WriteLineAsync(jump.Data.heading.ToString()).ConfigureAwait(false);
                await writer.WriteLineAsync(jump.Data.shift ? bool.TrueString : bool.FalseString).ConfigureAwait(false);
                await writer.WriteLineAsync(jump.Data.delay.ToString()).ConfigureAwait(false);
            }
        }
    }
}
