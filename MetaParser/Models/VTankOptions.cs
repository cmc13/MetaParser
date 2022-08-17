using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MetaParser.Models
{
    [TypeConverter(typeof(EnumDescriptionConverter))]
    public enum VTankOptions
    {
        [RegularExpression(@"^(true|false)$")] AutoCram,
        [RegularExpression(@"^(true|false)$")] AutoStack,
        [RegularExpression(@"^(true|false)$")] RandomHelperBuffs,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] RandomHelperIntervalSeconds,
        [RegularExpression(@"^(true|false)$")] IdlePeaceMode,
        [RegularExpression(@"^(true|false)$")] StopMacroOnDeath,
        [RegularExpression(@"^(true|false)$")] RefillWornMana,
        [RegularExpression(@"^\d+$")] [Description("RefillWornMana-Item-ManaPercent")] RefillWornMana_Item_ManaPercent,
        [RegularExpression(@"^\d+$")] ManaStoneLootCount,
        [RegularExpression(@"^\d+$")] ManaTankMinimumMana,
        [RegularExpression(@"^(true|false)$")] ManaChargesWhenOff,
        [RegularExpression(@"^(true|false)$")] AutoFellowManagement,
        [RegularExpression(@"^(true|false)$")] DeleteGhostMonsters,
        [RegularExpression(@"^\d+$")] GhostMonsterSpellAttemptCount,
        [RegularExpression(@"^(true|false)$")] WhoYouGonnaCall,
        [RegularExpression(@"^\d+$")] BlacklistMonsterAttemptCount,
        [RegularExpression(@"^\d+$")] BlacklistMonsterTimeoutSeconds,
        [RegularExpression(@"^(true|false)$")] DeleteGhostMonstersByHPTracker,

        [RegularExpression(@"^\d+$")] GhostDeleteHPTrackerSeconds,
        [RegularExpression(@"^(true|false)$")] GoToPeaceModeToUseKits,
        [RegularExpression(@"^(true|false)$")] EnableMeta,
        [RegularExpression(@"^\d+$")] DropToPeaceModeRetryCount,
        [RegularExpression(@"^\d+$")] BlacklistCorpseOpenAttemptCount,
        [RegularExpression(@"^\d+$")] BlacklistCorpseOpenTimeoutSeconds,

        [RegularExpression(@"^\d+$")][Description("Recharge-Norm-HitP")] Recharge_Norm_HitP,
        [RegularExpression(@"^\d+$")][Description("Recharge-Norm-Stam")] Recharge_Norm_Stam,
        [RegularExpression(@"^\d+$")][Description("Recharge-Norm-Mana")] Recharge_Norm_Mana,
        [RegularExpression(@"^\d+$")][Description("Recharge-NoTarg-HitP")] Recharge_NoTarg_HitP,
        [RegularExpression(@"^\d+$")][Description("Recharge-NoTarg-Stam")] Recharge_NoTarg_Stam,
        [RegularExpression(@"^\d+$")][Description("Recharge-NoTarg-Mana")] Recharge_NoTarg_Mana,
        [RegularExpression(@"^\d+$")][Description("Recharge-Helper-HitP")] Recharge_Helper_HitP,
        [RegularExpression(@"^\d+$")][Description("Recharge-Helper-Stam")] Recharge_Helper_Stam,
        [RegularExpression(@"^\d+$")][Description("Recharge-Helper-Mana")] Recharge_Helper_Mana,
        [RegularExpression(@"^(true|false)$")] DoHelp,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] HelperDistanceHitP,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] HelperDistanceStam,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] HelperDistanceMana,
        [RegularExpression(@"^(true|false)$")] CastDispelSelf,
        [RegularExpression(@"^(true|false)$")] UseDispelItems,
        [RegularExpression(@"^(true|false)$")] UseDispelDrum,
        [RegularExpression(@"^(true|false)$")] UseHealersHeart,
        [RegularExpression(@"^(true|false)$")] JumpOutWandCasting,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] RechargeBoostTimeSeconds,
        [RegularExpression(@"^\d+$")] RechargeBoostAmount,
        [RegularExpression(@"^\d+$")] MinimumHealKitSuccessChance,
        [RegularExpression(@"^(true|false)$")] UseKitsInMagicMode,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] StaminaToHealthMultiplier,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] ManaToHealthMultiplier,
        [RegularExpression(@"^(true|false)$")] ClearLevelBoostFlagOnCast,
        [RegularExpression(@"^\d+$")] IdleCraftCount_HealthKits,
        [RegularExpression(@"^\d+$")] IdleCraftCount_StamKits,
        [RegularExpression(@"^\d+$")] IdleCraftCount_ManaKits,
        [RegularExpression(@"^\d+$")] IdleCraftCount_HealthFood,
        [RegularExpression(@"^\d+$")] IdleCraftCount_StamFood,
        [RegularExpression(@"^\d+$")] IdleCraftCount_ManaFood,

        [RegularExpression(@"^(true|false)$")] EnableCombat,
        [RegularExpression(@"^1|2|3$")] DefaultMeleeAttackHeight,
        [RegularExpression(@"^(true|false)$")] TargetLock,
        [RegularExpression(@"^1|2|3$")] TargetSelectMethod,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] TargetSelectAngleRange,
        [RegularExpression(@"^1|2|3$")] DebuffEachFirst,
        [RegularExpression(@"^(true|false)$")] AutoAttackPower,
        [RegularExpression(@"^1|2$")] DebuffSelectionMethod,
        [RegularExpression(@"^(true|false)$")] UseRecklessness,

        [RegularExpression(@"^\d+$")][Description("SpellDiffExcessThreshold-Hunt")] SpellDiffExcessThreshold_Hunt,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] RingDistance,
        [RegularExpression(@"^\d+$")] MinimumRingTargets,
        [RegularExpression(@"^(true|false)$")] SwitchWandToDebuff,
        [RegularExpression(@"^(true|false)$")] DoJiggle,
        [RegularExpression(@"^(true|false)$")] UseArcs,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] ArcRange,
        [RegularExpression(@"^\d+$")] DebuffPrecastSeconds,
        [RegularExpression(@"^(true|false)$")] SummonPets,
        [RegularExpression(@"^0|1$")] PetRangeMode,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] PetCustomRange,
        [Description("PetRefillCount-Idle")] PetRefillCount_Idle,
        [Description("PetRefillCount-Normal")] PetRefillCount_Normal,
        [RegularExpression(@"^\d+$")] PetMonsterDensity,

        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] AttackDistance,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] AttackMinimumDistance,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] ApproachDistance,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")][Description("CorpseApproachRange-Max")] CorpseApproachRange_Max,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")][Description("CorpseApproachRange-Min")] CorpseApproachRange_Min,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] NavCloseStopRange,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] NavFarStopRange,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] UsePortalDistance,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] DoorIDRange,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] DoorOpenRange,

        [RegularExpression(@"^(true|false)$")] EnableNav,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] CorpseCacheTimeoutMinutes,
        [RegularExpression(@"^(true|false)$")] OpenDoors,
        [RegularExpression(@"^\d+$")] DoorLockpickDiffExcessThreshold,
        [RegularExpression(@"^(true|false)$")] NavPriorityBoost,
        [RegularExpression(@"^(true|false)$")] FollowAroundCorners,

        [RegularExpression(@"^(true|false)$")] EnableBuffing,
        [Description("SpellDiffExcessThreshold-Buff")] SpellDiffExcessThreshold_Buff,
        [RegularExpression(@"^(true|false)$")] IdleBuffTopoff,
        [RegularExpression(@"^\d+$")] IdleBuffTopoffTimeSeconds,
        [RegularExpression(@"^\d+$")] RebuffTimeRemainingSeconds,
        [RegularExpression(@"^[BPSAFLC]*$")][Description("BuffProfile-Prots")] BuffProfile_Prots,
        [RegularExpression(@"^[BPSAFLC]*$")][Description("BuffProfile-Banes")] BuffProfile_Banes,
        [RegularExpression(@"^\d+$")] BuffCastRecast_Seconds,
        [RegularExpression(@"^\d+$")] BuffCastRecastReset_Seconds,

        [RegularExpression(@"^\d+$")] ArrowheadFletchDiffExcessThreshold,
        [RegularExpression(@"^(true|false)$")] AutoCraftItems,
        [RegularExpression(@"^(true|false)$")] SplitPeas,
        [RegularExpression(@"^\d+$")][Description("SpellCompMin-Critical")] SpellCompMin_Critical,
        [RegularExpression(@"^\d+$")][Description("SpellCompMin-Normal")] SpellCompMin_Normal,
        [RegularExpression(@"^\d+$")][Description("SpellCompMin-Idle")] SpellCompMin_Idle,
        [RegularExpression(@"^0|1|2|3$")] UseSpecialAmmo,

        [RegularExpression(@"^(true|false)$")] EnableLooting,
        [RegularExpression(@"^(true|false)$")] ReadUnknownScrolls,
        [RegularExpression(@"^(true|false)$")] LootAllCorpses,
        [RegularExpression(@"^(true|false)$")] LootFellowCorpses,
        [RegularExpression(@"^(true|false)$")] LootPriorityBoost,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] CorpseItemAppearanceTimeoutSeconds,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] CorpseItemIDTimeoutSeconds,
        [RegularExpression(@"^(true|false)$")] CombineSalvage,
        [RegularExpression(@"^(true|false)$")] LootOnlyRareCorpses,
        [RegularExpression(@"^[+\-]?(([1-9]\d*\.|\d?\.)(\d+([eE][+\-]?[0-9]+)|[0-9]+)|([1-9]\d*|0))$")] CorpseOpenTimeoutSeconds,
        [RegularExpression(@"^\d+$")] CorpseLootItemMaxAttempts
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
                var regex = fi.GetCustomAttribute<RegularExpressionAttribute>();
                if (regex != null)
                {
                    var r = new Regex(regex.Pattern, RegexOptions.IgnoreCase);
                    if (!r.IsMatch(value))
                        return false;
                }
                else
                {
                    if (!double.TryParse(value, out _))
                        return false;
                }
            }

            if (option == VTankOptions.BuffProfile_Banes || option == VTankOptions.BuffProfile_Prots)
            {
                for (var i = 0; i < value.Length - 1; ++i)
                {
                    for (var j = 0; j < value.Length; ++j)
                        if (value[i] == value[j])
                            return false;
                }
            }

            return true;
        }
    }
}
