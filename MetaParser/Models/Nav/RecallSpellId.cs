using System.ComponentModel;

namespace MetaParser.Models;

[TypeConverter(typeof(EnumDescriptionConverter))]
public enum  RecallSpellId
{
    [Description("Primary Portal Recall")] PrimaryPortalRecall = 48,
    [Description("Secondary Portal Recall")] SecondaryPortalRecall = 2647,
    [Description("Lifestone Recall")]LifestoneRecall = 1635,
    [Description("Lifestone Sending")] LifestoneSending = 1636,
    [Description("Portal Recall")] PortalRecall = 2645,
    [Description("Aphus Lassel")] RecallAphusLassel = 2931,
    [Description("Sanctuary")] RecalltheSanctuary = 2023,
    [Description("Singularity Caul")] RecalltotheSingularityCaul = 2943,
    [Description("Glenden Wood")] GlendenWoodRecall = 3865,
    [Description("Aerlinthe")] AerlintheRecall = 2041,
    [Description("Mt. Lethe")] MountLetheRecall = 2813,
    [Description("Ulgrim's")] UlgrimsRecall = 2941,
    [Description("Bur")] BurRecall = 4084,
    [Description("P-T Olthoi Infested Area")] ParadoxTouchedOlthoiInfestedAreaRecall = 4198,
    [Description("Graveyard")] CalloftheMhoireForge = 4128,
    [Description("Colosseum")] ColosseumRecall = 4213,
    [Description("Facility Hub")] FacilityHubRecall = 5175,
    [Description("Gear Knight Camp")] GearKnightInvasionAreaCampRecall = 5330,
    [Description("Neftet")] LostCityofNeftetRecall = 5541,
    [Description("Candeth Keep")] ReturntotheKeep = 4214,
    [Description("Rynthid")] RynthidRecall = 6150,
    [Description("Viridian Rise")] ViridianRiseRecall = 6321,
    [Description("Viridian Rise Great Tree")] ViridianRiseGreatTreeRecall = 6322,
    [Description("Celestial Hand Stronghold")] CelestialHandStrongholdRecall = 6325,
    [Description("Radiant Blood Stronghold")] RadiantBloodStrongholdRecall = 6327,
    [Description("Eldrytch Web Stronghold")] EldrytchWebStrongholdRecall = 6326
}

public static class RecallSpellIdExtensions
{
    public static string GetDescription(this RecallSpellId spellId)
    {
        var fi = typeof(RecallSpellId).GetField(spellId.ToString());
        if (fi != null)
        {
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return ((attributes.Length > 0) && (!string.IsNullOrEmpty(attributes[0].Description))) ? attributes[0].Description : spellId.ToString();
        }

        return spellId.ToString();
    }
}
