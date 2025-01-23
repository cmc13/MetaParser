using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MetaParser.Models;

public partial class BoolOption : ValidationAttribute
{
    [GeneratedRegex(@"^(true|false|\d+)$")]
    private static partial Regex BoolOptionRegex();

    [GeneratedRegex(@"^getvar\[[a-zA-Z_][a-zA-Z0-9_]*\]$")]
    private static partial Regex GetVarRegex();

    public override bool IsValid(object value)
    {
        if (value is string str)
        {
            if (GetVarRegex().IsMatch(str))
                return true;
            if (bool.TryParse(str, out _) || BoolOptionRegex().IsMatch(str))
                return true;
        }

        return false;
    }
}

public partial class IntOption : ValidationAttribute
{
    [GeneratedRegex(@"^\d+$")]
    private static partial Regex IntOptionRegex();

    [GeneratedRegex(@"^getvar\[[a-zA-Z_][a-zA-Z0-9_]*\]$")]
    private static partial Regex GetVarRegex();

    public override bool IsValid(object value)
    {
        if (value is string str)
        {
            if (GetVarRegex().IsMatch(str))
                return true;
            if (IntOptionRegex().IsMatch(str) && int.TryParse(str, out _))
                return true;
        }

        return false;
    }
}

public partial class DoubleOption : ValidationAttribute
{
    [GeneratedRegex(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")]
    private static partial Regex DoubleOptionRegex();

    [GeneratedRegex(@"^getvar\[[a-zA-Z_][a-zA-Z0-9_]*\]$")]
    private static partial Regex GetVarRegex();

    public override bool IsValid(object value)
    {
        if (value is string str)
        {
            if (GetVarRegex().IsMatch(str))
                return true;
            if (DoubleOptionRegex().IsMatch(str) && double.TryParse(str, out _))
                return true;
        }

        return false;
    }
}

public partial class BuffProfileOption : ValidationAttribute
{
    [GeneratedRegex(@"^[BPSAFLC]*$")]
    private static partial Regex BuffProfileOptionRegex();

    [GeneratedRegex(@"^getvar\[[a-zA-Z_][a-zA-Z0-9_]*\]$")]
    private static partial Regex GetVarRegex();

    public override bool IsValid(object value)
    {
        if (value is string str)
        {
            if (GetVarRegex().IsMatch(str))
                return true;
            if (BuffProfileOptionRegex().IsMatch(str))
            {
                // Check if any characters appear more than once
                return str.GroupBy(c => c).Select(g => new { ch = g.Key, count = g.Count() }).All(a => a.count < 2);
            }
        }
        return false;
    }
}

[TypeConverter(typeof(EnumDisplayConverter))]
public enum VTankOptions
{
    [BoolOption][Display(Description = @"Moves items from the main pack to side packs [true/false]")] AutoCram,
    [BoolOption][Display(Description = @"Stacks items towards the bottom-most item in the pack, and into side packs. This means that the partial stack of an item will always be the top-most one in your inventory, which is the order that the game burns components. This prevents having to stack every spell cast, and prevents stacking from pulling items from side packs. [true/false]")] AutoStack,
    [BoolOption][Display(Description = @"Casts random buffs on nearby players [true/false]")] RandomHelperBuffs,
    [DoubleOption][Display(Description = @"Sets the timer for RandomHelperBuffs in seconds. [##]")] RandomHelperIntervalSeconds,
    [BoolOption][Display(Description = @"Loots, navigates, and salvages in peace mode. [true/false]")] IdlePeaceMode,
    [BoolOption][Display(Description = @"Causes the macro to become disabled when the character dies [true/false]")] StopMacroOnDeath,
    [BoolOption][Display(Description = @"Uses mana charge types specified in the Consumables panel to refill worn items [true/false]")] RefillWornMana,
    [IntOption][Display(Name = "RefillWornMana-Item-ManaPercent", Description = @"Sets the minimum percentage of mana left on an item at which to refill [##]")] RefillWornMana_Item_ManaPercent,
    [IntOption][Display(Description = @"The maximum number of usable mana stones the macro will try to keep in your inventory when looting [##]")] ManaStoneLootCount,
    [IntOption][Display(Description = @"The minimum mana required for an item to be used to fill mana stones [##]")] ManaTankMinimumMana,
    [BoolOption][Display(Description = @"When true, mana charges and stones are used to keep equipped mana filled even when the macro is disabled [true/false]")] ManaChargesWhenOff,
    [BoolOption][Display(Description = @"When true, automatically recruits players on request, and manages waiting lists and votes when acting as the fellowship leader [true/false]")] AutoFellowManagement,
    [BoolOption][Display(Description = @"Attempts to detect when monsters appear to be there but really do not exist, and signals the client to delete them [true/false]")] DeleteGhostMonsters,
    [IntOption][Display(Description = @"The number of times the macro must attempt to cast a spell on a monster, without the spell starting, before the monster is marked as ghost [##]")] GhostMonsterSpellAttemptCount,
    [BoolOption][Display(Description = @"No Function [true/false]")] WhoYouGonnaCall,
    [IntOption][Display(Description = @"The number of successful attacks that must miss a target before it is blacklisted [##]")] BlacklistMonsterAttemptCount,
    [IntOption][Display(Description = @"The amount of time a monster is blacklisted for being unhittable [##]")] BlacklistMonsterTimeoutSeconds,
    [BoolOption][Display(Description = @"An alternate method to force the deletion of ghost monsters which works for melee, missile and mage attacks. [true/false]")] DeleteGhostMonstersByHPTracker,
    [IntOption][Display(Description = @"The number of seconds that must pass with no received health updates for a target before it is considered a ghost [##]")] GhostDeleteHPTrackerSeconds,
    [BoolOption][Display(Description = @"Causes the macro to drop to peace mode before using a healing kit [true/false]")] GoToPeaceModeToUseKits,
    [BoolOption][Display(Description = @"Enables meta [true/false]")] EnableMeta,
    [IntOption][Display(Description = @"When trying to switch weapons, the macro will try repeatedly to drop to peace mode. If it fails this many times, assume the client is bugged and try to use a wand. (Default 34) [##]")] DropToPeaceModeRetryCount,
    [IntOption][Display(Description = @"The number of times the macro must attempt to open a corpse before it is blacklisted. [##]")] BlacklistCorpseOpenAttemptCount,
    [IntOption][Display(Description = @"The number of seconds an unopenable corpse is blacklisted for [##]")] BlacklistCorpseOpenTimeoutSeconds,

    [IntOption][Display(Name = "Recharge-Norm-HitP", Description = @"Sets the percentage at which the macro will Heal during combat [##]")] Recharge_Norm_HitP,
    [IntOption][Display(Name = "Recharge-Norm-Stam", Description = @"Sets the percentage at which the macro will Re-stam during combat [##]")] Recharge_Norm_Stam,
    [IntOption][Display(Name = "Recharge-Norm-Mana", Description = @"Sets the percentage at which the macro will Re-mana during combat [##]")] Recharge_Norm_Mana,
    [IntOption][Display(Name = "Recharge-NoTarg-HitP", Description = @"Sets the percentage to which the macro will Heal when Idle [##]")] Recharge_NoTarg_HitP,
    [IntOption][Display(Name = "Recharge-NoTarg-Stam", Description = @"Sets the percentage to which the macro will Re-stam when Idle [##]")] Recharge_NoTarg_Stam,
    [IntOption][Display(Name = "Recharge-NoTarg-Mana", Description = @"Sets the percentage to which the macro will Re-mana when Idle [##]")] Recharge_NoTarg_Mana,
    [IntOption][Display(Name = "Recharge-Helper-HitP", Description = @"Sets the percentage at which the macro will Heal a fellow [##]")] Recharge_Helper_HitP,
    [IntOption][Display(Name = "Recharge-Helper-Stam", Description = @"Sets the percentage at which the macro will give stamina to a fellow [##]")] Recharge_Helper_Stam,
    [IntOption][Display(Name = "Recharge-Helper-Mana", Description = @"Sets the percentage at which the macro will give mana to a fellow [##]")] Recharge_Helper_Mana,
    [BoolOption][Display(Description = @"If true, allies are healed/restamed/given mana. The fellowship window must be open to help fellows (or be in a Virindi Integrator 2 fellowship) [true/false]")] DoHelp,
    [DoubleOption][Display(Description = @"Sets the max distance at which the macro will Heal fellows [##.###]")] HelperDistanceHitP,
    [DoubleOption][Display(Description = @"Sets the max distance at which the macro will re-stam fellows [##.###]")] HelperDistanceStam,
    [DoubleOption][Display(Description = @"Sets the max distance at which the macro will give mana to fellows [##.###]")] HelperDistanceMana,
    [BoolOption][Display(Description = @"Casts dispel life magic self VII when the macro has a vuln on it [true/false]")] CastDispelSelf,
    [BoolOption][Display(Description = @"Uses either a Gem of Stillness or a Condensed Dispel Potion when the macro has a vuln on it [true/false]")] UseDispelItems,
    [BoolOption][Display(Description = @"[true/false]")] UseDispelDrum,
    [BoolOption][Display(Description = @"When set to true and you have a healer's heart in your item list, the macro uses it for Helper [true/false]")] UseHealersHeart,
    [BoolOption][Display(Description = @"For healer's heart, when enabled, the macro jumps right after using the heart. This causes the heal to go out immediately and your character to have to stand there doing nothing until the timer is up. [true/false]")] JumpOutWandCasting,
    [DoubleOption][Display(Description = @"Maximum time for the recharge boost effect to last. Boost causes the macro to prefer to recharge once it switches to magic mode to recharge [##.###]")] RechargeBoostTimeSeconds,
    [IntOption][Display(Description = @"Number of h/s/m points used for recharge boost. Boost causes the macro to prefer to recharge once it switches to magic mode to recharge [##]")] RechargeBoostAmount,
    [IntOption][Display(Description = @"The lowest chance of success, in percent, that the macro will accept when attempting a heal with a health kit [##]")] MinimumHealKitSuccessChance,
    [BoolOption][Display(Description = @"When true, the macro uses available healing kits to heal while in magic mode (with a higher priority than spells) [true/false]")] UseKitsInMagicMode,
    [DoubleOption][Display(Description = @"For a Stamina to Health spell to be used, it must be this much better than a normal healing spell [##.###]")] StaminaToHealthMultiplier,
    [DoubleOption][Display(Description = @"For a Mana to Health spell to be used, it must be thism uch better than a normal healing spell [##.###]")] ManaToHealthMultiplier,
    [BoolOption][Display(Description = @"Causes the macro to drop to peace mode before using a healing kit [true/false]")] ClearLevelBoostFlagOnCast,
    [IntOption][Display(Description = @"The number of health kits of each type to craft up to when idle [##]")] IdleCraftCount_HealthKits,
    [IntOption][Display(Description = @"The number of stamina kits of each type to craft up to when idle [##]")] IdleCraftCount_StamKits,
    [IntOption][Display(Description = @"The number of mana kits of each type to craft up to when idle [##]")] IdleCraftCount_ManaKits,
    [IntOption][Display(Description = @"The number of health foods/potions of each type to craft up to when idle [##]")] IdleCraftCount_HealthFood,
    [IntOption][Display(Description = @"The number of stamina foods/potions of each type to craft up to when idle [##]")] IdleCraftCount_StamFood,
    [IntOption][Display(Description = @"The number of mana foods/potions of each type to craft up to when idle [##]")] IdleCraftCount_ManaFood,

    [BoolOption][Display(Description = @"Enable/Disable Combat [true/false]")] EnableCombat,
    [RegularExpression(@"^1|2|3$")][Display(Description = @"Sets the default melee attack height. 1 = high, 2 = mid, 3 = low [1/2/3]")] DefaultMeleeAttackHeight,
    [BoolOption][Display(Description = @"Causes the macro to attack the selected monster first, as long as it is a valid target. Priority will not take over until target is dead [true/false]")] TargetLock,
    [RegularExpression(@"^1|2|3$")][Display(Description = @"Selects how targets are given preference. When set to ""both"", nearby targets are chosen by angle and distant targets are in order of distance . 1 = By Range, 2 = By Angle, 3 = Both [1/2/3]")] TargetSelectMethod,
    [DoubleOption][Display(Description = @"Above this range, when target select is ""both"", targets are selected by range [##.###]")] TargetSelectAngleRange,
    [RegularExpression(@"^1|2|3$")][Display(Description = @"Casts debuffs for each valid target before beginning attack. 1 = One: debuffs one monster then attacks it. 2 = Priority: debuffs all monsters of the same priority then attacks. 3 = All: debuffs all monsters before attacking regardless of priority [1/2/3]")] DebuffEachFirst,
    [BoolOption][Display(Description = @"When true, the macro sets melee attack power automatically based on the weapon damage type [true/false]")] AutoAttackPower,
    [RegularExpression(@"^1|2$")][Display(Description = @"Selects what the macro prefers when looking for a way to cast debuffs. Select ""Skill"" when fighting monsters with high magic defense. 1 = Spell Level, 2 = Skill [1/2]")] DebuffSelectionMethod,
    [BoolOption][Display(Description = @"When true, attacks using recklessness if available. [true/false]")] UseRecklessness,

    [IntOption][Display(Name = "SpellDiffExcessThreshold-Hunt", Description = @"A positive number raises the skill necessary to cast spells, a negative number lowers it. To attempt higher spells at a low level use a negative number. [##]")] SpellDiffExcessThreshold_Hunt,
    [DoubleOption][Display(Description = @"The minimum distance a target must be for the macro to use ring spells [##.###]")] RingDistance,
    [IntOption][Display(Description = @"The minimum amount of targets necesary within RingDistance for the macro to cast ring spells [##]")] MinimumRingTargets,
    [BoolOption][Display(Description = @"If enabled, themacro forces a wand switch to the appropriately selected wand for the monster, even to debuff it. Without this setting, wands are only switched when a war attack is required [true/false]")] SwitchWandToDebuff,
    [BoolOption][Display(Description = @"Secret spell dodge mode [true/false]")] DoJiggle,
    [BoolOption][Display(Description = @"Arc war spells are used rather than bolts, if both are known for the best level the macro is able to cast [true/false]")] UseArcs,
    [DoubleOption][Display(Description = @"Sets the minimum range to cast arc spells [##.###]")] ArcRange,
    [IntOption][Display(Description = @"The number of seconds before a target's debuff is set to expire before the macro will want to recast it. Does not apply to DoT spells. [##]")] DebuffPrecastSeconds,
    [BoolOption][Display(Description = @"When enabled, the macro summons pets if available and listed in items. [true/false]")] SummonPets,
    [RegularExpression(@"^0|1$")][Display(Description = @"Determines what monster range is used when summoning a combat pet. 0 = AttackDistance, 1 = PetCustomRange [0/1]")] PetRangeMode,
    [DoubleOption][Display(Description = @"Custom settings for PetRange. [##.###]")] PetCustomRange,
    [IntOption][Display(Name = "PetRefillCount-Idle", Description = @"If a pet summon item has this many charges or fewer, it will be refilled when idle. [##]")] PetRefillCount_Idle,
    [IntOption][Display(Name = "PetRefillCount-Normal", Description = @"If a pet summon item has this many charges or fewer, it will be refilled in combat. [##]")] PetRefillCount_Normal,
    [IntOption][Display(Description = @"The minimum number of monsters (configured for summon) that must be in pet range before a pet is summoned. [##]")] PetMonsterDensity,

    [DoubleOption][Display(Description = @"Sets the maximum attack distance. Controlled by Monster Range on the Virindi Tank Standard Options [##.###]")] AttackDistance,
    [DoubleOption][Display(Description = @"Sets the minimum attack distance. Default is 0. [##.###]")] AttackMinimumDistance,
    [DoubleOption][Display(Description = @"Sets the maximum approach distance [##.###]")] ApproachDistance,
    [DoubleOption][Display(Name = "CorpseApproachRange-Max", Description = @"The maximum approach distance for corpse looting [##.###]")] CorpseApproachRange_Max,
    [DoubleOption][Display(Name = "CorpseApproachRange-Min", Description = @"The minimum approach distance for corpse looting [##.###]")] CorpseApproachRange_Min,
    [DoubleOption][Display(Description = @"Follow/Nav Min [##.###]")] NavCloseStopRange,
    [DoubleOption][Display(Description = @"Causes navigation to be disabled if the macro runs too far off course [##.###]")] NavFarStopRange,
    [DoubleOption][Display(Description = @"[##.###]")] UsePortalDistance,
    [DoubleOption][Display(Description = @"Minimum distance the macro will ID a door [##.###]")] DoorIDRange,
    [DoubleOption][Display(Description = @"The distance at which the macro attempts to open doors [##.###]")] DoorOpenRange,

    [BoolOption][Display(Description = @"Enable/Disable Navigation [true/false]")] EnableNav,
    [DoubleOption][Display(Description = @"The time in minutes after which a corpse will be removed from the corpse cache [##.###]")] CorpseCacheTimeoutMinutes,
    [BoolOption][Display(Description = @"When True, will cause the macro to open doors within range [true/false]")] OpenDoors,
    [IntOption][Display(Description = @"The number of lockpick skill points above a door's pick difficuty that is required for a door to be considered pickable [##]")] DoorLockpickDiffExcessThreshold,
    [BoolOption][Display(Description = @"Causes the macro to navigate before attacking [true/false]")] NavPriorityBoost,
    [BoolOption][Display(Description = @"When following a character, if True the follower runs the exact route as leader, if False the follower runs directly at the leader. [true/false]")] FollowAroundCorners,

    [BoolOption][Display(Description = @"Turns on Buffing [true/false]")] EnableBuffing,
    [IntOption][Display(Name = "SpellDiffExcessThreshold-Buff", Description = @"A positive number raises the skill necessary to cast spells, a negative number lowers it. To attempt higher spells at a low level use a negative number. [##]")] SpellDiffExcessThreshold_Buff,
    [BoolOption][Display(Description = @"When the macro is idle, it recasts buffs [true/false]")] IdleBuffTopoff,
    [IntOption][Display(Description = @"Sets the time left on buffs to begin an IdleBuffTopOff [##]")] IdleBuffTopoffTimeSeconds,
    [IntOption][Display(Description = @"Sets the time left on buffs to rebuff normally")] RebuffTimeRemainingSeconds,
    [BuffProfileOption][Display(Name = "BuffProfile-Prots", Description = @"Specifies which self prots are cast. Note: BuffProfile_Prots must be set to 'Custom' in order to use custom element filter. [BPSALFC]")] BuffProfile_Prots,
    [BuffProfileOption][Display(Name = "BuffProfile-Banes", Description = @"Specifies which self banes are cast. Note: BuffProfile_Banes must be set to 'Custom' in order to use custom element filter. [BPSALFC]")] BuffProfile_Banes,
    [IntOption][Display(Description = @"After casting a single buff, the time until rebuffing every other buff is reduced by BuffCastRecast_Seconds for BuffCastReset_Seconds [##]")] BuffCastRecast_Seconds,
    [IntOption][Display(Description = @"After casting a single buff, the time until rebuffing every other buff is reduced by BuffCastRecast_Seconds for BuffCastReset_Seconds [##]")] BuffCastRecastReset_Seconds,

    [IntOption][Display(Description = @"[##]")] ArrowheadFletchDiffExcessThreshold,
    [BoolOption][Display(Description = @"When enabled, the macro crafts known components that result in items in the Consumables panel, if your character has none. For instance, this allows the macro to create blue kits out of bobo comps if ""Plentiful Healing Kit"" is listed in the Consumables panel [true/false]")] AutoCraftItems,
    [BoolOption][Display(Description = @"Specifies if the macro should automatically split peas. Components you would like to keep supplied by splitting peas should be add to your Consumables panel. [true/false]")] SplitPeas,
    [IntOption][Display(Name = "SpellCompMin-Critical", Description = @"The minimum number of each type of spell component. If you have fewer, a pea will be split. ""Critical"" splitting is higher priority than healing [##]")] SpellCompMin_Critical,
    [IntOption][Display(Name = "SpellCompMin-Normal", Description = @"The minimum number of each type of spell component. If you have fewer, a pea will be split. ""Normal"" splitting has a lower priority than normal buffing [##]")] SpellCompMin_Normal,
    [IntOption][Display(Name = "SpellCompMin-Idle", Description = @"The minimum number of each type of spell component. If you have fewer, a pea will be split. ""Idle"" spltting is only done when idle [##]")] SpellCompMin_Idle,
    [RegularExpression(@"^0|1|2|3$")][Display(Description = @"Specifies whether the macro should craft and use special ammunition types. ""Raider"" indicates Raider Lightning ammo, and ""SCurrency"" indicates ammo from Colosseum, Graveyard, and Paradox-touched Olthoi Infested Area. 0 = None, 1 = Raider, 2 = SCurrency, 3 = Both [0/1/2/3]")] UseSpecialAmmo,

    [BoolOption][Display(Description = @"Enable/Disable Looting [true/false]")] EnableLooting,
    [BoolOption][Display(Description = @"Loots and reads unknown scrolls (looting must be enabled) [true/false]")] ReadUnknownScrolls,
    [BoolOption][Display(Description = @"Causes the macro to loot corpses killed by neither you nor your fellows [true/false]")] LootAllCorpses,
    [BoolOption][Display(Description = @"Causes the macro to loot the corpses killed by your fellows [true/false]")] LootFellowCorpses,
    [BoolOption][Display(Description = @"When true, corpses are looted before attacking monsters [true/false]")] LootPriorityBoost,
    [DoubleOption][Display(Description = @"The time in seconds the macro will wait for all items on a corpse to appear [##.###]")] CorpseItemAppearanceTimeoutSeconds,
    [DoubleOption][Display(Description = @"The time in seconds the macro will wait for all the needed items on a corpse to be ID'd [##.###]")] CorpseItemIDTimeoutSeconds,
    [BoolOption][Display(Description = @"When true and looting is enabled, salvage bags are combined according to the range specified by the current loot profile [true/false]")] CombineSalvage,
    [BoolOption][Display(Description = @"When true, only rare-generating corpses are looted [true/false]")] LootOnlyRareCorpses,
    [DoubleOption][Display(Description = @"Time after trying to open a corpse before the macro will retry [##.###]")] CorpseOpenTimeoutSeconds,
    [IntOption][Display(Description = @"Number of attempts to loot an item before blacklisting. [##]")] CorpseLootItemMaxAttempts
}

public static class VTankOptionsExtensions
{
    public static bool TryParse(string rep, out VTankOptions option)
    {
        foreach (var field in typeof(VTankOptions).GetFields())
        {
            if (field.Name.Equals(rep, StringComparison.OrdinalIgnoreCase))
            {
                option = (VTankOptions)field.GetValue(null);
                return true;
            }
            else
            {
                var attribute = field.GetCustomAttribute<DisplayAttribute>();
                if (attribute != null && attribute.Name != null && attribute.Name.Equals(rep, StringComparison.OrdinalIgnoreCase))
                {
                    option = (VTankOptions)field.GetValue(null);
                    return true;
                }
            }
        }

        option = default;
        return false;
    }

    public static string GetDisplayName(this VTankOptions option)
    {
        var fi = typeof(VTankOptions).GetField(option.ToString());
        if (fi != null)
        {
            var att = fi.GetCustomAttribute<DisplayAttribute>();
            return att?.Name ?? option.ToString();
        }

        return option.ToString();
    }

    public static bool IsValidValue(this VTankOptions option, string value)
    {
        var fi = typeof(VTankOptions).GetField(option.ToString());
        if (fi != null)
        {
            var validationAttributes = fi.GetCustomAttributes<ValidationAttribute>();
            if (validationAttributes != null && validationAttributes.Any(a => !a.IsValid(value)))
            {
                return false;
            }
        }

        return true;
    }

    public static string GetDescription(this VTankOptions option)
    {
        var fi = typeof(VTankOptions).GetField(option.ToString());
        if (fi != null)
        {
            var att = fi.GetCustomAttribute<DisplayAttribute>();
            return att?.Description;
        }

        return null;
    }
}
