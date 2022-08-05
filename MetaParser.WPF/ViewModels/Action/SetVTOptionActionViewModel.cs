using MetaParser.Models;

namespace MetaParser.WPF.ViewModels
{
    public class SetVTOptionActionViewModel : ActionViewModel
    {
        public SetVTOptionActionViewModel(SetVTOptionMetaAction action, MetaViewModel meta) : base(action, meta)
        { }

        public string Option
        {
            get => ((SetVTOptionMetaAction)Action).Option;
            set
            {
                if (((SetVTOptionMetaAction)Action).Option != value)
                {
                    ((SetVTOptionMetaAction)Action).Option = value;
                    OnPropertyChanged(nameof(Option));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public string Value
        {
            get => ((SetVTOptionMetaAction)Action).Value;
            set
            {
                if (((SetVTOptionMetaAction)Action).Value != value)
                {
                    ((SetVTOptionMetaAction)Action).Value = value;
                    OnPropertyChanged(nameof(Value));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
