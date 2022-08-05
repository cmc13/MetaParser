using MetaParser.Models;
using System;
using System.Collections.Generic;

namespace MetaParser.WPF.ViewModels
{
    public static class ActionViewModelFactory
    {
        private static readonly Dictionary<Type, Type> typedict = new();

        public static ActionViewModel CreateViewModel(MetaAction action, MetaViewModel meta) => action switch
        {
            MetaAction<int> c => new ActionViewModel<int>(c, meta),
            MetaAction<string> c => new ActionViewModel<string>(c, meta),
            ExpressionMetaAction c => new ExpressionActionViewModel(c, meta),
            SetVTOptionMetaAction c => new SetVTOptionActionViewModel(c, meta),
            GetVTOptionMetaAction c => new GetVTOptionActionViewModel(c, meta),
            CallStateMetaAction c => new CallStateActionViewModel(c, meta),
            TableMetaAction c when c.Type == ActionType.DestroyAllViews => new DestroyAllViewsActionViewModel(c, meta),
            DestroyViewMetaAction c => new DestroyViewActionViewModel(c, meta),
            CreateViewMetaAction c => new CreateViewActionViewModel(c, meta),
            AllMetaAction c => new AllActionViewModel(c, meta),
            EmbeddedNavRouteMetaAction c => new LoadEmbeddedNavRouteActionViewModel(c, meta),
            WatchdogSetMetaAction c => new WatchdogSetActionViewModel(c, meta),
            _ => new ActionViewModel(action, meta)
        };
    }
}
