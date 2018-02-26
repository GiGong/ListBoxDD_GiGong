using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GiGong
{
    public class GGData
    {
        // int -> "Type" you want
        public int Item { get; set; }

        public GGData(int item)
        {
            this.Item = item;
        }

        public override string ToString()
        {
            return Item.ToString();
        }
    }

    /// <summary>
    /// DragDropListBox.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ListBoxDragDrop : UserControl
    {
        List<GGData> listDrag;

        bool isDrag = false;
        bool isOut = true;
        bool isKeyDown = false;

        Point startPoint;

        public ListBox ListBoxDD
        {
            get { return box; }
            set { box = value; }
        }

        public ListBoxDragDrop()
        {
            InitializeComponent();

            box.KeyDown += (s, e) => { isKeyDown = true; };
            box.KeyUp += (s, e) => { isKeyDown = false; };
        }

        #region Event ListBox

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isKeyDown == true)
                return;

            startPoint = e.GetPosition(null);
            var listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)(box.InputHitTest(e.GetPosition(box))));

            if (listBoxItem != null)
            {
                foreach (var item in box.SelectedItems)
                {
                    if (item == listBoxItem.Content)
                    {
                        e.Handled = true;
                        break;
                    }
                }
                isDrag = true;
            }
            else
                isDrag = false;
        }

        private void ListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)(box.InputHitTest(e.GetPosition(box))));

            Vector diff = startPoint - e.GetPosition(null);

            if (isKeyDown == false && diff.X == 0 && diff.Y == 0)
            {
                box.SelectedIndex = box.Items.IndexOf(listBoxItem.Content);
            }
        }

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if ((e.LeftButton == MouseButtonState.Pressed) && (isDrag == true) && (isOut == true))
            {
                var listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)(box.InputHitTest(e.GetPosition(box))));
                if (listBoxItem == null)
                    return;

                listDrag = new List<GGData>(box.SelectedItems.Count);

                foreach (GGData item in box.Items)
                {
                    if (box.SelectedItems.Contains(item))
                        listDrag.Add(item);
                }

                DataObject data = new DataObject("GGData", listDrag);
                DragDrop.DoDragDrop(listBoxItem, data, DragDropEffects.Move);
            }
        }

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            ScrollViewer scrollViewer = FindVisualChild<ScrollViewer>(box);

            double tolerance = 10;
            double verticalPos = e.GetPosition(box).Y;
            double offset = 1;

            if (verticalPos < tolerance) // Top of visible list?
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offset); //Scroll up.
            }
            else if (verticalPos > box.ActualHeight - tolerance) //Bottom of visible list?
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset); //Scroll down.    
            }
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("GGData"))
            {
                int index = -1;
                var target = FindAncestor<ListBoxItem>((DependencyObject)(box.InputHitTest(e.GetPosition(box))));

                if (target != null && listDrag.Contains(target.Content as GGData))
                {
                    index = box.Items.IndexOf(target.Content);
                    target = null;
                }

                foreach (var item in listDrag)
                {
                    box.Items.Remove(item);
                }

                if (target == null && index == -1)
                    // 만일 빈 곳에 Drop을 했을 경우 처리
                    index = box.Items.Count;
                else if (index == -1)
                    index = box.Items.IndexOf(target.Content);
                else if (index > box.Items.Count)
                    index = box.Items.Count;

                for (int i = 1; i <= listDrag.Count; i++)
                {
                    box.Items.Insert(index, listDrag[listDrag.Count - i]);
                }
                box.Items.Refresh();

                listDrag = null;
                isDrag = false;
            }
        }

        #endregion

        #region Event Item

        private void ListBoxItem_DragOver(object sender, DragEventArgs e)
        {
            var item = sender as ListBoxItem;

            if (listDrag.Contains(item.Content as GGData))
                return;

            item.Background = Brushes.PowderBlue;

            item.BorderThickness = (new Thickness { Top = 2 });
        }

        private void ListBoxItem_DragLeave(object sender, DragEventArgs e)
        {
            var item = sender as ListBoxItem;

            if (listDrag.Contains(item.Content as GGData))
                return;

            item.Background = Brushes.Transparent;
            item.BorderThickness = (new Thickness(0));
        }

        private void ListBoxItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var item = sender as ListBoxItem;

            Point pos = e.GetPosition(item);
            Point diff = new Point(item.ActualWidth - pos.X, item.ActualHeight - pos.Y);

            double tolerance = 2;

            if ((pos.X < tolerance) || (pos.Y < tolerance) || (diff.X < tolerance) || (diff.Y < tolerance))
                isOut = true;
            else
                isOut = false;
        }

        private void ListBoxItem_MouseEnter(object sender, MouseEventArgs e)
        {
            isOut = false;
        }

        #endregion

        #region Tree Function

        /// <summary>
        /// Helper to search up the VisualTree
        /// (https://wpftutorial.net/DragAndDrop.html)
        /// </summary>
        /// <typeparam name="T">Type to look for</typeparam>
        /// <param name="obj">start object</param>
        /// <returns></returns>
        private static T FindAncestor<T>(DependencyObject obj)
                    where T : DependencyObject
        {
            do
            {
                if (obj is T)
                {
                    return (T)obj;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
            while (obj != null);
            return null;
        }

        /// <summary>
        /// Search immediate children first (breadth-first)
        /// (https://stackoverflow.com/questions/1316251/wpf-listbox-auto-scroll-while-dragging)
        /// </summary>
        /// <typeparam name="T">Type to look for</typeparam>
        /// <param name="obj">start object</param>
        /// <returns></returns>
        private static T FindVisualChild<T>(DependencyObject obj)
                    where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child != null && child is T)
                    return (T)child;

                else
                {
                    T childOfChild = FindVisualChild<T>(child);

                    if (childOfChild != null)
                        return childOfChild;
                }
            }

            return null;
        }

        #endregion
    }
}
