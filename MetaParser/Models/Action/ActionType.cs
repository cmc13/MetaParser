using System.ComponentModel;

namespace MetaParser.Models
{
    [TypeConverter(typeof(EnumDescriptionConverter))]
    public enum ActionType
    {
        None = 0,
        [Description("Set State")]SetState = 1,
        [Description("Chat Command")]ChatCommand = 2,
        [Description("Do All")]Multiple = 3,
        [Description("Load Embedded Nav Route")]EmbeddedNavRoute = 4,
        [Description("Call State")]CallState = 5,
        [Description("Return from Call")]ReturnFromCall = 6,
        [Description("Expression Action")]ExpressionAct = 7,
        [Description("Chat With Expression")]ChatWithExpression = 8,
        [Description("Watchdog Set")]WatchdogSet = 9,
        [Description("Watchdog Clear")]WatchdogClear = 10,
        [Description("Get VTank Option")]GetVTOption = 11,
        [Description("Set VTank Option")]SetVTOption = 12,
        [Description("Create XML View")]CreateView = 13,
        [Description("Destroy XML View")]DestroyView = 14,
        [Description("Destroy All Views")]DestroyAllViews = 15
    }

    public static class ActionTypeExtensions
    {
        public static string GetDescription(this ActionType action)
        {
            var fi = typeof(ActionType).GetField(action.ToString());
            if (fi != null)
            {
                var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                return ((attributes.Length > 0) && (!string.IsNullOrEmpty(attributes[0].Description))) ? attributes[0].Description : action.ToString();
            }

            return action.ToString();
        }
    }
}
