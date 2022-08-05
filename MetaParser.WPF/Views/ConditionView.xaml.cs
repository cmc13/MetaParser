using MetaParser.WPF.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;

namespace MetaParser.WPF.Views
{
    /// <summary>
    /// Interaction logic for ConditionView.xaml
    /// </summary>
    public partial class ConditionView : UserControl
    {
        private Window windowReference = null;
        private EventHandler locationHandler = null;

        public ConditionView()
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

        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement map && e.ClickCount == 2)
            {
                var pos = e.GetPosition(map);
                var vm = map.DataContext as LandBlockConditionViewModel;
                var lbb = new System.Windows.Point(pos.X, map.ActualHeight - pos.Y);
                vm.MapClickCommand.Execute((pos.X / map.ActualWidth, 1 - pos.Y / map.ActualHeight));
                e.Handled = true;
            }
        }

        private void txtLandblock_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is WatermarkTextBox txt && txt.DataContext is LandBlockConditionViewModel lbb)
            {
                lbb.ShowMap = true;
            }
        }

        private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Canvas canvas)
            {
                var dc = canvas.DataContext as LandBlockConditionViewModel;
                var position = e.GetPosition(canvas);
                dc.MousePosition = (position.X / canvas.ActualWidth, 1 - (position.Y / canvas.ActualHeight));

                var tt = canvas.ToolTip as ToolTip;
                tt.Placement = PlacementMode.Relative;
                tt.HorizontalOffset = position.X + SystemParameters.CursorWidth;
                tt.VerticalOffset = position.Y + SystemParameters.CursorHeight;
                
            }
        }

        private void Grid_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && sender is FrameworkElement elem && elem.DataContext is LandBlockConditionViewModel vm)
            {
                vm.ShowMap = false;
            }
        }

        private void popMap_Opened(object sender, System.EventArgs e)
        {
            if (sender is Popup p)
            {
                windowReference = Window.GetWindow(p) ?? Application.Current.MainWindow;

                locationHandler = new EventHandler((s, e) =>
                {
                    var offset = p.HorizontalOffset;
                    p.HorizontalOffset += 0.001;
                    p.HorizontalOffset = offset;
                });

                windowReference.LocationChanged += locationHandler;
            }
        }

        private void popMap_Closed(object sender, System.EventArgs e)
        {
            if (windowReference != null && locationHandler != null)
            {
                windowReference.LocationChanged -= locationHandler;
                locationHandler = null;
                windowReference = null;
            }
        }
    }
}