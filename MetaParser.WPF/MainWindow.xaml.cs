using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MetaParser.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
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
