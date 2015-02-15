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
using Chapper.Model;
using AppNetDotNet.Model;
using AppNetDotNet.ApiCalls;
using System.ComponentModel;

namespace Chapper.Controls
{
    /// <summary>
    /// Interaction logic for ItemBox.xaml
    /// </summary>
    public partial class InteractionBox : UserControl
    {
       

        public InteractionBox()
        {
            InitializeComponent();

            InteractionEntry interaction = this.DataContext as InteractionEntry;
            if (interaction != null)
            {
                if (interaction.apnInteraction.action == "follow")
                {
                    textblock_text.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        private void Grid_MouseEnter_1(object sender, MouseEventArgs e)
        {
            //stackpanel_itemButtons.Visibility = System.Windows.Visibility.Visible;
        }

        private void grid_main_MouseLeave_1(object sender, MouseEventArgs e)
        {
            //stackpanel_itemButtons.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void button_reply_Click_1(object sender, RoutedEventArgs e)
        {
            Item item = this.DataContext as Item;
            if (item != null)
            {
                if (item.isRepost)
                {
                    item = new Item(item.apnPost.repost_of);
                }
                if (item != null)
                {
                    AppController.Current.composeReplyTo(item);
                }
            }
        
        }

        private void button_starItem_Click_1(object sender, RoutedEventArgs e)
        {
            Item item = this.DataContext as Item;
            if (item != null)
            {
                if (!item.isRepost)
                {
                    Tuple<Post, ApiCallResponse> response;
                    if (item.isStarred)
                    {
                        response = Posts.unstar(AppController.Current.account.accessToken, item.id);
                    }
                    else
                    {
                        response = Posts.star(AppController.Current.account.accessToken, item.id);
                    }
                    if (response.Item2.success)
                    {
                        item.isStarred = !item.isStarred;
                    }
                }
                else
                {
                    Tuple<Post, ApiCallResponse> response;
                    if (item.isStarred)
                    {
                        response = Posts.unstar(AppController.Current.account.accessToken, item.apnPost.repost_of.id);
                    }
                    else
                    {
                        response = Posts.star(AppController.Current.account.accessToken, item.apnPost.repost_of.id);
                    }
                    if (response.Item2.success)
                    {
                        item.isStarred = !item.isStarred;
                    }
                }
            }
        }

        private void button_repost_Click_1(object sender, RoutedEventArgs e)
        {
            Item item = this.DataContext as Item;
            if (item != null)
            {
                if (!item.isRepost)
                {
                    Tuple<Post, ApiCallResponse> response;
                    if (item.isOwnRepost)
                    {
                        response = Posts.unrepost(AppController.Current.account.accessToken, item.id);
                    }
                    else
                    {
                        response = Posts.repost(AppController.Current.account.accessToken, item.id);
                    }
                    if (response.Item2.success)
                    {
                        item.isOwnRepost = !item.isOwnRepost;
                    }
                }
                else
                {
                    Tuple<Post, ApiCallResponse> response;
                    if (item.isOwnRepost)
                    {
                        response = Posts.unrepost(AppController.Current.account.accessToken, item.apnPost.repost_of.id);
                    }
                    else
                    {
                        response = Posts.repost(AppController.Current.account.accessToken, item.apnPost.repost_of.id);
                    }
                    if (response.Item2.success)
                    {
                        item.isOwnRepost = !item.isOwnRepost;
                    }
                }
            }
        }

        private void button_sendMessage_Click_1(object sender, RoutedEventArgs e)
        {
            Item item = this.DataContext as Item;
            if (item != null)
            {
                AppController.Current.composeMessageTo(item);
            }
        }

        private void hyperlink_source_Click_1(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            if (link != null)
            {
                try
                {
                    System.Diagnostics.Process.Start(link.NavigateUri.AbsoluteUri);
                }
                catch
                {

                }
                
            }
        }

        private void buttonAuthor_Click_1(object sender, RoutedEventArgs e)
        {
            Item item = this.DataContext as Item;
            if (item != null)
            {
                AppController.Current.showUserInfo(item.user);
            }
        }

        private void buttonAuthorRepost_Click_1(object sender, RoutedEventArgs e)
        {
            Item item = this.DataContext as Item;
            if (item != null)
            {
                if (item.isRepost && item.apnPost != null)
                {
                    AppController.Current.showUserInfo(item.apnPost.repost_of.user);
                }
            }
        }

        private void button_embeddedImage_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                try
                {
                    string url = button.CommandParameter as string;
                    System.Diagnostics.Process.Start(url);
                }
                catch { }
            }
        }

        private void button_conversation_Click_1(object sender, RoutedEventArgs e)
        {
            Item item = this.DataContext as Item;
            if (item != null)
            {
                Conversation conversation = new Conversation(item);
                AppController.Current.addNewChapperCollection(conversation, true);
            }
        }

        private void button_user_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                User user = button.DataContext as User;
                if (user != null)
                {
                    AppController.Current.showUserInfo(user.username);
                }
            }
        }
    }
}
