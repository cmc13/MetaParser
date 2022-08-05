using MetaParser.Models;
using System.Linq;

namespace MetaParser.WPF.ViewModels
{
    public class WatchdogSetActionViewModel : ActionViewModel
    {
        public WatchdogSetActionViewModel(WatchdogSetMetaAction action, MetaViewModel meta) : base(action, meta)
        { }

        public string State
        {
            get => ((WatchdogSetMetaAction)Action).State;
            set
            {
                if (((WatchdogSetMetaAction)Action).State != value)
                {
                    ((WatchdogSetMetaAction)Action).State = value;
                    OnPropertyChanged(nameof(State));
                    OnPropertyChanged(nameof(Display));
                    OnPropertyChanged(nameof(IsValid));
                    OnStateChanged();
                    IsDirty = true;
                }
            }
        }

        public double Range
        {
            get => ((WatchdogSetMetaAction)Action).Range;
            set
            {
                if (((WatchdogSetMetaAction)Action).Range != value)
                {
                    ((WatchdogSetMetaAction)Action).Range = value;
                    OnPropertyChanged(nameof(Range));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public double Time
        {
            get => ((WatchdogSetMetaAction)Action).Time;
            set
            {
                if (((WatchdogSetMetaAction)Action).Time != value)
                {
                    ((WatchdogSetMetaAction)Action).Time = value;
                    OnPropertyChanged(nameof(Time));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public override bool IsValid => Meta.Rules.Any(r => r.State == State);
    }
}
