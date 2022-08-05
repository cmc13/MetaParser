using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MetaParser.WPF
{
    /// <summary>
    /// Interaction logic for ControlListBox.xaml
    /// </summary>
    public partial class ControlListBox : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IList), typeof(ControlListBox), new PropertyMetadata((IList)null));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(ControlListBox), new PropertyMetadata(null));
        public static readonly DependencyProperty AddCommandProperty = DependencyProperty.Register(nameof(AddCommand), typeof(ICommand), typeof(ControlListBox), new PropertyMetadata((ICommand)null));
        public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register(nameof(RemoveCommand), typeof(ICommand), typeof(ControlListBox), new PropertyMetadata((ICommand)null));
        public static readonly DependencyProperty MoveUpCommandProperty = DependencyProperty.Register(nameof(MoveUpCommand), typeof(ICommand), typeof(ControlListBox), new PropertyMetadata((ICommand)null));
        public static readonly DependencyProperty MoveDownCommandProperty = DependencyProperty.Register(nameof(MoveDownCommand), typeof(ICommand), typeof(ControlListBox), new PropertyMetadata((ICommand)null));
        public static readonly DependencyProperty CutCommandProperty = DependencyProperty.Register(nameof(CutCommand), typeof(ICommand), typeof(ControlListBox), new PropertyMetadata((ICommand)null));
        public static readonly DependencyProperty CopyCommandProperty = DependencyProperty.Register(nameof(CopyCommand), typeof(ICommand), typeof(ControlListBox), new PropertyMetadata((ICommand)null));
        public static readonly DependencyProperty PasteCommandProperty = DependencyProperty.Register(nameof(PasteCommand), typeof(ICommand), typeof(ControlListBox), new PropertyMetadata((ICommand)null));

        public ControlListBox()
        {
            InitializeComponent();

            //DataContext = this;
        }

        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public ICommand AddCommand
        {
            get => (ICommand)GetValue(AddCommandProperty);
            set => SetValue(AddCommandProperty, value);
        }

        public ICommand RemoveCommand
        {
            get => (ICommand)GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }

        public ICommand MoveUpCommand
        {
            get => (ICommand)GetValue(MoveUpCommandProperty);
            set => SetValue(MoveUpCommandProperty, value);
        }

        public ICommand MoveDownCommand
        {
            get => (ICommand)GetValue(MoveDownCommandProperty);
            set => SetValue(MoveDownCommandProperty, value);
        }

        public ICommand CutCommand
        {
            get => (ICommand)GetValue(CutCommandProperty);
            set => SetValue(CutCommandProperty, value);
        }

        public ICommand CopyCommand
        {
            get => (ICommand)GetValue(CopyCommandProperty);
            set => SetValue(CopyCommandProperty, value);
        }

        public ICommand PasteCommand
        {
            get => (ICommand)GetValue(PasteCommandProperty);
            set => SetValue(PasteCommandProperty, value);
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
