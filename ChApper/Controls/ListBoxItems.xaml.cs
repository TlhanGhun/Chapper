using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Chapper.Model;

namespace Chapper.Controls
{
    /// <summary>
    /// Interaction logic for ListBoxItems.xaml
    /// </summary>
    public partial class ListBoxItems : UserControl
    {

        public ListBoxItems()
        {
            InitializeComponent();
            listbox_items.Items.SortDescriptions.Add(new SortDescription("createdAt", ListSortDirection.Descending));
        }

        public TreeViewItem CurrentlyTopMostShownItem { get; set; }

        private ScrollViewer GetScrollViewer()
        {
            Border scroll_border = VisualTreeHelper.GetChild(this, 0) as Border;
            if (scroll_border is Border)
            {
                ScrollViewer scroll = scroll_border.Child as ScrollViewer;
                if (scroll is ScrollViewer)
                {
                    return scroll;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public Item GetCurrentlyTopmostShownItem()
        {
            try
            {
                HitTestResult hitTest = VisualTreeHelper.HitTest(listbox_items, new Point(10, 10));
                TreeViewItem item = GetListViewItemFromEvent(listbox_items, hitTest.VisualHit) as TreeViewItem;
                CurrentlyTopMostShownItem = item;
                if (item != null)
                {
                    topmost_item = item.DataContext as Item;
                    return item.DataContext as Item;
                }
            }
            catch
            {
                topmost_item = null;
                return null;
            }
            topmost_item = null;
            return null;
        }

        private Item topmost_item
        {
            get;
            set;
        }
        private bool currently_maintaining_scroll_position { get; set; }
        private int last_known_number_of_items { get; set; }
        private double vertical_from_bottom { get; set; }
        private double last_known_height { get; set; }
        private double last_known_offset { get; set; }

        public DateTime? GetCurrentlyTopmostShownDateTime()
        {
            Item item = GetCurrentlyTopmostShownItem();
            if (item != null)
            {
                return item.createdAt;
            }
            else
            {
                return null;
            }
        }


        private System.Windows.Controls.TreeViewItem GetListViewItemFromEvent(object sender, object originalSource)
        {
            if (listbox_items != null && originalSource != null)
            {
                DependencyObject depObj = originalSource as DependencyObject;
                if (depObj != null)
                {
                    // go up the visual hierarchy until we find the list view item the click came from  
                    // the click might have been on the grid or column headers so we need to cater for this  
                    DependencyObject current = depObj;
                    while (current != null && current != listbox_items)
                    {
                        System.Windows.Controls.TreeViewItem ListViewItem = current as System.Windows.Controls.TreeViewItem;
                        if (ListViewItem != null)
                        {
                            return ListViewItem;
                        }
                        current = VisualTreeHelper.GetParent(current);
                    }
                }
            }
            return null;
        }

        public void ScrollItemToTop(Item item)
        {
            if (item == null) { return; }
            listbox_items.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
            {
                try
                {
                    ScrollViewer myScrollViewer = (ScrollViewer)listbox_items.Template.FindName("_tv_scrollviewer_", listbox_items);
                    if (myScrollViewer == null)
                    {
                        return;
                    }
                    double originalScrollPosition = myScrollViewer.VerticalOffset;
                    bool scrollSuccess = false;
                    myScrollViewer.ScrollToEnd();
                    myScrollViewer.UpdateLayout();

                    Item topmostItem = null;
                    

                    while (topmostItem == null || topmostItem != item)
                    {
                        if (myScrollViewer.VerticalOffset <= 0)
                        {
                            break;
                        }
                        double netScrollPosition = myScrollViewer.VerticalOffset - 10.0;
                        myScrollViewer.ScrollToVerticalOffset(Math.Max(0, netScrollPosition));
                        myScrollViewer.UpdateLayout();
                        FrameworkElement dropNode = listbox_items.InputHitTest(new Point(5, 5)) as FrameworkElement;
                        if (dropNode != null)
                        {
                            topmostItem = dropNode.DataContext as Item;
                        }
                    }

                    if (topmostItem == item)
                    {
                        scrollSuccess = true;
                        while (topmostItem == item)
                        {
                            if (myScrollViewer.VerticalOffset <= 10)
                            {
                                myScrollViewer.ScrollToVerticalOffset(0.00);
                                break;
                            }
                            double netScrollPosition = myScrollViewer.VerticalOffset - 1.0;
                            myScrollViewer.ScrollToVerticalOffset(Math.Max(0, netScrollPosition));
                            myScrollViewer.UpdateLayout();
                            FrameworkElement dropNode = listbox_items.InputHitTest(new Point(5, 1)) as FrameworkElement;
                            if (dropNode != null)
                            {
                                topmostItem = dropNode.DataContext as Item;
                            }
                        }
                    }
                    if (!scrollSuccess)
                    {
                        myScrollViewer.ScrollToVerticalOffset(originalScrollPosition);
                    }
                    
                }
                catch (Exception exp)
                {
                    Console.WriteLine(exp.Message);

                }
               

            }));
        }

        private void listbox_items_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            return;
            if(listbox_items != null) {
                if (listbox_items.Items != null)
                {
            

                    if (last_known_number_of_items != listbox_items.Items.Count)
                    {
                        last_known_number_of_items = listbox_items.Items.Count;
                        listbox_items.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
                       {
                           try
                           {

                               ScrollViewer myScrollViewer = (ScrollViewer)listbox_items.Template.FindName("_tv_scrollviewer_", listbox_items);
                               if (myScrollViewer == null)
                               {
                                   return;
                               }
                               
                               object tree_view_item = listbox_items.Items.GetItemAt(0) as TreeViewItem;
                               if (tree_view_item != null)
                               {
                                 //  tree_view_item.UpdateLayout();
                                   listbox_items.UpdateLayout();
                                //   last_known_offset = last_known_offset + tree_view_item.Height;
                                   myScrollViewer.ScrollToVerticalOffset(last_known_offset);
                               }
                               return;

                               listbox_items.UpdateLayout();
                               AppController.Current.account.snarlInterface.Notify("", "From bottom: " + vertical_from_bottom.ToString(), "New height: " + myScrollViewer.ExtentHeight.ToString() + "\nOld offset: " + myScrollViewer.VerticalOffset.ToString());
                               myScrollViewer.ScrollToVerticalOffset(myScrollViewer.ActualHeight - vertical_from_bottom);
                               AppController.Current.account.snarlInterface.Notify("", "From bottom: " + vertical_from_bottom.ToString(), "New height: " + myScrollViewer.ExtentHeight.ToString() + "\nNew offset: " + myScrollViewer.VerticalOffset.ToString());
                           }
                           catch { }
                       }));

                    }
                    else
                    {
                        ScrollViewer myScrollViewer = (ScrollViewer)listbox_items.Template.FindName("_tv_scrollviewer_", listbox_items);
                        if (myScrollViewer != null)
                        {
                            listbox_items.UpdateLayout();
                            last_known_offset = myScrollViewer.VerticalOffset;
                            return;
                            
                            // seems like there is an update before and after the event itself
                            if (last_known_height != myScrollViewer.ExtentHeight)
                            {
                                vertical_from_bottom = myScrollViewer.ExtentHeight - myScrollViewer.VerticalOffset;
                                AppController.Current.account.snarlInterface.Notify("", "From bottom: " + vertical_from_bottom.ToString(), "Full height: " + myScrollViewer.ExtentHeight.ToString() + "\nCurrent offset: " + myScrollViewer.VerticalOffset.ToString());
                                last_known_height = myScrollViewer.ExtentHeight;
                            }
                        }
                    }
                } 
            }
        }

    }
}
