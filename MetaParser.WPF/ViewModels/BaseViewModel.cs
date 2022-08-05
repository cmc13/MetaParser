using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace MetaParser.WPF.ViewModels
{
    public class BaseViewModel : ObservableRecipient
    {
        private bool isDirty = false;

        public virtual void Clean() => IsDirty = false;

        public virtual bool IsDirty
        {
            get => isDirty;
            set
            {
                if (isDirty != value)
                {
                    isDirty = value;
                    OnPropertyChanged(nameof(IsDirty));
                }
            }
        }
    }
}
