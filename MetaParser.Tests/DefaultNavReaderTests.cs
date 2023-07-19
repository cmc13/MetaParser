using AutoFixture;
using MetaParser.Formatting;
using MetaParser.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MetaParser.Tests;

[TestClass]
public class DefaultNavReaderTests
{
    private readonly Fixture fixture = new();

    [TestMethod]
    public async Task ReadNavFollowAsync_HappyPath_SuccessfullyReadsFollow()
    {
        var expectedTargetName = fixture.Create<string>();
        var expectedTargetId = fixture.Create<int>();
        var sb = new StringBuilder()
            .AppendLine(expectedTargetName)
            .AppendLine(expectedTargetId.ToString());
        using var reader = new StringReader(sb.ToString());

        var follow = new NavFollow();
        var navReader = new DefaultNavReader();

        await navReader.ReadNavFollowAsync(reader, follow);

        Assert.AreEqual(expectedTargetName, follow.TargetName);
        Assert.AreEqual(expectedTargetId, follow.TargetId);
    }

    [TestMethod]
    public async Task ReadNavFollowAsync_InvalidTargetId_ThrowsException()
    {
        var expectedTargetId = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<string>())
            .AppendLine(expectedTargetId.ToString());
        using var reader = new StringReader(sb.ToString());

        var follow = new NavFollow();
        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavFollowAsync(reader, follow));
        Assert.AreEqual($"Invalid int (Expected: <Int32>; Actual: '{expectedTargetId}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_HappyPath_ReadsRecallNode()
    {
        var expectedX = fixture.Create<double>();
        var expectedY = fixture.Create<double>();
        var expectedZ = fixture.Create<double>();
        var expectedSpellId = (int)fixture.Create<RecallSpellId>();
        var sb = new StringBuilder()
            .AppendLine(expectedX.ToString())
            .AppendLine(expectedY.ToString())
            .AppendLine(expectedZ.ToString())
            .AppendLine("0")
            .AppendLine(expectedSpellId.ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodeRecall node = new();

        var navReader = new DefaultNavReader();

        await navReader.ReadNavNodeAsync(reader, node);

        Assert.AreEqual(expectedX, node.Point.x);
        Assert.AreEqual(expectedY, node.Point.y);
        Assert.AreEqual(expectedZ, node.Point.z);
        Assert.AreEqual(expectedSpellId, (int)node.Data);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_InvalidXCoordinate_ThrowsException()
    {
        var expectedX = fixture.Create<string>();
        var expectedSpellId = (int)fixture.Create<RecallSpellId>();
        var sb = new StringBuilder()
            .AppendLine(expectedX.ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(expectedSpellId.ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodeRecall node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Invalid double (Expected: <Double>; Actual: '{expectedX}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_InvalidYCoordinate_ThrowsException()
    {
        var expectedY = fixture.Create<string>();
        var expectedSpellId = (int)fixture.Create<RecallSpellId>();
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(expectedY.ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(expectedSpellId.ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodeRecall node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Invalid double (Expected: <Double>; Actual: '{expectedY}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_InvalidZCoordinate_ThrowsException()
    {
        var expectedZ = fixture.Create<string>();
        var expectedSpellId = (int)fixture.Create<RecallSpellId>();
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(expectedZ.ToString())
            .AppendLine("0")
            .AppendLine(expectedSpellId.ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodeRecall node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Invalid double (Expected: <Double>; Actual: '{expectedZ}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_HappyPath_ReadsPortalObsNode()
    {
        var expectedX = fixture.Create<double>();
        var expectedY = fixture.Create<double>();
        var expectedZ = fixture.Create<double>();
        var expectedPortalId = fixture.Create<int>();
        var sb = new StringBuilder()
            .AppendLine(expectedX.ToString())
            .AppendLine(expectedY.ToString())
            .AppendLine(expectedZ.ToString())
            .AppendLine("0")
            .AppendLine(expectedPortalId.ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodePortalObs node = new();

        var navReader = new DefaultNavReader();

        await navReader.ReadNavNodeAsync(reader, node);

        Assert.AreEqual(expectedX, node.Point.x);
        Assert.AreEqual(expectedY, node.Point.y);
        Assert.AreEqual(expectedZ, node.Point.z);
        Assert.AreEqual(expectedPortalId, node.Data);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_PortalObsNodeWithInvalidPortalId_ThrowsException()
    {
        var expectedPortalId = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(expectedPortalId.ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodePortalObs node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Invalid int (Expected: <Int32>; Actual: '{expectedPortalId}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_HappyPath_ReadsPauseNode()
    {
        var expectedX = fixture.Create<double>();
        var expectedY = fixture.Create<double>();
        var expectedZ = fixture.Create<double>();
        var expectedPauseTime = fixture.Create<double>();
        var sb = new StringBuilder()
            .AppendLine(expectedX.ToString())
            .AppendLine(expectedY.ToString())
            .AppendLine(expectedZ.ToString())
            .AppendLine("0")
            .AppendLine(expectedPauseTime.ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodePause node = new();

        var navReader = new DefaultNavReader();

        await navReader.ReadNavNodeAsync(reader, node);

        Assert.AreEqual(expectedX, node.Point.x);
        Assert.AreEqual(expectedY, node.Point.y);
        Assert.AreEqual(expectedZ, node.Point.z);
        Assert.AreEqual(expectedPauseTime, node.Data);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_PauseNodeWithInvalidPauseTime_ThrowsException()
    {
        var expectedPauseTime = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(expectedPauseTime.ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodePause node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Invalid double (Expected: <Double>; Actual: '{expectedPauseTime}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_HappyPath_ReadsChatNode()
    {
        var expectedX = fixture.Create<double>();
        var expectedY = fixture.Create<double>();
        var expectedZ = fixture.Create<double>();
        var expectedChat = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine(expectedX.ToString())
            .AppendLine(expectedY.ToString())
            .AppendLine(expectedZ.ToString())
            .AppendLine("0")
            .AppendLine(expectedChat);
        using var reader = new StringReader(sb.ToString());

        NavNodeChat node = new();

        var navReader = new DefaultNavReader();

        await navReader.ReadNavNodeAsync(reader, node);

        Assert.AreEqual(expectedX, node.Point.x);
        Assert.AreEqual(expectedY, node.Point.y);
        Assert.AreEqual(expectedZ, node.Point.z);
        Assert.AreEqual(expectedChat, node.Data);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_HappyPath_ReadsOpenVendorNode()
    {
        var expectedX = fixture.Create<double>();
        var expectedY = fixture.Create<double>();
        var expectedZ = fixture.Create<double>();
        var expectedVendorId = fixture.Create<int>();
        var expectedVendorName = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine(expectedX.ToString())
            .AppendLine(expectedY.ToString())
            .AppendLine(expectedZ.ToString())
            .AppendLine("0")
            .AppendLine(expectedVendorId.ToString())
            .AppendLine(expectedVendorName);
        using var reader = new StringReader(sb.ToString());

        NavNodeOpenVendor node = new();

        var navReader = new DefaultNavReader();

        await navReader.ReadNavNodeAsync(reader, node);

        Assert.AreEqual(expectedX, node.Point.x);
        Assert.AreEqual(expectedY, node.Point.y);
        Assert.AreEqual(expectedZ, node.Point.z);
        Assert.AreEqual(expectedVendorId, node.Data.vendorId);
        Assert.AreEqual(expectedVendorName, node.Data.vendorName);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_OpenVendorNodeWithInvalidVendorId_ThrowsException()
    {
        var expectedVendorId = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(expectedVendorId.ToString())
            .AppendLine(fixture.Create<string>());
        using var reader = new StringReader(sb.ToString());

        NavNodeOpenVendor node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Invalid int (Expected: <Int32>; Actual: '{expectedVendorId}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_HappyPath_ReadsPortalNode()
    {
        var expectedX = fixture.Create<double>();
        var expectedY = fixture.Create<double>();
        var expectedZ = fixture.Create<double>();
        var expectedClass = (int)fixture.Create<ObjectClass>();
        var expectedObjectName = fixture.Create<string>();
        var expectedTX = fixture.Create<double>();
        var expectedTY = fixture.Create<double>();
        var expectedTZ = fixture.Create<double>();
        var sb = new StringBuilder()
            .AppendLine(expectedX.ToString())
            .AppendLine(expectedY.ToString())
            .AppendLine(expectedZ.ToString())
            .AppendLine("0")
            .AppendLine(expectedObjectName)
            .AppendLine(expectedClass.ToString())
            .AppendLine(bool.TrueString)
            .AppendLine(expectedTX.ToString())
            .AppendLine(expectedTY.ToString())
            .AppendLine(expectedTZ.ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodePortal node = new();

        var navReader = new DefaultNavReader();

        await navReader.ReadNavNodeAsync(reader, node);

        Assert.AreEqual(expectedX, node.Point.x);
        Assert.AreEqual(expectedY, node.Point.y);
        Assert.AreEqual(expectedZ, node.Point.z);
        Assert.AreEqual(expectedObjectName, node.Data.objectName);
        Assert.AreEqual(expectedClass, (int)node.Data.objectClass);
        Assert.AreEqual(expectedTX, node.Data.x);
        Assert.AreEqual(expectedTY, node.Data.y);
        Assert.AreEqual(expectedTZ, node.Data.z);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_PortalNodeWithInvalidObjectClass_ThrowsException()
    {
        var expectedClass = int.MaxValue;
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(fixture.Create<string>())
            .AppendLine(expectedClass.ToString())
            .AppendLine(bool.TrueString)
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodePortal node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Unknown ObjectClass: 2147483647", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_PortalNodeWithInvalidTargetX_ThrowsException()
    {
        var expectedClass = (int)fixture.Create<ObjectClass>();
        var expectedTX = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(fixture.Create<string>())
            .AppendLine(expectedClass.ToString())
            .AppendLine(bool.TrueString)
            .AppendLine(expectedTX.ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodePortal node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Invalid double (Expected: <Double>; Actual: '{expectedTX}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_PortalNodeWithInvalidTargetY_ThrowsException()
    {
        var expectedClass = (int)fixture.Create<ObjectClass>();
        var expectedTY = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(fixture.Create<string>())
            .AppendLine(expectedClass.ToString())
            .AppendLine(bool.TrueString)
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(expectedTY.ToString())
            .AppendLine(fixture.Create<double>().ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodePortal node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Invalid double (Expected: <Double>; Actual: '{expectedTY}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_PortalNodeWithInvalidTargetZ_ThrowsException()
    {
        var expectedClass = (int)fixture.Create<ObjectClass>();
        var expectedTZ = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(fixture.Create<string>())
            .AppendLine(expectedClass.ToString())
            .AppendLine(bool.TrueString)
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(expectedTZ.ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodePortal node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Invalid double (Expected: <Double>; Actual: '{expectedTZ}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_HappyPath_ReadsNPCChatNode()
    {
        var expectedX = fixture.Create<double>();
        var expectedY = fixture.Create<double>();
        var expectedZ = fixture.Create<double>();
        var expectedClass = (int)ObjectClass.NPC;
        var expectedObjectName = fixture.Create<string>();
        var expectedTX = fixture.Create<double>();
        var expectedTY = fixture.Create<double>();
        var expectedTZ = fixture.Create<double>();
        var sb = new StringBuilder()
            .AppendLine(expectedX.ToString())
            .AppendLine(expectedY.ToString())
            .AppendLine(expectedZ.ToString())
            .AppendLine("0")
            .AppendLine(expectedObjectName)
            .AppendLine(expectedClass.ToString())
            .AppendLine(bool.TrueString)
            .AppendLine(expectedTX.ToString())
            .AppendLine(expectedTY.ToString())
            .AppendLine(expectedTZ.ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodeNPCChat node = new();

        var navReader = new DefaultNavReader();

        await navReader.ReadNavNodeAsync(reader, node);

        Assert.AreEqual(expectedX, node.Point.x);
        Assert.AreEqual(expectedY, node.Point.y);
        Assert.AreEqual(expectedZ, node.Point.z);
        Assert.AreEqual(expectedObjectName, node.Data.objectName);
        Assert.AreEqual(expectedClass, (int)node.Data.objectClass);
        Assert.AreEqual(expectedTX, node.Data.x);
        Assert.AreEqual(expectedTY, node.Data.y);
        Assert.AreEqual(expectedTZ, node.Data.z);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_NPCChatNodeWithInvalidObjectClass_ThrowsException()
    {
        var expectedClass = int.MaxValue;
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(fixture.Create<string>())
            .AppendLine(expectedClass.ToString())
            .AppendLine(bool.TrueString)
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodeNPCChat node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Unknown ObjectClass: 2147483647", ex.Message);
    }

    [TestMethod]
    [DataRow(ObjectClass.Container)]
    [DataRow(ObjectClass.Portal)]
    public async Task ReadNavNodeAsync_NPCChatNodeWithObjectClassOtherThanNPC_ThrowsException(ObjectClass objectClass)
    {
        var expectedClass = (int)objectClass;
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(fixture.Create<string>())
            .AppendLine(expectedClass.ToString())
            .AppendLine(bool.TrueString)
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodeNPCChat node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Object class must be NPC type for npc chat node (Expected: '37'; Actual: '{(int)objectClass}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_NPCChatNodeWithInvalidTargetX_ThrowsException()
    {
        var expectedClass = (int)ObjectClass.NPC;
        var expectedTX = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(fixture.Create<string>())
            .AppendLine(expectedClass.ToString())
            .AppendLine(bool.TrueString)
            .AppendLine(expectedTX.ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodeNPCChat node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Invalid double (Expected: <Double>; Actual: '{expectedTX}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_NPCChatNodeWithInvalidTargetY_ThrowsException()
    {
        var expectedClass = (int)ObjectClass.NPC;
        var expectedTY = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(fixture.Create<string>())
            .AppendLine(expectedClass.ToString())
            .AppendLine(bool.TrueString)
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(expectedTY.ToString())
            .AppendLine(fixture.Create<double>().ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodeNPCChat node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Invalid double (Expected: <Double>; Actual: '{expectedTY}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_NPCChatNodeWithInvalidTargetZ_ThrowsException()
    {
        var expectedClass = (int)ObjectClass.NPC;
        var expectedTZ = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(fixture.Create<string>())
            .AppendLine(expectedClass.ToString())
            .AppendLine(bool.TrueString)
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(expectedTZ.ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodeNPCChat node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Invalid double (Expected: <Double>; Actual: '{expectedTZ}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_NPCChatNodeWithInvalidTrueString_ThrowsException()
    {
        var expectedClass = (int)ObjectClass.NPC;
        var invalidTrueString = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(fixture.Create<string>())
            .AppendLine(expectedClass.ToString())
            .AppendLine(invalidTrueString)
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodeNPCChat node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Invalid npc nav route node (Expected: 'True'; Actual: '{invalidTrueString}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_HappyPath_ReadsJumpNode()
    {
        var expectedX = fixture.Create<double>();
        var expectedY = fixture.Create<double>();
        var expectedZ = fixture.Create<double>();
        var expectedHeading = fixture.Create<double>();
        var expectedShift = fixture.Create<bool>();
        var expectedDelay = fixture.Create<double>();
        var sb = new StringBuilder()
            .AppendLine(expectedX.ToString())
            .AppendLine(expectedY.ToString())
            .AppendLine(expectedZ.ToString())
            .AppendLine("0")
            .AppendLine(expectedHeading.ToString())
            .AppendLine(expectedShift ? bool.TrueString : bool.FalseString)
            .AppendLine(expectedDelay.ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodeJump node = new();

        var navReader = new DefaultNavReader();

        await navReader.ReadNavNodeAsync(reader, node);

        Assert.AreEqual(expectedX, node.Point.x);
        Assert.AreEqual(expectedY, node.Point.y);
        Assert.AreEqual(expectedZ, node.Point.z);
        Assert.AreEqual(expectedHeading, node.Data.heading);
        Assert.AreEqual(expectedShift, node.Data.shift);
        Assert.AreEqual(expectedDelay, node.Data.delay);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_JumpNodeWithInvalidHeading_ReadsJumpNode()
    {
        var expectedHeading = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(expectedHeading.ToString())
            .AppendLine(fixture.Create<bool>() ? bool.TrueString : bool.FalseString)
            .AppendLine(fixture.Create<double>().ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodeJump node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Invalid double (Expected: <Double>; Actual: '{expectedHeading}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_JumpNodeWithInvalidShift_ReadsJumpNode()
    {
        var expectedShift = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(expectedShift)
            .AppendLine(fixture.Create<double>().ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodeJump node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Invalid jump nav route node (Expected: 'True|False'; Actual: '{expectedShift}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavNodeAsync_JumpNodeWithInvalidDelay_ReadsJumpNode()
    {
        var expectedDelay = fixture.Create<string>();
        var sb = new StringBuilder()
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine("0")
            .AppendLine(fixture.Create<double>().ToString())
            .AppendLine(fixture.Create<bool>() ? bool.TrueString : bool.FalseString)
            .AppendLine(expectedDelay.ToString());
        using var reader = new StringReader(sb.ToString());

        NavNodeJump node = new();

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavNodeAsync(reader, node));
        Assert.AreEqual($"Invalid double (Expected: <Double>; Actual: '{expectedDelay}')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavAsync_HappyPath_ReadsEmptyNav()
    {
        var sb = new StringBuilder()
            .AppendLine("uTank2 NAV 1.2")
            .AppendLine(((int)NavType.Circular).ToString())
            .AppendLine("0");
        using var reader = new StringReader(sb.ToString());

        var navReader = new DefaultNavReader();

        var nav = await navReader.ReadNavAsync(reader);

        Assert.IsNotNull(nav);
        Assert.IsInstanceOfType(nav.Data, typeof(List<NavNode>));
        Assert.AreEqual(0, ((List<NavNode>)nav.Data).Count);
    }

    [TestMethod]
    public async Task ReadNavAsync_IncorrectNavHeader_ThrowsException()
    {
        var sb = new StringBuilder()
            .AppendLine("uTank2 NAV 1.3")
            .AppendLine(((int)NavType.Circular).ToString())
            .AppendLine("0");
        using var reader = new StringReader(sb.ToString());

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavAsync(reader));
        Assert.AreEqual("Invalid nav route (Expected: 'uTank2 NAV 1.2'; Actual: 'uTank2 NAV 1.3')", ex.Message);
    }

    [TestMethod]
    public async Task ReadNavAsync_InvalidNavType_ThrowsException()
    {
        var sb = new StringBuilder()
            .AppendLine("uTank2 NAV 1.2")
            .AppendLine(int.MaxValue.ToString())
            .AppendLine("0");
        using var reader = new StringReader(sb.ToString());

        var navReader = new DefaultNavReader();

        var ex = await Assert.ThrowsExceptionAsync<MetaParserException>(() => navReader.ReadNavAsync(reader));
        Assert.AreEqual("Unknown NavType: 2147483647", ex.Message);
    }
}
