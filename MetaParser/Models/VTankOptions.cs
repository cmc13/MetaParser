using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MetaParser.Models
{
    public class BoolOption : ValidationAttribute
    {
        private static readonly Regex r = new(@"^(true|false|\d+)$", RegexOptions.Compiled);

        public override bool IsValid(object value)
        {
            if (value is string str)
            {
                if (bool.TryParse(str, out _) || r.IsMatch(str))
                    return true;
            }

            return false;
        }
    }

    public class IntOption : ValidationAttribute
    {
        private static readonly Regex r = new(@"^\d+$", RegexOptions.Compiled);

        public override bool IsValid(object value)
        {
            if (value is string str)
            {
                if (r.IsMatch(str) && int.TryParse(str, out _))
                    return true;
            }

            return false;
        }
    }

    public class DoubleOption : ValidationAttribute
    {
        private static readonly Regex r = new(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$", RegexOptions.Compiled);

        public override bool IsValid(object value)
        {
            if (value is string str)
            {
                if (r.IsMatch(str) && double.TryParse(str, out _))
                    return true;
            }

            return false;
        }
    }

    public class BuffProfileOption : ValidationAttribute
    {
        private static readonly Regex r = new(@"^[BPSAFLC]*$", RegexOptions.Compiled);

        public override bool IsValid(object value)
        {
            if (value is string str)
            {
                if (r.IsMatch(str))
                {
                    // Check if any characters appear more than once
                    return str.GroupBy(c => c).Select(g => new { ch = g.Key, count = g.Count() }).All(a => a.count < 2);
                }
            }
            return false;
        }
    }

    [TypeConverter(typeof(EnumDescriptionConverter))]
    public enum VTankOptions
    {
        [BoolOption] AutoCram,
        [BoolOption] AutoStack,
        [BoolOption] RandomHelperBuffs,
        [DoubleOption] RandomHelperIntervalSeconds,
        [BoolOption] IdlePeaceMode,
        [BoolOption] StopMacroOnDeath,
        [BoolOption] RefillWornMana,
        [IntOption] [Description("RefillWornMana-Item-ManaPercent")] RefillWornMana_Item_ManaPercent,
        [IntOption] ManaStoneLootCount,
        [IntOption] ManaTankMinimumMana,
        [BoolOption] ManaChargesWhenOff,
        [BoolOption] AutoFellowManagement,
        [BoolOption] DeleteGhostMonsters,
        [IntOption] GhostMonsterSpellAttemptCount,
        [BoolOption] WhoYouGonnaCall,
        [IntOption] BlacklistMonsterAttemptCount,
        [IntOption] BlacklistMonsterTimeoutSeconds,
        [BoolOption] DeleteGhostMonstersByHPTracker,

        [IntOption] GhostDeleteHPTrackerSeconds,
        [BoolOption] GoToPeaceModeToUseKits,
        [BoolOption] EnableMeta,
        [IntOption] DropToPeaceModeRetryCount,
        [IntOption] BlacklistCorpseOpenAttemptCount,
        [IntOption] BlacklistCorpseOpenTimeoutSeconds,

        [IntOption][Description("Recharge-Norm-HitP")] Recharge_Norm_HitP,
        [IntOption][Description("Recharge-Norm-Stam")] Recharge_Norm_Stam,
        [IntOption][Description("Recharge-Norm-Mana")] Recharge_Norm_Mana,
        [IntOption][Description("Recharge-NoTarg-HitP")] Recharge_NoTarg_HitP,
        [IntOption][Description("Recharge-NoTarg-Stam")] Recharge_NoTarg_Stam,
        [IntOption][Description("Recharge-NoTarg-Mana")] Recharge_NoTarg_Mana,
        [IntOption][Description("Recharge-Helper-HitP")] Recharge_Helper_HitP,
        [IntOption][Description("Recharge-Helper-Stam")] Recharge_Helper_Stam,
        [IntOption][Description("Recharge-Helper-Mana")] Recharge_Helper_Mana,
        [BoolOption] DoHelp,
        [DoubleOption] HelperDistanceHitP,
        [DoubleOption] HelperDistanceStam,
        [DoubleOption] HelperDistanceMana,
        [BoolOption] CastDispelSelf,
        [BoolOption] UseDispelItems,
        [BoolOption] UseDispelDrum,
        [BoolOption] UseHealersHeart,
        [BoolOption] JumpOutWandCasting,
        [DoubleOption] RechargeBoostTimeSeconds,
        [IntOption] RechargeBoostAmount,
        [IntOption] MinimumHealKitSuccessChance,
        [BoolOption] UseKitsInMagicMode,
        [DoubleOption] StaminaToHealthMultiplier,
        [DoubleOption] ManaToHealthMultiplier,
        [BoolOption] ClearLevelBoostFlagOnCast,
        [IntOption] IdleCraftCount_HealthKits,
        [IntOption] IdleCraftCount_StamKits,
        [IntOption] IdleCraftCount_ManaKits,
        [IntOption] IdleCraftCount_HealthFood,
        [IntOption] IdleCraftCount_StamFood,
        [IntOption] IdleCraftCount_ManaFood,

        [BoolOption] EnableCombat,
        [RegularExpression(@"^1|2|3$")] DefaultMeleeAttackHeight,
        [BoolOption] TargetLock,
        [RegularExpression(@"^1|2|3$")] TargetSelectMethod,
        [DoubleOption] TargetSelectAngleRange,
        [RegularExpression(@"^1|2|3$")] DebuffEachFirst,
        [BoolOption] AutoAttackPower,
        [RegularExpression(@"^1|2$")] DebuffSelectionMethod,
        [BoolOption] UseRecklessness,

        [IntOption][Description("SpellDiffExcessThreshold-Hunt")] SpellDiffExcessThreshold_Hunt,
        [DoubleOption] RingDistance,
        [IntOption] MinimumRingTargets,
        [BoolOption] SwitchWandToDebuff,
        [BoolOption] DoJiggle,
        [BoolOption] UseArcs,
        [DoubleOption] ArcRange,
        [IntOption] DebuffPrecastSeconds,
        [BoolOption] SummonPets,
        [RegularExpression(@"^0|1$")] PetRangeMode,
        [DoubleOption] PetCustomRange,
        [IntOption][Description("PetRefillCount-Idle")] PetRefillCount_Idle,
        [IntOption][Description("PetRefillCount-Normal")] PetRefillCount_Normal,
        [IntOption] PetMonsterDensity,

        [DoubleOption] AttackDistance,
        [DoubleOption] AttackMinimumDistance,
        [DoubleOption] ApproachDistance,
        [DoubleOption][Description("CorpseApproachRange-Max")] CorpseApproachRange_Max,
        [DoubleOption][Description("CorpseApproachRange-Min")] CorpseApproachRange_Min,
        [DoubleOption] NavCloseStopRange,
        [DoubleOption] NavFarStopRange,
        [DoubleOption] UsePortalDistance,
        [DoubleOption] DoorIDRange,
        [DoubleOption] DoorOpenRange,

        [BoolOption] EnableNav,
        [DoubleOption] CorpseCacheTimeoutMinutes,
        [BoolOption] OpenDoors,
        [IntOption] DoorLockpickDiffExcessThreshold,
        [BoolOption] NavPriorityBoost,
        [BoolOption] FollowAroundCorners,

        [BoolOption] EnableBuffing,
        [IntOption][Description("SpellDiffExcessThreshold-Buff")] SpellDiffExcessThreshold_Buff,
        [BoolOption] IdleBuffTopoff,
        [IntOption] IdleBuffTopoffTimeSeconds,
        [IntOption] RebuffTimeRemainingSeconds,
        [BuffProfileOption][Description("BuffProfile-Prots")] BuffProfile_Prots,
        [BuffProfileOption][Description("BuffProfile-Banes")] BuffProfile_Banes,
        [IntOption] BuffCastRecast_Seconds,
        [IntOption] BuffCastRecastReset_Seconds,

        [IntOption] ArrowheadFletchDiffExcessThreshold,
        [BoolOption] AutoCraftItems,
        [BoolOption] SplitPeas,
        [IntOption][Description("SpellCompMin-Critical")] SpellCompMin_Critical,
        [IntOption][Description("SpellCompMin-Normal")] SpellCompMin_Normal,
        [IntOption][Description("SpellCompMin-Idle")] SpellCompMin_Idle,
        [RegularExpression(@"^0|1|2|3$")] UseSpecialAmmo,

        [BoolOption] EnableLooting,
        [BoolOption] ReadUnknownScrolls,
        [BoolOption] LootAllCorpses,
        [BoolOption] LootFellowCorpses,
        [BoolOption] LootPriorityBoost,
        [DoubleOption] CorpseItemAppearanceTimeoutSeconds,
        [DoubleOption] CorpseItemIDTimeoutSeconds,
        [BoolOption] CombineSalvage,
        [BoolOption] LootOnlyRareCorpses,
        [DoubleOption] CorpseOpenTimeoutSeconds,
        [IntOption] CorpseLootItemMaxAttempts
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
                else if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description.Equals(rep, StringComparison.OrdinalIgnoreCase))
                    {
                        option = (VTankOptions)field.GetValue(null);
                        return true;
                    }
                }
            }

            option = default(VTankOptions);
            return false;
        }

        public static string GetDescription(this VTankOptions option)
        {
            var fi = typeof(VTankOptions).GetField(option.ToString());
            if (fi != null)
            {
                var att = fi.GetCustomAttribute<DescriptionAttribute>();
                return att?.Description ?? option.ToString();
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
    }
}
