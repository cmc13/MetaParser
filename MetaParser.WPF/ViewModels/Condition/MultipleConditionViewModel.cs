using CommunityToolkit.Mvvm.Input;
using GongSolutions.Wpf.DragDrop;
using MetaParser.Models;
using MetaParser.WPF.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MetaParser.WPF.ViewModels
{
    public partial class MultipleConditionViewModel : ConditionViewModel, IDropTarget
    {
        private readonly ObservableCollection<ConditionViewModel> conditionList = new();
        private ConditionViewModel selectedCondition;
        private readonly ConditionViewModelFactory conditionViewModelFactory;
        private readonly ClipboardService clipboardService;

        public MultipleConditionViewModel(MultipleCondition condition, ConditionViewModelFactory conditionViewModelFactory, ClipboardService clipboardService) : base(condition)
        {
            this.conditionViewModelFactory = conditionViewModelFactory;
            this.clipboardService = clipboardService;

            foreach (var subCondition in condition.Data)
            {
                var c = conditionViewModelFactory.CreateViewModel(subCondition);
                conditionList.Add(c);

                c.PropertyChanged += C_PropertyChanged;
            }

            conditionList.CollectionChanged += ConditionList_CollectionChanged;

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
            var c = Models.Condition.CreateCondition(ConditionType.Always);
            var vm = conditionViewModelFactory.CreateViewModel(c);
            ConditionList.Add(vm);
            SelectedCondition = vm;
        }

        [RelayCommand(CanExecute = nameof(ConditionIsSelected))]
        void Remove()
        {
            ConditionList.Remove(SelectedCondition);
            SelectedCondition = null;
        }

        private bool ConditionIsSelected() => SelectedCondition != null;

        [RelayCommand(CanExecute = nameof(ConditionIsSelected))]
        async Task Cut()
        {
            await Copy().ConfigureAwait(false);
            Remove();
        }

        [RelayCommand(CanExecute = nameof(ConditionIsSelected))]
        async Task Copy()
        {
            using var sw = new StringWriter();
            await sw.WriteLineAsync(((int)SelectedCondition.Type).ToString());
            await Formatters.MetaWriter.WriteConditionAsync(sw, SelectedCondition.Condition).ConfigureAwait(false);
            var conditionText = sw.ToString();
            clipboardService.SetData(typeof(Models.Condition).Name, conditionText);
            Application.Current.Dispatcher.Invoke(PasteCommand.NotifyCanExecuteChanged);
        }

        [RelayCommand(CanExecute = nameof(PasteCanExecute))]
        async Task Paste()
        {
            var conditionText = (string)Clipboard.GetData(typeof(Models.Condition).Name);
            using var sr = new StringReader(conditionText);
            var conditionType = (ConditionType)int.Parse(await sr.ReadLineAsync().ConfigureAwait(false));
            var condition = Models.Condition.CreateCondition(conditionType);
            await Formatters.DefaultMetaReader.ReadConditionAsync(sr, condition).ConfigureAwait(false);
            var vm = conditionViewModelFactory.CreateViewModel(condition);
            ConditionList.Add(vm);
            SelectedCondition = vm;
        }

        bool PasteCanExecute() => clipboardService.ContainsData(typeof(Models.Condition).Name);

        [RelayCommand(CanExecute = nameof(MoveUpCanExecute))]
        void MoveUp()
        {
            var idx = ConditionList.IndexOf(SelectedCondition);
            if (idx > 0)
                ConditionList.Move(idx, idx - 1);
        }

        bool MoveUpCanExecute() => SelectedCondition != null && ConditionList.IndexOf(SelectedCondition) > 0;

        [RelayCommand(CanExecute = nameof(MoveDownCanExecute))]
        void MoveDown()
        {
            var idx = ConditionList.IndexOf(SelectedCondition);
            if (idx < ConditionList.Count - 1)
                ConditionList.Move(idx, idx + 1);
        }

        bool MoveDownCanExecute() => SelectedCondition != null && ConditionList.IndexOf(SelectedCondition) < ConditionList.Count - 1;

        [RelayCommand(CanExecute = nameof(ConditionIsSelected))]
        void WrapInAll() => WrapCondition(ConditionType.All);

        [RelayCommand(CanExecute = nameof(ConditionIsSelected))]
        void WrapInAny() => WrapCondition(ConditionType.Any);

        private void WrapCondition(ConditionType type)
        {
            var uc = Condition as MultipleCondition;
            var idx = ConditionList.IndexOf(SelectedCondition);
            uc.Data[idx] = Models.Condition.CreateCondition(ConditionType.All) as MultipleCondition;
            ((MultipleCondition)uc.Data[idx]).Data.Add(SelectedCondition.Condition);
            ConditionList[idx] = conditionViewModelFactory.CreateViewModel(uc.Data[idx]);
            SelectedCondition = ConditionList[idx];
            UnwrapCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(ConditionIsSelected))]
        void Invert()
        {
            var uc = Condition as MultipleCondition;
            var idx = ConditionList.IndexOf(SelectedCondition);
            if (SelectedCondition is NotConditionViewModel nc)
            {
                var cond = nc.InnerCondition.Condition;
                uc.Data[idx] = cond;
                ConditionList[idx] = conditionViewModelFactory.CreateViewModel(uc.Data[idx]);
                SelectedCondition = ConditionList[idx];
            }
            else
            {
                var cond = SelectedCondition.Condition;
                uc.Data[idx] = Models.Condition.CreateCondition(ConditionType.Not) as NotCondition;
                ((NotCondition)uc.Data[idx]).Data = cond;
                ConditionList[idx] = conditionViewModelFactory.CreateViewModel(uc.Data[idx]);
                SelectedCondition = ConditionList[idx];
            }
        }

        [RelayCommand(CanExecute = nameof(UnwrapCanExecute))]
        void Unwrap()
        {
            if (SelectedCondition.Condition is MultipleCondition mc && mc.Data.Count == 1)
            {
                var uc = Condition as MultipleCondition;
                var idx = ConditionList.IndexOf(SelectedCondition);
                uc.Data[idx] = mc.Data[0];
                ConditionList[idx] = conditionViewModelFactory.CreateViewModel(uc.Data[idx]);
                SelectedCondition = ConditionList[idx];
            }
        }

        bool UnwrapCanExecute() => SelectedCondition != null && SelectedCondition is MultipleConditionViewModel mc && mc.ConditionList.Count == 1;

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
                    ConditionList[idx] = conditionViewModelFactory.CreateViewModel(((MultipleCondition)Condition).Data[idx]);
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