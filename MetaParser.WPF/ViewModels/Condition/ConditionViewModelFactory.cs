using MetaParser.Models;
using System;
using System.Collections.Generic;

namespace MetaParser.WPF.ViewModels
{
    public static class ConditionViewModelFactory
    {
        private static readonly Dictionary<Type, Type> typedict = new();

        public static ConditionViewModel CreateViewModel(Condition condition) => condition switch
        {
            NotCondition nc => new NotConditionViewModel(nc),
            MultipleCondition mc => new MultipleConditionViewModel(mc),
            Condition<int> c => c.Type == ConditionType.LandBlockE ?
                new LandBlockConditionViewModel(c) :
                new ConditionViewModel<int>(c),
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
