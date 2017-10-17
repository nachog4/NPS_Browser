using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace NPS
{
    public class DownloadWorker
    {
        public Item currentDownload;
        private WebClient webClient;
        private DateTime lastUpdate;
        private long lastBytes;
        public ProgressBar progress = new ProgressBar();
        public ListViewItem lvi;

        public bool isRunning { get; private set; }
        public bool isCompleted { get; private set; }
        public bool isCanceled { get; private set; }
        Timer timer = new Timer();

        public DownloadWorker(Item itm)
        {
            currentDownload = itm;
            lvi = new ListViewItem(itm.TitleName);
            lvi.SubItems.Add("Waiting");
            lvi.SubItems.Add("");
            lvi.SubItems.Add("");
            lvi.Tag = this;
            isRunning = false;
            isCanceled = false;
            isCompleted = false;

            timer.Interval = 1000;
            timer.Tick += Timer_Tick;

        }



        public void Start()
        {
            Console.WriteLine("start process " + currentDownload.TitleName);
            timer.Start();
            isRunning = true;
            DownloadFile(/*currentDownload*/);
        }

        public void Cancel()
        {
            timer.Stop();
            this.isCanceled = true;

            if (webClient != null)
                webClient.CancelAsync();
            lvi.SubItems[1].Text = "";
            lvi.SubItems[2].Text = "Canceled";
            progress.Value = 0;
        }

        public void DeletePkg()
        {
            if (currentDownload != null)
                if (File.Exists(Settings.instance.downloadDir + "\\" + currentDownload.TitleId + ".pkg"))
                    File.Delete(Settings.instance.downloadDir + "\\" + currentDownload.TitleId + ".pkg");
            progress.Value = 0;
        }

        public void Unpack()
        {
            if (!this.isCompleted) return;
            System.Diagnostics.ProcessStartInfo a = new System.Diagnostics.ProcessStartInfo();
            a.WorkingDirectory = Settings.instance.downloadDir + "\\";
            a.FileName = string.Format("\"{0}\"", Settings.instance.pkgPath);
            a.Arguments = Settings.instance.pkgParams.ToLower().Replace("{pkgfile}", "\"" + Settings.instance.downloadDir + "\\" + currentDownload.TitleId + ".pkg\"").Replace("{zrifkey}", currentDownload.zRfi);
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = a;
            proc.Start();
        }

        private void DownloadFile(/*Item item*/)
        {
            try
            {
                FetchFileName();
                //currentDownload = item;
                webClient = new WebClient();
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompleted);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadProgressChanged += (sender, e) => progressChangeForSpeed(e.BytesReceived);
                webClient.DownloadFileAsync(new Uri(currentDownload.pkg), Settings.instance.downloadDir + "\\" + currentDownload.TitleId + ".pkg");
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        void FetchFileName()
        {
            int count = 1;
            string orgTitle = currentDownload.TitleId;

            while (File.Exists(Settings.instance.downloadDir + "\\" + currentDownload.TitleId + ".pkg"))
            {
                currentDownload.TitleId = orgTitle + "_" + count;
                count++;
            }


        }

        private string currentSpeed = "Queued", progressString = "Queued";
        int percent = 0;
        private void progressChangeForSpeed(long bytes)
        {
            if (lastBytes == 0)
            {
                lastUpdate = DateTime.Now;
                lastBytes = bytes;
                return;
            }

            var now = DateTime.Now;
            var timeSpan = now - lastUpdate;
            var bytesChange = bytes - lastBytes;
            if (timeSpan.Seconds == 0) return;
            var bytesPerSecond = bytesChange / timeSpan.Seconds;
            lastBytes = bytes;
            lastUpdate = now;

            bytesPerSecond = bytesPerSecond / 1024;
            if (bytesPerSecond < 1500)
                currentSpeed = bytesPerSecond.ToString() + " KB/s";
            else
            {
                currentSpeed = ((float)((float)bytesPerSecond / 1024)).ToString("0.00") + " MB/s";
            }

        }
        //  
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressString = ((float)((float)e.BytesReceived / (1024 * 1024))).ToString("0.00") + " MB / " + ((float)((float)e.TotalBytesToReceive / (1024 * 1024))).ToString("0.00") + " MB";
            //progressBar1.Value = e.ProgressPercentage;
            percent = e.ProgressPercentage;
        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            timer.Stop();

            isRunning = false;

            if (!e.Cancelled)
            {
                this.isCompleted = true;
                Unpack();

                lvi.SubItems[1].Text = "";
                lvi.SubItems[2].Text = "Completed";
                progress.Value = 100;
            }
            else
            {
                lvi.SubItems[1].Text = "";
                lvi.SubItems[2].Text = "Canceled";

            }

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            lvi.SubItems[1].Text = currentSpeed;
            lvi.SubItems[2].Text = progressString;
            progress.Value = percent;

        }

    }


}
