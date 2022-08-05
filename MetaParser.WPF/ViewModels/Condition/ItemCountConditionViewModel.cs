using MetaParser.Models;

namespace MetaParser.WPF.ViewModels
{
    public class ItemCountConditionViewModel : ConditionViewModel
    {
        public ItemCountConditionViewModel(ItemCountCondition condition) : base(condition)
        { }

        public string ItemName
        {
            get => ((ItemCountCondition)Condition).ItemName;
            set
            {
                if (((ItemCountCondition)Condition).ItemName != value)
                {
                    ((ItemCountCondition)Condition).ItemName = value;
                    OnPropertyChanged(nameof(ItemName));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public int Count
        {
            get => ((ItemCountCondition)Condition).Count;
            set
            {
                if (((ItemCountCondition)Condition).Count != value)
                {
                    ((ItemCountCondition)Condition).Count = value;
                    OnPropertyChanged(nameof(Count));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
