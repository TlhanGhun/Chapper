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
using AppNetDotNet.Model;
using AppNetDotNet.ApiCalls;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Chapper.Model;

namespace Chapper.Controls
{
    /// <summary>
    /// Interaction logic for UserInfo.xaml
    /// </summary>
    public partial class UserInfo : UserControl
    {
        public User user { get; set; }
        private BackgroundWorker backgroundWorkerRecentPosts { get; set; }
        private ObservableCollection<Item> recentPosts { get; set; }

        public UserInfo(User tobeShownUser)
        {
            InitializeComponent();
            user = tobeShownUser;
            this.DataContext = user;

            recentPosts = new ObservableCollection<Item>();
            this.listboxRecentPosts.listbox_items.ItemsSource = recentPosts;

            if (tobeShownUser.you_follow)
            {
                button_follow.Content = "Unfollow";
            }

            backgroundWorkerRecentPosts = new BackgroundWorker();
            backgroundWorkerRecentPosts.WorkerReportsProgress = true;
            backgroundWorkerRecentPosts.WorkerSupportsCancellation = true;
            backgroundWorkerRecentPosts.DoWork += backgroundWorkerRecentPosts_DoWork;
            backgroundWorkerRecentPosts.RunWorkerCompleted += backgroundWorkerRecentPosts_RunWorkerCompleted;
            backgroundWorkerRecentPosts.ProgressChanged += backgroundWorkerRecentPosts_ProgressChanged;
            backgroundWorkerRecentPosts.RunWorkerAsync();

            if (!string.IsNullOrEmpty(user.verified_domain))
            {
                button_verifiedDomain.Visibility = System.Windows.Visibility.Visible;
                textblockVerifiedDomain.Text = "Verified domain: " + user.verified_domain;
            }

        }

        #region Background worker

        #region recent posts

        void backgroundWorkerRecentPosts_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Item item = e.UserState as Item;
            if (item != null)
            {
                recentPosts.Add(item);
            }
        }

        void backgroundWorkerRecentPosts_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            backgroundWorkerRecentPosts.Dispose();
        }

        void backgroundWorkerRecentPosts_DoWork(object sender, DoWorkEventArgs e)
        {
            Tuple<List<Post>, ApiCallResponse> items;
            ParametersMyStream parameter = new ParametersMyStream();
            parameter.count = 20;
            items = Posts.getByUsernameOrId(AppController.Current.account.accessToken, user.id, parameter);
            if (items.Item2.success)
            {
                foreach (Post post in items.Item1)
                {
                    Item item = new Item(post);
                    if (item != null)
                    {
                        if (!item.apnPost.is_deleted && !string.IsNullOrEmpty(item.text))
                        {
                            backgroundWorkerRecentPosts.ReportProgress(100, item);
                        }
                    }
                }
            }
        }

        #endregion

        private void button_follow_Click_1(object sender, RoutedEventArgs e)
        {
            if (user.you_follow)
            {
                Tuple<User, ApiCallResponse> response = Users.unfollowByUsernameOrId(AppController.Current.account.accessToken, user.id);
                if (response.Item2.success)
                {
                    button_follow.Content = "Follow";
                    user.you_follow = !user.you_follow;
                    user.counts.followers--;
                    infobox_follwers.textBlockContent.Text = user.counts.followers.ToString();
                }
            }
            else
            {
                Tuple<User, ApiCallResponse> response = Users.followByUsernameOrId(AppController.Current.account.accessToken, user.id);
                if (response.Item2.success)
                {
                    user.you_follow = !user.you_follow;
                    user.counts.followers++;
                    button_follow.Content = "Unfollow";
                    infobox_follwers.textBlockContent.Text = user.counts.followers.ToString();
                }
            }
            infobox_posts.Focus();
        }

#endregion

        private void button_verifiedDomain_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://" + user.verified_domain);
            }
            catch { }
        }

        private void button_send_pm_Click(object sender, RoutedEventArgs e)
        {
            AppController.Current.composeMessageTo(user);
        }
    }
}
