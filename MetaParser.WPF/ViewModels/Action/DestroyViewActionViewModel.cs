using MetaParser.Models;

namespace MetaParser.WPF.ViewModels
{
    public class DestroyViewActionViewModel : ActionViewModel
    {
        public DestroyViewActionViewModel(DestroyViewMetaAction action, MetaViewModel meta) : base(action, meta)
        { }

        public string ViewName
        {
            get => ((DestroyViewMetaAction)Action).ViewName;
            set
            {
                if (((DestroyViewMetaAction)Action).ViewName != value)
                {
                    ((DestroyViewMetaAction)Action).ViewName = value;
                    OnPropertyChanged(nameof(ViewName));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
