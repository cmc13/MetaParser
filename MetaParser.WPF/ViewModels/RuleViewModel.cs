using MetaParser.Models;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Linq;

namespace MetaParser.WPF.ViewModels
{
    public class RuleViewModel : BaseViewModel
    {
        private ConditionViewModel condition;
        private ActionViewModel action;
        public event EventHandler StateChanged;

        public RuleViewModel(Rule rule, MetaViewModel meta)
        {
            Rule = rule;
            Meta = meta;
            condition = ConditionViewModelFactory.CreateViewModel(rule.Condition);
            action = ActionViewModelFactory.CreateViewModel(rule.Action, meta);
            action.StateChanged += Action_StateChanged;
            condition.PropertyChanged += Condition_PropertyChanged;
            action.PropertyChanged += Action_PropertyChanged;
            if (condition is MultipleConditionViewModel mc)
            {
                mc.ConditionList.CollectionChanged += ConditionList_CollectionChanged;
            }
            if (action is AllActionViewModel ama)
            {
                ama.ActionList.CollectionChanged += ActionList_CollectionChanged;
            }

            InvertConditionCommand = new(() =>
            {
                if (condition != null)
                    condition.PropertyChanged -= Condition_PropertyChanged;
                if (condition.Condition is NotCondition nc)
                {
                    condition = ConditionViewModelFactory.CreateViewModel(nc.Data);
                    IsDirty = true;
                    OnPropertyChanged(nameof(Condition));
                    OnPropertyChanged(nameof(SelectedConditionType));
                }
                else
                {
                    var notCond = Models.Condition.CreateCondition(ConditionType.Not) as NotCondition;
                    notCond.Data = Condition.Condition;
                    condition = ConditionViewModelFactory.CreateViewModel(notCond);
                    IsDirty = true;
                    OnPropertyChanged(nameof(Condition));
                    OnPropertyChanged(nameof(SelectedConditionType));
                }
                if (condition != null)
                    condition.PropertyChanged += Condition_PropertyChanged;
            });

            WrapConditionInAllCommand = new(() =>
            {
                if (condition != null)
                    condition.PropertyChanged -= Condition_PropertyChanged;
                var allCond = Models.Condition.CreateCondition(ConditionType.All) as MultipleCondition;
                allCond.Data.Add(Condition.Condition);
                condition = ConditionViewModelFactory.CreateViewModel(allCond);
                IsDirty = true;
                OnPropertyChanged(nameof(Condition));
                OnPropertyChanged(nameof(SelectedConditionType));
                UnwrapConditionCommand.NotifyCanExecuteChanged();
                condition.PropertyChanged += Condition_PropertyChanged;
            });

            WrapConditionInAnyCommand = new(() =>
            {
                if (condition != null)
                    condition.PropertyChanged -= Condition_PropertyChanged;
                var anyCond = Models.Condition.CreateCondition(ConditionType.Any) as MultipleCondition;
                anyCond.Data.Add(Condition.Condition);
                condition = ConditionViewModelFactory.CreateViewModel(anyCond);
                IsDirty = true;
                OnPropertyChanged(nameof(Condition));
                OnPropertyChanged(nameof(SelectedConditionType));
                UnwrapConditionCommand.NotifyCanExecuteChanged();
                condition.PropertyChanged += Condition_PropertyChanged;
            });

            UnwrapConditionCommand = new(() =>
            {
                if (condition.Condition is MultipleCondition mc && mc.Data.Count == 1)
                {
                    condition.PropertyChanged -= Condition_PropertyChanged;
                    condition = ConditionViewModelFactory.CreateViewModel(mc.Data[0]);
                    IsDirty = true;
                    OnPropertyChanged(nameof(Condition));
                    OnPropertyChanged(nameof(SelectedConditionType));
                    UnwrapConditionCommand.NotifyCanExecuteChanged();
                    condition.PropertyChanged += Condition_PropertyChanged;
                }
            }, () => Condition is MultipleConditionViewModel mc && mc.ConditionList.Count == 1);

            UnwrapActionCommand = new(() =>
            {
                if (action.Action is AllMetaAction ama && ama.Data.Count == 1)
                {
                    action.PropertyChanged -= Action_PropertyChanged;
                    action.StateChanged -= Action_StateChanged;
                    action = ActionViewModelFactory.CreateViewModel(ama.Data[0], Meta);
                    IsDirty = true;
                    OnPropertyChanged(nameof(Action));
                    OnPropertyChanged(nameof(SelectedActionType));
                    UnwrapActionCommand.NotifyCanExecuteChanged();
                    action.StateChanged += Action_StateChanged;
                    action.PropertyChanged += Action_PropertyChanged;
                }
            }, () => Action is AllActionViewModel ama && ama.ActionList.Count == 1);

            WrapActionInAllCommand = new(() =>
            {
                if (action != null)
                {
                    action.StateChanged -= Action_StateChanged;
                    action.PropertyChanged -= Action_PropertyChanged;
                }
                var allCond = Models.MetaAction.CreateMetaAction(ActionType.Multiple) as AllMetaAction;
                allCond.Data.Add(Action.Action);
                action = ActionViewModelFactory.CreateViewModel(allCond, Meta);
                IsDirty = true;
                OnPropertyChanged(nameof(Action));
                OnPropertyChanged(nameof(SelectedActionType));
                UnwrapActionCommand.NotifyCanExecuteChanged();
                action.StateChanged += Action_StateChanged;
                action.PropertyChanged += Action_PropertyChanged;
            });
        }

        private void ActionList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UnwrapActionCommand.NotifyCanExecuteChanged();
        }

        private void ConditionList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UnwrapConditionCommand.NotifyCanExecuteChanged();
        }

        private void Action_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ActionViewModel.IsDirty))
            {
                OnPropertyChanged(nameof(IsDirty));
            }
            else if (e.PropertyName == nameof(ActionViewModel.IsValid))
            {
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private void Condition_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ConditionViewModel.IsDirty))
            {
                OnPropertyChanged(nameof(IsDirty));
            }
        }

        private void Action_StateChanged(object sender, System.EventArgs e) => StateChanged?.Invoke(this, EventArgs.Empty);

        public string State
        {
            get => Rule.State;
            set
            {
                if (Rule.State != value)
                {
                    var args = new StateChangedEventArgs
                    {
                        OldState = Rule.State,
                        NewState = value
                    };
                    Rule.State = value;
                    OnPropertyChanged(nameof(State));
                    StateChanged?.Invoke(this, args);
                    IsDirty = true;
                }
            }
        }

        public RelayCommand InvertConditionCommand { get; }

        public RelayCommand WrapConditionInAllCommand { get; }

        public RelayCommand WrapConditionInAnyCommand { get; }

        public RelayCommand WrapActionInAllCommand { get; }

        public RelayCommand UnwrapConditionCommand { get; }

        public RelayCommand UnwrapActionCommand { get; }

        public MetaViewModel Meta { get; }

        public ConditionViewModel Condition => condition;

        public override bool IsDirty
        {
            get => base.IsDirty || condition.IsDirty || action.IsDirty;
            set => base.IsDirty = value;
        }

        public bool IsValid => Action.IsValid;

        public ConditionType SelectedConditionType
        {
            get => Condition.Type;
            set
            {
                if (Condition.Type != value)
                {
                    if (condition != null)
                        condition.PropertyChanged -= Condition_PropertyChanged;
                    Rule.Condition = Models.Condition.CreateCondition(value);
                    condition = ConditionViewModelFactory.CreateViewModel(Rule.Condition);
                    IsDirty = true;
                    OnPropertyChanged(nameof(Condition));
                    OnPropertyChanged(nameof(SelectedConditionType));
                    UnwrapConditionCommand.NotifyCanExecuteChanged();
                    condition.PropertyChanged += Condition_PropertyChanged;
                }
            }
        }

        public ActionType SelectedActionType
        {
            get => Action.Type;
            set
            {
                if (Action.Type != value)
                {
                    action.StateChanged -= Action_StateChanged;
                    action.PropertyChanged -= Action_PropertyChanged;
                    Rule.Action = MetaAction.CreateMetaAction(value);
                    action = ActionViewModelFactory.CreateViewModel(Rule.Action, Meta);
                    action.StateChanged += Action_StateChanged;
                    action.PropertyChanged += Action_PropertyChanged;
                    IsDirty = true;
                    OnPropertyChanged(nameof(Action));
                    OnPropertyChanged(nameof(SelectedActionType));
                    UnwrapActionCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public ActionViewModel Action => action;

        public Rule Rule { get; }

        public override void Clean()
        {
            base.Clean();
            Condition.Clean();
            Action.Clean();
        }
    }
}