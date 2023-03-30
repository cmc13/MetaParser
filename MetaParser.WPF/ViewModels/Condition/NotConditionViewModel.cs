using MetaParser.Models;

namespace MetaParser.WPF.ViewModels
{
    class NotConditionViewModel : ConditionViewModel
    {
        private ConditionViewModel vm;
        private readonly ConditionViewModelFactory conditionViewModelFactory;

        public NotConditionViewModel(NotCondition condition, ConditionViewModelFactory conditionViewModelFactory)
            :base(condition)
        {
            vm = conditionViewModelFactory.CreateViewModel(condition.Data);
            vm.PropertyChanged += Vm_PropertyChanged;
            this.conditionViewModelFactory = conditionViewModelFactory;
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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

        public ConditionType CurrentConditionType
        {
            get => vm.Type;
            set
            {
                if (vm.Type != value)
                {
                    ((NotCondition)Condition).Data = Condition.CreateCondition(value);
                    InnerCondition = conditionViewModelFactory.CreateViewModel(((NotCondition)Condition).Data);

                    OnPropertyChanged(nameof(CurrentConditionType));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public ConditionViewModel InnerCondition
        {
            get => vm;
            set
            {
                if (vm != value)
                {
                    if (vm != null)
                        vm.PropertyChanged -= Vm_PropertyChanged;
                    vm = value;
                    if (vm != null)
                        vm.PropertyChanged += Vm_PropertyChanged;
                    OnPropertyChanged(nameof(InnerCondition));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public override bool IsDirty
        {
            get => base.IsDirty || InnerCondition.IsDirty;
            set => base.IsDirty = value;
        }

        public override void Clean()
        {
            InnerCondition.Clean();
            base.Clean();
        }
    }
}
