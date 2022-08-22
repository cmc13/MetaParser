using MetaParser.Models;
using System.Collections.Generic;
using System.Linq;

namespace MetaParser.WPF.ViewModels
{
    public class GetVTOptionActionViewModel : ActionViewModel
    {
        public GetVTOptionActionViewModel(GetVTOptionMetaAction action, MetaViewModel meta) : base(action, meta)
        { }

        public IEnumerable<string> Options => System.Enum.GetValues<VTankOptions>().Select(o => o.GetDisplayName());

        public string Option
        {
            get => ((GetVTOptionMetaAction)Action).Option;
            set
            {
                if (((GetVTOptionMetaAction)Action).Option != value)
                {
                    ((GetVTOptionMetaAction)Action).Option = value;
                    OnPropertyChanged(nameof(Option));
                    OnPropertyChanged(nameof(Display));
                    OnPropertyChanged(nameof(IsValid));
                    IsDirty = true;
                }
            }
        }

        public string Variable
        {
            get => ((GetVTOptionMetaAction)Action).Variable;
            set
            {
                if (((GetVTOptionMetaAction)Action).Variable != value)
                {
                    ((GetVTOptionMetaAction)Action).Variable = value;
                    OnPropertyChanged(nameof(Variable));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public override bool IsValid => VTankOptionsExtensions.TryParse(Option, out _);
    }
}
