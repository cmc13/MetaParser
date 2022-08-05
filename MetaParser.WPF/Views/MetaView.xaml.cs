using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Media;

namespace MetaParser.WPF.Views
{
    /// <summary>
    /// Interaction logic for MetaView.xaml
    /// </summary>
    public partial class MetaView : UserControl
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetDoubleClickTime();

        public MetaView()
        {
            InitializeComponent();
        }

        private readonly Dictionary<object, long> tickCount = new();

        private void Grid_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var now = DateTime.Now.Ticks;

            if (!tickCount.TryGetValue(sender, out var ticks))
                ticks = 0;

            tickCount[sender] = now;

            if (DateTime.Now.Ticks - ticks < GetDoubleClickTime() * TimeSpan.TicksPerMillisecond)
            {
                var grid = sender as Grid;
                var parent = VisualTreeHelper.GetParent(grid) as Grid;
                var expander = parent.Children[0] as Expander;
                expander.IsExpanded = !expander.IsExpanded;
                tickCount.Remove(sender);
            }
        }
    }
}
