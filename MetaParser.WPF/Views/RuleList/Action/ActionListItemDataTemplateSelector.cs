using MetaParser.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MetaParser.WPF.Views.RuleList.Action
{
    class ActionListItemDataTemplateSelector
        : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate SetStateTemplate { get; set; }
        public DataTemplate CallStateTemplate { get; set; }
        public DataTemplate MultipleActionTemplate { get; set; }
        public DataTemplate SetWatchdogTemplate { get; set; }
        public DataTemplate LoadNavTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                ActionViewModel<string> sa when sa.Type == Models.ActionType.SetState => SetStateTemplate ?? DefaultTemplate,
                CallStateActionViewModel => CallStateTemplate ?? DefaultTemplate,
                AllActionViewModel => MultipleActionTemplate ?? DefaultTemplate,
                WatchdogSetActionViewModel => SetWatchdogTemplate ?? DefaultTemplate,
                LoadEmbeddedNavRouteActionViewModel => LoadNavTemplate ?? DefaultTemplate,
                _ => DefaultTemplate
            };
        }
    }
}
