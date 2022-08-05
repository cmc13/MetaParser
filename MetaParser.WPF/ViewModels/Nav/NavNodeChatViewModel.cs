using MetaParser.Models;

namespace MetaParser.WPF.ViewModels
{
    public class NavNodeChatViewModel : NavNodeViewModel
    {
        public NavNodeChatViewModel(NavNodeChat node) : base(node)
        { }

        public string Chat
        {
            get => ((NavNodeChat)Node).Data;
            set
            {
                if (Chat != value)
                {
                    ((NavNodeChat)Node).Data = value;
                    OnPropertyChanged(nameof(Chat));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }
    }
}
