using MetaParser.Models;
using System;

namespace MetaParser.WPF.ViewModels
{
    public class TableActionViewModel : ActionViewModel
    {
        public TableActionViewModel(TableMetaAction action, MetaViewModel meta) : base(action, meta)
        {
            if (action.Type != ActionType.DestroyAllViews && action.Type != ActionType.WatchdogClear)
                throw new ArgumentException("Invalid action");
        }
    }
}
