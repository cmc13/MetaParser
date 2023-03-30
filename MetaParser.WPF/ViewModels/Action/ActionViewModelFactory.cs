using MetaParser.Models;
using MetaParser.WPF.Services;

namespace MetaParser.WPF.ViewModels;

public class ActionViewModelFactory
{
    private readonly ClipboardService clipboardService;

    public ActionViewModelFactory(ClipboardService clipboardService)
    {
        this.clipboardService = clipboardService;
    }

    public ActionViewModel CreateViewModel(MetaAction action, MetaViewModel meta) => action switch
    {
        MetaAction<int> c => new ActionViewModel<int>(c, meta),
        MetaAction<string> c => new ActionViewModel<string>(c, meta),
        ExpressionMetaAction c => new ExpressionActionViewModel(c, meta),
        SetVTOptionMetaAction c => new SetVTOptionActionViewModel(c, meta),
        GetVTOptionMetaAction c => new GetVTOptionActionViewModel(c, meta),
        CallStateMetaAction c => new CallStateActionViewModel(c, meta),
        DestroyViewMetaAction c => new DestroyViewActionViewModel(c, meta),
        CreateViewMetaAction c => new CreateViewActionViewModel(c, meta),
        AllMetaAction c => new AllActionViewModel(c, meta, this, clipboardService),
        EmbeddedNavRouteMetaAction c => new LoadEmbeddedNavRouteActionViewModel(c, meta),
        WatchdogSetMetaAction c => new WatchdogSetActionViewModel(c, meta),
        TableMetaAction c => new TableActionViewModel(c, meta),
        _ => new ActionViewModel(action, meta)
    };
}
