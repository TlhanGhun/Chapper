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
using System.Timers;
using System.Windows.Threading;


namespace Chapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer dispatcherTimer;
        DispatcherTimer dispatcherHideError;
        private TabItem currentTab;
        public TabItem lastTab;
        public AppNetDotNet.Model.Annotations.Issue currentError { get; set; }
        public CurrentPostTarget currentPostTarget;
        private bool init_completed = false;
        private int tabCounter { get; set; }
        int maxNumberOfChars = 255;

        public MainWindow()
        {
            InitializeComponent();
            this.PreviewKeyDown += MainWindow_KeyDown;
            combobox_language.ItemsSource = AppController.available_languages;

            currentPostTarget = new CurrentPostTarget();

            if (Properties.Settings.Default.windowHeight > 0 && Properties.Settings.Default.windowWidth > 0)
            {
                this.Height = Properties.Settings.Default.windowHeight;
                this.Width = Properties.Settings.Default.windowWidth;
                // gibt es nicht in .NET 4.5 ?
               // if (Properties.Settings.Default.windowLocationX < System.Windows.Forms.SystemInformation.Width && Properties.Settings.Default.windowLocationY < System.Windows.Forms.SystemInformation.VirtualScreen.Height)
               // {
                    this.Left = Properties.Settings.Default.windowLocationX;
                    this.Top = Properties.Settings.Default.windowLocationY;
               // }

            }
            init_completed = true;
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Down || e.Key == Key.Up)  && !textbox_postText.textBoxContent.IsFocused)
            {
                // TabItem tabitem = tabControl_main.SelectedItem as TabItem;
                e.Handled = true;
            }
        }

        public void startDispatcherTimer()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 20);
            dispatcherTimer.Start();
        }

        public void showErrorMessage(string title, string description, string stacktrace)
        {
            if (!Properties.Settings.Default.show_errors)
            {
                return;
            }
            textblock_error_message_title.Text = title;
            textblock_error_description.Text = description;
            currentError = new AppNetDotNet.Model.Annotations.Issue();
            currentError.title = title;
            currentError.description = description;
            currentError.stacktrace = stacktrace; ;
            border_around_error_message.Visibility = Visibility.Visible;
            dispatcherHideError = new DispatcherTimer();
            dispatcherHideError.Tick += dispatcherHideError_Tick;
            dispatcherHideError.Interval = new TimeSpan(0, 0, 10);
            dispatcherHideError.Start();
        }

        public void showErrorMessage(ApiCallResponse apiCallResponse)
        {
            showErrorMessage(apiCallResponse.errorMessage, apiCallResponse.errorDescription, "");
        }

        void dispatcherHideError_Tick(object sender, EventArgs e)
        {
            if (dispatcherHideError != null)
            {
                dispatcherHideError.Stop();
            }
            border_around_error_message.Visibility = Visibility.Collapsed;
        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            AppController.Current.account.updateItems();
        }

        

   

        public void textbox_postText_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(textbox_postText.textBoxContent.Text))
            {
                textblock_enterPostHere.Visibility = System.Windows.Visibility.Collapsed;
                button_sendPost.IsEnabled = true;
                button_trashPost.IsEnabled = true;


                int remaingTextLength = maxNumberOfChars - textbox_postText.NumberOfChars;
                textblock_remainingChars.Text = remaingTextLength.ToString();
                if (remaingTextLength < 0)
                {
                    textblock_remainingChars.Foreground = Brushes.Red;
                    button_sendPost.IsEnabled = false;
                }
                else if (remaingTextLength < 3)
                {
                    textblock_remainingChars.Foreground = Brushes.Yellow;
                }
                else
                {
                    textblock_remainingChars.Foreground = System.Windows.Application.Current.Resources["color_overlays_composebox"] as SolidColorBrush;
                }
            }
            else
            {
                textblock_enterPostHere.Visibility = System.Windows.Visibility.Visible;
                button_sendPost.IsEnabled = false;
                button_trashPost.IsEnabled = false;
                textblock_remainingChars.Text = maxNumberOfChars.ToString();
            }

        }

        private void button_sendPost_Click_1(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(textbox_postText.Text))
            {

                AppNetDotNet.Model.Entities entities = null;
                string toBePostedText = textbox_postText.textBoxContent.Text;
                if (textbox_postText.MarkdownLinksInText.Count() > 0)
                {
                    entities = new AppNetDotNet.Model.Entities();
                    entities.links = new List<AppNetDotNet.Model.Entities.Link>();
                    entities.hashtags = null;
                    entities.mentions = null;
                    foreach (KeyValuePair<string, string> link in textbox_postText.MarkdownLinksInText)
                    {
                        AppNetDotNet.Model.Entities.Link linkEntity = new AppNetDotNet.Model.Entities.Link();
                        linkEntity.text = link.Value;
                        linkEntity.url = link.Key;
                        int startPosition = toBePostedText.IndexOf(string.Format("[{0}]({1})", linkEntity.text, linkEntity.url));
                        linkEntity.pos = startPosition;
                        linkEntity.len = linkEntity.text.Length;
                        toBePostedText = toBePostedText.Replace(string.Format("[{0}]({1})", linkEntity.text, linkEntity.url), linkEntity.text);
                        entities.links.Add(linkEntity);
                    }
                }

                //List<IAnnotation> annotations = null;
                if (!string.IsNullOrEmpty(Properties.Settings.Default.language_posts))
                {
                   /* annotations = new List<IAnnotation>();
                    AppNetDotNet.Model.Annotations.Language language_annotation = new AppNetDotNet.Model.Annotations.Language();
                    language_annotation.language = Properties.Settings.Default.language_posts;
                    annotations.Add(language_annotation); */
                }


                if (currentPostTarget.pmToUser == null && currentPostTarget.channelName == null)
                {
                    List<File> toBeAddedFiles = null;
                    if (!string.IsNullOrEmpty(currentPostTarget.filePathOfToBeAddedFile))
                    {
                        if(System.IO.File.Exists(currentPostTarget.filePathOfToBeAddedFile)) {
                            Tuple<File, ApiCallResponse> uploadedFile = AppNetDotNet.ApiCalls.Files.create(AppController.Current.account.accessToken, currentPostTarget.filePathOfToBeAddedFile, type: "com.nymphicusapp.chapper.image");
                            if (uploadedFile.Item2.success)
                            {
                                toBeAddedFiles = new List<File>();
                                toBeAddedFiles.Add(uploadedFile.Item1);
                            }
                        }
                    }


                    Tuple<Post, ApiCallResponse> response;
                    if (currentPostTarget.replyToItem != null)
                    {
                        if (currentPostTarget.replyToItem.apnPost == null)
                        {
                            currentPostTarget.replyToItem = null;
                        }
                    }
                    if (currentPostTarget.replyToItem == null)
                    {
                        response = Posts.create(AppController.Current.account.accessToken, toBePostedText, toBeEmbeddedFiles: toBeAddedFiles, entities:entities, parse_links:true);
                    }
                    else
                    {
                        response = Posts.create(AppController.Current.account.accessToken, toBePostedText, currentPostTarget.replyToItem.id, toBeEmbeddedFiles: toBeAddedFiles, entities: entities, parse_links:true);
                    }
                    if (response.Item2.success)
                    {
                        AppController.Current.account.updateItems();
                        clear_postbox();
                        if (string.IsNullOrEmpty(currentPostTarget.channelName))
                        {
                            textblock_composeOverlay.Text = "";
                        }
                    }
                    else
                    {
                        showErrorMessage(response.Item2);
                    }
                }
                else
                {
                    Tuple<Message, ApiCallResponse> response;
                    List<string> receiver = new List<string>();
                    if (currentPostTarget.pmToUser != null)
                    {
                        receiver.Add("@" + currentPostTarget.pmToUser.username);
                    }

                    if (receiver.Count == 0 && currentPostTarget.replyToItem != null)
                    {
                        receiver.Add("@" + currentPostTarget.replyToItem.user.username);
                    }

                    if (currentPostTarget.replyToItem != null)
                    {
                        if (currentPostTarget.replyToItem.apnMessage == null)
                        {
                            currentPostTarget.replyToItem = null;
                        }
                    }

                    if (currentPostTarget.replyToItem != null)
                    {
                        if (currentPostTarget.replyToItem.isPrivateMessage)
                        {
                            response = Messages.createPrivateMessage(AppController.Current.account.accessToken, toBePostedText, receiver, currentPostTarget.replyToItem.id, machineOnly: 0, entities: entities, parse_links:true);
                        }
                        else if (currentPostTarget.replyToItem.apnMessage != null)
                        {
                            {
                                response = Messages.create(AppController.Current.account.accessToken, toBePostedText, currentPostTarget.replyToItem.apnMessage.channel_id, receiver, machineOnly: 0, reply_to: currentPostTarget.replyToItem.id, entities: entities, parse_links:true);
                            }
                        }
                        else
                        {
                            response = Messages.createPrivateMessage(AppController.Current.account.accessToken, toBePostedText, receiver, currentPostTarget.replyToItem.id, machineOnly: 0, entities: entities, parse_links:true);
                        }
                        
                    }
                    else
                    {
                        if (currentPostTarget.channelId != null)
                        {
                            string channelId = currentPostTarget.channelId;
                            response = Messages.create(AppController.Current.account.accessToken, textbox_postText.Text, channelId, receiver, machineOnly: 0, parse_links:true);
                        }
                        else
                        {
                            response = Messages.createPrivateMessage(AppController.Current.account.accessToken, textbox_postText.Text, receiver, machineOnly: 0, parse_links:true);
                        }
                    }
                    if (response.Item2.success)
                    {
                        clear_postbox();
                    }
                    else
                    {
                        showErrorMessage(response.Item2);
                    }
                }
            }
        }

        public void clear_postbox(bool dontDetectTab = false)
        {
            AppController.Current.account.updateItems();
            currentPostTarget = new CurrentPostTarget();
            textbox_postText.textBoxContent.Text = "";
            textblock_composeOverlay.Text = "";
            image_addImage.Opacity = 0.6;
            updateCurrentTarget(dontDetectTab);
        }

        private void button_trashPost_Click_1(object sender, RoutedEventArgs e)
        {
            clear_postbox();
        }

        private void textbox_postText_GotFocus_1(object sender, RoutedEventArgs e)
        {
            textblock_enterPostHere.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void textbox_postText_LostFocus_1(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(textbox_postText.Text)) {
                textblock_enterPostHere.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public TabItem addTabControlWithListOfItems(string header, string name, System.Collections.ObjectModel.ObservableCollection<Chapper.Model.Item> source, string iconSource = null, bool showCloseButton = false, string tooltip = null)
        {
            Grid grid = new Grid();
            TabItem newItem = new TabItem();
            try
            {
                newItem.Name = name;
            }
            catch
            {
                newItem.Name = "tab" + tabCounter.ToString();
                tabCounter++;
            }
            if(tooltip == null) {
                tooltip = header;
            }
            
            StackPanel headerBlock = new StackPanel();
            headerBlock.Orientation = Orientation.Horizontal;
            headerBlock.ToolTip = header;
            if (header.Length > 16)
            {
                header = header.Substring(0, 13) + "...";
            }
            
            if (iconSource != null)
            {
                headerBlock.Orientation = Orientation.Horizontal;
                System.Windows.Controls.Image tabitemImage = new System.Windows.Controls.Image();
                //tabitemImage.Source = System.Windows.Application.Current.Resources[iconSource] as ImageSource;
                tabitemImage.SetResourceReference(System.Windows.Controls.Image.SourceProperty, iconSource); 
                
                if(!string.IsNullOrEmpty(header)) {
                    tabitemImage.Height = 12.0;
                }
                else
                {
                    tabitemImage.Height = 16.0;
                }
                tabitemImage.Margin = new Thickness(0, 0, 4, 0);
                headerBlock.Children.Add(tabitemImage);
            }

            if(!string.IsNullOrEmpty(header)) {
            TextBlock headerText = new TextBlock();
                headerText.Text = header;
                headerBlock.Children.Add(headerText);
            }

            if (showCloseButton)
            {
                Controls.CloseButton closeButton = getTabItemCloseButton(newItem);
                
                headerBlock.Children.Add(closeButton);
            }
            newItem.Header = headerBlock;
            Controls.ListBoxItems listbox = new Controls.ListBoxItems();
            listbox.Name = "lb_" + name;
            listbox.listbox_items.ItemsSource = source;
            source.CollectionChanged += source_CollectionChanged;
            grid.Children.Add(listbox);
            newItem.Content = grid;
            this.tabControl_main.Items.Add(newItem);
            return newItem;
        }

        void source_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void addTabControlWithUserInfo(User user)
        {
            Grid grid = new Grid();
            TabItem newItem = new TabItem();
            newItem.Name = "user_" + user.username;
            newItem.DataContext = user;
            newItem.Header = getHeaderWithCloseButton(newItem, "@" + user.username);
            Controls.UserInfo userInfo = new Controls.UserInfo(user);
            grid.Children.Add(userInfo);
            newItem.Content = grid;
            this.tabControl_main.Items.Add(newItem);
            this.tabControl_main.SelectedItem = newItem;
        }

        public void addTabControlHashtagSearch(string hashtag)
        {
            if (hashtag.StartsWith("#"))
            {
                hashtag = hashtag.TrimStart('#');
            }
            System.Collections.ObjectModel.ObservableCollection<Item> collection = new System.Collections.ObjectModel.ObservableCollection<Item>();
            Tuple<List<AppNetDotNet.Model.Post>, AppNetDotNet.ApiCalls.ApiCallResponse> response = AppNetDotNet.ApiCalls.Posts.getTaggedPosts(hashtag);
            if (response.Item2.success)
            {
                foreach (AppNetDotNet.Model.Post post in response.Item1)
                {
                    collection.Add(new Item(post));
                }
                addTabControlWithListOfItems("#" + hashtag, "hashtag_" + hashtag, collection, tooltip: "#" + hashtag, showCloseButton: true);
            }
        }

        private object getHeaderWithCloseButton(TabItem tabitem, string title)
        {
            StackPanel stackPanel = new StackPanel();
            TextBlock headerTextblock = new TextBlock();
            headerTextblock.Inlines.Add(title);
            headerTextblock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            Controls.CloseButton closeButton = getTabItemCloseButton(tabitem);
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Children.Add(headerTextblock);
            stackPanel.Children.Add(closeButton);
            return stackPanel;
        }

        private Controls.CloseButton getTabItemCloseButton(TabItem tabitem)
        {
            Controls.CloseButton closeButton = new Controls.CloseButton();
            Button buttonCloseTabControl = closeButton.button_close;
            closeButton.Width = 10.0;
            closeButton.Height = 10.0;
            buttonCloseTabControl.CommandParameter = tabitem;
            buttonCloseTabControl.Click += buttonCloseTabControl_Click;
            buttonCloseTabControl.ToolTip = "Close this tab";
            closeButton.Margin = new Thickness(5, 0, 0, 0);
            closeButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            return closeButton;
        }

        void buttonCloseTabControl_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && e != null)
            {
                Button button = sender as Button;
                if (button != null)
                {
                    TabItem tabitem = button.CommandParameter as TabItem;
                    if(tabitem != null) 
                    {
                        if (this.tabControl_main.Items.Contains(tabitem))
                        {
                            if (lastTab != null)
                            {
                                if (tabControl_main.Items.Contains(lastTab) && tabitem == this.tabControl_main.SelectedItem)
                                {
                                    tabControl_main.SelectedItem = lastTab;
                                }
                                else
                                {
                                    tabControl_main.SelectedIndex = 0;
                                }
                            }

                            if (tabitem.DataContext != null)
                            {
                                if (tabitem.DataContext.GetType() == typeof(Model.Search))
                                {
                                    AppController.Current.account.remove_search(tabitem.DataContext as Search);
                                }
                            }

                            this.tabControl_main.Items.Remove(tabitem);
                            tabitem = null;
                        }
                    }
                }
            }
        }

        public void textbox_postText_PreviewKeyDown_1(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Return || e.Key == Key.Enter) && Keyboard.Modifiers == ModifierKeys.Control) {
                if (!string.IsNullOrEmpty(textbox_postText.textBoxContent.Text))
                {
                    if (textbox_postText.textBoxContent.Text.Length <= 255)
                    {
                        button_sendPost_Click_1(null, null);
                    }
                }
            }
        }

        public Item getTopMostItem(string type)
        {
            if (type == "myStream")
            {
                return this.listbox_myStream.GetCurrentlyTopmostShownItem();
            }
            else if (type == "mentions")
            {
                return this.listbox_mentions.GetCurrentlyTopmostShownItem();
            }
            else if (type == "pms")
            {
                return this.listbox_privateMessages.GetCurrentlyTopmostShownItem();
            }
            return null;
        }

        public void scrollItemIntoView(string type, Item item)
        {
            if (type == "myStream")
            {
                this.listbox_myStream.ScrollItemToTop(item);
            }
            else if (type == "mentions")
            {
                this.listbox_mentions.ScrollItemToTop(item);
            }
            else if (type == "pms")
            {
                this.listbox_privateMessages.ScrollItemToTop(item);
            }
        }

        private void tabControl_main_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                updateCurrentTarget();
            }
        }

        public void updateCurrentTarget(bool dontDetectTab = false)
        {
            TabItem tabitem = tabControl_main.SelectedItem as TabItem;
            if (tabitem != null)
            {
                textblock_enterPostHere.Text = "Enter your post...";
                if (currentPostTarget.isPm)
                {
                    textblock_enterPostHere.Text = "Enter your private message...";
                }
                textblock_composeOverlay.Text = "";
                lastTab = currentTab;
                currentTab = tabitem;
                if (tabitem.Name.StartsWith("user_") && !dontDetectTab && string.IsNullOrEmpty(textbox_postText.textBoxContent.Text))
                {
                    User user = tabitem.DataContext as User;
                    textbox_postText.textBoxContent.Text = "@" + user.username + " ";
                }
                else if (tabitem.Name.StartsWith("channel_") && !dontDetectTab)
                {
                    IChapperCollection collection = tabitem.DataContext as IChapperCollection;
                    currentPostTarget.channelName = collection.displayName;
                    currentPostTarget.channelId = tabitem.Name;
                    textblock_composeOverlay.Text = "Send message to " + collection.displayName;
                    textblock_enterPostHere.Text = "Enter your message...";
                }
                else if (tabitem.Name == "tabItem_patter" && !dontDetectTab)
                {
                    PatterRoom patterRoom = combobox_patterrooms.SelectedItem as PatterRoom;
                    if (patterRoom != null)
                    {
                        currentPostTarget.channelName = patterRoom.displayName;
                        currentPostTarget.channelId = patterRoom.id;
                        textblock_composeOverlay.Text = "Send message to Patter room " + patterRoom.displayName;
                        textblock_enterPostHere.Text = "Enter your message...";
                    }
                    else
                    {
                        textblock_composeOverlay.Text = "Error getting channel name";
                    }
                }
                else if (tabitem.Name == "tabItem_pm" && !dontDetectTab)
                {
                    PrivateMessageChannel privateMessageChannel = combobox_private_message_channel.SelectedItem as PrivateMessageChannel;
                    if (privateMessageChannel != null)
                    {
                        currentPostTarget.channelName = privateMessageChannel.displayName;
                        currentPostTarget.channelId = privateMessageChannel.id;
                        textblock_composeOverlay.Text = "Send private message to " + privateMessageChannel.displayName;
                        textblock_enterPostHere.Text = "Enter your message...";
                    }
                    else
                    {
                        textblock_composeOverlay.Text = "Error getting channel name";
                    }
                }
                else
                {
                    currentPostTarget.channelName = null;
                    currentPostTarget.channelId = null;
                    if (currentPostTarget.pmToUser != null)
                    {
                        textblock_composeOverlay.Text = "Write private message to @" + currentPostTarget.pmToUser.username;
                        textblock_enterPostHere.Text = "Enter your message...";
                    }
                    if (currentPostTarget.replyToItem != null)
                    {
                        textblock_composeOverlay.Text  = "Write reply post to @" + currentPostTarget.replyToItem.user.username;
                    }
                    else
                    {
                        textblock_composeOverlay.Text = "";
                    }
                }

                if (currentPostTarget.channelName != null)
                {
                    maxNumberOfChars = AppController.Current.account.configuration.message.text_max_length;
                }
                else
                {
                    maxNumberOfChars = AppController.Current.account.configuration.post.text_max_length;
                }

                textbox_postText_TextChanged_1(null, null);
            }
        }


        private void button_openSettings_Click_1(object sender, RoutedEventArgs e)
        {
            UserInterface.Preferences preferences = new UserInterface.Preferences();
            preferences.Left = this.Left;
            preferences.Top = this.Top;
            preferences.Show();
        }

        private void button_addImage_Click_1(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".png"; // Default file extension
            dlg.Filter = "Images (*.png,*.jpg,*.gif,*.tif)|*.png;*.jpeg;*.jpg;*.gif;*.tif;*.tiff"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string filename = dlg.FileName;
                if (System.IO.File.Exists(filename))
                {
                    currentPostTarget.filePathOfToBeAddedFile = filename;
                    image_addImage.Opacity = 1.0;
                    textbox_postText.textBoxContent.Text = textbox_postText.textBoxContent.Text.Insert(textbox_postText.textBoxContent.CaretIndex, "[" + System.IO.Path.GetFileNameWithoutExtension(currentPostTarget.filePathOfToBeAddedFile) + "](photos.app.net/{post_id}/1)");
                }
            }
        }

        private void window_main_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.windowWidth = this.Width;
            Properties.Settings.Default.windowHeight = this.Height;
            Properties.Settings.Default.windowLocationX = this.Left;
            Properties.Settings.Default.windowLocationY = this.Top;
            Properties.Settings.Default.Save();
        }

        private void button_close_window_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void border_around_main_window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && 
                Mouse.DirectlyOver == border_around_main_window)
                this.DragMove();
        }

        private void button_minimize_window_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void thumb_resize_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {

            this.Width = Math.Max(this.Width + e.HorizontalChange,this.MinWidth);
            this.Height = Math.Max(this.Height + e.VerticalChange,this.MinHeight);
        }

        private void button_addLink_Click(object sender, RoutedEventArgs e)
        {
            UserInterface.AddLink add_link_window = new UserInterface.AddLink();
            add_link_window.Left = this.Left;
            add_link_window.Top = this.Top + this.Height - 172;
            add_link_window.Width = this.Width;
            add_link_window.Height = 172.0;
            add_link_window.InsertLink += add_link_window_InsertLink;
            add_link_window.Show();
        }

        void add_link_window_InsertLink(object sender, UserInterface.AddLink.InsertLinkEventArgs e)
        {
            if (e != null)
            {
                if (e.success)
                {
                    textbox_postText.textBoxContent.Text = textbox_postText.textBoxContent.Text.Insert(textbox_postText.textBoxContent.CaretIndex, e.insert_string);
                }
            }
        }

        private void button_error_dismiss_Click(object sender, RoutedEventArgs e)
        {
            dispatcherHideError_Tick(null, null);
        }

        private void sendIssue(AppNetDotNet.Model.Annotations.Issue issue)
        {
            if (issue == null && currentError != null)
            {
                currentError = null;
            }
            if (issue != null)
            {
                UserInterface.Report_issue issue_window = new UserInterface.Report_issue(issue);
                issue_window.Show();
            }
        }

        private void sendIssue(AppNetDotNet.ApiCalls.ApiCallResponse issue)
        {
            if (issue != null)
            {
                UserInterface.Report_issue issue_window = new UserInterface.Report_issue(issue);
                issue_window.Show();
            }
        }

        private void button_error_send_to_lighun_Click(object sender, RoutedEventArgs e)
        {
            sendIssue(currentError);
        }


        public class CurrentPostTarget
        {
            public bool isPm { get; set; }
            public bool isMessage { get; set; }
            public bool isPrivateChannel { get; set; }
            public string channelId { get; set; }
            public string channelName { get; set; }
            public Item replyToItem { get; set; }
            public User pmToUser { get; set; }
            public string filePathOfToBeAddedFile { get; set; }

        }

        private void button_filter_Click(object sender, RoutedEventArgs e)
        {
            UserInterface.Filter filter_window = new UserInterface.Filter();
            filter_window.Left = this.Left;
            filter_window.Top = this.Top + this.Height - 130;
            filter_window.Width = this.Width;
            filter_window.Height = 130.0;
            filter_window.Show();
        }

        public void filter_update()
        {
            List<TreeView> all_treeviews = new List<TreeView>();
            all_treeviews.Add(listbox_myStream.listbox_items);
            all_treeviews.Add(listbox_mentions.listbox_items);
            all_treeviews.Add(listbox_privateMessages.listbox_items);
            all_treeviews.Add(listbox_globalStream.listbox_items);
            all_treeviews.Add(listbox_patter.listbox_items);

            if (string.IsNullOrWhiteSpace(AppController.Current.current_filter_text) && (string.IsNullOrWhiteSpace(AppController.Current.current_filter_language) || AppController.Current.current_filter_language == "---"))
            {
                this.button_filter.Opacity = 0.6;
            }
            else
            {
                this.button_filter.Opacity = 1;
            }

            foreach (TreeView treeview in all_treeviews)
            {
                if (treeview != null)
                {
                    treeview.Items.Filter = delegate(object obj)
                      {
                          Item item = obj as Item;
                          if (item == null)
                          {
                              return false;
                          }

                          bool filter_text = true;
                          bool filter_language = true;
                          bool filter_language_exclude_empty = false;
                          if (AppController.Current.current_filter_language_exclude_empty == true)
                          {
                              filter_language_exclude_empty = true;
                          }


                          if (string.IsNullOrWhiteSpace(AppController.Current.current_filter_text))
                          {
                              filter_text = true;
                          }
                          else
                          {

                              if (string.IsNullOrEmpty(item.text) && string.IsNullOrEmpty(item.displayName))
                              {
                                  filter_text = false;
                              }
                              else
                              {

                                  if (item.text.ToLower().Contains(AppController.Current.current_filter_text.ToLower()) || item.displayName.ToLower().Contains(AppController.Current.current_filter_text.ToLower()))
                                  {
                                      filter_text = true;
                                  }
                                  else
                                  {
                                      filter_text = false;
                                  }
                              }
                          }

                          if (string.IsNullOrWhiteSpace(AppController.Current.current_filter_language))
                          {
                              filter_language = true;
                          }
                          else if (AppController.Current.current_filter_language == "---")
                          {
                              filter_language = true;
                          }
                          else
                          {
                              if (string.IsNullOrEmpty(item.language) && filter_language_exclude_empty)
                              {
                                  filter_language = true;
                              }
                              else if (string.IsNullOrEmpty(item.language))
                              {
                                  filter_language = false;
                              }
                              else if (item.language.ToLower().StartsWith(AppController.Current.current_filter_language.ToLower()))
                              {
                                  filter_language = true;
                              }
                              else
                              {
                                  filter_language = false;
                              }
                          }

                          return (filter_text && filter_language);
                      };
                }

            }
        }

        private void combobox_language_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init_completed)
            {
                Properties.Settings.Default.language_posts = combobox_language.SelectedItem as string;
                if (Properties.Settings.Default.language_posts == "---")
                {
                    Properties.Settings.Default.language_posts = "";
                }
            }
        }

        private void button_search_Click(object sender, RoutedEventArgs e)
        {
            UserInterface.Search search_window = new UserInterface.Search();
            search_window.Left = this.Left;
            search_window.Top = this.Top + this.Height - 130;
            search_window.Width = this.Width;
            search_window.Height = 130.0;
            search_window.Show();
        }

        private void OpaqueClickableImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.nymphicusapp.com/chapper/purchase-a-license/");
        }

    }

}
