using MetaParser.Models;

namespace MetaParser.WPF.ViewModels
{
    public class NoMonstersWithinDistanceConditionViewModel : ConditionViewModel
    {
        public NoMonstersWithinDistanceConditionViewModel(NoMonstersInDistanceCondition condition) : base(condition)
        { }

        public double Distance
        {
            get => ((NoMonstersInDistanceCondition)Condition).Distance;
            set
            {
                if (((NoMonstersInDistanceCondition)Condition).Distance != value)
                {
                    ((NoMonstersInDistanceCondition)Condition).Distance = value;
                    OnPropertyChanged(nameof(Distance));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
