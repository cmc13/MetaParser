using MetaParser.Models;
using Microsoft.Toolkit.Mvvm.Input;
using System.Diagnostics;

namespace MetaParser.WPF.ViewModels
{
    public class ExpressionConditionViewModel : ConditionViewModel
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
