using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MetaParser.WPF
{
    public class OverlayGroupingBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            this.AssociatedObject.LayoutUpdated += new EventHandler(this.OnLayoutUpdated);
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            ListBoxItem topListBoxItem1;
            GroupItem topGroupItem1, topGroupItem2;
            ContentPresenter topPresenter1, topPresenter2 = null;
            double topOffset1, topOffset2 = -1;

            // find the first ListBoxItem which is at the top of the control
            var vsp = FindVisualChild<VirtualizingStackPanel>(AssociatedObject);
            var offset = vsp.VerticalOffset;
            topListBoxItem1 = this.GetItemAtMinimumYOffset<ListBoxItem>();
            if (topListBoxItem1 == null)
                return;

            // get all group items order by their distance to the top
            var groupItems = FindVisualChildren<GroupItem>(this.AssociatedObject)
                                        .OrderBy(this.GetYOffset)
                                        .ToList();

            // from the GroupItem, find the ContentPresenter on which we can apply the transform
            topGroupItem1 = FindVisualAncestor<GroupItem>(topListBoxItem1);
            var vc = FindVisualChildren<ContentPresenter>(topGroupItem1);
            topPresenter1 = vc.Last();
            topOffset1 = this.GetYOffset(topPresenter1);

            // try to find the next GroupItem and its presenter
            var index = groupItems.IndexOf(topGroupItem1);
            if (index + 1 < groupItems.Count)
            {
                topGroupItem2 = groupItems.ElementAt(index + 1);
                topPresenter2 = FindVisualChildren<ContentPresenter>(topGroupItem2).Last();
                topOffset2 = this.GetYOffset(topPresenter2);
            }

            // update transforms
            if (topOffset2 < 0 || topOffset2 > topPresenter1.ActualHeight)
            {
                this.SetGroupItemOffset(topPresenter1, topOffset1);
            }

            if (topPresenter2 != null)
                topPresenter2.RenderTransform = null;
        }

        private T GetItemAtMinimumYOffset<T>() where T : UIElement
        {
            var minOffset = double.MaxValue;
            T topItem = null;
            foreach (var item in FindVisualChildren<T>(this.AssociatedObject))
            {
                var offset = this.GetYOffset(item);
                if (Math.Abs(offset) <= Math.Abs(minOffset))
                {
                    minOffset = offset;
                    topItem = item;
                }
            }

            return topItem;
        }

        private void SetGroupItemOffset(ContentPresenter groupHeader, double offset)
        {
            if (groupHeader.RenderTransform as TranslateTransform == null)
                groupHeader.RenderTransform = new TranslateTransform();

            ((TranslateTransform)groupHeader.RenderTransform).Y -= offset;
        }

        private double GetYOffset(UIElement uiElement)
        {
            var transform = (MatrixTransform)uiElement.TransformToVisual(this.AssociatedObject as ListBox);
            return transform.Matrix.OffsetY;
        }

        /// <summary>
        /// Find all visual children of a given type
        /// </summary>
        /// <typeparam name="T">Type of the children to find</typeparam>
        /// <param name="obj">Source dependency object</param>
        /// <returns>A list of T objects that are visual children of the source dependency object</returns>
        public static List<T> FindVisualChildren<T>(DependencyObject obj) where T : DependencyObject
        {
            List<T> matches = new List<T>();
            return FindVisualChildren(obj, matches);
        }

        /// <summary>
        /// Find the first visual child of a given type
        /// </summary>
        /// <typeparam name="T">Type of the visual child to retrieve</typeparam>
        /// <param name="obj">Source dependency object</param>
        /// <returns>First child that matches the given type</returns>
        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    T childOfchild = FindVisualChild<T>(child);
                    if (childOfchild != null)
                    {
                        return childOfchild;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Find the first visual ancestor of a given type
        /// </summary>
        /// <typeparam name="T">Type of the ancestor</typeparam>
        /// <param name="element">Source visual</param>
        /// <returns>First ancestor that matches the type in the visual tree</returns>
        public static T FindVisualAncestor<T>(UIElement element) where T : class
        {
            while (element != null && !(element is T))
            {
                element = (UIElement)VisualTreeHelper.GetParent(element);
            }

            return element as T;
        }

        /// <summary>
        /// Find all visual children of a given type
        /// </summary>
        /// <typeparam name="T">Type of the children to find</typeparam>
        /// <param name="obj">Source dependency object</param>
        /// <param name="matches">List of matches</param>
        /// <returns>A list of T objects that are visual children of the source dependency object</returns>
        private static List<T> FindVisualChildren<T>(DependencyObject obj, List<T> matches) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject children = VisualTreeHelper.GetChild(obj, i);
                if (children != null && children is T)
                {
                    matches.Add((T)children);
                }
                FindVisualChildren(children, matches);
            }

            return matches;
        }
    }
}
