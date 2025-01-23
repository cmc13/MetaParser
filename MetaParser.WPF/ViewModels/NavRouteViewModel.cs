using MetaParser.Models;
using MetaParser.WPF.Services;

namespace MetaParser.WPF.ViewModels
{
    public class NavRouteViewModel : BaseViewModel
    {
        private readonly NavRoute nav;
        private readonly ClipboardService clipboardService;
        private BaseViewModel navViewModel;

        public NavRouteViewModel(NavRoute nav, ClipboardService clipboardService)
        {
            if (nav.Type == NavType.Follow)
                navViewModel = new NavFollowViewModel(nav.Data as NavFollow);
            else
                navViewModel = new NavNodeListViewModel(nav, clipboardService);
            this.nav = nav;

            navViewModel.PropertyChanged += NavViewModel_PropertyChanged;
        }

        private void NavViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BaseViewModel.IsDirty))
            {
                OnPropertyChanged(nameof(IsDirty));
            }
        }

        public BaseViewModel NavViewModel
        {
            get => navViewModel;
            set
            {
                if (navViewModel != value)
                {
                    navViewModel.PropertyChanged -= NavViewModel_PropertyChanged;
                    navViewModel = value;
                    OnPropertyChanged(nameof(NavViewModel));
                    OnPropertyChanged(nameof(Type));
                    IsDirty = true;
                    navViewModel.PropertyChanged += NavViewModel_PropertyChanged;
                }
            }
        }

        public string Indicator
        {
            get
            {
                if (Type == NavType.Follow)
                    return "F";
                else if (NavViewModel is NavNodeListViewModel list)
                    return list.NavNodes.Count.ToString();
                return "";
            }
        }

        public NavType Type
        {
            get => nav.Type;
            set
            {
                if (nav.Type != value)
                {
                    if (value == NavType.Follow)
                    {
                        var follow = new NavFollow();
                        nav.Data = follow;
                        NavViewModel = new NavFollowViewModel(follow);
                    }
                    else if (nav.Type == NavType.Follow)
                    {
                        nav.Data = null;
                        NavViewModel = new NavNodeListViewModel(nav, clipboardService);
                    }
                    nav.Type = value;
                }
            }
        }

        public string Display => nav.ToString();

        public override void Clean()
        {
            base.Clean();
            if (NavViewModel.IsDirty)
                NavViewModel.Clean();
        }

        public override bool IsDirty
        {
            get => base.IsDirty || NavViewModel.IsDirty;
            set => base.IsDirty = value;
        }
    }
}
