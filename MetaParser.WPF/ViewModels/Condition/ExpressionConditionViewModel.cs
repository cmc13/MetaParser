using CommunityToolkit.Mvvm.Input;
using MetaParser.Models;
using System.Diagnostics;

namespace MetaParser.WPF.ViewModels
{
    public partial class ExpressionConditionViewModel : ConditionViewModel
    {
        public ExpressionConditionViewModel(ExpressionCondition condition) : base(condition)
        {

        }

        public string Expression
        {
            get => ((ExpressionCondition)Condition).Expression;
            set
            {
                if (((ExpressionCondition)Condition).Expression != value)
                {
                    ((ExpressionCondition)Condition).Expression = value;
                    OnPropertyChanged(nameof(Expression));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                }
            }
        }

        [RelayCommand]
        void LaunchExpressionHelp()
        {
            var psi = new ProcessStartInfo()
            {
                FileName = "http://virindi.net/wiki/index.php/Meta_Expressions",
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
