using MetaParser.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace MetaParser.WPF.ViewModels
{
    public class NavFollowViewModel : BaseViewModel
    {
        private readonly NavFollow navFollow;

        public NavFollowViewModel(NavFollow navFollow)
        {
            this.navFollow = navFollow;
        }

        public string TargetName
        {
            get => navFollow.TargetName;
            set
            {
                if (navFollow.TargetName != value)
                {
                    navFollow.TargetName = value;
                    OnPropertyChanged(nameof(TargetName));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public int TargetId
        {
            get => navFollow.TargetId;
            set
            {
                if (navFollow.TargetId != value)
                {
                    navFollow.TargetId = value;
                    OnPropertyChanged(nameof(TargetId));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public string Display => navFollow.ToString();
    }
}
