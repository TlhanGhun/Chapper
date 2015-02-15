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

namespace Chapper.UserInterface
{
    /// <summary>
    /// Interaction logic for Filter.xaml
    /// </summary>
    public partial class Filter : Window
    {
        public Filter()
        {
            InitializeComponent();

            combobox_language.ItemsSource = AppController.available_languages;
            combobox_language.SelectedIndex = 0;
            textbox_filter_string.Text = AppController.Current.current_filter_text;
            combobox_language.SelectedItem = AppController.Current.current_filter_language;
            checkbox_language_exclude_empty.IsChecked = AppController.Current.current_filter_language_exclude_empty;

            init_complete = true;
        }

        private bool init_complete { get; set; }

        private void textbox_filter_string_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (init_complete)
            {
                AppController.Current.set_filter_text(textbox_filter_string.Text);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        
        private void combobox_language_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init_complete)
            {
                AppController.Current.set_filter_language(combobox_language.SelectedItem as string, checkbox_language_exclude_empty.IsChecked);
            }
        }

        private void checkbox_language_exclude_empty_Checked(object sender, RoutedEventArgs e)
        {
            if (init_complete)
            {
                AppController.Current.set_filter_language(combobox_language.SelectedItem as string, checkbox_language_exclude_empty.IsChecked);
            }
        }

        private void checkbox_language_exclude_empty_Unchecked(object sender, RoutedEventArgs e)
        {
            if (init_complete)
            {
                AppController.Current.set_filter_language(combobox_language.SelectedItem as string, checkbox_language_exclude_empty.IsChecked);
            }
        }
    }
}
