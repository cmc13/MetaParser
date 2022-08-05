using MetaParser.Models;

namespace MetaParser.WPF.ViewModels
{
    public class NavNodeRecallViewModel : NavNodeViewModel
    {
        public NavNodeRecallViewModel(NavNodeRecall node) : base(node)
        {}

        public RecallSpellId SpellId
        {
            get => ((NavNodeRecall)Node).Data;
            set
            {
                if (SpellId != value)
                {
                    ((NavNodeRecall)Node).Data = value;
                    OnPropertyChanged(nameof(SpellId));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
