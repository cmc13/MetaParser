using MetaParser.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MetaParser.Tests
{
    [TestClass]
    public class NavNodeTests
    {
        [TestMethod]
        [DataRow(NavNodeType.Chat, typeof(NavNodeChat))]
        [DataRow(NavNodeType.Checkpoint, typeof(NavNodeCheckpoint))]
        [DataRow(NavNodeType.Point, typeof(NavNodePoint))]
        [DataRow(NavNodeType.PortalObs, typeof(NavNodePortalObs))]
        [DataRow(NavNodeType.Recall, typeof(NavNodeRecall))]
        [DataRow(NavNodeType.Pause, typeof(NavNodePause))]
        [DataRow(NavNodeType.OpenVendor, typeof(NavNodeOpenVendor))]
        [DataRow(NavNodeType.Portal, typeof(NavNodePortal))]
        [DataRow(NavNodeType.NPCChat, typeof(NavNodeNPCChat))]
        [DataRow(NavNodeType.Jump, typeof(NavNodeJump))]
        public void Create_CreatesCorrectNode(NavNodeType type, Type expectedType)
        {
            var node = NavNode.Create(type);

            Assert.IsNotNull(node);
            Assert.IsInstanceOfType(node, expectedType);
        }
    }
}
