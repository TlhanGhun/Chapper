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
using System.Windows.Shapes;
using Chapper.Model;

namespace Chapper.UserInterface
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class Search : Window
    {
        public Search()
        {
            InitializeComponent();
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button_search_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textbox_search_string.Text))
            {
                Chapper.Model.Search search = new Model.Search(textbox_search_string.Text, textbox_search_string.Text, AppController.Current.account.accessToken);
                if (checkbox_auto_refresh.IsChecked.HasValue)
                {
                    if (checkbox_auto_refresh.IsChecked.Value)
                    {
                        search.auto_refresh = true;
                    }
                }

                if (checkbox_save_search.IsChecked.HasValue)
                {
                    if (checkbox_save_search.IsChecked.Value)
                    {
                        search.save_search = true;
                    }
                }
                search.UpdateItems();
                AppController.Current.addTabControlWithListOfItems(search.displayName, "tabSearch" + search.id, search.items, icon_source: "image_search", showCloseButton: true, switch_to_this_tab: true, datacontext:search);
                AppController.Current.account.add_search(search);
                this.Close();
            }
        }

        private void textbox_search_string_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                e.Handled = true;
                button_search_Click(null, null);
            }
        }

        private void checkbox_save_search_Checked(object sender, RoutedEventArgs e)
        {
            checkbox_auto_refresh.IsChecked = true;
        }

        private void checkbox_auto_refresh_Unchecked(object sender, RoutedEventArgs e)
        {
            checkbox_save_search.IsChecked = false;
        }
    }
}
