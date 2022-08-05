using System.Windows;
using System.Windows.Controls;

namespace MetaParser.WPF.Views
{
    /// <summary>
    /// Interaction logic for NavView.xaml
    /// </summary>
    public partial class NavView : UserControl
    {
        public NavView()
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
    }
}
