using MetaParser.Models;

namespace MetaParser.WPF.ViewModels
{
    public class MonsterCountWithinDistanceConditionViewModel : ConditionViewModel
    {
        public MonsterCountWithinDistanceConditionViewModel(MonsterCountWithinDistanceCondition condition) : base(condition)
        { }

        public string MonsterNameRx
        {
            get => ((MonsterCountWithinDistanceCondition)Condition).MonsterNameRx;
            set
            {
                if (((MonsterCountWithinDistanceCondition)Condition).MonsterNameRx != value)
                {
                    ((MonsterCountWithinDistanceCondition)Condition).MonsterNameRx = value;
                    OnPropertyChanged(nameof(MonsterNameRx));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public int Count
        {
            get => ((MonsterCountWithinDistanceCondition)Condition).Count;
            set
            {
                if (((MonsterCountWithinDistanceCondition)Condition).Count != value)
                {
                    ((MonsterCountWithinDistanceCondition)Condition).Count = value;
                    OnPropertyChanged(nameof(Count));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public double Distance
        {
            get => ((MonsterCountWithinDistanceCondition)Condition).Distance;
            set
            {
                if (((MonsterCountWithinDistanceCondition)Condition).Distance != value)
                {
                    ((MonsterCountWithinDistanceCondition)Condition).Distance = value;
                    OnPropertyChanged(nameof(Distance));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
