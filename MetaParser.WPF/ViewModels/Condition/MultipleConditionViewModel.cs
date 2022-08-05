using GongSolutions.Wpf.DragDrop;
using MetaParser.Models;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace MetaParser.WPF.ViewModels
{
    public class MultipleConditionViewModel : ConditionViewModel, IDropTarget
    {
        private readonly ObservableCollection<ConditionViewModel> conditionList = new();
        private ConditionViewModel selectedCondition;

        public MultipleConditionViewModel(MultipleCondition condition) : base(condition)
        {
            foreach (var subCondition in condition.Data)
            {
                var c = ConditionViewModelFactory.CreateViewModel(subCondition);
                conditionList.Add(c);

                c.PropertyChanged += C_PropertyChanged;
            }

            conditionList.CollectionChanged += ConditionList_CollectionChanged;

            AddCommand = new(() =>
            {
                var c = Models.Condition.CreateCondition(ConditionType.Always);
                var vm = ConditionViewModelFactory.CreateViewModel(c);
                ConditionList.Add(vm);
                SelectedCondition = vm;
            });

            RemoveCommand = new(() =>
            {
                ConditionList.Remove(SelectedCondition);
                SelectedCondition = null;
            }, () => SelectedCondition != null);

            CutCommand = new(async () =>
            {
                await CopyCommand.ExecuteAsync(null);
                RemoveCommand.Execute(null);
            }, () => SelectedCondition != null);

            CopyCommand = new(async () =>
            {
                using var sw = new StringWriter();
                await sw.WriteLineAsync(((int)SelectedCondition.Type).ToString());
                await Formatters.MetaWriter.WriteConditionAsync(sw, SelectedCondition.Condition).ConfigureAwait(false);
                var conditionText = sw.ToString();
                Clipboard.SetData(typeof(Models.Condition).Name, conditionText);
                PasteCommand.NotifyCanExecuteChanged();
            }, () => SelectedCondition != null);

            PasteCommand = new(async () =>
            {
                var conditionText = (string)Clipboard.GetData(typeof(Models.Condition).Name);
                using var sr = new StringReader(conditionText);
                var conditionType = (ConditionType)int.Parse(await sr.ReadLineAsync().ConfigureAwait(false));
                var condition = Models.Condition.CreateCondition(conditionType);
                await Formatters.DefaultMetaReader.ReadConditionAsync(sr, condition).ConfigureAwait(false);
                var vm = ConditionViewModelFactory.CreateViewModel(condition);
                ConditionList.Add(vm);
                SelectedCondition = vm;
            }, () => Clipboard.ContainsData(typeof(Models.Condition).Name));

            MoveUpCommand = new(() =>
            {
                var idx = ConditionList.IndexOf(SelectedCondition);
                if (idx > 0)
                    ConditionList.Move(idx, idx - 1);
            }, () => SelectedCondition != null && ConditionList.IndexOf(SelectedCondition) > 0);

            MoveDownCommand = new(() =>
            {
                var idx = ConditionList.IndexOf(SelectedCondition);
                if (idx < ConditionList.Count - 1)
                    ConditionList.Move(idx, idx + 1);
            }, () => SelectedCondition != null && ConditionList.IndexOf(SelectedCondition) < ConditionList.Count - 1);

            WrapInAllCommand = new(() =>
            {
                var uc = Condition as MultipleCondition;
                var idx = ConditionList.IndexOf(SelectedCondition);
                uc.Data[idx] = Models.Condition.CreateCondition(ConditionType.All) as MultipleCondition;
                ((MultipleCondition)uc.Data[idx]).Data.Add(SelectedCondition.Condition);
                ConditionList[idx] = ConditionViewModelFactory.CreateViewModel(uc.Data[idx]);
                SelectedCondition = ConditionList[idx];
                UnwrapCommand.NotifyCanExecuteChanged();
            }, () => SelectedCondition != null);

            WrapInAnyCommand = new(() =>
            {
                var uc = Condition as MultipleCondition;
                var idx = ConditionList.IndexOf(SelectedCondition);
                uc.Data[idx] = Models.Condition.CreateCondition(ConditionType.Any) as MultipleCondition;
                ((MultipleCondition)uc.Data[idx]).Data.Add(SelectedCondition.Condition);
                ConditionList[idx] = ConditionViewModelFactory.CreateViewModel(uc.Data[idx]);
                SelectedCondition = ConditionList[idx];
                UnwrapCommand.NotifyCanExecuteChanged();
            }, () => SelectedCondition != null);

            InvertCommand = new(() =>
            {
                var uc = Condition as MultipleCondition;
                var idx = ConditionList.IndexOf(SelectedCondition);
                if (SelectedCondition is NotConditionViewModel nc)
                {
                    var cond = nc.InnerCondition.Condition;
                    uc.Data[idx] = cond;
                    ConditionList[idx] = ConditionViewModelFactory.CreateViewModel(uc.Data[idx]);
                    SelectedCondition = ConditionList[idx];
                }
                else
                {
                    var cond = SelectedCondition.Condition;
                    uc.Data[idx] = Models.Condition.CreateCondition(ConditionType.Not) as NotCondition;
                    ((NotCondition)uc.Data[idx]).Data = cond;
                    ConditionList[idx] = ConditionViewModelFactory.CreateViewModel(uc.Data[idx]);
                    SelectedCondition = ConditionList[idx];
                }
            }, () => SelectedCondition != null);

            UnwrapCommand = new(() =>
            {
                if (SelectedCondition.Condition is MultipleCondition mc && mc.Data.Count == 1)
                {
                    var uc = Condition as MultipleCondition;
                    var idx = ConditionList.IndexOf(SelectedCondition);
                    uc.Data[idx] = mc.Data[0];
                    ConditionList[idx] = ConditionViewModelFactory.CreateViewModel(uc.Data[idx]);
                    SelectedCondition = ConditionList[idx];
                }
            }, () => SelectedCondition != null && SelectedCondition is MultipleConditionViewModel mc && mc.ConditionList.Count == 1);
        }

        public RelayCommand AddCommand { get; }

        public RelayCommand RemoveCommand { get; }

        public AsyncRelayCommand CutCommand { get; }

        public AsyncRelayCommand CopyCommand { get; }

        public AsyncRelayCommand PasteCommand { get; }

        public RelayCommand MoveUpCommand { get; }

        public RelayCommand MoveDownCommand { get; }

        public RelayCommand WrapInAllCommand { get; }

        public RelayCommand WrapInAnyCommand { get; }

        public RelayCommand InvertCommand { get; }

        public RelayCommand UnwrapCommand { get; }

        private void ConditionList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                var c = base.Condition as MultipleCondition;
                foreach (ConditionViewModel vm in e.NewItems)
                {
                    c.Data.Add(vm.Condition);
                    vm.PropertyChanged += C_PropertyChanged;
                }
                OnPropertyChanged(nameof(Display));
                IsDirty = true;
                WrapInAllCommand.NotifyCanExecuteChanged();
                WrapInAnyCommand.NotifyCanExecuteChanged();
                UnwrapCommand.NotifyCanExecuteChanged();
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                var c = base.Condition as MultipleCondition;
                foreach (ConditionViewModel vm in e.OldItems)
                {
                    vm.PropertyChanged -= C_PropertyChanged;
                    c.Data.Remove(vm.Condition);
                }
                OnPropertyChanged(nameof(Display));
                IsDirty = true;
                WrapInAllCommand.NotifyCanExecuteChanged();
                WrapInAnyCommand.NotifyCanExecuteChanged();
                UnwrapCommand.NotifyCanExecuteChanged();
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (ConditionViewModel vm in e.OldItems)
                    vm.PropertyChanged -= C_PropertyChanged;
                foreach (ConditionViewModel vm in e.NewItems)
                    vm.PropertyChanged += C_PropertyChanged;
                OnPropertyChanged(nameof(Display));
                IsDirty = true;
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move)
            {
                var mc = Condition as MultipleCondition;
                mc.Data.RemoveAt(e.OldStartingIndex);
                mc.Data.Insert(e.NewStartingIndex, (e.OldItems[0] as ConditionViewModel).Condition);
                OnPropertyChanged(nameof(Display));
                IsDirty = true;

                MoveUpCommand.NotifyCanExecuteChanged();
                MoveDownCommand.NotifyCanExecuteChanged();
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
        }

        public ObservableCollection<ConditionViewModel> ConditionList => conditionList;

        public ConditionViewModel SelectedCondition
        {
            get => selectedCondition;
            set
            {
                if (selectedCondition != value)
                {
                    selectedCondition = value;
                    OnPropertyChanged(nameof(SelectedCondition));
                    OnPropertyChanged(nameof(SelectedConditionType));
                    RemoveCommand.NotifyCanExecuteChanged();
                    CutCommand.NotifyCanExecuteChanged();
                    CopyCommand.NotifyCanExecuteChanged();
                    MoveUpCommand.NotifyCanExecuteChanged();
                    MoveDownCommand.NotifyCanExecuteChanged();
                    WrapInAllCommand.NotifyCanExecuteChanged();
                    WrapInAnyCommand.NotifyCanExecuteChanged();
                    UnwrapCommand.NotifyCanExecuteChanged();
                    InvertCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public ConditionType? SelectedConditionType
        {
            get => SelectedCondition?.Type;
            set
            {
                if (SelectedCondition != null && SelectedConditionType != value)
                {
                    var idx = ((MultipleCondition)Condition).Data.IndexOf(SelectedCondition.Condition);
                    ((MultipleCondition)Condition).Data[idx] = Models.Condition.CreateCondition(value.Value);
                    ConditionList[idx].PropertyChanged -= C_PropertyChanged;
                    ConditionList[idx] = ConditionViewModelFactory.CreateViewModel(((MultipleCondition)Condition).Data[idx]);
                    ConditionList[idx].PropertyChanged += C_PropertyChanged;

                    IsDirty = true;

                    SelectedCondition = ConditionList[idx];
                }
            }
        }

        public override bool IsDirty
        {
            get => base.IsDirty || ConditionList.Any(c => c.IsDirty);
            set => base.IsDirty = value;
        }

        public override void Clean()
        {
            foreach (var vm in ConditionList)
                vm.Clean();
            base.Clean();
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ConditionViewModel)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ConditionViewModel)
            {
                var moveIndex = dropInfo.InsertIndex;
                if (moveIndex > dropInfo.DragInfo.SourceIndex)
                    moveIndex--;
                if (dropInfo.DragInfo.SourceIndex != moveIndex)
                    ConditionList.Move(dropInfo.DragInfo.SourceIndex, moveIndex);
            }
        }
    }
}