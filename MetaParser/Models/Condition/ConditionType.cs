using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MetaParser.Models
{
    [TypeConverter(typeof(EnumDescriptionConverter))]
    public enum ConditionType
    {
        Never = 0,
        Always = 1,
        All = 2,
        Any = 3,
        [Description("Chat Message")]ChatMessage = 4,
        [Description("Main Pack Slots ≤"), Range(0, 102)]MainPackSlotsLE = 5,
        [Description("Seconds in State ≥"), Range(0, int.MaxValue)] SecondsInStateGE = 6,
        [Description("Navroute Empty")]NavrouteEmpty = 7,
        [Description("Character Died")]Died = 8,
        [Description("Vendor Open")]VendorOpen = 9,
        [Description("Vendor Closed")]VendorClosed = 10,
        [Description("Inventory Item Count ≤"), Range(0, int.MaxValue)]ItemCountLE = 11,
        [Description("Inventory Item Count ≥"), Range(0, int.MaxValue)]ItemCountGE = 12,
        [Description("Monster Count Within Distance")]MonsterCountWithinDistance = 13,
        [Description("Monsters With Priority Within Distance")]MonstersWithPriorityWithinDistance = 14,
        [Description("Need to Buff")]NeedToBuff = 15,
        [Description("No Monsters In Distance")]NoMonstersWithinDistance = 16,
        [Description("LandBlock ="), Range(int.MinValue, int.MaxValue)]LandBlockE = 17,
        [Description("LandCell ="), Range(int.MinValue, int.MaxValue)]LandCellE = 18,
        [Description("Portalspace Enter")]PortalspaceEnter = 19,
        [Description("Portalspace Exit")]PortalspaceExit = 20,
        Not = 21,
        [Description("Seconds in State (Persistent) ≥"), Range(0, int.MaxValue)]SecondsInStatePersistGE = 22,
        [Description("Time Left on Spell ≥"), Range(0, 14400)] TimeLeftOnSpellGE = 23,
        [Description("Burden Percent ≥"), Range(0, 300)] BurdenPercentGE = 24,
        [Description("Distance to Any Route Point ≥"), Range(0, 10000)]DistanceToAnyRoutePointGE = 25,
        [Description("Expression")]Expression = 26,
        //ClientDialogPopup = 27,
        [Description("Chat Message Capture")]ChatMessageCapture = 28
    }

    public static class ConditionTypeExtensions
    {
        public static string GetDescription(this ConditionType condition)
        {
            var fi = typeof(ConditionType).GetField(condition.ToString());
            if (fi != null)
            {
                var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                return ((attributes.Length > 0) && (!string.IsNullOrEmpty(attributes[0].Description))) ? attributes[0].Description : condition.ToString();
            }

            return condition.ToString();
        }

        public static (int minValue, int maxValue)? GetRange(this ConditionType conditionType)
        {
            var fi = typeof(ConditionType).GetField(conditionType.ToString());
            if (fi != null)
            {
                var attributes = (RangeAttribute[])fi.GetCustomAttributes(typeof(RangeAttribute), false);
                if (attributes.Length > 0)
                    return ((int)attributes[0].Minimum, (int)attributes[0].Maximum);
            }

            return null;
        }
    }
}
