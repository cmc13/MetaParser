using MetaParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MetaParser.Formatting;

using static MetafRegex;

public sealed partial class MetafNavReader : INavReader
{
    private static readonly Dictionary<string, Regex> NavNodeRegex = new(8)
    {
        { "prt", PortalObsRegex() },
        { "rcl", RecallRegex() },
        { "pau", PauseRegex() },
        { "cht", ChatRegex() },
        { "vnd", VendorRegex() },
        { "ptl", PortalRegex() },
        { "tlk", PortalRegex() },
        { "jmp", JumpRegex() }
    };

    private static readonly Dictionary<string, RecallSpellId> RecallSpellList = new(26)
    {
        { "Primary Portal Recall", RecallSpellId.PrimaryPortalRecall },
        { "Secondary Portal Recall", RecallSpellId.SecondaryPortalRecall },
        { "Lifestone Recall", RecallSpellId.LifestoneRecall },
        { "Lifestone Sending", RecallSpellId.LifestoneSending },
        { "Portal Recall", RecallSpellId.PortalRecall },
        { "Recall Aphus Lassel", RecallSpellId.RecallAphusLassel },
        { "Recall the Sanctuary", RecallSpellId.RecalltheSanctuary },
        { "Recall to the Singularity Caul", RecallSpellId.RecalltotheSingularityCaul },
        { "Glenden Wood Recall", RecallSpellId.GlendenWoodRecall },
        { "Aerlinthe Recall", RecallSpellId.AerlintheRecall },
        { "Mount Lethe Recall", RecallSpellId.MountLetheRecall },
        { "Ulgrim's Recall", RecallSpellId.UlgrimsRecall },
        { "Bur Recall", RecallSpellId.BurRecall },
        { "Paradox-touched Olthoi Infested Area Recall", RecallSpellId.ParadoxTouchedOlthoiInfestedAreaRecall },
        { "Call of the Mhoire Forge", RecallSpellId.CalloftheMhoireForge },
        { "Colosseum Recall", RecallSpellId.ColosseumRecall },
        { "Facility Hub Recall", RecallSpellId.FacilityHubRecall },
        { "Gear Knight Invasion Area Camp Recall", RecallSpellId.GearKnightInvasionAreaCampRecall },
        { "Lost City of Neftet Recall", RecallSpellId.LostCityofNeftetRecall },
        { "Return to the Keep", RecallSpellId.ReturntotheKeep },
        { "Rynthid Recall", RecallSpellId.RynthidRecall },
        { "Viridian Rise Recall", RecallSpellId.ViridianRiseRecall },
        { "Viridian Rise Great Tree Recall", RecallSpellId.ViridianRiseGreatTreeRecall },
        { "Celestial Hand Stronghold Recall", RecallSpellId.CelestialHandStrongholdRecall },
        { "Radiant Blood Stronghold Recall", RecallSpellId.RadiantBloodStrongholdRecall },
        { "Eldrytch Web Stronghold Recall", RecallSpellId.EldrytchWebStrongholdRecall }
    };

    public async Task<NavRoute> ReadNavAsync(TextReader reader)
    {
        string line;

        do { line = await reader.ReadLineAsync().ConfigureAwait(false); } while (line != null && EmptyLineRegex().IsMatch(line));

        var m = NavLineRegex().Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid nav declaration", "NAV: ...", line);

        (_, var route) = await ReadNavAsync(line[(m.Index + m.Length)..], reader, null);
        return route;
    }

    public async Task<(string, NavRoute)> ReadNavAsync(string line, TextReader reader, IDictionary<string, NavRoute> navReferences = null)
    {
        if (line == null)
        {
            do { line = await reader.ReadLineAsync().ConfigureAwait(false); } while (line != null && EmptyLineRegex().IsMatch(line));
        }

        if (line == null)
            return (line, null);

        var m = NavRegex().Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid nav declaration", "<nav reference> [circular|once|follow|linear]", line);

        if (navReferences == null || !navReferences.TryGetValue(m.Groups["navRef"].Value, out var route))
        {
            route = new();

            navReferences?.Add(m.Groups["navRef"].Value, route);
        }

        if (!Enum.TryParse<NavType>(m.Groups["navType"].Value, true, out var type))
            throw new MetaParserException("Invalid nav type", "follow|once|circular|linear", m.Groups["navType"].Value);

        route.Type = type;

        if (route.Type == NavType.Follow)
        {
            var follow = new NavFollow();
            await ReadNavFollowAsync(reader, follow);
            route.Data = follow;

            do { line = await reader.ReadLineAsync().ConfigureAwait(false); } while (line != null && EmptyLineRegex().IsMatch(line));
        }
        else
        {
            var list = new List<NavNode>();
            route.Data = list;

            do
            {
                do { line = await reader.ReadLineAsync().ConfigureAwait(false); } while (line != null && EmptyLineRegex().IsMatch(line));

                if (line != null && PointDefRegex().IsMatch(line))
                    list.Add(ParseNavNode(line));
            }
            while (line != null && PointDefRegex().IsMatch(line));
        }

        return (line, route);
    }

    public async Task ReadNavNodeAsync(TextReader reader, NavNode node)
    {
        string line;

        do { line = await reader.ReadLineAsync().ConfigureAwait(false); } while (line != null && EmptyLineRegex().IsMatch(line));

        if (line == null)
            return;

        var n = ParseNavNode(line);
    }

    private NavNode ParseNavNode(string line)
    {
        var m = NavTypeRegex().Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid nav node definition", "pnt|prt|rcl|pau|cht|vnd|ptl|tlk|chk|jmp ...", line);

        line = line.Substring(m.Index + m.Length);
        (double x, double y, double z) pt = ParsePoint(ref line), pt2;
        switch (m.Groups["nodeType"].Value)
        {
            case "pnt":
                return new NavNodePoint() { Point = pt };

            case "prt":
                m = NavNodeRegex[m.Groups["nodeType"].Value].Match(line);
                if (!m.Success)
                    throw new MetaParserException("Invalid nav portal definition", "<hex number>", line);
                return new NavNodePortalObs()
                {
                    Point = pt,
                    Data = int.TryParse(m.Groups["id"].Value, System.Globalization.NumberStyles.HexNumber, null, out var id) ? id : throw new MetaParserException("Invalid nav portal definition")
                };

            case "rcl":
                m = NavNodeRegex[m.Groups["nodeType"].Value].Match(line);
                if (!m.Success)
                    throw new MetaParserException("Invalid nav recall definition", "{<recall spell name>}", line);
                return new NavNodeRecall()
                {
                    Point = pt,
                    Data = RecallSpellList.ContainsKey(m.Groups["spell"].Value.UnescapeString()) ? RecallSpellList[m.Groups["spell"].Value] : throw new MetaParserException($"Invalid spell name: {m.Groups["spell"].Value}")
                };

            case "pau":
                m = NavNodeRegex[m.Groups["nodeType"].Value].Match(line);
                if (!m.Success)
                    throw new MetaParserException("Invalid nav pause definition", "<double>", line);
                return new NavNodePause()
                {
                    Point = pt,
                    Data = double.TryParse(m.Groups["time"].Value, out var pause) ? pause : throw new MetaParserException("Invalid nav pause definition")
                };

            case "cht":
                m = NavNodeRegex[m.Groups["nodeType"].Value].Match(line);
                if (!m.Success)
                    throw new MetaParserException("Invalid nav chat definition", "{<chat>}", line);
                return new NavNodeChat() { Point = pt, Data = m.Groups["chat"].Value.UnescapeString() };

            case "vnd":
                m = NavNodeRegex[m.Groups["nodeType"].Value].Match(line);
                if (!m.Success)
                    throw new MetaParserException("Invalid nav vendor definition", "<hex number> {<target name>}", line);
                return new NavNodeOpenVendor()
                {
                    Point = pt,
                    Data = (
                        int.TryParse(m.Groups["id"].Value, System.Globalization.NumberStyles.HexNumber, null, out var vid) ? vid : throw new MetaParserException("Invalid nav vendor definition"),
                        m.Groups["name"].Value.UnescapeString())
                };

            case "ptl":
                pt2 = ParsePoint(ref line);
                m = NavNodeRegex[m.Groups["nodeType"].Value].Match(line);
                if (!m.Success)
                    throw new MetaParserException("Invalid nav portal definition", "<object class> {<target name>}", line);
                return new NavNodePortal()
                {
                    Point = pt,
                    Data = (
                        m.Groups["name"].Value.UnescapeString(),
                        (ObjectClass)(int.TryParse(m.Groups["oc"].Value, out var oc) ? oc : throw new MetaParserException("Invalid nav portal definition")),
                        pt2.x,
                        pt2.y,
                        pt2.z)
                };

            case "tlk":
                pt2 = ParsePoint(ref line);
                m = NavNodeRegex[m.Groups["nodeType"].Value].Match(line);
                if (!m.Success)
                    throw new MetaParserException("Invalid nav npc chat definition", "<object class> {<target name>}", line);
                return new NavNodeNPCChat()
                {
                    Point = pt,
                    Data = (
                        m.Groups["name"].Value.UnescapeString(),
                        (ObjectClass)(int.TryParse(m.Groups["oc"].Value, out var oc2) ? oc2 : throw new MetaParserException("Invalid nav npc chat definition")),
                        pt2.x,
                        pt2.y,
                        pt2.z)
                };

            case "chk":
                return new NavNodeCheckpoint() { Point = pt };

            case "jmp":
                m = NavNodeRegex[m.Groups["nodeType"].Value].Match(line);
                if (!m.Success)
                    throw new MetaParserException("Invalid nav jump definition", "<double> <True|False> <double>", line);
                return new NavNodeJump()
                {
                    Point = pt,
                    Data = (
                        double.TryParse(m.Groups["heading"].Value, out var heading) ? heading : throw new MetaParserException("Invalid nav jump definition"),
                        m.Groups["tf"].Value != "False",
                        double.TryParse(m.Groups["time"].Value, out var time) ? time : throw new MetaParserException("Invalid nav jump definition"))
                };

            default:
                throw new MetaParserException($"Invalid nav node type: {m.Groups["nodeType"].Value}");
        }
    }

    private static (double x, double y, double z) ParsePoint(ref string line)
    {
        var m = PointRegex().Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid nav point definition", "<x> <y> <z> ...", line);

        line = line.Substring(m.Index + m.Length);

        return (
            double.Parse(m.Groups["x"].Value),
            double.Parse(m.Groups["y"].Value),
            double.Parse(m.Groups["z"].Value)
        );
    }

    public async Task ReadNavFollowAsync(TextReader reader, NavFollow follow)
    {
        string line;

        do { line = await reader.ReadLineAsync().ConfigureAwait(false); } while (line != null && EmptyLineRegex().IsMatch(line));

        if (line == null)
            return;

        var m = FollowRegex().Match(line);
        if (!m.Success)
            throw new MetaParserException("Invalid nav follow definition", "flw <hex number> {<target name>}", line);

        follow.TargetId = int.TryParse(m.Groups["id"].Value, System.Globalization.NumberStyles.HexNumber, null, out var id) ? id : throw new MetaParserException("Invalid nav follow definition");
        follow.TargetName = m.Groups["name"].Value.UnescapeString();
    }
}
