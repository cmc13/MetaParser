using MetaParser.Models;

namespace MetaParser.WPF.ViewModels
{
    public static class NavNodeViewModelFactory
    {
        public static NavNodeViewModel CreateViewModel(NavNode node) => node switch
        {
            NavNodeChat c => new NavNodeChatViewModel(c),
            NavNodePause c => new NavNodePauseViewModel(c),
            NavNodeRecall c => new NavNodeRecallViewModel(c),
            NavNodeOpenVendor c => new NavNodeOpenVendorViewModel(c),
            NavNodePortal c => new NavNodePortalViewModel(c),
            NavNodeNPCChat c => new NavNodeNPCChatViewModel(c),
            NavNodeJump c => new NavNodeJumpViewModel(c),
            _ => new NavNodeViewModel(node)
        };
    }
}
