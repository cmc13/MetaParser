using MetaParser.Models;

namespace MetaParser.WPF.ViewModels
{
    public class MonstersWithPriorityWithinDistanceConditionViewModel : ConditionViewModel
    {
        public MonstersWithPriorityWithinDistanceConditionViewModel(MonstersWithPriorityWithinDistanceCondition condition) : base(condition)
        { }

        public int Priority
        {
            get => ((MonstersWithPriorityWithinDistanceCondition)Condition).Priority;
            set
            {
                if (((MonstersWithPriorityWithinDistanceCondition)Condition).Priority != value)
                {
                    ((MonstersWithPriorityWithinDistanceCondition)Condition).Priority = value;
                    OnPropertyChanged(nameof(Priority));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public int Count
        {
            get => ((MonstersWithPriorityWithinDistanceCondition)Condition).Count;
            set
            {
                if (((MonstersWithPriorityWithinDistanceCondition)Condition).Count != value)
                {
                    ((MonstersWithPriorityWithinDistanceCondition)Condition).Count = value;
                    OnPropertyChanged(nameof(Count));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public double Distance
        {
            get => ((MonstersWithPriorityWithinDistanceCondition)Condition).Distance;
            set
            {
                if (((MonstersWithPriorityWithinDistanceCondition)Condition).Distance != value)
                {
                    ((MonstersWithPriorityWithinDistanceCondition)Condition).Distance = value;
                    OnPropertyChanged(nameof(Distance));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
