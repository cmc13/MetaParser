using MetaParser.Models;

namespace MetaParser.WPF.ViewModels
{
    public class DistanceToAnyRoutePointGEConditionViewModel : ConditionViewModel
    {
        public DistanceToAnyRoutePointGEConditionViewModel(DistanceToAnyRoutePointGECondition condition) : base(condition)
        { }

        public double Distance
        {
            get => ((DistanceToAnyRoutePointGECondition)Condition).Distance;
            set
            {
                if (((DistanceToAnyRoutePointGECondition)Condition).Distance != value)
                {
                    ((DistanceToAnyRoutePointGECondition)Condition).Distance = value;
                    OnPropertyChanged(nameof(Distance));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
