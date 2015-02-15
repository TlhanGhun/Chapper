using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Controls;

namespace Chapper.Model
{
    public class InteractionEntry : INotifyPropertyChanged
    {
        public AppNetDotNet.Model.Interaction  apnInteraction { get; set; }
        private BackgroundWorker backgroundWorkerTimeAgo;
        private Timer timerHumanReadableAgo;


        public InteractionEntry(AppNetDotNet.Model.Interaction interaction)
        {
            if(interaction == null) {
                return;
            }

            backgroundWorkerTimeAgo = new BackgroundWorker();
            backgroundWorkerTimeAgo.WorkerReportsProgress = false;
            backgroundWorkerTimeAgo.WorkerSupportsCancellation = true;
            backgroundWorkerTimeAgo.DoWork += new DoWorkEventHandler(backgroundWorkerTimeAgo_DoWork);
            backgroundWorkerTimeAgo.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerTimeAgo_RunWorkerCompleted); 


            this.apnInteraction = interaction;
                displayDate = String.Format("{0:F}", interaction.event_date);
                if (timerHumanReadableAgo != null)
                {
                    timerHumanReadableAgo.Dispose();
                }
                if (!backgroundWorkerTimeAgo.IsBusy)
                {
                    backgroundWorkerTimeAgo.RunWorkerAsync(new backgroundWorkerTimeAgoArgument(createdAt, null));
                }


                
        }
    

        public string displayDate { get; set; }

        public DateTime createdAt
        {
            get
            {
                return apnInteraction.event_date;
            }
        }

        public string main_avatar
        {
            get
            {
                string iconPath = "";
                switch (apnInteraction.action)
                {
                        
                    case "star":
                        iconPath = System.Windows.Application.Current.Resources["interaction_star"] as string;
                        return iconPath;

                    case "reply":
                        iconPath = System.Windows.Application.Current.Resources["interaction_reply"] as string;
                        return iconPath;
                        
                    case "follow":
                        iconPath = System.Windows.Application.Current.Resources["interaction_follow"] as string;
                        return iconPath;
                    
                    case "repost":
                        iconPath = System.Windows.Application.Current.Resources["interaction_repost"] as string;
                        return iconPath;
    
                    default:
                        return main_user.avatar_image.url;                        
                }
            }
        }

        public AppNetDotNet.Model.User main_user
        {
            get
            {
                return apnInteraction.users[0];
            }
        }

        public AppNetDotNet.Model.Post main_post
        {
            get
            {
                if (apnInteraction.posts != null)
                {
                    if (apnInteraction.posts.Count > 0)
                    {
                        return apnInteraction.posts[0];
                    }
                }
                return null;
            }
        }

        public string action_description
        {
            get
            {
                switch (apnInteraction.action)
                {
                    case "star":
                        return "starred your post:";

                    case "reply":
                        return "replied to your post:";

                    case "follow":
                        return "started following you.";

                    case "repost":
                        return "reposted your post:";

                    default:
                        return apnInteraction.ToString();
                }
            }
        }

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
