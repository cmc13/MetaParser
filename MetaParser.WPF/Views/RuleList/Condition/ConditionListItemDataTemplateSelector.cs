using MetaParser.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MetaParser.WPF.Views.RuleList.Condition
{
    public class ConditionListItemDataTemplateSelector
        : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate MultipleTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DefaultTemplate ??= base.SelectTemplate(item, container);

            return item switch
            {
                MultipleConditionViewModel mc => MultipleTemplate ?? DefaultTemplate,
                _ => DefaultTemplate
            };
        }
    }
}
