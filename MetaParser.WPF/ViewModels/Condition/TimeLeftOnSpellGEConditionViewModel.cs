using MetaParser.Models;

namespace MetaParser.WPF.ViewModels
{
    public class TimeLeftOnSpellGEConditionViewModel : ConditionViewModel
    {
        public TimeLeftOnSpellGEConditionViewModel(TimeLeftOnSpellGECondition condition) : base(condition)
        { }

        public int SpellId
        {
            get => ((TimeLeftOnSpellGECondition)Condition).SpellId;
            set
            {
                if (((TimeLeftOnSpellGECondition)Condition).SpellId != value)
                {
                    ((TimeLeftOnSpellGECondition)Condition).SpellId = value;
                    OnPropertyChanged(nameof(SpellId));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public int Seconds
        {
            get => ((TimeLeftOnSpellGECondition)Condition).Seconds;
            set
            {
                if (((TimeLeftOnSpellGECondition)Condition).Seconds != value)
                {
                    ((TimeLeftOnSpellGECondition)Condition).Seconds = value;
                    OnPropertyChanged(nameof(Seconds));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
