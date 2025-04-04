﻿using MetaParser.WPF.ViewModels;
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
        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ToolBar toolbar)
            {
                if (toolbar.Template.FindName("OverflowGrid", toolbar) is FrameworkElement overflowGrid)
                    overflowGrid.Visibility = Visibility.Collapsed;
            }
        }
    }
}
