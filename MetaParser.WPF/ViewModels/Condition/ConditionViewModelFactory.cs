using MetaParser.Models;
using MetaParser.WPF.Services;

namespace MetaParser.WPF.ViewModels
{
    public class ConditionViewModelFactory
    {
        private readonly ClipboardService clipboardService;

        public ConditionViewModelFactory(ClipboardService clipboardService)
        {
            this.clipboardService = clipboardService;
        }

        public ConditionViewModel CreateViewModel(Condition condition) => condition switch
        {
            NotCondition nc => new NotConditionViewModel(nc, this),
            MultipleCondition mc => new MultipleConditionViewModel(mc, this, clipboardService),
            Condition<int> c => c.Type switch
            {
                ConditionType.LandBlockE => new LandBlockConditionViewModel(c),
                ConditionType.LandCellE => new LandCellConditionViewModel(c),
                _ => new ConditionViewModel<int>(c)
            },
            Condition<string> c => new ConditionViewModel<string>(c),
            ExpressionCondition ec => new ExpressionConditionViewModel(ec),
            DistanceToAnyRoutePointGECondition dc => new DistanceToAnyRoutePointGEConditionViewModel(dc),
            ItemCountCondition ic => new ItemCountConditionViewModel(ic),
            NoMonstersInDistanceCondition mc => new NoMonstersWithinDistanceConditionViewModel(mc),
            ChatMessageCaptureCondition cc => new ChatMessageCaptureConditionViewModel(cc),
            MonsterCountWithinDistanceCondition mc => new MonsterCountWithinDistanceConditionViewModel(mc),
            MonstersWithPriorityWithinDistanceCondition pc => new MonstersWithPriorityWithinDistanceConditionViewModel(pc),
            TimeLeftOnSpellGECondition tc => new TimeLeftOnSpellGEConditionViewModel(tc),
            _ => new ConditionViewModel(condition)
        };
    }
}
