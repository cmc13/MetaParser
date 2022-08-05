using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;

namespace MetaParser.WPF
{
    public class ScrollIntoViewBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            base.AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            base.AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                if (listBox.SelectedItem != null)
                {
                    listBox.Dispatcher.Invoke(() =>
                    {
                        listBox.UpdateLayout();
                        if (listBox.SelectedItem != null)
                            listBox.ScrollIntoView(listBox.SelectedItem);
                    });
                }
            }
        }
    }
}
