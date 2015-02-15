using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.Xml;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace UpdateCheck
{
    /// <summary>
    /// Interaction logic for UpdateAvailable.xaml
    /// </summary>
    public partial class UpdateAvailable : Window
    {
        public string UpdateCheckUrl { get; set; }
        public static string DownloadUrlNewVersion { get; set; }
        public event updateCheckDoneEventHandler updateCheckDone;
        public delegate void updateCheckDoneEventHandler(object sender, updateCheckDoneEventArgs e);
        public class updateCheckDoneEventArgs : EventArgs
        {
            public bool tryNextTimeAgain
            {
                get;
                set;
            }
            public bool updateFound { get; set; }
            public bool closeApp { get; set; }
            public string toBeIgnoredVersion { get; set; }
            public string title { get; set; }
            public string text { get; set; }
        }

        updateCheckDoneEventArgs doneArgs;

        private string newVersion = "";

        public UpdateAvailable(string newVersionString, string titleLable, string text) {
            InitializeComponent();

            doneArgs = new updateCheckDoneEventArgs();
            doneArgs.tryNextTimeAgain = true;
            doneArgs.updateFound = false;
            doneArgs.closeApp = false;

            label_oldNew.Content = titleLable;
            textBox_news.Text = text;
            newVersion = newVersionString;

            Show();
        }

        public static updateCheckDoneEventArgs checkNow(string updateCheckUrl, string downloadUrlOfUpdate, Version installedVersion, string userAgent, string rootElement) {
            DownloadUrlNewVersion = downloadUrlOfUpdate;
            updateCheckDoneEventArgs doneArgs = new updateCheckDoneEventArgs();
            doneArgs = new updateCheckDoneEventArgs();
            doneArgs.tryNextTimeAgain = true;
            doneArgs.updateFound = false;
            doneArgs.closeApp = false;

            XmlDocument XMLdoc = null;
            HttpWebRequest request;
            HttpWebResponse response = null;
            try
            {

                request = (HttpWebRequest)WebRequest.Create(string.Format(updateCheckUrl));

                request.UserAgent = userAgent;
                response = (HttpWebResponse)request.GetResponse(); 
                XMLdoc = new XmlDocument();
                XMLdoc.Load(response.GetResponseStream()); 

                string availableVersionString = XMLdoc.SelectSingleNode(rootElement + "/Version").InnerText;

                if (availableVersionString == Properties.Settings.Default.IgnoredNewVersion)
                {
                    // this version shall be ignored
                    return doneArgs;
                }
                Version availableVersion = new Version(availableVersionString);
                if (availableVersion > installedVersion)
                {
                    doneArgs.title = string.Format("Installed version: {0}  - now available: {1}", Converter.prettyVersion.getNiceVersionString(installedVersion.ToString()), Converter.prettyVersion.getNiceVersionString(availableVersionString));
                    doneArgs.text = XMLdoc.SelectSingleNode(rootElement + "/Description").InnerText;
                    doneArgs.updateFound = true;
                    doneArgs.toBeIgnoredVersion = availableVersionString;
                }
                else
                {

                    doneArgs.updateFound = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                doneArgs.updateFound = false;
            }
            return doneArgs;
        }

        private void button_getUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                System.Diagnostics.Process.Start(DownloadUrlNewVersion);

                doneArgs.closeApp = true;
                doneArgs.updateFound = true;
                updateCheckDone(this, doneArgs);
                Close();
            }
            catch 
            {
                
            }
        }

        private void button_ignore_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)checkBox_remindMeAgain.IsChecked)
            {
                doneArgs.tryNextTimeAgain = false;
                doneArgs.toBeIgnoredVersion = newVersion;
                updateCheckDone(this, doneArgs);
            }
            Close();
        }
    }
}
