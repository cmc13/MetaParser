using AutoFixture;
using MetaParser.Formatting;
using MetaParser.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MetaParser.Tests;

[TestClass]
public class MetafNavReaderTests
{
    private readonly Fixture fixture = new();

    [TestMethod]
    [DataRow("once", NavType.Once)]
    [DataRow("circular", NavType.Circular)]
    [DataRow("linear", NavType.Linear)]
    public async Task ReadNavAsync_HappyPath_ReadsCorrectNavType(string navTypeString, NavType expectedType)
    {
        var sb = new StringBuilder()
            .Append("NAV: _ ").AppendLine(navTypeString);
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var nav = await navReader.ReadNavAsync(reader);

        Assert.IsNotNull(nav);
        Assert.AreEqual(expectedType, nav.Type);
    }

    [TestMethod]
    public async Task ReadNavAsync_InvalidNavType_ThrowsException()
    {
        var navType = fixture.Create<string>();
        var sb = new StringBuilder()
            .Append("NAV: _ ").AppendLine(navType);
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavAsync(reader));
        Assert.AreEqual("Invalid nav declaration", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavAsync_HappyPath_CorrectlyReadsFollowNav()
    {
        var followId = fixture.Create<int>();
        var followName = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("NAV: _ follow")
            .AppendLine($"flw {followId:X} {{{followName}}}");
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var nav = await navReader.ReadNavAsync(reader);

        Assert.AreEqual(NavType.Follow, nav.Type);
        Assert.IsInstanceOfType<NavFollow>(nav.Data);
        var follow = nav.Data as NavFollow;
        Assert.AreEqual(followId, follow.TargetId);
        Assert.AreEqual(followName, follow.TargetName);
    }

    [TestMethod]
    public async Task ReadNavAsync_InvalidNavFollowDefinition_ThrowsException()
    {
        var navType = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("NAV: _ follow")
            .AppendLine("flw");
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavAsync(reader));
        Assert.AreEqual("Invalid nav follow definition", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavAsync_HappyPath_ReadsMultipleNodes()
    {
        var sb = new StringBuilder()
            .AppendLine("NAV: _ once");

        for (var i = 0; i < 10; ++i)
            sb.AppendLine("pnt 0.0 0.0 0.0");
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var nav = await navReader.ReadNavAsync(reader);

        Assert.IsNotNull(nav?.Data);
        Assert.IsInstanceOfType<List<NavNode>>(nav.Data);
        var navNodeList = nav.Data as List<NavNode>;

        Assert.AreEqual(10, navNodeList.Count);
    }

    [TestMethod]
    public async Task ReadNavAsync_HappyPath_ReadsNavNode()
    {
        var expectedX = fixture.Create<double>();
        var expectedY = fixture.Create<double>();
        var expectedZ = fixture.Create<double>();
        var sb = new StringBuilder()
            .AppendLine("NAV: _ once")
            .AppendLine($"pnt {expectedX} {expectedY} {expectedZ}");
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var nav = await navReader.ReadNavAsync(reader);

        Assert.IsNotNull(nav?.Data);
        Assert.IsInstanceOfType<List<NavNode>>(nav.Data);
        var navNodeList = nav.Data as List<NavNode>;

        Assert.AreEqual(1, navNodeList.Count);
        Assert.AreEqual(expectedX, navNodeList[0].Point.x);
        Assert.AreEqual(expectedY, navNodeList[0].Point.y);
        Assert.AreEqual(expectedZ, navNodeList[0].Point.z);
    }

    [TestMethod]
    public async Task ReadNavAsync_HappyPath_ReadsPointNode()
    {
        var expectedX = fixture.Create<double>();
        var expectedY = fixture.Create<double>();
        var expectedZ = fixture.Create<double>();
        var sb = new StringBuilder()
            .AppendLine("NAV: _ once")
            .AppendLine($"pnt {expectedX} {expectedY} {expectedZ}");
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var nav = await navReader.ReadNavAsync(reader);

        Assert.IsNotNull(nav?.Data);
        Assert.IsInstanceOfType<List<NavNode>>(nav.Data);
        var navNodeList = nav.Data as List<NavNode>;

        Assert.AreEqual(1, navNodeList.Count);
        Assert.AreEqual(NavNodeType.Point, navNodeList[0].Type);
        Assert.IsInstanceOfType<NavNodePoint>(navNodeList[0]);
    }

    [TestMethod]
    public async Task ReadNavAsync_HappyPath_ReadsCheckpointNode()
    {
        var expectedX = fixture.Create<double>();
        var expectedY = fixture.Create<double>();
        var expectedZ = fixture.Create<double>();
        var sb = new StringBuilder()
            .AppendLine("NAV: _ once")
            .AppendLine($"chk {expectedX} {expectedY} {expectedZ}");
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var nav = await navReader.ReadNavAsync(reader);

        Assert.IsNotNull(nav?.Data);
        Assert.IsInstanceOfType<List<NavNode>>(nav.Data);
        var navNodeList = nav.Data as List<NavNode>;

        Assert.AreEqual(1, navNodeList.Count);
        Assert.AreEqual(NavNodeType.Checkpoint, navNodeList[0].Type);
        Assert.IsInstanceOfType<NavNodeCheckpoint>(navNodeList[0]);
    }

    [TestMethod]
    public async Task ReadNavAsync_HappyPath_ReadsPortalObsNode()
    {
        var expectedTargetId = fixture.Create<int>();
        var sb = new StringBuilder()
            .AppendLine("NAV: _ once")
            .AppendLine($"prt 0.0 0.0 0.0 {expectedTargetId:X}");
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var nav = await navReader.ReadNavAsync(reader);

        Assert.IsNotNull(nav?.Data);
        Assert.IsInstanceOfType<List<NavNode>>(nav.Data);
        var navNodeList = nav.Data as List<NavNode>;

        Assert.AreEqual(1, navNodeList.Count);
        Assert.AreEqual(NavNodeType.PortalObs, navNodeList[0].Type);
        Assert.IsInstanceOfType<NavNodePortalObs>(navNodeList[0]);
        var navNode = navNodeList[0] as NavNodePortalObs;

        Assert.AreEqual(expectedTargetId, navNode.Data);
    }

    [TestMethod]
    [DataRow("Primary Portal Recall", RecallSpellId.PrimaryPortalRecall)]
    [DataRow("Secondary Portal Recall", RecallSpellId.SecondaryPortalRecall)]
    [DataRow("Lifestone Recall", RecallSpellId.LifestoneRecall)]
    [DataRow("Lifestone Sending", RecallSpellId.LifestoneSending)]
    [DataRow("Portal Recall", RecallSpellId.PortalRecall)]
    [DataRow("Recall Aphus Lassel", RecallSpellId.RecallAphusLassel)]
    [DataRow("Recall the Sanctuary", RecallSpellId.RecalltheSanctuary)]
    [DataRow("Recall to the Singularity Caul", RecallSpellId.RecalltotheSingularityCaul)]
    [DataRow("Glenden Wood Recall", RecallSpellId.GlendenWoodRecall)]
    [DataRow("Aerlinthe Recall", RecallSpellId.AerlintheRecall)]
    [DataRow("Mount Lethe Recall", RecallSpellId.MountLetheRecall)]
    [DataRow("Ulgrim's Recall", RecallSpellId.UlgrimsRecall)]
    [DataRow("Bur Recall", RecallSpellId.BurRecall)]
    [DataRow("Paradox-touched Olthoi Infested Area Recall", RecallSpellId.ParadoxTouchedOlthoiInfestedAreaRecall)]
    [DataRow("Call of the Mhoire Forge", RecallSpellId.CalloftheMhoireForge)]
    [DataRow("Colosseum Recall", RecallSpellId.ColosseumRecall)]
    [DataRow("Facility Hub Recall", RecallSpellId.FacilityHubRecall)]
    [DataRow("Gear Knight Invasion Area Camp Recall", RecallSpellId.GearKnightInvasionAreaCampRecall)]
    [DataRow("Lost City of Neftet Recall", RecallSpellId.LostCityofNeftetRecall)]
    [DataRow("Return to the Keep", RecallSpellId.ReturntotheKeep)]
    [DataRow("Rynthid Recall", RecallSpellId.RynthidRecall)]
    [DataRow("Viridian Rise Recall", RecallSpellId.ViridianRiseRecall)]
    [DataRow("Viridian Rise Great Tree Recall", RecallSpellId.ViridianRiseGreatTreeRecall)]
    [DataRow("Celestial Hand Stronghold Recall", RecallSpellId.CelestialHandStrongholdRecall)]
    [DataRow("Radiant Blood Stronghold Recall", RecallSpellId.RadiantBloodStrongholdRecall)]
    [DataRow("Eldrytch Web Stronghold Recall", RecallSpellId.EldrytchWebStrongholdRecall)]
    public async Task ReadNavAsync_HappyPath_ReadsRecallNode(string spellName, RecallSpellId expectedRecallSpellId)
    {
        var sb = new StringBuilder()
            .AppendLine("NAV: _ once")
            .AppendLine($"rcl 0.0 0.0 0.0 {{{spellName}}}");
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var nav = await navReader.ReadNavAsync(reader);

        Assert.IsNotNull(nav?.Data);
        Assert.IsInstanceOfType<List<NavNode>>(nav.Data);
        var navNodeList = nav.Data as List<NavNode>;

        Assert.AreEqual(1, navNodeList.Count);
        Assert.AreEqual(NavNodeType.Recall, navNodeList[0].Type);
        Assert.IsInstanceOfType<NavNodeRecall>(navNodeList[0]);
        var navNode = navNodeList[0] as NavNodeRecall;

        Assert.AreEqual(expectedRecallSpellId, navNode.Data);
    }

    [TestMethod]
    public async Task ReadNavAsync_InvalidRecallSpellName_ThrowsException()
    {
        var invalidSpellName = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("NAV: _ once")
            .AppendLine($"rcl 0.0 0.0 0.0 {{{invalidSpellName}}}");
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavAsync(reader));
        Assert.AreEqual($"Invalid spell name: {invalidSpellName}", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavAsync_HappyPath_ReadsPauseNode()
    {
        var pauseTime = fixture.Create<double>();
        var sb = new StringBuilder()
            .AppendLine("NAV: _ once")
            .AppendLine($"pau 0.0 0.0 0.0 {pauseTime}");
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var nav = await navReader.ReadNavAsync(reader);

        Assert.IsNotNull(nav?.Data);
        Assert.IsInstanceOfType<List<NavNode>>(nav.Data);
        var navNodeList = nav.Data as List<NavNode>;

        Assert.AreEqual(1, navNodeList.Count);
        Assert.AreEqual(NavNodeType.Pause, navNodeList[0].Type);
        Assert.IsInstanceOfType<NavNodePause>(navNodeList[0]);
        var navNode = navNodeList[0] as NavNodePause;

        Assert.AreEqual(pauseTime, navNode.Data);
    }

    [TestMethod]
    public async Task ReadNavAsync_HappyPath_ReadsChatNode()
    {
        var sentence = string.Join(' ', fixture.CreateMany<string>());
        var sb = new StringBuilder()
            .AppendLine("NAV: _ once")
            .AppendLine($"cht 0.0 0.0 0.0 {{{sentence}}}");
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var nav = await navReader.ReadNavAsync(reader);

        Assert.IsNotNull(nav?.Data);
        Assert.IsInstanceOfType<List<NavNode>>(nav.Data);
        var navNodeList = nav.Data as List<NavNode>;

        Assert.AreEqual(1, navNodeList.Count);
        Assert.AreEqual(NavNodeType.Chat, navNodeList[0].Type);
        Assert.IsInstanceOfType<NavNodeChat>(navNodeList[0]);
        var navNode = navNodeList[0] as NavNodeChat;

        Assert.AreEqual(sentence, navNode.Data);
    }

    [TestMethod]
    public async Task ReadNavAsync_HappyPath_ReadsOpenVendorNode()
    {
        var expectedTargetId = fixture.Create<int>();
        var expectedTargetName = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("NAV: _ once")
            .AppendLine($"vnd 0.0 0.0 0.0 {expectedTargetId:X} {{{expectedTargetName}}}");
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var nav = await navReader.ReadNavAsync(reader);

        Assert.IsNotNull(nav?.Data);
        Assert.IsInstanceOfType<List<NavNode>>(nav.Data);
        var navNodeList = nav.Data as List<NavNode>;

        Assert.AreEqual(1, navNodeList.Count);
        Assert.AreEqual(NavNodeType.OpenVendor, navNodeList[0].Type);
        Assert.IsInstanceOfType<NavNodeOpenVendor>(navNodeList[0]);
        var navNode = navNodeList[0] as NavNodeOpenVendor;

        Assert.AreEqual(expectedTargetId, navNode.Data.vendorId);
        Assert.AreEqual(expectedTargetName, navNode.Data.vendorName);
    }

    [TestMethod]
    public async Task ReadNavAsync_HappyPath_ReadsPortalNode()
    {
        var expectedX = fixture.Create<double>();
        var expectedY = fixture.Create<double>();
        var expectedZ = fixture.Create<double>();
        var expectedOC = (int)fixture.Create<ObjectClass>();
        var expectedTargetName = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("NAV: _ once")
            .AppendLine($"ptl 0.0 0.0 0.0 {expectedX} {expectedY} {expectedZ} {expectedOC} {{{expectedTargetName}}}");
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var nav = await navReader.ReadNavAsync(reader);

        Assert.IsNotNull(nav?.Data);
        Assert.IsInstanceOfType<List<NavNode>>(nav.Data);
        var navNodeList = nav.Data as List<NavNode>;

        Assert.AreEqual(1, navNodeList.Count);
        Assert.AreEqual(NavNodeType.Portal, navNodeList[0].Type);
        Assert.IsInstanceOfType<NavNodePortal>(navNodeList[0]);
        var navNode = navNodeList[0] as NavNodePortal;

        Assert.AreEqual(expectedX, navNode.Data.x);
        Assert.AreEqual(expectedY, navNode.Data.y);
        Assert.AreEqual(expectedZ, navNode.Data.z);
        Assert.AreEqual(expectedOC, (int)navNode.Data.objectClass);
        Assert.AreEqual(expectedTargetName, navNode.Data.objectName);
    }

    [TestMethod]
    public async Task ReadNavAsync_HappyPath_ReadsNPCChatNode()
    {
        var expectedX = fixture.Create<double>();
        var expectedY = fixture.Create<double>();
        var expectedZ = fixture.Create<double>();
        var expectedOC = (int)fixture.Create<ObjectClass>();
        var expectedTargetName = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine("NAV: _ once")
            .AppendLine($"tlk 0.0 0.0 0.0 {expectedX} {expectedY} {expectedZ} {expectedOC} {{{expectedTargetName}}}");
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var nav = await navReader.ReadNavAsync(reader);

        Assert.IsNotNull(nav?.Data);
        Assert.IsInstanceOfType<List<NavNode>>(nav.Data);
        var navNodeList = nav.Data as List<NavNode>;

        Assert.AreEqual(1, navNodeList.Count);
        Assert.AreEqual(NavNodeType.NPCChat, navNodeList[0].Type);
        Assert.IsInstanceOfType<NavNodeNPCChat>(navNodeList[0]);
        var navNode = navNodeList[0] as NavNodeNPCChat;

        Assert.AreEqual(expectedX, navNode.Data.x);
        Assert.AreEqual(expectedY, navNode.Data.y);
        Assert.AreEqual(expectedZ, navNode.Data.z);
        Assert.AreEqual(expectedOC, (int)navNode.Data.objectClass);
        Assert.AreEqual(expectedTargetName, navNode.Data.objectName);
    }

    [TestMethod]
    public async Task ReadNavAsync_HappyPath_ReadsJumpNode()
    {
        var expectedHeading = fixture.Create<double>();
        var expectedShift = fixture.Create<bool>();
        var expectedDelay = fixture.Create<double>();
        var sb = new StringBuilder()
            .AppendLine("NAV: _ once")
            .AppendLine($"jmp 0.0 0.0 0.0 {expectedHeading} {{{(expectedShift ? bool.TrueString : bool.FalseString)}}} {expectedDelay}");
        using var reader = new StringReader(sb.ToString());

        var navReader = new MetafNavReader();

        var nav = await navReader.ReadNavAsync(reader);

        Assert.IsNotNull(nav?.Data);
        Assert.IsInstanceOfType<List<NavNode>>(nav.Data);
        var navNodeList = nav.Data as List<NavNode>;

        Assert.AreEqual(1, navNodeList.Count);
        Assert.AreEqual(NavNodeType.Jump, navNodeList[0].Type);
        Assert.IsInstanceOfType<NavNodeJump>(navNodeList[0]);
        var navNode = navNodeList[0] as NavNodeJump;

        Assert.AreEqual(expectedHeading, navNode.Data.heading);
        Assert.AreEqual(expectedShift, navNode.Data.shift);
        Assert.AreEqual(expectedDelay, navNode.Data.delay);
    }
}
