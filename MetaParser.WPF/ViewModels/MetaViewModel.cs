using GongSolutions.Wpf.DragDrop;
using MetaParser.Models;
using MetaParser.WPF.MetaValidation;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace MetaParser.WPF.ViewModels
{
    public class MetaViewModel
        : BaseViewModel, IDropTarget
    {
        private RuleViewModel selectedRule = null;
        private bool showValidationErrors = false;
        private MetaValidationResult selectedValidationResult = null;
        private static readonly IMetaValidator validator = new AggregateMetaValidator();

        public MetaViewModel(Meta meta)
        {
            Meta = meta;

            foreach (var rule in meta.Rules)
            {
                var vm = new RuleViewModel(rule, this);
                Rules.Add(vm);
                vm.StateChanged += Vm_StateChanged;
                vm.PropertyChanged += Vm_PropertyChanged;
            }

            Rules.CollectionChanged += Rules_CollectionChanged;

            CutCommand = new AsyncRelayCommand(async () =>
            {
                await CopyCommand.ExecuteAsync(null).ConfigureAwait(false);
                Rules.Remove(SelectedRule);
            }, () => SelectedRule != null);

            CopyCommand = new AsyncRelayCommand(async () =>
            {
                using var sw = new StringWriter();
                await Formatters.MetaWriter.WriteRuleAsync(sw, SelectedRule.Rule).ConfigureAwait(false);
                var ruleText = sw.ToString();
                Clipboard.SetData(typeof(Rule).Name, ruleText);
                PasteCommand.NotifyCanExecuteChanged();
            }, () => SelectedRule != null);

            PasteCommand = new AsyncRelayCommand(async () =>
            {
                var ruleText = (string)Clipboard.GetData(typeof(Rule).Name);
                using var sr = new StringReader(ruleText);
                var rule = await Formatters.DefaultMetaReader.ReadRuleAsync(sr).ConfigureAwait(false);
                var vm = new RuleViewModel(rule, this);
                Rules.Add(vm);
                SelectedRule = vm;
            }, () => Clipboard.ContainsData(typeof(Rule).Name));

            AddCommand = new(() =>
            {
                var rule = new Rule()
                {
                    Action = MetaAction.CreateMetaAction(ActionType.None),
                    Condition = Models.Condition.CreateCondition(ConditionType.Always),
                    State = "Default"
                };

                var vm = new RuleViewModel(rule, this);
                Rules.Add(vm);
                SelectedRule = vm;
            });

            RemoveCommand = new RelayCommand(() =>
            {
                Rules.Remove(SelectedRule);
                SelectedRule = null;
            }, () => SelectedRule != null);

            MoveUpCommand = new(() =>
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
            }, () => {
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
            });

            MoveDownCommand = new(() =>
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
            }, () => {
                if (SelectedRule != null)
                {
                    var idx = Rules.IndexOf(SelectedRule);
                    for (var i = idx + 1;  i < Rules.Count; ++i)
                    {
                        if (Rules[i].State == SelectedRule.State)
                            return true;
                    }
                }
                return false;
            });

            ValidateCommand = new(() =>
            {
                var results = validator.ValidateMeta(Meta);

                ValidationResults.Clear();
                foreach (var result in results)
                {
                    ValidationResults.Add(result);
                }

                if (ValidationResults.Count == 0)
                    ShowValidationErrors = false;
            });

            CloseValidationErrorsCommand = new(() => ShowValidationErrors = false, () => ShowValidationErrors);

            MoveToStateCommand = new(state =>
            {
                SelectedRule.State = state;
            }, state =>
            {
                if (state == null)
                    return false;
                if (SelectedRule == null)
                    return false;
                if (state == SelectedState)
                    return false;
                return true;
            });

            GoToStateCommand = new(state =>
            {
                if (state == null)
                    return;

                SelectedState = state;
            });
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RuleViewModel.IsDirty))
            {
                OnPropertyChanged(nameof(IsDirty));
            }
        }

        private void Vm_StateChanged(object sender, System.EventArgs e) => OnPropertyChanged(nameof(StateList));

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

        private void Rules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (RuleViewModel r in e.NewItems)
                {
                    Meta.Rules.Add(r.Rule);
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
                if (moveIndex > dropInfo.DragInfo.SourceIndex)
                    moveIndex--;
                if (dropInfo.DragInfo.SourceIndex != moveIndex)
                    Rules.Move(dropInfo.DragInfo.SourceIndex, moveIndex);
                if (dropInfo.TargetGroup != null && source.State != (string)dropInfo.TargetGroup.Name)
                    source.State = (string)dropInfo.TargetGroup.Name;
            }
        }

        public MetaViewModel() : this(new()) { }

        public AsyncRelayCommand CutCommand { get; }

        public AsyncRelayCommand CopyCommand { get; }

        public AsyncRelayCommand PasteCommand { get; }

        public RelayCommand AddCommand { get; }

        public RelayCommand RemoveCommand { get; }

        public RelayCommand MoveUpCommand { get; }

        public RelayCommand MoveDownCommand { get; }

        public RelayCommand ValidateCommand { get; }

        public RelayCommand<string> GoToStateCommand { get; }

        public RelayCommand CloseValidationErrorsCommand { get; }

        public RelayCommand<string> MoveToStateCommand { get; }

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
}
