using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using AppNetDotNet.Model;
using AppNetDotNet.ApiCalls;

namespace Chapper.Model
{
    public class Search : IChapperCollection
    {
        public string access_token { get; set; }
        public bool initial_update_completed { get; set; }

        public bool auto_refresh { get; set; }
        public bool save_search { get; set; }

        public Search(string name, string query, string access_token)
        {
            items = new ThreadSaveObservableCollection<Item>();
            this.displayName = name;
            this.id = query;
            this.access_token = access_token;

            backgroundWorkerUpdateItems = new BackgroundWorker();
            backgroundWorkerUpdateItems.WorkerReportsProgress = true;
            backgroundWorkerUpdateItems.WorkerSupportsCancellation = true;
            backgroundWorkerUpdateItems.DoWork += backgroundWorkerUpdateItems_DoWork;
            backgroundWorkerUpdateItems.RunWorkerCompleted += backgroundWorkerUpdateItems_RunWorkerCompleted;
            backgroundWorkerUpdateItems.ProgressChanged += backgroundWorkerUpdateItems_ProgressChanged;
        }

        void backgroundWorkerUpdateItems_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e != null)
            {
                switch (e.ProgressPercentage)
                {
                    case 50:
                        Item item = e.UserState as Item;
                        if (item != null)
                        {
                            if (items.Count > 250)
                            {
                                items.RemoveAt(items.Count() - 1);
                            }

                            items.Insert(0, item);
                            
                            if (initial_update_completed)
                            {
                                AppController.Current.account.snarlInterface.Notify("Search " + id, item.displayName, item.text, 10, item.avatar, "");
                            }
                        }
                        break;
                }
            }
        }

        void backgroundWorkerUpdateItems_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            initial_update_completed = true;
        }

        void backgroundWorkerUpdateItems_DoWork(object sender, DoWorkEventArgs e)
        {
            Tuple<List<Post>, ApiCallResponse> fetched_items;
            Parameters parameter = new Parameters();
            parameter.count = 50;
            parameter.include_annotations = true;
            if (items.Count > 0)
            {
                parameter.since_id = items.Max(i => i.id_decimal).ToString();
            }
            fetched_items = Searches.search_string(access_token, id, parameter);

            if (fetched_items.Item2.success)
            {
                fetched_items.Item1.Reverse();
                foreach (Post post in fetched_items.Item1)
                {

                    if (post != null)
                    {
                        Item item = new Item(post);
                        if (item != null)
                        {
                            if (!item.apnPost.is_deleted && !string.IsNullOrEmpty(item.text))
                            {
                                if (!string.IsNullOrEmpty(item.apnPost.reply_to))
                                {
                                    Tuple<Post, ApiCallResponse> reply = Posts.getById(this.access_token, item.apnPost.reply_to);
                                    if (reply.Item2.success)
                                    {
                                        item.inReplyToPost = reply.Item1;
                                    }
                                }
                                backgroundWorkerUpdateItems.ReportProgress(50, item);
                            }
                        }

                    }
                }
                if (fetched_items.Item1 != null)
                {
                   // backgroundWorkerUpdateItems.ReportProgress(99, items.Item2);
                }
            }
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
        public string id
        {
            get;
            set;
        }
        public void UpdateItems()
        {
            if (!backgroundWorkerUpdateItems.IsBusy)
            {
                backgroundWorkerUpdateItems.RunWorkerAsync();
            }
        }
        public void Kill()
        {
            items = null;
        }
        private BackgroundWorker backgroundWorkerUpdateItems;

        
    }
}
