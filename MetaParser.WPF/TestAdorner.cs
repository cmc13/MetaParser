using MetaParser.WPF.ViewModels;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MetaParser.WPF
{
    public class TestAdorner : Adorner
    {
        public TestAdorner(FrameworkElement image) : base(image)
        {

        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var pt = new Rect(AdornedElement.DesiredSize);
            var brush = new SolidColorBrush(Colors.Green) { Opacity = 0.2 };
            var pen = new Pen(new SolidColorBrush(Colors.DarkGreen), 1.5);
            var radius = 5.0;
            var data = ((AdornedElement as FrameworkElement).DataContext as LandBlockConditionViewModel).Data;
            var point = new Point(pt.BottomLeft.X + ((data / 0x1000000) / 256) * pt.Width, pt.BottomLeft.Y - (((data / 0x10000) % 256) / 256) * pt.Height);

            drawingContext.DrawEllipse(brush, pen, point, radius, radius);
        }
    }
}
