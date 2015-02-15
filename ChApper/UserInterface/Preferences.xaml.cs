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
    /// Interaction logic for Preferences.xaml
    /// </summary>
    public partial class Preferences : Window
    {
        private bool init_completed;

        public Preferences()
        {
            InitializeComponent();



            listbox_themes.ItemsSource = AppController.Themes;
            this.textblock_chapperNameAndVersion.Text = "This is Chapper " + UpdateCheck.Converter.prettyVersion.getNiceVersionString(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()) + " by @lighun";
            if (SnarlConnector.SnarlInterface.IsSnarlRunning())
            {
                textBlockSnarlRunningOrNot.Visibility = Visibility.Hidden;
            }

            init_completed = true;
        }

        void hyperlinkHomepage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.nymphicusapp.com/chapper/");
            }
            catch { }
        }

        private void button_save_Click_1(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = sender as Hyperlink;
            if (hyperlink != null)
            {
                try
                {
                    System.Diagnostics.Process.Start(hyperlink.NavigateUri.AbsoluteUri);
                }
                catch { }
            }
        }

        private void buttonSnarlHompeage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://snarl.fullphat.net/");
            }
            catch
            {
                
            }
        }

        private void button_switch_layout_Click_1(object sender, RoutedEventArgs e)
        {
            Themes.ThemeInfo theme = listbox_themes.SelectedItem as Themes.ThemeInfo;
            if (theme != null)
            {
                AppController.load_theme(theme);
            }
        }

        private void listbox_themes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            button_open_layout_homepage.Visibility = System.Windows.Visibility.Collapsed;
            Themes.ThemeInfo theme = listbox_themes.SelectedItem as Themes.ThemeInfo;
            if (theme != null)
            {
                if (!string.IsNullOrEmpty(theme.homepage))
                {
                    if (theme.homepage.ToLower().StartsWith("http://") || theme.homepage.ToLower().StartsWith("https://"))
                    {
                        button_open_layout_homepage.Visibility = System.Windows.Visibility.Visible;
                        button_open_layout_homepage.ToolTip = theme.homepage;
                    }
                }   
            }
        }

        private void button_open_layout_homepage_Click(object sender, RoutedEventArgs e)
        {
            Themes.ThemeInfo theme = listbox_themes.SelectedItem as Themes.ThemeInfo;
            if (theme != null)
            {
                if (!string.IsNullOrEmpty(theme.homepage))
                {
                    if (theme.homepage.ToLower().StartsWith("http://") || theme.homepage.ToLower().StartsWith("https://"))
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(theme.homepage);
                        }
                        catch { }
                    }
                }
            }
        }

        private void button_open_themes_dir_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(AppController.themesPath);
        }






    }
}
