using MetaParser.Models;
using MetaParser.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MetaParser.WPF.Views
{
    public class ConditionDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate BurdenPercentGETemplate { get; set; }
        public DataTemplate ExpressionTemplate { get; set; }
        public DataTemplate DistanceTemplate { get; set; }
        public DataTemplate ItemCountTemplate { get; set; }
        public DataTemplate ChatMessageCaptureTemplate { get; set; }
        public DataTemplate MonsterCountWithinDistanceTemplate { get; set; }
        public DataTemplate MonstersWithPriorityWithinDistanceTemplate { get; set; }
        public DataTemplate NotTemplate { get; set; }
        public DataTemplate TimeLeftOnSpellGETemplate { get; set; }
        public DataTemplate MultipleTemplate { get; set; }
        public DataTemplate ChatMessageTemplate { get; set; }
        public DataTemplate LandBlockTemplate { get; set; }
        public DataTemplate LandCellTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) => item switch
        {
            NotConditionViewModel nc => NotTemplate ?? DefaultTemplate,
            MultipleConditionViewModel mc => MultipleTemplate ?? DefaultTemplate,
            ExpressionConditionViewModel ec => ExpressionTemplate ?? DefaultTemplate,
            DistanceToAnyRoutePointGEConditionViewModel dc => DistanceTemplate ?? DefaultTemplate,
            ItemCountConditionViewModel ic => ItemCountTemplate ?? DefaultTemplate,
            NoMonstersWithinDistanceConditionViewModel mc => DistanceTemplate ?? DefaultTemplate,
            ChatMessageCaptureConditionViewModel cc => ChatMessageCaptureTemplate ?? DefaultTemplate,
            MonsterCountWithinDistanceConditionViewModel mc => MonsterCountWithinDistanceTemplate ?? DefaultTemplate,
            MonstersWithPriorityWithinDistanceConditionViewModel mc => MonstersWithPriorityWithinDistanceTemplate ?? DefaultTemplate,
            TimeLeftOnSpellGEConditionViewModel tc => TimeLeftOnSpellGETemplate ?? DefaultTemplate,
            ConditionViewModel<int> c => c.Condition.Type switch
            {
                ConditionType.BurdenPercentGE or
                ConditionType.MainPackSlotsLE or
                ConditionType.SecondsInStateGE or
                ConditionType.SecondsInStatePersistGE => BurdenPercentGETemplate ?? DefaultTemplate,
                ConditionType.LandCellE => LandCellTemplate ?? DefaultTemplate,
                ConditionType.LandBlockE => LandBlockTemplate ?? DefaultTemplate,
                _ => DefaultTemplate
            },
            ConditionViewModel<string> c => ChatMessageTemplate ?? DefaultTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}
