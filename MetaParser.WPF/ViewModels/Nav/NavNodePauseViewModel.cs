using MetaParser.Models;

namespace MetaParser.WPF.ViewModels
{
    public class NavNodePauseViewModel : NavNodeViewModel
    {
        public NavNodePauseViewModel(NavNodePause navNode) : base(navNode) { }

        public double Pause
        {
            get => ((NavNodePause)Node).Data;
            set
            {
                if (Pause != value)
                {
                    ((NavNodePause)Node).Data = value;
                    OnPropertyChanged(nameof(Pause));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
