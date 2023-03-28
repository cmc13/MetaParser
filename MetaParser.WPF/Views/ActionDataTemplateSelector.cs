using MetaParser.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MetaParser.WPF.Views
{
    public class ActionDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate ExpressionTemplate { get; set; }
        public DataTemplate StringValueTemplate { get; set; }
        public DataTemplate SetVTOptionTemplate { get; set; }
        public DataTemplate GetVTOptionTemplate { get; set; }
        public DataTemplate CallStateTemplate { get; set; }
        public DataTemplate WatchdogSetTemplate { get; set; }
        public DataTemplate DestroyViewTemplate { get; set; }
        public DataTemplate CreateViewTemplate { get; set; }
        public DataTemplate AllTemplate { get; set; }
        public DataTemplate SetStateTemplate { get; set; }
        public DataTemplate LoadNavTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) => item switch
        {
            ActionViewModel<int> or TableActionViewModel => DefaultTemplate,
            ExpressionActionViewModel => ExpressionTemplate ?? DefaultTemplate,
            ActionViewModel<string> c when c.Type == Models.ActionType.SetState => SetStateTemplate ?? DefaultTemplate,
            ActionViewModel<string> => StringValueTemplate ?? DefaultTemplate,
            SetVTOptionActionViewModel => SetVTOptionTemplate ?? DefaultTemplate,
            GetVTOptionActionViewModel => GetVTOptionTemplate ?? DefaultTemplate,
            CallStateActionViewModel => CallStateTemplate ?? DefaultTemplate,
            WatchdogSetActionViewModel => WatchdogSetTemplate ?? DefaultTemplate,
            DestroyViewActionViewModel => DestroyViewTemplate ?? DefaultTemplate,
            CreateViewActionViewModel => CreateViewTemplate ?? DefaultTemplate,
            AllActionViewModel => AllTemplate ?? DefaultTemplate,
            LoadEmbeddedNavRouteActionViewModel => LoadNavTemplate ?? DefaultTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}
