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
using System.Windows.Shapes;
using AppNetDotNet;
using AppNetDotNet.Model;

namespace Chapper.UserInterface
{
    /// <summary>
    /// Interaction logic for Report_issue.xaml
    /// </summary>
    public partial class Report_issue : Window
    {
        public AppNetDotNet.Model.Annotations.Issue issue { get; set; }

        public Report_issue(AppNetDotNet.Model.Annotations.Issue to_be_reported_issue)
        {
            InitializeComponent();

            issue = to_be_reported_issue;

            fillTextboxes();
        }
        public Report_issue(AppNetDotNet.ApiCalls.ApiCallResponse apiCallReponse)
        {
            InitializeComponent();

            issue = new AppNetDotNet.Model.Annotations.Issue();
            if(apiCallReponse != null) {
                issue.title = apiCallReponse.errorMessage;
                issue.description = apiCallReponse.errorDescription;
            }
            fillTextboxes();
        }

        private void fillTextboxes()
        {
            textbox_title.Text = issue.title;
            textbox_description.Text = issue.description;
            textbox_stacktrace.Text = issue.stacktrace;
        }


        private void button_send_Click(object sender, RoutedEventArgs e)
        {
            List<string> receivers = new List<string>();
            receivers.Add("lighun");

            issue.title = textbox_title.Text;
            issue.description = textbox_description.Text;
            issue.stacktrace = textbox_stacktrace.Text;
            issue.user_comment = textbox_comment.Text;
            issue.guid = Guid.NewGuid().ToString();
            issue.state = "new";

            List<IAnnotation> annotations = new List<IAnnotation>();
            Annotation annotation = new Annotation();
            annotation.type = "de.li-ghun.issue";
            annotation.value = issue;
            annotation.parsedObject = null;
            annotations.Add(annotation);

            string text = issue.title;
            if (string.IsNullOrEmpty(text))
            {
                text = issue.description;
            }
            if (string.IsNullOrEmpty(text))
            {
                text = issue.user_comment;
            }
            if (string.IsNullOrEmpty(text))
            {
                text = issue.stacktrace;
            }
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Please provide some informations first", "Not enough infos", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            if (text.Length > 255)
            {
                text = text.Substring(0, 255);
            }
            Tuple<AppNetDotNet.Model.Message, AppNetDotNet.ApiCalls.ApiCallResponse> message_response = AppNetDotNet.ApiCalls.Messages.create(AppController.Current.account.accessToken, text, "14981", receivers);
            if (checkbox_subscribe_to_issue_channel.IsChecked != true && message_response.Item2.success)
            {
                AppNetDotNet.ApiCalls.Channels.Subscriptions.unsubscribe(AppController.Current.account.accessToken, "14981");
            }
            if (checkbox_subscribe_to_issue_channel.IsChecked == true && message_response.Item2.success)
            {
                AppNetDotNet.ApiCalls.Channels.Subscriptions.subscribe(AppController.Current.account.accessToken, "14981");
            }
        }
    }
}
