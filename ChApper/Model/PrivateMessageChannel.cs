using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using AppNetDotNet.Model;

namespace Chapper.Model
{
    class PrivateMessageChannel : IChapperCollection
    {
        public PrivateMessageChannel(Channel subscribedChannel)
        {
            if (subscribedChannel == null)
            {
                return;
            }
            displayName = "";
            items = new ThreadSaveObservableCollection<Item>();
            items.CollectionChanged += items_CollectionChanged;
            channel = subscribedChannel;

            if (channel.owner.username != AppController.Current.account.username)
            {
                displayName = "@" + channel.owner.username + ", ";
            }
            List<string> already_added_ids = new List<string>();
            if(channel.readers != null) {
                foreach(string user_id in  channel.readers.user_ids) {
                    if (user_id == AppController.Current.account.user.id || already_added_ids.Contains(user_id)) 
                    { 
                        continue; 
                    }
                    Tuple<AppNetDotNet.Model.User, AppNetDotNet.ApiCalls.ApiCallResponse> response = AppNetDotNet.ApiCalls.Users.getUserByUsernameOrId(AppController.Current.account.accessToken, user_id);
                    if(response.Item2.success) {
                        already_added_ids.Add(user_id);
                        displayName += "@" + response.Item1.username + ", ";
                    }
                }
            }
            if(channel.writers != null) {
                foreach(string user_id in  channel.writers.user_ids) {
                    if (user_id == AppController.Current.account.user.id || already_added_ids.Contains(user_id))
                    {
                        continue;
                    }
                    Tuple<AppNetDotNet.Model.User, AppNetDotNet.ApiCalls.ApiCallResponse> response = AppNetDotNet.ApiCalls.Users.getUserByUsernameOrId(AppController.Current.account.accessToken, user_id);
                    if (response.Item2.success)
                    {
                        already_added_ids.Add(user_id);
                        displayName += "@" + response.Item1.username + ", ";
                    }
                }
            }
            displayName = displayName.TrimEnd();
            displayName = displayName.TrimEnd(',');
        }

        void items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (AppController.Current.account != null)
            {
                if (AppController.Current.account.initialUpdateCompleted)
                {
                    if (e.NewItems != null)
                    {
                        foreach (Item item in e.NewItems)
                        {
                            AppController.Current.account.snarlInterface.Notify("Patter " + this.displayName, item.displayName, item.text, 10, item.avatar, "");
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return displayName;
        }

        public ThreadSaveObservableCollection<Item> items
        {
            get;
            set;
        }

        public string displayName
        {
            get;
            set;
        }

        public void UpdateItems()
        {
            // those are updated by the complete fetch within messages retrival
            return;
        }

        public void Kill()
        {
            items = null;
        }

        public Channel channel { get; set; }



        public string id
        {
            get
            {
                if (channel != null)
                {
                    return channel.id;
                }
                return "";
            }
            set
            {

            }
        }
    }
}
