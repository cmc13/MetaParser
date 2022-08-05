using MetaParser.Models;
using System;

namespace MetaParser.WPF.ViewModels
{
    public class DestroyAllViewsActionViewModel : ActionViewModel
    {
        public DestroyAllViewsActionViewModel(TableMetaAction action, MetaViewModel meta) : base(action, meta)
        {
            if (action.Type != ActionType.DestroyAllViews)
                throw new ArgumentException("Invalid action");
        }
    }
}
