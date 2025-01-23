using CommunityToolkit.Mvvm.Input;
using GongSolutions.Wpf.DragDrop;
using MetaParser.Models;
using MetaParser.WPF.MetaValidation;
using MetaParser.WPF.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MetaParser.WPF.ViewModels;

public partial class MetaViewModel
    : BaseViewModel, IDropTarget
{
    private RuleViewModel selectedRule = null;
    private bool showValidationErrors = false;
    private MetaValidationResult selectedValidationResult = null;
    private static readonly IMetaValidator validator = new AggregateMetaValidator();
    private readonly ConditionViewModelFactory conditionViewModelFactory;
    private readonly ActionViewModelFactory actionViewModelFactory;
    private readonly ClipboardService clipboardService;

    public MetaViewModel(ConditionViewModelFactory conditionViewModelFactory, ActionViewModelFactory actionViewModelFactory, ClipboardService clipboardService)
        : this(new(), conditionViewModelFactory, actionViewModelFactory, clipboardService)
    {}

    public MetaViewModel(Meta meta, ConditionViewModelFactory conditionViewModelFactory, ActionViewModelFactory actionViewModelFactory, ClipboardService clipboardService)
    {
        Meta = meta;
        this.conditionViewModelFactory = conditionViewModelFactory;
        this.actionViewModelFactory = actionViewModelFactory;
        this.clipboardService = clipboardService;
        Dictionary<string, List<RuleViewModel>> rulesByState = new();
        foreach (var rule in meta.Rules)
        {
            var vm = new RuleViewModel(rule, this, conditionViewModelFactory, actionViewModelFactory);

            List<RuleViewModel> list;
            if (!rulesByState.ContainsKey(rule.State))
                list = rulesByState[rule.State] = new();
            else
                list = rulesByState[rule.State];

            list.Add(vm);
            vm.StateChanged += Vm_StateChanged;
            vm.PropertyChanged += Vm_PropertyChanged;
        }

        foreach (var kv in rulesByState.OrderBy(k => k.Key))
        {
            foreach (var rule in kv.Value)
                Rules.Add(rule);
        }

        Rules.CollectionChanged += Rules_CollectionChanged;

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

    private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RuleViewModel.IsDirty))
        {
            OnPropertyChanged(nameof(IsDirty));
        }
    }

    private void Vm_StateChanged(object sender, System.EventArgs e)
    {
        if (sender is RuleViewModel rule)
        {
            Rules.Remove(rule);
            var idx = FindIndexForState(rule.State);
            Rules.Insert(idx, rule);
        }
        OnPropertyChanged(nameof(StateList));
    }

    private int FindIndexForState(string state)
    {
        for (var i = 0; i < Rules.Count; ++i)
        {
            if (state.CompareTo(Rules[i].State) < 0)
                return i;
        }
        return Rules.Count;
    }

    public override bool IsDirty
    {
        get => base.IsDirty || Rules.Any(r => r.IsDirty);
        set => base.IsDirty = value;
    }

    public RuleViewModel SelectedRule
    {
        get => selectedRule;
        set
        {
            if (selectedRule != value)
            {
                if (value == null && Rules.Contains(SelectedRule))
                    return;

                selectedRule = value;
                OnPropertyChanged(nameof(SelectedRule));
                OnPropertyChanged(nameof(SelectedState));
                OnPropertyChanged(nameof(SelectedTreeCondition));
                CutCommand.NotifyCanExecuteChanged();
                CopyCommand.NotifyCanExecuteChanged();
                RemoveCommand.NotifyCanExecuteChanged();
                MoveUpCommand.NotifyCanExecuteChanged();
                MoveDownCommand.NotifyCanExecuteChanged();
                MoveToStateCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string SelectedState
    {
        get => SelectedRule?.State;
        set
        {
            if (SelectedState != value)
            {
                var r = Rules.FirstOrDefault(r => r.State == value);
                if (r != null)
                    SelectedRule = r;
            }
        }
    }

    public string RuleCountString => $"{Rules.Count} Rule" + (Rules.Count != 1 ? "s" : "");
    public string StateListCountString => $"{StateList.Count()} State" + (StateList.Count() != 1 ? "s" : "");

    private void Rules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            foreach (RuleViewModel r in e.NewItems)
            {
                Meta.Rules.Insert(e.NewStartingIndex, r.Rule);
                r.StateChanged += Vm_StateChanged;
                r.PropertyChanged += Vm_PropertyChanged;
            }
            OnPropertyChanged(nameof(StateList));
            IsDirty = true;
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            foreach (RuleViewModel r in e.OldItems)
            {
                Meta.Rules.Remove(r.Rule);
                r.StateChanged -= Vm_StateChanged;
                r.PropertyChanged -= Vm_PropertyChanged;
            }
            OnPropertyChanged(nameof(StateList));
            IsDirty = true;
        }
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
        {
            Meta.Rules.RemoveAt(e.OldStartingIndex);
            Meta.Rules.Insert(e.NewStartingIndex, (e.OldItems[0] as RuleViewModel).Rule);
            IsDirty = true;

            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
        }
    }

    public void DragOver(IDropInfo dropInfo)
    {
        if (dropInfo.Data is RuleViewModel)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = DragDropEffects.Move;
        }
    }

    public void Drop(IDropInfo dropInfo)
    {
        if (dropInfo.Data is RuleViewModel source)
        {
            var moveIndex = dropInfo.InsertIndex;
            if (moveIndex > Rules.IndexOf(source))
                moveIndex--;
            if (dropInfo.DragInfo.SourceIndex != moveIndex)
            {
                Rules.Remove(source);
                Rules.Insert(moveIndex, source);
                SelectedRule = source;
            }
            if (dropInfo.TargetGroup != null && source.State != (string)dropInfo.TargetGroup.Name)
                source.State = (string)dropInfo.TargetGroup.Name;
        }
    }

    [RelayCommand(CanExecute = nameof(CutCanExecute))]
    async Task Cut()
    {
        await Copy().ConfigureAwait(false);
        Rules.Remove(SelectedRule);
    }

    bool CutCanExecute() => SelectedRule != null;

    [RelayCommand(CanExecute = nameof(CutCanExecute))]
    async Task Copy()
    {
        using var sw = new StringWriter();
        await Formatters.MetaWriter.WriteRuleAsync(sw, SelectedRule.Rule).ConfigureAwait(false);
        var ruleText = sw.ToString();
        clipboardService.SetData(typeof(Rule).Name, ruleText);
        Application.Current.Dispatcher.Invoke(PasteCommand.NotifyCanExecuteChanged);
    }

    [RelayCommand(CanExecute = nameof(PasteCanExecute))]
    async Task Paste()
    {
        var ruleText = (string)Clipboard.GetData(typeof(Rule).Name);
        using var sr = new StringReader(ruleText);
        var rule = await Formatters.DefaultMetaReader.ReadRuleAsync(sr).ConfigureAwait(false);
        var vm = new RuleViewModel(rule, this, conditionViewModelFactory, actionViewModelFactory);
        var idx = FindIndexForState(vm.State);
        Rules.Insert(idx, vm);
        SelectedRule = vm;
    }

    bool PasteCanExecute() => clipboardService.ContainsData(typeof(Rule).Name);

    [RelayCommand]
    void Add()
    {
        var rule = new Rule()
        {
            Action = MetaAction.CreateMetaAction(ActionType.None),
            Condition = Models.Condition.CreateCondition(ConditionType.Always),
            State = "Default"
        };

        var vm = new RuleViewModel(rule, this, conditionViewModelFactory, actionViewModelFactory);
        var idx = FindIndexForState(vm.State);
        Rules.Insert(idx, vm);
        SelectedRule = vm;
    }

    [RelayCommand(CanExecute = nameof(CutCanExecute))]
    void Remove()
    {
        Rules.Remove(SelectedRule);
        SelectedRule = null;
    }

    [RelayCommand(CanExecute = nameof(MoveUpCanExecute))]
    void MoveUp()
    {
        var idx = Rules.IndexOf(SelectedRule);
        if (idx > 0)
        {
            for (var i = idx - 1; i >= 0; --i)
            {
                if (Rules[i].State == SelectedRule.State)
                {
                    Rules.Move(idx, i);
                    break;
                }
            }
        }
    }

    bool MoveUpCanExecute()
    {
        if (SelectedRule != null)
        {
            var idx = Rules.IndexOf(SelectedRule);
            for (var i = idx - 1; i >= 0; --i)
            {
                if (Rules[i].State == SelectedRule.State)
                {
                    return true;
                }
            }
        }

        return false;
    }

    [RelayCommand(CanExecute = nameof(MoveDownCanExecute))]
    void MoveDown()
    {
        var idx = Rules.IndexOf(SelectedRule);
        if (idx < Rules.Count - 1)
        {
            for (var i = idx + 1; i < Rules.Count; ++i)
            {
                if (Rules[i].State == SelectedRule.State)
                {
                    Rules.Move(idx, i);
                    break;
                }
            }
        }
    }

    bool MoveDownCanExecute()
    {
        if (SelectedRule != null)
        {
            var idx = Rules.IndexOf(SelectedRule);
            for (var i = idx + 1; i < Rules.Count; ++i)
            {
                if (Rules[i].State == SelectedRule.State)
                    return true;
            }
        }
        return false;
    }

    [RelayCommand]
    void Validate()
    {
        var results = validator.ValidateMeta(Meta);

        ValidationResults.Clear();
        foreach (var result in results)
        {
            ValidationResults.Add(result);
        }

        if (ValidationResults.Count == 0)
            ShowValidationErrors = false;
    }

    [RelayCommand]
    void GoToState(string state)
    {
        if (state == null)
            return;

        SelectedState = state;
    }

    [RelayCommand(CanExecute = nameof(CloseValidationErrorsCanExecute))]
    void CloseValidationErrors() => ShowValidationErrors = false;

    bool CloseValidationErrorsCanExecute() => ShowValidationErrors;

    [RelayCommand(CanExecute = nameof(MoveToStateCanExecute))]
    void MoveToState(string state)
    {
        SelectedRule.State = state;
    }

    bool MoveToStateCanExecute(string state)
    {
        if (state == null)
            return false;
        if (SelectedRule == null)
            return false;
        if (state == SelectedState)
            return false;
        return true;
    }

    public ObservableCollection<RuleViewModel> Rules { get; } = new();

    public ObservableCollection<MetaValidationResult> ValidationResults { get; } = new();

    public bool ShowValidationErrors
    {
        get => showValidationErrors;
        set
        {
            if (ShowValidationErrors != value)
            {
                showValidationErrors = value;
                OnPropertyChanged(nameof(ShowValidationErrors));
                CloseValidationErrorsCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public BaseViewModel SelectedTreeAction
    {
        set
        {
            bool ContainsAction(ActionViewModel avm)
            {
                if (object.ReferenceEquals(avm, value))
                    return true;
                else if (avm is LoadEmbeddedNavRouteActionViewModel lvm)
                {
                    if (value is NavNodeListViewModel navListVm)
                    {
                        if (object.ReferenceEquals(lvm.Nav.NavViewModel, navListVm))
                            return true;
                    }
                    else if (value is NavFollowViewModel navFollowVm)
                    {
                        if (object.ReferenceEquals(lvm.Nav.NavViewModel, navFollowVm))
                            return true;
                    }
                    else if (value is NavNodeViewModel navNodeVm && lvm.Nav.NavViewModel is NavNodeListViewModel listVm)
                    {
                        var node = listVm.NavNodes.FirstOrDefault(n => object.ReferenceEquals(n, navNodeVm));
                        if (node != null)
                        {
                            listVm.SelectedNode = node;
                            return true;
                        }
                    }
                }
                else if (avm is AllActionViewModel ma)
                {
                    var a = ma.ActionList.FirstOrDefault(ContainsAction);
                    if (a != null)
                    {
                        ma.SelectedAction = a;
                        return true;
                    }
                }

                return false;
            }

            foreach (var rule in Rules)
            {
                if (ContainsAction(rule.Action))
                {
                    SelectedRule = rule;
                    break;
                }
            }
        }
    }

    public ConditionViewModel SelectedTreeCondition
    {
        set
        {
            bool ContainsCondition(ConditionViewModel cvm)
            {
                if (object.ReferenceEquals(cvm, value))
                    return true;
                else if (cvm is MultipleConditionViewModel mc)
                {
                    var c = mc.ConditionList.FirstOrDefault(ContainsCondition);
                    if (c != null)
                    {
                        mc.SelectedCondition = c;
                        return true;
                    }
                }

                return false;
            }

            foreach (var rule in Rules)
            {
                if (ContainsCondition(rule.Condition))
                {
                    SelectedRule = rule;
                    break;
                }
            }
        }
    }

    public BaseViewModel SelectedTreeNavNode
    {
        set
        {
            if (value is LoadEmbeddedNavRouteActionViewModel vm)
            {
                bool ContainsAction(ActionViewModel avm)
                {
                    if (object.ReferenceEquals(avm, value))
                        return true;
                    else if (avm is AllActionViewModel ma)
                    {
                        var a = ma.ActionList.FirstOrDefault(ContainsAction);
                        if (a != null)
                        {
                            ma.SelectedAction = a;
                            return true;
                        }
                    }

                    return false;
                }

                foreach (var rule in Rules)
                {
                    if (ContainsAction(rule.Action))
                    {
                        SelectedRule = rule;
                        break;
                    }
                }
            }
            else if (value is NavNodeListViewModel vm2)
            {
                bool ContainsNavNodeList(ActionViewModel vm)
                {
                    if (vm is LoadEmbeddedNavRouteActionViewModel navVm)
                    {
                        if (object.ReferenceEquals(vm2, navVm.Nav.NavViewModel))
                            return true;
                    }
                    else if (vm is AllActionViewModel ma)
                    {
                        var a = ma.ActionList.FirstOrDefault(ContainsNavNodeList);
                        if (a != null)
                        {
                            ma.SelectedAction = a;
                            return true;
                        }
                    }

                    return false;
                }

                foreach (var rule in Rules)
                {
                    if (ContainsNavNodeList(rule.Action))
                    {
                        SelectedRule = rule;
                        break;
                    }
                }
            }
            else if (value is NavFollowViewModel vm3)
            {
                bool ContainsNavNodeFollow(ActionViewModel vm)
                {
                    if (vm is LoadEmbeddedNavRouteActionViewModel navVm)
                    {
                        if (object.ReferenceEquals(vm3, navVm.Nav.NavViewModel))
                            return true;
                    }
                    else if (vm is AllActionViewModel ma)
                    {
                        var a = ma.ActionList.FirstOrDefault(ContainsNavNodeFollow);
                        if (a != null)
                        {
                            ma.SelectedAction = a;
                            return true;
                        }
                    }

                    return false;
                }

                foreach (var rule in Rules)
                {
                    if (ContainsNavNodeFollow(rule.Action))
                    {
                        SelectedRule = rule;
                        break;
                    }
                }
            }
            else if (value is NavNodeViewModel vm4)
            {
                bool ContainsNavNode(ActionViewModel vm)
                {
                    if (vm is LoadEmbeddedNavRouteActionViewModel navVm && navVm.Nav.NavViewModel is NavNodeListViewModel navListVm)
                    {
                        var node = navListVm.NavNodes.FirstOrDefault(n => object.ReferenceEquals(vm4, n));
                        if (node != null)
                        {
                            navListVm.SelectedNode = node;
                            return true;
                        }
                    }
                    else if (vm is AllActionViewModel ma)
                    {
                        var a = ma.ActionList.FirstOrDefault(ContainsNavNode);
                        if (a != null)
                        {
                            ma.SelectedAction = a;
                            return true;
                        }
                    }

                    return false;
                }

                foreach (var rule in Rules)
                {
                    if (ContainsNavNode(rule.Action))
                    {
                        SelectedRule = rule;
                        break;
                    }
                }
            }
        }
    }

    public IEnumerable<string> StateList
    {
        get
        {
            IEnumerable<string> GetStates(ActionViewModel action)
            {
                switch (action.Type)
                {
                    case ActionType.CallState:
                        var csa = action as CallStateActionViewModel;
                        yield return csa.CallState;
                        yield return csa.ReturnState;
                        break;

                    case ActionType.Multiple:
                        var msa = action as AllActionViewModel;
                        foreach (var ac in msa.ActionList.SelectMany(a => GetStates(a)))
                            yield return ac;
                        break;

                    case ActionType.SetState:
                        var ssa = action as ActionViewModel<string>;
                        yield return ssa.Data;
                        break;

                    case ActionType.WatchdogSet:
                        var wsa = action as WatchdogSetActionViewModel;
                        yield return wsa.State;
                        break;
                }
            }

            var states = Rules.Select(r => r.State);
            states = states.Concat(Rules.SelectMany(r => GetStates(r.Action)));
            return states.Where(s => !string.IsNullOrEmpty(s)).OrderBy(s => s).Distinct();
        }
    }

    public IEnumerable<string> ViewNameList
    {
        get
        {
            IEnumerable<string> GetViewNames(ActionViewModel vm)
            {
                if (vm is CreateViewActionViewModel cvm)
                {
                    yield return cvm.ViewName;
                }
                else if (vm is DestroyViewActionViewModel dvm)
                {
                    yield return dvm.ViewName;
                }
                else if (vm is AllActionViewModel avm)
                {
                    foreach (var view in avm.ActionList.SelectMany(a => GetViewNames(a)))
                        yield return view;
                }
            }

            var views = Rules.SelectMany(r => GetViewNames(r.Action));
            return views.Where(s => !string.IsNullOrEmpty(s)).Distinct();
        }
    }

    public Meta Meta { get; }

    public MetaValidationResult SelectedValidationResult
    {
        get => selectedValidationResult;
        set
        {
            if (selectedValidationResult != value)
            {
                selectedValidationResult = value;
                OnPropertyChanged(nameof(SelectedValidationResult));

                if (SelectedValidationResult?.Rule != null)
                {
                    var item = Rules.FirstOrDefault(r => r.Rule == SelectedValidationResult.Rule);
                    if (item != null)
                        SelectedRule = item;
                }
            }
        }
    }

    public override void Clean()
    {
        base.Clean();
        foreach (var rule in Rules)
            rule.Clean();
    }
}
