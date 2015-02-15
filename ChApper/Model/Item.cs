using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppNetDotNet.ApiCalls;
using AppNetDotNet.Model;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;

namespace Chapper.Model
{
    public class Item : INotifyPropertyChanged
    {
        public bool isOwnItem { get; set; }
        public bool hasReceiver { get; set; }
        public string displayName { get; set; }
        public string repostDisplayName { get; set; }
        public string text { get; set; }
        public string avatar { get; set; }
        public string mainAvatar
        {
            get
            {
                if (apnPost != null && isRepost)
                {

                    if (apnPost.user != null)
                    {
                        if (apnPost.user.avatar_image != null)
                        {
                            return apnPost.repost_of.user.avatar_image.url;
                        }

                    }
                }
                return avatar;
            }
        }
        public string id { get; set; }
        public decimal id_decimal
        {
            get
            {
                decimal id_as_decimal = 0;
                decimal.TryParse(id, out id_as_decimal);

                return id_as_decimal;
            }
        }
        public User messageReceiver { get; set; }
        public DateTime createdAt
        {
            get
            {
                return _createdAt;
            }
            set
            {
                displayDate = String.Format("{0:F}", value);
                _createdAt = value;
                if (timerHumanReadableAgo != null)
                {
                    timerHumanReadableAgo.Dispose();
                }
                if (!backgroundWorkerTimeAgo.IsBusy)
                {
                    backgroundWorkerTimeAgo.RunWorkerAsync(new backgroundWorkerTimeAgoArgument(value, null));
                }
                
            }
        }
        private DateTime _createdAt { get; set; }
        public string language { get; set; }
        public bool isPrivateMessage { get; set; }
        public bool isConversation
        {
            get
            {
                if (this.apnMessage != null)
                {
                    return true;
                }
                else
                {
                    if (this.apnPost != null)
                    {
                        return (this.apnPost.reply_to != null);
                    }
                }
                
                return false;
            }
        }
        public string tooltip
        {
            get
            {
                if (inReplyToPost != null)
                {
                    return string.Format("in reply to:\r\n{0}\r\n{1}", inReplyToPost.text, inReplyToPost.created_at.ToLongDateString());
                }
                return null;
                //return string.Format("@{0}: {1}", this.user.username, this.text);
            }
        }
        public AppNetDotNet.Model.Post inReplyToPost { get; set; }
        public bool isRepost { get; set; }
        public User user { get; set; }
        public Message apnMessage { get; set; }
        public Post apnPost { get; set; }
        public Entities entities { get; set; }
        public Application source { get; set; }
        public ObservableCollection<AppNetDotNet.Model.Annotations.EmbeddedMedia> imagesInPost
        {
            get
            {
                return _imagesInPost;
            }
            set
            {
                _imagesInPost = value;
            }
            }
        private ObservableCollection<AppNetDotNet.Model.Annotations.EmbeddedMedia> _imagesInPost { get; set; }
        public bool hasEmbeddedImages { get; set; }
        public string displayDate { get; set; }
        public string description { get; set; }

        public bool isStarred
        {
            get
            {
                return _isStarred;
            }
            set
            {
                _isStarred = value;
                NotifyPropertyChanged("isStarred");
            }
        }
        private bool _isStarred { get; set; }

        public bool isOwnRepost
        {
            get
            {
                return _isOwnRepost;
            }
            set
            {
                _isOwnRepost = value;
                NotifyPropertyChanged("isOwnRepost");
            }
        }
        private bool _isOwnRepost { get; set; }

        private BackgroundWorker backgroundWorkerTimeAgo;
        private Timer timerHumanReadableAgo;

        

        public Item(Post post) {

            backgroundWorkerTimeAgo = new BackgroundWorker();
            backgroundWorkerTimeAgo.WorkerReportsProgress = false;
            backgroundWorkerTimeAgo.WorkerSupportsCancellation = true;
            backgroundWorkerTimeAgo.DoWork += new DoWorkEventHandler(backgroundWorkerTimeAgo_DoWork);
            backgroundWorkerTimeAgo.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerTimeAgo_RunWorkerCompleted); 

            this.displayName = post.user.name + " (@" + post.user.username + ")";
            this.text = post.text;
            this.id = post.id;
            this.avatar = post.user.avatar_image.url;
            this.description = post.ToString();
            this.isStarred = post.you_starred;
            this.isOwnRepost = post.you_reposted;
            this.entities = post.entities;
            this.source = new Application();
            this.source.name = post.source.name;
            this.source.link = post.source.link;
            if (AppController.Current.account.username == post.user.username)
            {
                isOwnItem = true;
            }
            if (post.repost_of != null)
            {
                isRepost = true;
                text = post.repost_of.text;
                this.repostDisplayName = post.repost_of.user.name + " (@" + post.repost_of.user.username + ")";
                this.entities = post.repost_of.entities;
                scanForImages(post.repost_of.annotations);
                scanForLanguage(post.repost_of.annotations);
            }
            else
            {
                scanForImages(post.annotations);
                scanForLanguage(post.annotations);
            }
            user = post.user;
            apnPost = post;
            this.createdAt = post.created_at;
            
            
        }

        public Item(Message message)
        {

            backgroundWorkerTimeAgo = new BackgroundWorker();
            backgroundWorkerTimeAgo.WorkerReportsProgress = false;
            backgroundWorkerTimeAgo.WorkerSupportsCancellation = true;
            backgroundWorkerTimeAgo.DoWork += new DoWorkEventHandler(backgroundWorkerTimeAgo_DoWork);
            backgroundWorkerTimeAgo.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerTimeAgo_RunWorkerCompleted); 

            this.apnMessage = message;
            this.displayName = message.user.name + " (@" + message.user.username + ")";
            this.text = message.text;
            this.id = message.id;
            this.avatar = message.user.avatar_image.url;
            
            this.description = message.ToString();
            entities = message.entities;
            this.source = message.source;
            
            scanForImages(message.annotations);
            scanForLanguage(message.annotations);
            if (AppController.Current.account.username == message.user.username)
            {
                isOwnItem = true;
                Tuple<Channel, ApiCallResponse> channel_response = Channels.get(AppController.Current.account.accessToken,message.channel_id);
                if(channel_response.Item2.success) {
                   
                        foreach (string user_id in channel_response.Item1.writers.user_ids)
                        {
                            if (user_id == AppController.Current.account.user.id)
                            {
                                continue;
                            }
                            Tuple<User, ApiCallResponse> response = AppNetDotNet.ApiCalls.Users.getUserByUsernameOrId(AppController.Current.account.accessToken, user_id);
                            if (response.Item2.success)
                            {
                                messageReceiver = response.Item1;
                                if (channel_response.Item1.type != "net.patter-app.room")
                                {
                                    hasReceiver = true;
                                }
                                break;
                            }
                        }
                        if (!hasReceiver)
                        {
                            foreach (string user_id in channel_response.Item1.readers.user_ids)
                            {
                                if (user_id == AppController.Current.account.user.id)
                                {
                                    continue;
                                }
                                Tuple<User, ApiCallResponse> response = AppNetDotNet.ApiCalls.Users.getUserByUsernameOrId(AppController.Current.account.accessToken, user_id);
                                if (response.Item2.success)
                                {
                                    messageReceiver = response.Item1;
                                    if (channel_response.Item1.type != "net.patter-app.room")
                                    {
                                        hasReceiver = true;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            
            
            user = message.user;

            this.createdAt = message.created_at;

        }

        public override string ToString()
        {
            return description;
        }

        private void scanForImages(List<Annotation> annotations) {
            imagesInPost = new ObservableCollection<AppNetDotNet.Model.Annotations.EmbeddedMedia>();
            if(annotations != null) {
                foreach (Annotation annotation in annotations)
                {
                    if (annotation.type == "net.app.core.oembed")
                    {
                        AppNetDotNet.Model.Annotations.EmbeddedMedia media = annotation.parsedObject as AppNetDotNet.Model.Annotations.EmbeddedMedia;
                        if (media != null)
                        {
                            if (!string.IsNullOrEmpty(media.thumbnail_url) || !string.IsNullOrEmpty(media.url))
                            {
                                if (string.IsNullOrEmpty(media.thumbnail_url))
                                {
                                    media.thumbnail_url = media.url;
                                }
                                imagesInPost.Add(media);
                                hasEmbeddedImages = true;
                            }
                        }
                    }
                }
            }
        }

        private void scanForLanguage(List<Annotation> annotations)
        {
            if (annotations != null)
            {
                foreach (Annotation annotation in annotations)
                {
                    if (annotation.type == "net.app.core.language")
                    {
                        AppNetDotNet.Model.Annotations.Language language_annotation = annotation.parsedObject as AppNetDotNet.Model.Annotations.Language;
                        if (language_annotation != null)
                        {
                            this.language = language_annotation.language;
                        }
                    }
                }

            }
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged == null)
            {
                PropertyChanged += new PropertyChangedEventHandler(Item_PropertyChanged);
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        #endregion

        #region HumanReadableAgo

        public void timerHumanReadableAgoUpdate(Object state)
        {
            try
            {
                if (!backgroundWorkerTimeAgo.IsBusy)
                {
                    backgroundWorkerTimeAgo.RunWorkerAsync(new backgroundWorkerTimeAgoArgument(createdAt, displayDate));
                }
                else
                {
                    timerHumanReadableAgo.Dispose();
                    timerHumanReadableAgo = new Timer(new TimerCallback(timerHumanReadableAgoUpdate), null, 60000, 0);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        private class backgroundWorkerTimeAgoArgument
        {
            public DateTime CreatedAt { get; set; }
            public string OldHumanReadableString { get; set; }

            public backgroundWorkerTimeAgoArgument(DateTime createdAt, string oldHumanReadableString)
            {
                CreatedAt = createdAt;
                OldHumanReadableString = oldHumanReadableString;
            }
        }

        private class backgroundWorkerTimeAgoResult
        {
            public string HumanReadableTimeAgo { get; set; }
            public int SecondsTillNextUpdate { get; set; }
            public bool StopUpdating { get; set; }

            public backgroundWorkerTimeAgoResult(string humanReadableTimeAgo, int secondsTillNextUpdate, bool stopUpdating = false)
            {
                HumanReadableTimeAgo = humanReadableTimeAgo;
                SecondsTillNextUpdate = secondsTillNextUpdate;
                StopUpdating = stopUpdating;
            }
        }

        void backgroundWorkerTimeAgo_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                int nextUpdateInMilliSeconds = 60000;
                if (e.Result != null)
                {
                    try
                    {
                        backgroundWorkerTimeAgoResult result = e.Result as backgroundWorkerTimeAgoResult;
                        displayDate = result.HumanReadableTimeAgo;
                        NotifyPropertyChanged("displayDate");
                        if (result.StopUpdating)
                        {
                            return;
                        }
                        if (result.SecondsTillNextUpdate < 1)
                        {
                            result.SecondsTillNextUpdate = 120;
                        }
                        nextUpdateInMilliSeconds = result.SecondsTillNextUpdate * 1000;

                        if (timerHumanReadableAgo != null)
                        {
                            timerHumanReadableAgo.Dispose();
                        }
                        timerHumanReadableAgo = new Timer(new TimerCallback(timerHumanReadableAgoUpdate), null, Math.Max(1000, nextUpdateInMilliSeconds), 0);
                    }
                    catch
                    {
                        displayDate = "";
                    }

                }
                else
                {
                    if (timerHumanReadableAgo != null)
                    {
                        timerHumanReadableAgo.Dispose();
                    }
                    timerHumanReadableAgo = new Timer(new TimerCallback(timerHumanReadableAgoUpdate), null, Math.Max(1000, nextUpdateInMilliSeconds), 0);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        void backgroundWorkerTimeAgo_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                backgroundWorkerTimeAgoArgument arguments;
                try
                {
                    arguments = e.Argument as backgroundWorkerTimeAgoArgument;
                }
                catch
                {
                    arguments = new backgroundWorkerTimeAgoArgument(DateTime.Now, null);
                }

                if (e.Cancel)
                {
                    return;
                }


                bool stopUpdating = false;
                int NextUpdateInXSeconds = 60;
                string output = Helper.DateHelpers.getHumanReadableAgo(arguments.CreatedAt, out NextUpdateInXSeconds, out stopUpdating);
                e.Result = new backgroundWorkerTimeAgoResult(output, NextUpdateInXSeconds, false);
                if (string.IsNullOrEmpty(arguments.OldHumanReadableString))
                {
                    e.Result = new backgroundWorkerTimeAgoResult(output, NextUpdateInXSeconds, stopUpdating);
                }
                else if (output != arguments.OldHumanReadableString)
                {
                    e.Result = new backgroundWorkerTimeAgoResult(output, NextUpdateInXSeconds, stopUpdating);
                }
                else
                {
                    e.Result = new backgroundWorkerTimeAgoResult(output, NextUpdateInXSeconds, stopUpdating);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        #endregion
    }
}
