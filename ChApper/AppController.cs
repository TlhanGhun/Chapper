using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chapper.Model;
using AppNetDotNet.Model;
using AppNetDotNet.ApiCalls;
using System.Windows;
using System.Threading;
using UpdateCheck;

namespace Chapper
{
    class AppController
    {
        public static AppController Current;
        public Account account { get; set; }
        public static string appDataPath { get; set; }
        public static string appProgramPath { get; set; }
        public static string themesPath { get; set; }
        public Layouts.Layout CurrentLayout { get; set; }
        private bool switchTabActive = true;
        public string current_filter_text { get; set; }
        public string current_filter_language { get; set; }
        public bool? current_filter_language_exclude_empty { get; set; }

        public static ThreadSaveObservableCollection<string> SavedUsernames = new ThreadSaveObservableCollection<string>();
        public static ThreadSaveObservableCollection<string> SavedHashtags = new ThreadSaveObservableCollection<string>();
        public static List<Themes.ThemeInfo> Themes = new List<Chapper.Themes.ThemeInfo>();

        MainWindow mainwindow;

        public static void Start()
        {

            if (Current == null)
            {
                Current = new AppController();
            }
        }

        private AppController()
        {
            Current = this;
            appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\liGhun\\Chapper\\";
            themesPath = appDataPath + "themes\\";
            if (!System.IO.Directory.Exists(themesPath))
            {
                System.IO.Directory.CreateDirectory(themesPath);
            }
            appProgramPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);

            current_filter_language_exclude_empty = false;
            init_languages();

            mainwindow = new MainWindow();
            mainwindow.Closing += mainwindow_Closing;



            CurrentLayout = new Layouts.Layout();

            Chapper.Themes.ThemeInfo defaultLight = new Chapper.Themes.ThemeInfo();
            defaultLight.name = "DefaultLight";
            defaultLight.filepath = @"Themes/DefaultLight.xaml";
            defaultLight.author = "Sven Walther";
            defaultLight.description = "The default light theme embedded into Chapper";
            defaultLight.useDefaultDarkItems = true;
            Themes.Add(defaultLight);

            Chapper.Themes.ThemeInfo defaultDark = new Chapper.Themes.ThemeInfo();
            defaultDark.name = "DefaultDark";
            defaultDark.filepath = @"Themes/DefaultDark.xaml";
            defaultDark.author = "Sven Walther";
            defaultDark.description = "The default dark theme embedded into Chapper";
            defaultDark.useDefaultDarkItems = false;
            Themes.Add(defaultDark);

            string[] themeFiles = System.IO.Directory.GetFiles(themesPath, "*xaml");
            if (themeFiles != null)
            {
                foreach (string theme in themeFiles)
                {
                    Chapper.Themes.ThemeInfo themeInfo = new Chapper.Themes.ThemeInfo();
                    themeInfo.name = System.IO.Path.GetFileNameWithoutExtension(theme);
                    themeInfo.filepath = theme;
                    string themeInfoPath = themesPath + "\\" + themeInfo.name + ".info";
                    if (System.IO.File.Exists(themeInfoPath))
                    {
                        try
                        {
                            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Chapper.Themes.ThemeInfo));
                            Chapper.Themes.ThemeInfo loadedInfo = new Themes.ThemeInfo();
                            using (System.IO.FileStream file_stream = new System.IO.FileStream(themeInfoPath, System.IO.FileMode.Open))
                            {
                                loadedInfo = (Chapper.Themes.ThemeInfo)serializer.Deserialize(file_stream);
                            }
                            loadedInfo.name = themeInfo.name;
                            loadedInfo.filepath = themeInfo.filepath;
                            themeInfo = loadedInfo;
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show("Can't read the contents of the theme info file of " + themeInfo.name + "\r\n" + exp.Message, "Error loading theme info", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    Themes.Add(themeInfo);
                }
            }



            try
            {
                if (!Properties.Settings.Default.settingsUpdated)
                {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.settingsUpdated = true;
                }
            }
            catch
            {
                try
                {
                    Properties.Settings.Default.Reset();
                }
                catch { }
            }




            //Properties.Settings.Default.accessToken = null;
            if (string.IsNullOrEmpty(Properties.Settings.Default.accessToken))
            {
                account = new Account();
                AppNetDotNet.Model.Authorization.clientSideFlow apnClientAuthProcess = new Authorization.clientSideFlow(ApiKeys.appNetClientID, ApiKeys.appNetRedirectUri, "basic stream write_post follow messages files update_profile");
                apnClientAuthProcess.AuthSuccess += apnClientAuthProcess_AuthSuccess;
                apnClientAuthProcess.showAuthWindow();
            }
            else
            {
                bool authSuccess = false;
                account = new Account();
                account.accessToken = Properties.Settings.Default.accessToken;
                if (account.verifyCredentials())
                {
                    if (account.token.scopes.Contains("files"))
                    {
                        authSuccess = true;
                        openMainWindow();
                    }
                }

                if (!authSuccess)
                {
                    AppNetDotNet.Model.Authorization.registerAppInRegistry(Authorization.registerBrowserEmulationValue.IE8Always, alsoCreateVshostEntry: true);
                    AppNetDotNet.Model.Authorization.clientSideFlow apnClientAuthProcess = new Authorization.clientSideFlow(ApiKeys.appNetClientID, ApiKeys.appNetRedirectUri, "basic stream write_post follow messages files update_profile");
                    apnClientAuthProcess.AuthSuccess += apnClientAuthProcess_AuthSuccess;
                    apnClientAuthProcess.showAuthWindow();
                }
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.theme_currentPath))
            {
                Chapper.Themes.ThemeInfo theme = null;
                IEnumerable<Chapper.Themes.ThemeInfo> themes = Themes.Where(t => t.filepath == Properties.Settings.Default.theme_currentPath);
                if (themes != null)
                {
                    if (themes.Count() > 0)
                    {
                        theme = themes.First();
                        load_theme(theme);
                    }
                }
            }


        }

        void apnClientAuthProcess_AuthSuccess(object sender, AppNetDotNet.AuthorizationWindow.AuthEventArgs e)
        {
            if (e != null)
            {
                if (e.success)
                {
                    account.accessToken = e.accessToken;
                    if (account.verifyCredentials())
                    {
                        Properties.Settings.Default.accessToken = account.accessToken;
                        openMainWindow();
                        return;
                    }
                    else
                    {
                        MessageBox.Show(":(", "Authorization failed");
                    }
                }
                else
                {
                    MessageBox.Show(e.error, "Authorization failed");
                }
                AppNetDotNet.Model.Authorization.clientSideFlow apnClientAuthProcess = new AppNetDotNet.Model.Authorization.clientSideFlow(ApiKeys.appNetClientID, ApiKeys.appNetRedirectUri, "basic stream write_post follow messages");
                apnClientAuthProcess.AuthSuccess += apnClientAuthProcess_AuthSuccess;
                apnClientAuthProcess.showAuthWindow();
            }

        }

        private void openMainWindow()
        {
           
            account.registerSnarlClasses(appProgramPath + "\\Images\\Artwork\\ChapperIcon.png");
            mainwindow.listbox_myStream.listbox_items.ItemsSource = account.myStream;
            mainwindow.listbox_mentions.listbox_items.ItemsSource = account.mentions;
            mainwindow.listbox_privateMessages.listbox_items.DataContext = account;
            mainwindow.listbox_privateMessages.listbox_items.ItemsSource = account.privateMessages;
            mainwindow.listbox_interactions.listbox_items.ItemsSource = account.interactions;

            setMainwindowTitle();

            if (Properties.Settings.Default.useUnifiedStream)
            {
                mainwindow.image_headerStream.ToolTip = "Unified Stream";
            }

            mainwindow.tabItem_patter.Visibility = Visibility.Collapsed;
            account.patterChannels.CollectionChanged += patterChannels_CollectionChanged;
            account.privateMessagesChannels.CollectionChanged += privateMessagesChannels_CollectionChanged;
            mainwindow.combobox_patterrooms.ItemsSource = account.patterChannels;
            mainwindow.combobox_patterrooms.SelectionChanged += combobox_patterrooms_SelectionChanged;
            mainwindow.combobox_private_message_channel.ItemsSource = account.privateMessagesChannels;
            mainwindow.combobox_private_message_channel.SelectionChanged += combobox_private_message_channel_SelectionChanged;

            mainwindow.Show();

            if (Properties.Settings.Default.showGeneralStream)
            {
                mainwindow.listbox_globalStream.listbox_items.ItemsSource = account.globalStream;
            }
            else
            {
                mainwindow.tabItem_global.Visibility = Visibility.Collapsed;
            }

            if (!Properties.Settings.Default.showPatterRooms)
            {
                mainwindow.tabItem_patter.Visibility = Visibility.Collapsed;
            }

            if (Properties.Settings.Default.saved_searches != null)
            {
                foreach (string query in Properties.Settings.Default.saved_searches)
                {
                    Search search = new Search(query, query, access_token: account.accessToken);
                    search.save_search = true;
                    search.auto_refresh = true;
                    AppController.Current.addTabControlWithListOfItems(search.displayName, "tabSearch" + search.id, search.items, icon_source: "image_search", showCloseButton: true, switch_to_this_tab: true, datacontext:search);
                    account.add_search(search);
                }
            }

            startUpdating();
        }


        void patterChannels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (account.patterChannels.Count() > 0)
            {
                mainwindow.tabItem_patter.Visibility = Visibility.Visible;
                if (mainwindow.combobox_patterrooms.SelectedItem == null)
                {
                    switchTabActive = false;
                    mainwindow.combobox_patterrooms.SelectedIndex = 0;
                }
            }
        }

        void privateMessagesChannels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (account.privateMessagesChannels.Count() > 0)
            {
                mainwindow.tabItem_pm.Visibility = Visibility.Visible;
                if (mainwindow.combobox_private_message_channel.SelectedItem == null)
                {
                    switchTabActive = false;
                    mainwindow.combobox_private_message_channel.SelectedIndex = 0;
                }
            }
            else
            {
                mainwindow.tabItem_pm.Visibility = Visibility.Collapsed;
            }

            if (account.patterChannels.Count() > 0)
            {
                mainwindow.tabItem_patter.Visibility = Visibility.Visible;
                if (mainwindow.combobox_patterrooms.SelectedItem == null)
                {
                    switchTabActive = false;
                    mainwindow.combobox_patterrooms.SelectedIndex = 0;
                }
            }
            else
            {
                mainwindow.tabItem_patter.Visibility = Visibility.Collapsed;
            }
        }
    

        void combobox_patterrooms_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            PatterRoom patterRoom = mainwindow.combobox_patterrooms.SelectedItem as PatterRoom;
            if (patterRoom != null)
            {
                mainwindow.listbox_patter.listbox_items.ItemsSource = patterRoom.items;
                if (switchTabActive)
                {
                    mainwindow.tabItem_patter.IsSelected = true;
                }
                else
                {
                    switchTabActive = true;
                }
                mainwindow.updateCurrentTarget();
            }
        }

        void combobox_private_message_channel_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            PrivateMessageChannel channel = mainwindow.combobox_private_message_channel.SelectedItem as PrivateMessageChannel;
            if (channel != null)
            {
                mainwindow.listbox_privateMessages.listbox_items.ItemsSource = channel.items;
                if (switchTabActive)
                {
                    mainwindow.tabItem_pm.IsSelected = true;
                }
                else
                {
                    switchTabActive = true;
                }
                mainwindow.updateCurrentTarget();
            }
        }

        public void addHashtagSearch(string hashtag)
        {
            mainwindow.addTabControlHashtagSearch(hashtag);
        }

        public void setMainwindowTitle()
        {
            if (mainwindow != null)
            {
                mainwindow.Title = "Chapper " + UpdateCheck.Converter.prettyVersion.getNiceVersionString(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()) + " - @" + account.username;
            }
        }

        private void startUpdating()
        {
            account.updateItems();
            mainwindow.startDispatcherTimer();
        }



        void mainwindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }



        public void composeReplyTo(Item item)
        {
            if (mainwindow != null)
            {
                if (item != null)
                {
                    if (item.user != null)
                    {
                        mainwindow.clear_postbox();
                        mainwindow.currentPostTarget.pmToUser = null;
                        mainwindow.currentPostTarget.channelId = null;
                        mainwindow.currentPostTarget.replyToItem = item;
                        if (item.apnMessage != null)
                        {
                            mainwindow.currentPostTarget.pmToUser = item.user;
                            mainwindow.currentPostTarget.isPm = true;
                            mainwindow.currentPostTarget.channelId = "pm";
                        }
                        
                        mainwindow.textbox_postText.Text = "@" + item.user.username + " ";
                        if (item.entities != null)
                        {
                            if (item.entities.mentions != null)
                            {
                                foreach (Entities.Mention mention in item.entities.mentions)
                                {
                                    if (mention.name != account.username && mention.name != item.user.username)
                                    {
                                        mainwindow.textbox_postText.Text += "@" + mention.name + " ";
                                    }
                                }
                            }
                        }
                        mainwindow.textblock_composeOverlay.Text = "Reply to: " + item.ToString().Replace(System.Environment.NewLine, " ");
                        mainwindow.textbox_postText.Focus();
                        mainwindow.textbox_postText.textBoxContent.CaretIndex = mainwindow.textbox_postText.Text.Length;
                    }
                }
            }
        }

        public void composeMessageTo(Item item)
        {
            if (mainwindow != null)
            {
                if (item != null)
                {
                    if (item.user != null)
                    {
                        composeMessageTo(item.user);
                    }
                }
            }
        }


        public void composeMessageTo(User user)
        {
            if (mainwindow != null)
            {
                if (user != null)
                {
                    mainwindow.currentPostTarget = new MainWindow.CurrentPostTarget();
                    mainwindow.clear_postbox(dontDetectTab: true);
                    mainwindow.currentPostTarget.pmToUser = user;
                    mainwindow.currentPostTarget.isPm = true;
                    mainwindow.textblock_composeOverlay.Text = "Private message to @" + user.username;
                    mainwindow.textblock_enterPostHere.Text = "Enter your private message...";
                    mainwindow.textbox_postText.Focus();
                }
            }
        }

        public Item getTopMostItem(string type)
        {
            if (mainwindow != null)
            {
                return mainwindow.getTopMostItem(type);
            }
            return null;
        }

        public void scrollItemIntoView(string type, Item item)
        {
            if (mainwindow != null)
            {
                mainwindow.scrollItemIntoView(type, item);
            }
        }

        public void showUserInfo(string username)
        {
            Tuple<User, ApiCallResponse> user = AppNetDotNet.ApiCalls.Users.getUserByUsernameOrId(account.accessToken, username);
            if (user.Item2.success)
            {
                showUserInfo(user.Item1);
            }
        }

        public void showUserInfo(User user)
        {
            if (user == null)
            {
                return;
            }
            mainwindow.addTabControlWithUserInfo(user);
        }

        public void addNewChapperCollection(IChapperCollection collection, bool switchToThisTab = false)
        {
            string tabName = collection.id;
            if (!tabName.StartsWith("conversation"))
            {
                tabName = "channel_" + tabName;
            }
            string icon_source = null;
            if (collection.GetType() == typeof(PatterRoom))
            {
                icon_source = "header_patter";
            }
            else if (collection.GetType() == typeof(PrivateMessageChannel))
            {
                icon_source = "header_pms";
            }
            else
            {
                icon_source = "header_messageroom";
                System.Windows.Controls.TabItem tabitem = mainwindow.addTabControlWithListOfItems(collection.displayName, tabName, collection.items, iconSource: icon_source, showCloseButton: true);
                tabitem.DataContext = collection;
                if (switchToThisTab)
                {
                    mainwindow.tabControl_main.SelectedItem = tabitem;
                }
            }
            
        }

        public void addTabControlWithListOfItems(string header, string name, System.Collections.ObjectModel.ObservableCollection<Chapper.Model.Item> source, string icon_source = null, bool showCloseButton = false, string tooltip = null, bool switch_to_this_tab = false, object datacontext = null)
        {
            System.Windows.Controls.TabItem tabitem = mainwindow.addTabControlWithListOfItems(header, name, source, iconSource: icon_source, showCloseButton: showCloseButton, tooltip:tooltip);
            if (datacontext == null)
            {
                tabitem.DataContext = source;
            }
            else
            {
                tabitem.DataContext = datacontext;
            }
            if (switch_to_this_tab)
            {
                mainwindow.tabControl_main.SelectedItem = tabitem;
            }
        }

        public void textbox_post_isChanging(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            mainwindow.textbox_postText_TextChanged_1(sender, e);
        }

        public void textbox_post_preview_key_down(object sender, System.Windows.Input.KeyEventArgs e)
        {
            mainwindow.textbox_postText_PreviewKeyDown_1(sender, e);
        }

        public static void load_theme(Themes.ThemeInfo theme)
        {

            string path = theme.filepath;

            if (!System.IO.File.Exists(path) && !path.StartsWith("Themes"))
            {
                MessageBox.Show("The theme file has not been found in " + path, "Theme not found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            System.Windows.Application.Current.Resources.MergedDictionaries.Clear();


            ResourceDictionary defaults = new ResourceDictionary();
            defaults.Source = new Uri(@"Themes/GeneralDefaults.xaml", UriKind.Relative);
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(defaults);

            if (theme.useDefaultDarkItems)
            {
                ResourceDictionary defaultButtonsDark = new ResourceDictionary();
                defaultButtonsDark.Source = new Uri(@"Themes/DefaultDarkButtons.xaml", UriKind.Relative);
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(defaultButtonsDark);
            }

            ResourceDictionary skin = new ResourceDictionary();
            if (path.StartsWith("Themes"))
            {
                skin.Source = new Uri(path, UriKind.Relative);
                Properties.Settings.Default.theme_currentPath = path;
            }
            else
            {
                try
                {
                    string tempFile = System.IO.Path.GetTempFileName() + ".xaml";
                    System.IO.File.Copy(path, tempFile);
                    string theme_content = System.IO.File.ReadAllText(tempFile);
                    theme_content = theme_content.Replace("%THEMES_DIR%", AppController.themesPath);
                    System.IO.File.WriteAllText(tempFile, theme_content);
                    skin.Source = new Uri(tempFile, UriKind.Absolute);
                    Properties.Settings.Default.theme_currentPath = path;
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message, "Loading of theme failed");
                }

            }
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(skin);
        }

        public void setPostComposeText(string text)
        {
            mainwindow.textbox_postText.textBoxContent.Text = text;
        }

        public void set_filter_text(string text)
        {
            current_filter_text = text;
            mainwindow.filter_update();
        }

        public void set_filter_language(string language, bool? exclude_untagged)
        {
            current_filter_language = language;
            current_filter_language_exclude_empty = exclude_untagged;
            mainwindow.filter_update();
        }

        public static List<string> available_languages { get; set; }

        private void init_languages()
        {
            available_languages = new List<string>();
            available_languages.Add("---");
            available_languages.Add("ar");
            available_languages.Add("az");
            available_languages.Add("bg");
            available_languages.Add("bn");
            available_languages.Add("bs");
            available_languages.Add("ca");
            available_languages.Add("cs");
            available_languages.Add("cy");
            available_languages.Add("da");
            available_languages.Add("de");
            available_languages.Add("el");
            available_languages.Add("en");
            available_languages.Add("en_GB");
            available_languages.Add("es");
            available_languages.Add("es_AR");
            available_languages.Add("es_MX");
            available_languages.Add("es_NI");
            available_languages.Add("et");
            available_languages.Add("eu");
            available_languages.Add("ea");
            available_languages.Add("fa");
            available_languages.Add("fi");
            available_languages.Add("fr");
            available_languages.Add("fy_NL");
            available_languages.Add("ga");
            available_languages.Add("gl");
            available_languages.Add("he");
            available_languages.Add("hi");
            available_languages.Add("hr");
            available_languages.Add("hu");
            available_languages.Add("id");
            available_languages.Add("is");
            available_languages.Add("it");
            available_languages.Add("ja");
            available_languages.Add("ka");
            available_languages.Add("kk");
            available_languages.Add("km");
            available_languages.Add("kn");
            available_languages.Add("ko");
            available_languages.Add("lt");
            available_languages.Add("lv");
            available_languages.Add("mk");
            available_languages.Add("ml");
            available_languages.Add("mn");
            available_languages.Add("nb");
            available_languages.Add("ne");
            available_languages.Add("nl");
            available_languages.Add("nn");
            available_languages.Add("na");
            available_languages.Add("no");
            available_languages.Add("pl");
            available_languages.Add("pt");
            available_languages.Add("pt_BR");
            available_languages.Add("ro");
            available_languages.Add("ru");
            available_languages.Add("sk");
            available_languages.Add("sl");
            available_languages.Add("sq");
            available_languages.Add("sr");
            available_languages.Add("sr_Latn");
            available_languages.Add("sv");
            available_languages.Add("sw");
            available_languages.Add("ta");
            available_languages.Add("te");
            available_languages.Add("th");
            available_languages.Add("tr");
            available_languages.Add("tt");
            available_languages.Add("uk");
            available_languages.Add("ur");
            available_languages.Add("vi");
            available_languages.Add("zh_CN");
            available_languages.Add("zh_TW");
            
        }
    }
}
