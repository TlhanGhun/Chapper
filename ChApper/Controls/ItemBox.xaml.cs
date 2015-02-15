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
    public partial class ItemBox : UserControl
    {
       

        public ItemBox()
        {
            InitializeComponent();
        }

        private void Grid_MouseEnter_1(object sender, MouseEventArgs e)
        {
            stackpanel_itemButtons.Visibility = System.Windows.Visibility.Visible;
        }

        private void grid_main_MouseLeave_1(object sender, MouseEventArgs e)
        {
            stackpanel_itemButtons.Visibility = System.Windows.Visibility.Collapsed;
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

        private void button_quote_Click(object sender, RoutedEventArgs e)
        {
            Item item = this.DataContext as Item;
            if (item != null)
            {
                string quoteText = string.Format("RP @{0}: \"{1}\"", item.user.username, item.text);
                AppController.Current.setPostComposeText(quoteText);
            }
        }

        private void context_click_mute_repost_Click(object sender, RoutedEventArgs e)
        {
            Item item = this.DataContext as Item;
            if (item != null)
            {
                AppNetDotNet.ApiCalls.Users.mute(AppController.Current.account.accessToken, item.inReplyToPost.user.id);
            }
        }

        private void context_click_block_repost_Click(object sender, RoutedEventArgs e)
        {
            Item item = this.DataContext as Item;
            if (item != null)
            {
                AppNetDotNet.ApiCalls.Users.block(AppController.Current.account.accessToken, item.inReplyToPost.user.id);
            }
        }

        private void context_click_mute_Click(object sender, RoutedEventArgs e)
        {
            Item item = this.DataContext as Item;
            if (item != null)
            {
                AppNetDotNet.ApiCalls.Users.mute(AppController.Current.account.accessToken, item.user.id);
            }
        }

        private void context_click_block_Click(object sender, RoutedEventArgs e)
        {
            Item item = this.DataContext as Item;
            if (item != null)
            {
                AppNetDotNet.ApiCalls.Users.block(AppController.Current.account.accessToken, item.user.id);
            }
        }

        private void context_click_mute_receiver_Click(object sender, RoutedEventArgs e)
        {
            Item item = this.DataContext as Item;
            if (item != null)
            {
                AppNetDotNet.ApiCalls.Users.mute(AppController.Current.account.accessToken, item.messageReceiver.id);
            }
        }

        private void context_click_block_receiver_Click(object sender, RoutedEventArgs e)
        {
            Item item = this.DataContext as Item;
            if (item != null)
            {
                AppNetDotNet.ApiCalls.Users.block(AppController.Current.account.accessToken, item.messageReceiver.id);
            }
        }

        private void buttonMessageReceiver_Click(object sender, RoutedEventArgs e)
        {
            Item item = this.DataContext as Item;
            if (item != null)
            {
                AppController.Current.showUserInfo(item.messageReceiver);
            }
        }

        private void button_delete_Click(object sender, RoutedEventArgs e)
        {
            Item item = this.DataContext as Item;
            if (item != null)
            {
                if (item.apnMessage != null)
                {
                    Messages.delete(AppController.Current.account.accessToken, item.apnMessage.channel_id, item.apnMessage.id);
                    try
                    {
                        if (AppController.Current.account.privateMessages.Contains(item))
                        {
                            AppController.Current.account.privateMessages.Remove(item);
                        }
                        else
                        {
                            IChapperCollection collection = AppController.Current.account.allNonPrivateMessagesChannels.Where(i => i.id == item.apnMessage.channel_id).First();
                            if (collection != null)
                            {
                                collection.items.Remove(item);
                            }
                        }
                    }
                    catch { }
                }
                else if (item.apnPost != null)
                {
                    Posts.delete(AppController.Current.account.accessToken, item.apnPost.id);
                    try
                    {
                        if (AppController.Current.account.myStream.Contains(item))
                        {
                            AppController.Current.account.myStream.Remove(item);
                        }
                        if (AppController.Current.account.mentions.Contains(item))
                        {
                            AppController.Current.account.mentions.Remove(item);
                        }
                    }
                    catch { }
                }
            }
        }
    }
}
