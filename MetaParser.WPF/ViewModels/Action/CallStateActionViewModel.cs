using MetaParser.Models;
using System;
using System.Linq;

namespace MetaParser.WPF.ViewModels
{
    public class CallStateActionViewModel : ActionViewModel
    {
        public CallStateActionViewModel(CallStateMetaAction action, MetaViewModel meta) : base(action, meta)
        { }

        public string CallState
        {
            get => ((CallStateMetaAction)Action).CallState;
            set
            {
                if (((CallStateMetaAction)Action).CallState != value)
                {
                    ((CallStateMetaAction)Action).CallState = value;
                    OnPropertyChanged(nameof(CallState));
                    OnPropertyChanged(nameof(Display));
                    OnPropertyChanged(nameof(IsValid));
                    OnStateChanged();
                    IsDirty = true;
                }
            }
        }

        public string ReturnState
        {
            get => ((CallStateMetaAction)Action).ReturnState;
            set
            {
                if (((CallStateMetaAction)Action).ReturnState != value)
                {
                    ((CallStateMetaAction)Action).ReturnState = value;
                    OnPropertyChanged(nameof(ReturnState));
                    OnPropertyChanged(nameof(Display));
                    OnPropertyChanged(nameof(IsValid));
                    OnStateChanged();
                    IsDirty = true;
                }
            }
        }

        public override bool IsValid => Meta.Rules.Any(r => r.State == CallState) && Meta.Rules.Any(r => r.State == ReturnState);
    }
}
