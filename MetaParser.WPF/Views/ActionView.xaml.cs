using System.Windows;
using System.Windows.Controls;

namespace MetaParser.WPF.Views
{
    /// <summary>
    /// Interaction logic for ActionView.xaml
    /// </summary>
    public partial class ActionView : UserControl
    {
        public ActionView()
        {
            InitializeComponent();
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ToolBar toolbar)
            {
                var overflowGrid = toolbar.Template.FindName("OverflowGrid", toolbar) as FrameworkElement;
                if (overflowGrid != null)
                    overflowGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void ComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var cmb = sender as ComboBox;
                var p = cmb.Parent as FrameworkElement;
                var txt = p.FindName("txtValue") as TextBox;
                var be = txt.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }
            catch { }
        }
    }
}
