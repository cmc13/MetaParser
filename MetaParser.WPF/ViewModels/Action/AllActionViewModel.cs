using CommunityToolkit.Mvvm.Input;
using GongSolutions.Wpf.DragDrop;
using MetaParser.Models;
using MetaParser.WPF.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MetaParser.WPF.ViewModels;

internal partial class AllActionViewModel : ActionViewModel, IDropTarget
{
    private readonly ObservableCollection<ActionViewModel> actionList = new();
    private ActionViewModel selectedAction;
    private readonly ActionViewModelFactory actionViewModelFactory;
    private readonly ClipboardService clipboardService;

    public AllActionViewModel(AllMetaAction action, MetaViewModel meta, ActionViewModelFactory actionViewModelFactory, ClipboardService clipboardService) : base(action, meta)
    {
        this.actionViewModelFactory = actionViewModelFactory;
        this.clipboardService = clipboardService;

        foreach (var subAction in action.Data)
        {
            var c = actionViewModelFactory.CreateViewModel(subAction, meta);
            actionList.Add(c);

            c.PropertyChanged += C_PropertyChanged;
            c.StateChanged += C_StateChanged;
        }

        actionList.CollectionChanged += ActionList_CollectionChanged;

        bool? prevValue = null;
        clipboardService.ClipboardChanged += (sender, e) =>
        {
            var canPaste = PasteCanExecute();
            if (canPaste != prevValue)
            {
                prevValue = canPaste;
                Application.Current.Dispatcher.Invoke(PasteCommand.NotifyCanExecuteChanged);
            }
        };
    }

    [RelayCommand]
    void Add()
    {
        var c = MetaAction.CreateMetaAction(ActionType.None);
        var vm = actionViewModelFactory.CreateViewModel(c, Meta);
        ActionList.Add(vm);
        SelectedAction = vm;
    }

    [RelayCommand(CanExecute = nameof(ActionIsSelected))]
    void Remove()
    {
        ActionList.Remove(SelectedAction);
        SelectedAction = null;
    }

    private bool ActionIsSelected() => SelectedAction != null;

    [RelayCommand(CanExecute = nameof(ActionIsSelected))]
    async Task Cut()
    {
        await Copy().ConfigureAwait(false);
        Remove();
    }

    [RelayCommand(CanExecute = nameof(ActionIsSelected))]
    async Task Copy()
    {
        using var sw = new StringWriter();
        await sw.WriteLineAsync(((int)SelectedAction.Type).ToString());
        await Formatters.MetaWriter.WriteActionAsync(sw, SelectedAction.Action).ConfigureAwait(false);
        var actionText = sw.ToString();
        clipboardService.SetData(typeof(MetaAction).Name, actionText);
        PasteCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(PasteCanExecute))]
    async Task Paste()
    {
        var actionText = (string)Clipboard.GetData(typeof(MetaAction).Name);
        using var sr = new StringReader(actionText);
        var actionType = (ActionType)int.Parse(await sr.ReadLineAsync().ConfigureAwait(false));
        var action = MetaAction.CreateMetaAction(actionType);
        await Formatters.DefaultMetaReader.ReadActionAsync(sr, action).ConfigureAwait(false);
        var vm = actionViewModelFactory.CreateViewModel(action, Meta);
        ActionList.Add(vm);
        SelectedAction = vm;
    }

    bool PasteCanExecute() => clipboardService.ContainsData(typeof(MetaAction).Name);

    [RelayCommand(CanExecute = nameof(MoveUpCanExecute))]
    void MoveUp()
    {
        var idx = ActionList.IndexOf(SelectedAction);
        if (idx > 0)
            ActionList.Move(idx, idx - 1);

        MoveUpCommand.NotifyCanExecuteChanged();
        MoveDownCommand.NotifyCanExecuteChanged();
    }

    bool MoveUpCanExecute() => SelectedAction != null && ActionList.IndexOf(SelectedAction) > 0;

    [RelayCommand(CanExecute = nameof(MoveDownCanExecute))]
    void MoveDown()
    {
        var idx = ActionList.IndexOf(SelectedAction);
        if (idx < ActionList.Count - 1)
            ActionList.Move(idx, idx + 1);

        MoveUpCommand.NotifyCanExecuteChanged();
        MoveDownCommand.NotifyCanExecuteChanged();
    }

    bool MoveDownCanExecute() => SelectedAction != null && ActionList.IndexOf(SelectedAction) < ActionList.Count - 1;

    [RelayCommand(CanExecute = nameof(ActionIsSelected))]
    void Wrap()
    {
        //var ua = Action as AllMetaAction;
        var idx = ActionList.IndexOf(SelectedAction);
        var a = MetaAction.CreateMetaAction(ActionType.Multiple) as AllMetaAction;
        a.Data.Add(SelectedAction.Action);
        //ua.Data[idx] = Models.MetaAction.CreateMetaAction(ActionType.Multiple) as AllMetaAction;
        //((AllMetaAction)ua.Data[idx]).Data.Add(SelectedAction.Action);
        ActionList[idx] = actionViewModelFactory.CreateViewModel(a, Meta);
        SelectedAction = ActionList[idx];
    }

    [RelayCommand(CanExecute = nameof(UnwrapCanExecute))]
    void Unwrap()
    {
        if (SelectedAction.Action is AllMetaAction ama && ama.Data.Count == 1)
        {
            //var ua = Action as AllMetaAction;
            var idx = ActionList.IndexOf(SelectedAction);
            //ua.Data[idx] = ama.Data[0];
            ActionList[idx] = actionViewModelFactory.CreateViewModel(ama.Data[0], Meta);
            SelectedAction = ActionList[idx];
        }
    }

    bool UnwrapCanExecute() => SelectedAction != null && SelectedAction is AllActionViewModel avm && avm.ActionList.Count == 1;

    private void C_StateChanged(object sender, System.EventArgs e) => OnStateChanged();

    private void ActionList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            var c = base.Action as AllMetaAction;
            foreach (ActionViewModel vm in e.NewItems)
            {
                c.Data.Add(vm.Action);
                vm.PropertyChanged += C_PropertyChanged;
                vm.StateChanged += C_StateChanged;
            }
            OnPropertyChanged(nameof(Display));
            OnPropertyChanged(nameof(IsValid));
            IsDirty = true;
            WrapCommand.NotifyCanExecuteChanged();
            UnwrapCommand.NotifyCanExecuteChanged();
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            var c = base.Action as AllMetaAction;
            foreach (ActionViewModel vm in e.OldItems)
            {
                vm.PropertyChanged -= C_PropertyChanged;
                vm.PropertyChanged -= C_PropertyChanged;
                c.Data.Remove(vm.Action);
            }
            OnPropertyChanged(nameof(Display));
            OnPropertyChanged(nameof(IsValid));
            IsDirty = true;
            WrapCommand.NotifyCanExecuteChanged();
            UnwrapCommand.NotifyCanExecuteChanged();
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
        {
            foreach (ActionViewModel vm in e.OldItems)
            {
                vm.PropertyChanged -= C_PropertyChanged;
                vm.StateChanged -= C_StateChanged;
            }

            foreach (ActionViewModel vm in e.NewItems)
            {
                vm.PropertyChanged += C_PropertyChanged;
                vm.StateChanged += C_StateChanged;
            }

            var c = base.Action as AllMetaAction;
            c.Data[e.OldStartingIndex] = ((ActionViewModel)e.NewItems[0]).Action;
            OnPropertyChanged(nameof(Display));
            IsDirty = true;
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
        {
            var mc = Action as AllMetaAction;
            mc.Data.RemoveAt(e.OldStartingIndex);
            mc.Data.Insert(e.NewStartingIndex, (e.OldItems[0] as ActionViewModel).Action);
            OnPropertyChanged(nameof(Display));
            IsDirty = true;

            MoveDownCommand.NotifyCanExecuteChanged();
            MoveUpCommand.NotifyCanExecuteChanged();
        }
    }

    private void C_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IsDirty))
        {
            OnPropertyChanged(nameof(IsDirty));
        }
        else if (e.PropertyName == nameof(Display))
        {
            OnPropertyChanged(nameof(Display));
        }
        else if (e.PropertyName == nameof(IsValid))
        {
            OnPropertyChanged(nameof(IsValid));
        }
    }

    public ObservableCollection<ActionViewModel> ActionList => actionList;

    public ActionViewModel SelectedAction
    {
        get => selectedAction;
        set
        {
            if (selectedAction != value)
            {
                selectedAction = value;
                OnPropertyChanged(nameof(SelectedAction));
                OnPropertyChanged(nameof(SelectedActionType));
                OnPropertyChanged(nameof(Display));
                RemoveCommand.NotifyCanExecuteChanged();
                CutCommand.NotifyCanExecuteChanged();
                CopyCommand.NotifyCanExecuteChanged();
                MoveUpCommand.NotifyCanExecuteChanged();
                MoveDownCommand.NotifyCanExecuteChanged();
                WrapCommand.NotifyCanExecuteChanged();
                UnwrapCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public ActionType? SelectedActionType
    {
        get => SelectedAction?.Type;
        set
        {
            if (SelectedAction != null && SelectedActionType != value)
            {
                var idx = ((AllMetaAction)Action).Data.IndexOf(SelectedAction.Action);
                SelectedAction = null;
                var a = MetaAction.CreateMetaAction(value.Value);
                ActionList[idx] = actionViewModelFactory.CreateViewModel(a, Meta);

                IsDirty = true;

                SelectedAction = ActionList[idx];
                OnPropertyChanged(nameof(IsValid));
            }
        }
    }

    public override bool IsDirty
    {
        get => base.IsDirty || ActionList.Any(c => c.IsDirty);
        set => base.IsDirty = value;
    }

    public override void Clean()
    {
        foreach (var action in ActionList)
            action.Clean();
        base.Clean();
    }

    public void DragOver(IDropInfo dropInfo)
    {
        if (dropInfo.Data is ActionViewModel)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = DragDropEffects.Move;
        }
    }

    public void Drop(IDropInfo dropInfo)
    {
        if (dropInfo.Data is ActionViewModel)
        {
            var moveIndex = dropInfo.InsertIndex;
            if (moveIndex > dropInfo.DragInfo.SourceIndex)
                moveIndex--;
            if (dropInfo.DragInfo.SourceIndex != moveIndex)
                ActionList.Move(dropInfo.DragInfo.SourceIndex, moveIndex);
        }
    }

    public override bool IsValid => ActionList.All(a => a.IsValid);
}