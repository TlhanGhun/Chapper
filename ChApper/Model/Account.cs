using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppNetDotNet.Model;
using AppNetDotNet.ApiCalls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SnarlConnector;

namespace Chapper.Model
{
    class Account : INotifyPropertyChanged
    {
        public Account()
        {
            myStream = new ThreadSaveObservableCollection<Item>();
            mentions = new ThreadSaveObservableCollection<Item>();
            privateMessages = new ThreadSaveObservableCollection<Item>();
            globalStream = new ThreadSaveObservableCollection<Item>();
            interactions = new ThreadSaveObservableCollection<InteractionEntry>();
            allNonPrivateMessagesChannels = new ThreadSaveObservableCollection<IChapperCollection>();
            patterChannels = new ThreadSaveObservableCollection<IChapperCollection>();
            privateMessagesChannels = new ThreadSaveObservableCollection<PrivateMessageChannel>();
            searches = new ThreadSaveObservableCollection<Search>();
            newestAlreadyFetchedDateTime = new DateTime(0);

            KnownChannels = new List<Channel>();
            KnownChannelIds = new List<string>();


             Tuple<Configuration, ApiCallResponse> configuration_response = Configurations.get();

             if (configuration_response.Item2.success)
             {
                 configuration = configuration_response.Item1;
             }
             else
             {
                 configuration = new Configuration();
                 configuration.message = new ResourceConfiguration();
                 configuration.message.text_max_length = 256;
                 configuration.post = new ResourceConfiguration();
                 configuration.post.text_max_length = 256;
             }

            accessToken = "";

            defaultSnarlIconPath = "";
            snarlInterface = new SnarlInterface();
            // would be clicks...            snarlInterface.CallbackEvent += new SnarlInterface.CallbackEventHandler(snarl_CallbackEvent);
            snarlInterface.GlobalSnarlEvent += new SnarlInterface.GlobalEventHandler(snarl_GlobalSnarlEvent);

            backgroundWorkerPersonalStream = new BackgroundWorker();
            backgroundWorkerPersonalStream.WorkerReportsProgress = true;
            backgroundWorkerPersonalStream.WorkerSupportsCancellation = true;
            backgroundWorkerPersonalStream.DoWork += backgroundWorkerPersonalStream_DoWork;
            backgroundWorkerPersonalStream.RunWorkerCompleted += backgroundWorkerPersonalStream_RunWorkerCompleted;
            backgroundWorkerPersonalStream.ProgressChanged += backgroundWorkerPersonalStream_ProgressChanged;

            backgroundWorkerMentions = new BackgroundWorker();
            backgroundWorkerMentions.WorkerReportsProgress = true;
            backgroundWorkerMentions.WorkerSupportsCancellation = true;
            backgroundWorkerMentions.DoWork += backgroundWorkerMentions_DoWork;
            backgroundWorkerMentions.RunWorkerCompleted += backgroundWorkerMentions_RunWorkerCompleted;
            backgroundWorkerMentions.ProgressChanged += backgroundWorkerMentions_ProgressChanged;

            backgroundWorkerMessages = new BackgroundWorker();
            backgroundWorkerMessages.WorkerReportsProgress = true;
            backgroundWorkerMessages.WorkerSupportsCancellation = true;
            backgroundWorkerMessages.DoWork += backgroundWorkerMessages_DoWork;
            backgroundWorkerMessages.RunWorkerCompleted += backgroundWorkerMessages_RunWorkerCompleted;
            backgroundWorkerMessages.ProgressChanged += backgroundWorkerMessages_ProgressChanged;

            backgroundWorkerGlobalStream = new BackgroundWorker();
            backgroundWorkerGlobalStream.WorkerReportsProgress = true;
            backgroundWorkerGlobalStream.WorkerSupportsCancellation = true;
            backgroundWorkerGlobalStream.DoWork += backgroundWorkerGlobalStream_DoWork;
            backgroundWorkerGlobalStream.RunWorkerCompleted += backgroundWorkerGlobalStream_RunWorkerCompleted;
            backgroundWorkerGlobalStream.ProgressChanged += backgroundWorkerGlobalStream_ProgressChanged;

            backgroundWorkerInteractions = new BackgroundWorker();
            backgroundWorkerInteractions.WorkerReportsProgress = true;
            backgroundWorkerInteractions.WorkerSupportsCancellation = true;
            backgroundWorkerInteractions.DoWork += backgroundWorkerInteractions_DoWork;
            backgroundWorkerInteractions.RunWorkerCompleted += backgroundWorkerInteractions_RunWorkerCompleted;
            backgroundWorkerInteractions.ProgressChanged += backgroundWorkerInteractions_ProgressChanged;
        }

        ~Account()
        {
            if (snarlInterface != null)
            {
                snarlInterface.Unregister();
            }
        }

        public SnarlInterface snarlInterface;
        public string defaultSnarlIconPath { get; set; }
        private BackgroundWorker backgroundWorkerPersonalStream;
        private BackgroundWorker backgroundWorkerMentions;
        private BackgroundWorker backgroundWorkerMessages;
        private BackgroundWorker backgroundWorkerGlobalStream;
        private BackgroundWorker backgroundWorkerInteractions;

        private Streaming.UserStream userStream { get; set; }
        private bool streamingIsActive { get; set; }
        private string snarlIcon { get; set; }

        private int fetchCounter = 0;
        List<Channel> KnownChannels;
        List<string> KnownChannelIds;
        public ThreadSaveObservableCollection<IChapperCollection> allNonPrivateMessagesChannels { get; set; }
        public ThreadSaveObservableCollection<IChapperCollection> patterChannels { get; set; }
        public ThreadSaveObservableCollection<PrivateMessageChannel> privateMessagesChannels { get; set; }
        public ThreadSaveObservableCollection<Search> searches { get; set; }

        public string accessToken
        {
            get
            {
                return _accessToken;
            }
            set
            {
                _accessToken = value;
                if (!string.IsNullOrEmpty(_accessToken))
                {
                    Tuple<Token, ApiCallResponse> response = Tokens.get(_accessToken);
                    if (response.Item2.success)
                    {
                        token = response.Item1;
                        user = token.user;
                    }
                }
            }
        }
        private string _accessToken { get; set; }
        public Token token { get; set; }
        public Configuration configuration { get; set; }

        public string username
        {
            get
            {
                return user.username;
            }
        }
        public string userFullname { get; set; }
        public string displayName { get; set; }
        public string id { get; set; }
        public User user { get; set; }
        private DateTime newestAlreadyFetchedDateTime { get; set; }

        public ThreadSaveObservableCollection<Item> myStream { get; set; }
        public ThreadSaveObservableCollection<Item> mentions { get; set; }
        public ThreadSaveObservableCollection<Item> privateMessages { get; set; }
        public ThreadSaveObservableCollection<Item> globalStream { get; set; }
        public ThreadSaveObservableCollection<InteractionEntry> interactions { get; set; }

        public Item lastSaveStreamMarker_myStream { get; set; }
        public Item lastSaveStreamMarker_mentions { get; set; }
        public Item lastSaveStreamMarker_personalMessages { get; set; }
        public Item lastSaveStreamMarker_globalStream { get; set; }

        public bool initialUpdateCompleted
        {
            get
            {
                return (initialUpdateCompletedMyStream && initialUpdateCompletedMentions && initialUpdateCompletedPrivateMessages && initialUpdateCompletedGlobalStream && fetchCounter > 1);
            }
        }
        public bool initialUpdateCompletedMyStream { get; set; }
        public bool initialUpdateCompletedMentions { get; set; }
        public bool initialUpdateCompletedPrivateMessages { get; set; }
        public bool initialUpdateCompletedGlobalStream { get; set; }
        public bool initialUpdateCompletedMessages { get; set; }
        public bool initialUpdateCompletedInteractions { get; set; }

        public bool verifyCredentials()
        {
            return (user != null);
        }

        public void updateItems()
        {
            fetchCounter++;


            if (!streamingIsActive)
            {
                if (!backgroundWorkerPersonalStream.IsBusy)
                {
                    backgroundWorkerPersonalStream.RunWorkerAsync();
                }
                if (!backgroundWorkerMentions.IsBusy)
                {
                    backgroundWorkerMentions.RunWorkerAsync();
                }
                if (!backgroundWorkerMessages.IsBusy)
                {
                    backgroundWorkerMessages.RunWorkerAsync();
                }
            }
            if (!backgroundWorkerGlobalStream.IsBusy && Properties.Settings.Default.showGeneralStream)
            {
                backgroundWorkerGlobalStream.RunWorkerAsync();
            }
            if (!backgroundWorkerInteractions.IsBusy)
            {
                backgroundWorkerInteractions.RunWorkerAsync();
            }

            foreach (Search search in searches)
            {
                if (search.auto_refresh)
                {
                    search.UpdateItems();
                }
            }

            startStreaming();
            
        }

        private void startStreaming()
        {
            if (!streamingIsActive)
            {
                StreamingOptions streamingOptions = new StreamingOptions();
                streamingOptions.include_annotations = true;
                streamingOptions.include_html = false;
                streamingOptions.include_marker = true;
                streamingOptions.include_channel_annotations = false;
                streamingOptions.include_message_annotations = true;
                streamingOptions.include_post_annotations = true;
                streamingOptions.include_user_annotations = true;

                userStream = new Streaming.UserStream(this.accessToken, streamingOptions);

                IAsyncResult asyncResult = userStream.StartUserStream(
                    streamCallback: streamCallback,
                    unifiedCallback: unifiedCallback,
                    mentionsCallback: mentionsCallback,
                    channelsCallback: channelsCallback,
                    streamStoppedCallback: streamStoppedCallback);

                SubscriptionOptions subscriptionOptions = new SubscriptionOptions();
                subscriptionOptions.include_deleted = true;
                subscriptionOptions.include_incomplete = false;
                subscriptionOptions.include_muted = false;
                subscriptionOptions.include_private = true;
                subscriptionOptions.include_read = true;

                if (Properties.Settings.Default.useUnifiedStream)
                {
                    userStream.subscribe_to_endpoint(userStream.available_endpoints["Unified"]);
                }
                else
                {
                    userStream.subscribe_to_endpoint(userStream.available_endpoints["Stream"]);
                }
                userStream.subscribe_to_endpoint(userStream.available_endpoints["Mentions"]);

                List<string> recognized_channel_types = new List<string>();
                recognized_channel_types.Add("net.app.core.pm");
                recognized_channel_types.Add("net.patter-app.room");
                subscriptionOptions.channel_types = recognized_channel_types;
                userStream.available_endpoints["Channels"].options = subscriptionOptions;
                userStream.subscribe_to_endpoint(userStream.available_endpoints["Channels"]);

                streamingIsActive = true;
            }

        }

        #region Background worker

        #region personal stream

        void backgroundWorkerPersonalStream_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e != null)
            {
                switch (e.ProgressPercentage)
                {
                    case 50:
                        Item item = e.UserState as Item;
                        if (item != null)
                        {
                            myStream.Insert(0, item);
                            //myStream.Add(item);
                            saveUsernameAndHashtags(item);
                            if (initialUpdateCompletedMyStream)
                            {
                                if (!Properties.Settings.Default.useUnifiedStream)
                                {
                                    snarlInterface.Notify("My Stream", item.displayName, item.text, 10, item.avatar, "");
                                }
                                else
                                {
                                    snarlInterface.Notify("Unified Stream", item.displayName, item.text, 10, item.avatar, "");
                                }
                            }
                        }
                        break;

                    case 99:
                        ApiCallResponse apiCallResponse = e.UserState as ApiCallResponse;
                        if (apiCallResponse != null)
                        {
                            if (apiCallResponse.meta != null)
                            {
                                if (apiCallResponse.meta.marker != null)
                                {
                                    if (lastSaveStreamMarker_myStream != null)
                                    {
                                        if (lastSaveStreamMarker_myStream.id != apiCallResponse.meta.marker.id)
                                        {
                                            try
                                            {
                                                Item streamMarkerItem = myStream.Where(i => i.id == apiCallResponse.meta.marker.id).First();
                                                if (streamMarkerItem != null)
                                                {
                                                    AppController.Current.scrollItemIntoView("myStream", streamMarkerItem);
                                                    lastSaveStreamMarker_myStream = streamMarkerItem;
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            Item streamMarkerItem = myStream.Where(i => i.id == apiCallResponse.meta.marker.id).First();
                                            if (streamMarkerItem != null)
                                            {
                                                AppController.Current.scrollItemIntoView("myStream", streamMarkerItem);
                                                lastSaveStreamMarker_myStream = streamMarkerItem;
                                            }
                                        }
                                        catch { }

                                    }
                                }
                            }
                        }
                        
                        

                        break;
                }
            }
        }

        void backgroundWorkerPersonalStream_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            initialUpdateCompletedMyStream = true;
            Item topmostItem = AppController.Current.getTopMostItem("myStream");
            if (topmostItem != null && topmostItem != lastSaveStreamMarker_myStream)
            {
                AppNetDotNet.ApiCalls.StreamMarkers.set(this.accessToken, "my_stream", topmostItem.id, 100);
                AppNetDotNet.ApiCalls.StreamMarkers.set(this.accessToken, "unified", topmostItem.id, 100);
                lastSaveStreamMarker_myStream = topmostItem;
            }
        }

        void backgroundWorkerPersonalStream_DoWork(object sender, DoWorkEventArgs e)
        {
            Tuple<List<Post>, ApiCallResponse> items;
            ParametersMyStream parameter = new ParametersMyStream();
            parameter.count = 100;
            parameter.include_annotations = true;
            if (myStream.Count > 0)
            {
                parameter.since_id = myStream.Max(i => i.id_decimal).ToString();
            }
            if (!Properties.Settings.Default.useUnifiedStream)
            {
                items = SimpleStreams.getUserStream(this.accessToken, parameter);
            }
            else
            {
                items = SimpleStreams.getUnifiedStream(this.accessToken, parameter);
            }

            if (items.Item2.success)
            {
                items.Item1.Reverse();
                foreach (Post post in items.Item1)
                {
                    
                    if (post != null)
                    {
                        Item item = new Item(post);
                        if (item != null)
                        {
                            if (!item.apnPost.is_deleted && !string.IsNullOrEmpty(item.text))
                            {
                                Tuple<Post, ApiCallResponse> reply = Posts.getById(this.accessToken, item.apnPost.reply_to);
                                if (reply.Item2.success)
                                {
                                    item.inReplyToPost = reply.Item1;
                                }
                                backgroundWorkerPersonalStream.ReportProgress(50, item);
                            }
                        }
                    
                    }
                }
                if (items.Item1 != null)
                {
                    backgroundWorkerPersonalStream.ReportProgress(99, items.Item2);
                }
            }
        }

        #endregion

        #region mentions

        void backgroundWorkerMentions_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e != null)
            {
                switch (e.ProgressPercentage)
                {
                    case 50:
                        Item item = e.UserState as Item;
                        if (item != null)
                        {
                            if (!item.apnPost.is_deleted && !string.IsNullOrEmpty(item.text))
                            {
                                mentions.Add(item);
                                saveUsernameAndHashtags(item);
                                if (initialUpdateCompletedMentions)
                                {
                                    snarlInterface.Notify("Mentions", item.displayName, item.text, 10, item.avatar, "");
                                }
                            }
                        }
                        break;

                    case 99:
                        ApiCallResponse apiCallResponse = e.UserState as ApiCallResponse;
                        if (apiCallResponse != null)
                        {
                            if (apiCallResponse.meta != null)
                            {
                                if (apiCallResponse.meta.marker != null)
                                {
                                    if (lastSaveStreamMarker_mentions != null)
                                    {
                                        if (lastSaveStreamMarker_mentions.id != apiCallResponse.meta.marker.id)
                                        {
                                            try
                                            {
                                                Item streamMarkerItem = mentions.Where(i => i.id == apiCallResponse.meta.marker.id).First();
                                                if (streamMarkerItem != null)
                                                {
                                                    AppController.Current.scrollItemIntoView("mentions", streamMarkerItem);
                                                    lastSaveStreamMarker_mentions = streamMarkerItem;
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            Item streamMarkerItem = mentions.Where(i => i.id == apiCallResponse.meta.marker.id).First();
                                            if (streamMarkerItem != null)
                                            {
                                                AppController.Current.scrollItemIntoView("mentions", streamMarkerItem);
                                                lastSaveStreamMarker_mentions = streamMarkerItem;
                                            }
                                        }
                                        catch { }

                                    }
                                }
                            }
                        }

                        

                        break;
                }
            }
        }

        void backgroundWorkerMentions_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            initialUpdateCompletedMentions = true;
            Item topmostItem = AppController.Current.getTopMostItem("mentions");
            if (topmostItem != null && topmostItem != lastSaveStreamMarker_mentions)
            {
                AppNetDotNet.ApiCalls.StreamMarkers.set(this.accessToken, "mentions", topmostItem.id, 100);
                lastSaveStreamMarker_mentions = topmostItem;
            }
        }

        void backgroundWorkerMentions_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!initialUpdateCompletedMentions)
            {
                System.Threading.Thread.Sleep(3000);
            }
            Tuple<List<Post>, ApiCallResponse> items;
            ParametersMyStream parameter = new ParametersMyStream();
            parameter.count = 100;
            parameter.include_annotations = true;
            if (mentions.Count > 0)
            {
                parameter.since_id = mentions.Max(i => i.id_decimal).ToString();
            }
            items = Posts.getMentionsOfUsernameOrId(this.accessToken, this.username, parameter);

            if (items.Item2.success)
            {
                foreach (Post post in items.Item1)
                {

                    if (post != null)
                    {
                        Item item = new Item(post);
                        Tuple<Post, ApiCallResponse> reply = Posts.getById(this.accessToken, item.apnPost.reply_to);
                        if (reply.Item2.success)
                        {
                            item.inReplyToPost = reply.Item1;
                        }
                        backgroundWorkerMentions.ReportProgress(50, item);
                    }
                }
                if (items.Item1 != null)
                {
                    backgroundWorkerMentions.ReportProgress(99, items.Item2);
                }
            }
        }

        public void registerSnarlClasses(string defaultIconPath, bool registerChannels = false)
        {
            defaultSnarlIconPath = defaultIconPath.Substring(6);
            snarlIcon = defaultIconPath;
            snarlInterface.RegisterWithEvents("Chapper", "Chapper", defaultSnarlIconPath, "ukl78o35o37", IntPtr.Zero,null);
            if (!Properties.Settings.Default.useUnifiedStream)
            {
                snarlInterface.AddClass("My Stream", "My Stream");
            }
            else
            {
                snarlInterface.AddClass("Unified Stream", "Unified Stream");
            }
            snarlInterface.AddClass("Mentions", "Mentions");
            snarlInterface.AddClass("Private Messages", "Private Messages");
            snarlInterface.AddClass("Messages", "Messages");
            snarlInterface.AddClass("Interactions", "Interactions");
            snarlInterface.AddClass("Global Stream", "Global Stream");

            if (registerChannels)
            {
                foreach (PatterRoom patter_room in patterChannels)
                {
                    snarlInterface.AddClass("Patter " + patter_room.displayName, "Patter " + patter_room.displayName);
                }
            }
        }

        void snarl_GlobalSnarlEvent(SnarlInterface sender, SnarlInterface.GlobalEventArgs args)
        {
            if (args.GlobalEvent == SnarlInterface.GlobalEvent.SnarlLaunched)
            {
                registerSnarlClasses(defaultSnarlIconPath, registerChannels:true);
            }
            else if (args.GlobalEvent == SnarlInterface.GlobalEvent.SnarlQuit)
            {
                //
            }
        }

        #endregion

        #region messages

        void backgroundWorkerMessages_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e != null)
            {
                switch (e.ProgressPercentage)
                {
                    case 20:
                        // Channel list
                        KnownChannels = e.UserState as List<Channel>;
                        if (KnownChannels != null)
                        {
                            foreach (Channel channel in KnownChannels)
                            {
                                if (!KnownChannelIds.Contains(channel.id))
                                {
                                    KnownChannelIds.Add(channel.id);
                                    addNewChannel(channel);
                                }
                            }
                        }
                        break;

                    case 50:
                        Item item = e.UserState as Item;
                        if (item != null)
                        {
                            if (item.isPrivateMessage)
                            {
                                privateMessages.Add(item);
                                saveUsernameAndHashtags(item);
                                if (initialUpdateCompletedPrivateMessages)
                                {
                                    snarlInterface.Notify("Private Messages", item.displayName, item.text, 10, item.avatar, "");
                                }
                                try
                                {
                                    IChapperCollection collection = privateMessagesChannels.Where(c => c.id == item.apnMessage.channel_id).First();
                                    if (collection != null)
                                    {
                                        collection.items.Add(item);
                                    }
                                }
                                catch { }
                            }
                            else
                            {
                                try
                                {
                                    IChapperCollection collection = allNonPrivateMessagesChannels.Where(c => c.id == item.apnMessage.channel_id).First();
                                    if (collection != null)
                                    {
                                        collection.items.Add(item);
                                        if (initialUpdateCompletedMessages)
                                        {
                                            snarlInterface.Notify("Messages", item.displayName, item.text, 10, item.avatar, "");
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                        break;

                    case 99:
                       
                        break;
                }
            }
        }

        void backgroundWorkerMessages_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            initialUpdateCompletedPrivateMessages = true;
            initialUpdateCompletedMessages = true;
            Item topmostItem = AppController.Current.getTopMostItem("pms");
            if (topmostItem != null && topmostItem != lastSaveStreamMarker_personalMessages)
            {
                // AppNetDotNet.ApiCalls.StreamMarkers.set(this.accessToken, "messages", topmostItem.id, 100);
                lastSaveStreamMarker_personalMessages = topmostItem;
            }
        }

        void backgroundWorkerMessages_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Message> messages = new List<Message>();

            Tuple<List<Message>, ApiCallResponse> items;
            Channels.channelParameters parameter = new Channels.channelParameters();
            parameter.include_annotations = true;
            parameter.include_marker = true;
            Tuple<List<Channel>, ApiCallResponse> channels = Channels.Subscriptions.getOfCurrentUser(this.accessToken,parameter);
            if (channels.Item2.success)
            {
                backgroundWorkerMessages.ReportProgress(20, channels.Item1);
            }
            else
            {
                return;
            }

            bool newerItemFound = false;
            DateTime newestItemInThisFetch = new DateTime(0);

            List<string> messagesBeingPMs = new List<string>();

            Messages.messageParameters parameters = new Messages.messageParameters();
            parameters.include_annotations = true;
            parameters.include_deleted = false;

            foreach (Channel channel in channels.Item1)
            {
                items = Messages.getMessagesInChannel(this.accessToken, channel.id,parameters:parameters);
                if (items.Item2.success)
                {
                    foreach (Message message in items.Item1)
                    {
                        if (message.created_at > newestAlreadyFetchedDateTime)
                        {
                            if (message.created_at > newestItemInThisFetch)
                            {
                                newestItemInThisFetch = message.created_at;
                                newerItemFound = true;
                            }
                            if (channel.type == "net.app.core.pm")
                            {
                                messagesBeingPMs.Add(message.id);
                            }
                            messages.Add(message);
                        }
                    }
                }
            }

            if (newerItemFound)
            {
                newestAlreadyFetchedDateTime = newestItemInThisFetch;
            }

            foreach (Message message in messages)
            {
                Item item = new Item(message);
                if (item != null)
                {
                    if (item != null)
                    {
                        if (!item.apnMessage.is_deleted && !string.IsNullOrEmpty(item.text))
                        {
                            if (messagesBeingPMs.Contains(message.id))
                            {
                                item.isPrivateMessage = true;
                            }
                            backgroundWorkerMessages.ReportProgress(50, item);
                        }
                    }
                }
            }

            backgroundWorkerMessages.ReportProgress(99, null);
        }

        #endregion

        #region global stream

        void backgroundWorkerGlobalStream_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e != null)
            {
                switch (e.ProgressPercentage)
                {
                    case 50:
                        Item item = e.UserState as Item;
                        if (item != null)
                        {
                            if (globalStream.Count > 250)
                            {
                                globalStream.RemoveAt(globalStream.Count() - 1);
                            }

                            globalStream.Insert(0, item);
                            //globalStream.Add(item);
                            if (initialUpdateCompletedGlobalStream)
                            {
                                snarlInterface.Notify("Global Stream", item.displayName, item.text, 10, item.avatar, "");
                            }
                        }
                        break;

                    case 99:
                        ApiCallResponse apiCallResponse = e.UserState as ApiCallResponse;
                        if (apiCallResponse != null)
                        {
                            if (apiCallResponse.meta != null)
                            {
                                if (apiCallResponse.meta.marker != null)
                                {
                                    if (lastSaveStreamMarker_globalStream != null)
                                    {
                                        if (lastSaveStreamMarker_globalStream.id != apiCallResponse.meta.marker.id)
                                        {
                                            try
                                            {
                                                Item streamMarkerItem = globalStream.Where(i => i.id == apiCallResponse.meta.marker.id).First();
                                                if (streamMarkerItem != null)
                                                {
                                                    AppController.Current.scrollItemIntoView("globalStream", streamMarkerItem);
                                                    lastSaveStreamMarker_globalStream = streamMarkerItem;
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            Item streamMarkerItem = globalStream.Where(i => i.id == apiCallResponse.meta.marker.id).First();
                                            if (streamMarkerItem != null)
                                            {
                                                AppController.Current.scrollItemIntoView("globalStream", streamMarkerItem);
                                                lastSaveStreamMarker_globalStream = streamMarkerItem;
                                            }
                                        }
                                        catch { }

                                    }
                                }
                            }
                        }



                        break;
                }
            }
        }

        void backgroundWorkerGlobalStream_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            initialUpdateCompletedGlobalStream = true;
            Item topmostItem = AppController.Current.getTopMostItem("globalStream");
            if (topmostItem != null && topmostItem != lastSaveStreamMarker_myStream)
            {
                AppNetDotNet.ApiCalls.StreamMarkers.set(this.accessToken, "gloabl_stream", topmostItem.id, 100);
                
                lastSaveStreamMarker_globalStream = topmostItem;
            }
        }

        void backgroundWorkerGlobalStream_DoWork(object sender, DoWorkEventArgs e)
        {
            Tuple<List<Post>, ApiCallResponse> items;
            ParametersMyStream parameter = new ParametersMyStream();
            parameter.count = 20;
            parameter.include_annotations = true;
            if (globalStream.Count > 0)
            {
                parameter.since_id = globalStream.Max(i => i.id_decimal).ToString();
            }
            items = SimpleStreams.getGlobalStream(this.accessToken, parameter);

            if (items.Item2.success)
            {
                items.Item1.Reverse();
                foreach (Post post in items.Item1)
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
                                    Tuple<Post,ApiCallResponse> reply = Posts.getById(this.accessToken, item.apnPost.reply_to);
                                    if (reply.Item2.success)
                                    {
                                        item.inReplyToPost = reply.Item1;
                                    }
                                }
                                backgroundWorkerGlobalStream.ReportProgress(50, item);
                            }
                        }

                    }
                }
                if (items.Item1 != null)
                {
                    backgroundWorkerGlobalStream.ReportProgress(99, items.Item2);
                }
            }
        }

        #endregion

        #region interactions

        void backgroundWorkerInteractions_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e != null)
            {
                Interaction fetched_interaction = e.UserState as Interaction;
                if (fetched_interaction != null)
                {
                    switch (e.ProgressPercentage)
                    {
                        case 50:
                            fetched_interaction.event_date = fetched_interaction.event_date.ToLocalTime();
                            InteractionEntry entry = new InteractionEntry(fetched_interaction);
                            interactions.Add(new InteractionEntry(fetched_interaction));
                            if (initialUpdateCompletedGlobalStream)
                            {
                                snarlInterface.Notify("Interactions", entry.action_description, "", 10, entry.main_avatar, "");
                            }
                            break;
                    }
                }
            }
        }

        void backgroundWorkerInteractions_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            initialUpdateCompletedInteractions = true;
        }

        void backgroundWorkerInteractions_DoWork(object sender, DoWorkEventArgs e)
        {
            Tuple<List<Interaction>, ApiCallResponse> fetched_interactions;
            fetched_interactions = Interactions.getUserInteractionsWithMe(this.accessToken);

            if (fetched_interactions.Item2.success)
            {
                foreach (Interaction interaction in fetched_interactions.Item1)
                {
                    try
                    {
                        IEnumerable<InteractionEntry> alreadyAvailableEntries = interactions.Where(entry => (entry.apnInteraction.event_date == interaction.event_date.ToLocalTime() && entry.apnInteraction.action == interaction.action));
                        if (alreadyAvailableEntries != null)
                        {
                            if (alreadyAvailableEntries.Count() > 0)
                            {
                                continue;
                            }
                        }
                    }
                    catch { }
                    backgroundWorkerInteractions.ReportProgress(50, interaction);
                }
            }
        }

        #endregion


        #endregion

        private static void saveUsernameAndHashtags(Item item)
        {
            if(!AppController.SavedUsernames.Contains("@" + item.user.username.ToLower())) {
                                AppController.SavedUsernames.Add("@" + item.user.username);
            }
            if (item.entities != null)
            {
                if (item.entities.hashtags != null)
                {
                    foreach (Entities.Hashtag hashtag in item.entities.hashtags)
                    {
                        if (!AppController.SavedHashtags.Contains("#" + hashtag.name))
                        {
                            AppController.SavedHashtags.Add("#" + hashtag.name);
                        }
                    }
                }
            }
        }

        public IChapperCollection addNewChannel(Channel channel)
        {
            switch (channel.type)
            {
                case "net.app.core.pm":
                    PrivateMessageChannel privateMessageChannel = new PrivateMessageChannel(channel);
                    privateMessagesChannels.Add(privateMessageChannel);
                    AppController.Current.addNewChapperCollection(privateMessageChannel);
                    return privateMessageChannel;

                case "net.patter-app.room":
                    if (Properties.Settings.Default.showPatterRooms)
                    {
                        PatterRoom patterRoom = new PatterRoom(channel);
                        allNonPrivateMessagesChannels.Add(patterRoom);
                        patterChannels.Add(patterRoom);
                        snarlInterface.AddClass("Patter " + patterRoom.displayName, "Patter " + patterRoom.displayName);
                        AppController.Current.addNewChapperCollection(patterRoom);
                        return patterRoom;
                    }
                    break;
            }
            return null;
        }

        public void add_search(Search search)
        {
            if (search != null)
            {
                searches.Add(search);
                if (search.auto_refresh)
                {
                    snarlInterface.AddClass("Search " + search.id, "Search " + search.id);
                }
                save_searches();
            }
        }

        public void remove_search(Search search)
        {
            if (search != null && searches.Contains(search))
            {
                searches.Remove(search);
                snarlInterface.RemoveClass("Search " + search.id);
                save_searches();
            }
        }

        private void save_searches()
        {
            if (fetchCounter > 0)
            {
                if (Properties.Settings.Default.saved_searches == null)
                {
                    Properties.Settings.Default.saved_searches = new System.Collections.Specialized.StringCollection();
                }
                Properties.Settings.Default.saved_searches.Clear();
                foreach (Search search in searches)
                {
                    if (search.save_search)
                    {
                        Properties.Settings.Default.saved_searches.Add(search.id);
                    }
                }
            }
        }

        #region Streaming callbacks

        public void streamCallback(List<Post> posts, bool is_deleted = false)
        {
            if (posts != null)
            {
                foreach (Post post in posts)
                {
                    if (post == null)
                    {
                        continue;
                    }
                    if (post.machine_only || string.IsNullOrEmpty(post.text))
                    {
                        continue;
                    }
                    if (!post.is_deleted)
                    {
                        Item item = new Item(post);
                        myStream.Add(item);
                        saveUsernameAndHashtags(item);
                        snarlInterface.Notify("My Stream", item.displayName, item.text, 10, item.avatar, "");
                    }
                    else
                    {
                        IEnumerable<Item> existing_items = myStream.Where(item => item.id == post.id);
                        if (existing_items != null)
                        {
                            if (existing_items.Count() > 0)
                            {
                                List<Item> cache = new List<Item>();
                                foreach (Item item in existing_items)
                                {
                                    cache.Add(item);
                                }
                                foreach (Item item in cache)
                                {
                                    myStream.Remove(item);
                                }
                                cache = null;
                            }
                        }
                    }
                }
            }
        }

        public void unifiedCallback(List<Post> posts, bool is_deleted = false)
        {
            if (posts != null)
            {
                foreach (Post post in posts)
                {
                    if (post == null)
                    {
                        continue;
                    }
                    if (post.machine_only || string.IsNullOrEmpty(post.text))
                    {
                        continue;
                    }
                    if (!post.is_deleted)
                    {
                        Item item = new Item(post);
                        myStream.Add(item);
                        saveUsernameAndHashtags(item);
                        snarlInterface.Notify("Unified Stream", item.displayName, item.text, 10, item.avatar, "");
                    }
                    else
                    {
                        IEnumerable<Item> existing_items = myStream.Where(item => item.id == post.id);
                        if (existing_items != null)
                        {
                            if (existing_items.Count() > 0)
                            {
                                List<Item> cache = new List<Item>();
                                foreach (Item item in existing_items)
                                {
                                    cache.Add(item);
                                }
                                foreach (Item item in cache)
                                {
                                    myStream.Remove(item);
                                }
                                cache = null;
                            }
                        }
                    }
                }
            }
        }

        public void mentionsCallback(List<Post> posts, bool is_deleted = false)
        {
            if (posts != null)
            {
                foreach (Post post in posts)
                {
                    if (post == null)
                    {
                        continue;
                    }
                    if (post.machine_only || string.IsNullOrEmpty(post.text))
                    {
                        continue;
                    }
                    if (!post.is_deleted)
                    {
                        Item item = new Item(post);
                        mentions.Add(item);
                        saveUsernameAndHashtags(item);
                        snarlInterface.Notify("Mentions", item.displayName, item.text, 10, item.avatar, "");
                    }
                    else
                    {
                        IEnumerable<Item> existing_items = mentions.Where(item => item.id == post.id);
                        if (existing_items != null)
                        {
                            if (existing_items.Count() > 0)
                            {
                                List<Item> cache = new List<Item>();
                                foreach (Item item in existing_items)
                                {
                                    cache.Add(item);
                                }
                                foreach (Item item in cache)
                                {
                                    mentions.Remove(item);
                                }
                                cache = null;
                            }
                        }
                    }
                }
            }

        }

        public void channelsCallback(List<Message> messages, bool is_deleted = false)
        {
            if (messages != null)
            {
                foreach (Message message in messages)
                {
                    if (message == null)
                    {
                        continue;
                    }
                    if (message.machine_only || string.IsNullOrEmpty(message.text))
                    {
                        continue;
                    }

                    if (!message.is_deleted)
                    {
                        Item item = new Item(message);
                        privateMessages.Add(item);
                        saveUsernameAndHashtags(item);
                        try
                        {
                            IChapperCollection collection = privateMessagesChannels.Where(c => c.id == item.apnMessage.channel_id).First();
                            if (collection != null)
                            {
                                collection.items.Add(item);
                                snarlInterface.Notify("Private Messages", item.displayName, item.text, 10, item.avatar, "");
                            }

                        }
                        catch {
                            try
                            {
                                IChapperCollection patter_collection = patterChannels.Where(c => c.id == item.apnMessage.channel_id).First();
                                if (patter_collection != null)
                                {
                                    patter_collection.items.Add(item);
                                    snarlInterface.Notify("Patter " + patter_collection.displayName, item.displayName, item.text, 10, item.avatar, "");
                                }
                            }
                            catch {
                                Channels.channelParameters parameter = new Channels.channelParameters();
                                parameter.include_annotations = true;
                                parameter.include_marker = true;
                                Tuple<Channel, ApiCallResponse> channel_response = Channels.get(this.accessToken, message.channel_id, parameters:parameter);
                                if (channel_response.Item2.success)
                                {
                                    IChapperCollection collection = this.addNewChannel(channel_response.Item1);
                                    if (collection != null)
                                    {
                                        collection.items.Add(item);
                                    }
                                    
                                }
                            }
                        }
                    }
                    else
                    {
                        IEnumerable<Item> existing_items = privateMessages.Where(item => item.id == message.id);
                        if (existing_items != null)
                        {
                            if (existing_items.Count() > 0)
                            {
                                List<Item> cache = new List<Item>();
                                foreach (Item item in existing_items)
                                {
                                    cache.Add(item);
                                }
                                foreach (Item item in cache)
                                {
                                    privateMessages.Remove(item);
                                    try
                                    {
                                        IChapperCollection collection = privateMessagesChannels.Where(c => c.id == item.apnMessage.channel_id).First();
                                        if (collection != null)
                                        {
                                            collection.items.Remove(item);
                                        }
                                        else
                                        {
                                            IChapperCollection patter_collection = patterChannels.Where(c => c.id == item.apnMessage.channel_id).First();
                                            if (patter_collection != null)
                                            {
                                                patter_collection.items.Remove(item);
                                            }
                                        }
                                    }
                                    catch { }
                                }
                                cache = null;
                            }
                        }
                    }
                }
            }
        }

        public void streamStoppedCallback(Streaming.StopReasons reason)
        {
            Console.WriteLine(reason);
            Console.WriteLine(userStream.last_error);
            streamingIsActive = false;
        }


        #endregion

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
    }
}
