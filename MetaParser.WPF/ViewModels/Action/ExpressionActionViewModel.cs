using CommunityToolkit.Mvvm.Input;
using MetaParser.Models;
using System.Diagnostics;

namespace MetaParser.WPF.ViewModels
{
    public class ExpressionActionViewModel : ActionViewModel
    {
        public ExpressionActionViewModel(ExpressionMetaAction action, MetaViewModel meta) : base(action, meta)
        { }

        public string Expression
        {
            get => ((ExpressionMetaAction)Action).Expression;
            set
            {
                if (((ExpressionMetaAction)Action).Expression != value)
                {
                    ((ExpressionMetaAction)Action).Expression = value;
                    OnPropertyChanged(nameof(Expression));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        public RelayCommand LaunchExpressionHelpCommand => new(() =>
        {
            var psi = new ProcessStartInfo()
            {
                FileName = "http://virindi.net/wiki/index.php/Meta_Expressions",
                UseShellExecute = true
            };
            Process.Start(psi);
        });
    }
}
