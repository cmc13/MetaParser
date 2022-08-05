using MetaParser.Models;

namespace MetaParser.WPF.ViewModels
{
    public class NavNodeJumpViewModel : NavNodeViewModel
    {
        public NavNodeJumpViewModel(NavNodeJump node) : base(node)
        { }

        public double Heading
        {
            get => ((NavNodeJump)Node).Data.heading;
            set
            {
                if (Heading != value)
                {
                    ((NavNodeJump)Node).Data = (value, Shift, Delay);
                    OnPropertyChanged(nameof(Heading));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public bool Shift
        {
            get => ((NavNodeJump)Node).Data.shift;
            set
            {
                if (Shift != value)
                {
                    ((NavNodeJump)Node).Data = (Heading, value, Delay);
                    OnPropertyChanged(nameof(Shift));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public double Delay
        {
            get => ((NavNodeJump)Node).Data.delay;
            set
            {
                if (Delay != value)
                {
                    ((NavNodeJump)Node).Data = (Heading, Shift, value);
                    OnPropertyChanged(nameof(Delay));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
