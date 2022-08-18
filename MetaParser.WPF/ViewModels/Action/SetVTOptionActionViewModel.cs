using MetaParser.Models;
using System.Collections.Generic;
using System.Linq;

namespace MetaParser.WPF.ViewModels
{
    public class SetVTOptionActionViewModel : ActionViewModel
    {
        public SetVTOptionActionViewModel(SetVTOptionMetaAction action, MetaViewModel meta) : base(action, meta)
        {
        }

        public IEnumerable<string> Options => System.Enum.GetValues<VTankOptions>().Select(o => o.GetDescription());

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
                    OnPropertyChanged(nameof(IsValid));
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
                    OnPropertyChanged(nameof(IsValid));
                    IsDirty = true;
                }
            }
        }

        public override bool IsValid => VTankOptionsExtensions.TryParse(Option, out var opt) && opt.IsValidValue(Value);
    }
}
