using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace UpdateCheck
{
    public class Check
    {
        private BackgroundWorker backgroundWorkerCheckUpdate;
        private Version currentlyInstalledVersion;
        private string updateUrl;
        private string downloadUrl;
        private string rootXmlElement;
        private string userAgentInRequests;

        public Check(string updateCheckUrl, string downloadUrlOfUpdate, Version version, string userAgent, string rootElement)
        {
            currentlyInstalledVersion = version;
            updateUrl = updateCheckUrl;
            downloadUrl = downloadUrlOfUpdate;
            userAgentInRequests = userAgent;
            rootXmlElement = rootElement;
            backgroundWorkerCheckUpdate = new BackgroundWorker();
            backgroundWorkerCheckUpdate.WorkerSupportsCancellation = true;
            backgroundWorkerCheckUpdate.DoWork += new DoWorkEventHandler(backgroundWorkerCheckUpdate_DoWork);
            backgroundWorkerCheckUpdate.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerCheckUpdate_RunWorkerCompleted);
            backgroundWorkerCheckUpdate.RunWorkerAsync();
        }

        void backgroundWorkerCheckUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateAvailable.updateCheckDoneEventArgs doneArgs = UpdateAvailable.checkNow(updateUrl, downloadUrl, currentlyInstalledVersion, userAgentInRequests, rootXmlElement);
            e.Result = doneArgs;
        }

        void myUpdateCheck_updateCheckDone(object sender, UpdateAvailable.updateCheckDoneEventArgs e)
        {
            if (!e.tryNextTimeAgain)
            {
                Properties.Settings.Default.IgnoredNewVersion = e.toBeIgnoredVersion;
            }
            else if (e.closeApp)
            {
                App.Current.Shutdown();
            }
        }

        void backgroundWorkerCheckUpdate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateAvailable.updateCheckDoneEventArgs doneArgs = e.Result as UpdateAvailable.updateCheckDoneEventArgs;
            if (doneArgs.updateFound)
            {
                UpdateAvailable myUpdateCheck = new UpdateAvailable(doneArgs.toBeIgnoredVersion, doneArgs.title, doneArgs.text);
                myUpdateCheck.updateCheckDone += new UpdateAvailable.updateCheckDoneEventHandler(myUpdateCheck_updateCheckDone);
            }
        }
    }
}
