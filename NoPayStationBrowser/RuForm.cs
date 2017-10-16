using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace NoPayStationBrowser
{
    public partial class NoPayStationBrowser : Form
    {

        List<Item> currentDatabase = new List<Item>();
        List<Item> gamesDbs = new List<Item>();
        List<Item> dlcsDbs = new List<Item>();

        public NoPayStationBrowser()
        {
            InitializeComponent();
            new Settings();

            if (string.IsNullOrEmpty(Settings.instance.pkgPath))
            {
                MessageBox.Show("You need to perform initial configuration", "Whops!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Options o = new Options();
                o.ShowDialog();
            }
            else if (!File.Exists(Settings.instance.pkgPath))
            {
                MessageBox.Show("You are missing your pkg_dec.exe", "Whops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Options o = new Options();
                o.ShowDialog();
            }


            //   Helpers.ImgDownloader.GetImage();

        }



        /// <summary>
        /// Exit Application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Aboutbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>



        private void NoPayStationBrowser_Load(object sender, EventArgs e)
        {

            gamesDbs = LoadDatabase("https://docs.google.com/spreadsheets/d/18PTwQP7mlwZH1smpycHsxbEwpJnT8IwFP7YZWQT7ZSs/export?format=tsv&id=18PTwQP7mlwZH1smpycHsxbEwpJnT8IwFP7YZWQT7ZSs&gid=1180017671");



            dlcsDbs = LoadDatabase("https://docs.google.com/spreadsheets/d/18PTwQP7mlwZH1smpycHsxbEwpJnT8IwFP7YZWQT7ZSs/export?format=tsv&id=18PTwQP7mlwZH1smpycHsxbEwpJnT8IwFP7YZWQT7ZSs&gid=743196745");

            currentDatabase = gamesDbs;

            RefreshList(currentDatabase);
            if (Settings.instance.records != 0)
            {
                var _new = gamesDbs.Count - Settings.instance.records;
                if (_new > 0)
                    label1.Text += " (" + _new.ToString() + " new since last launch)";
            }

            Settings.instance.records = gamesDbs.Count;

        }

        List<Item> LoadDatabase(string path)
        {
            List<Item> dbs = new List<Item>();
            WebClient wc = new WebClient();
            string content = wc.DownloadString(new Uri(path));
            wc.Dispose();
            content = Encoding.UTF8.GetString(Encoding.Default.GetBytes(content));

            string[] lines = content.Split(new string[] { "\r\n", "\n\r", "\n", "\r" }, StringSplitOptions.None);

            for (int i = 1; i < lines.Length; i++)
            {
                var a = lines[i].Split('\t');
                var itm = new Item(a[0], a[1], a[2], a[3], a[4]);
                if (!itm.zRfi.ToLower().Contains("missing") && itm.pkg.ToLower().Contains("http://"))
                    dbs.Add(itm);
            }

            dbs = dbs.OrderBy(i => i.TitleName).ToList();

            return dbs;
        }


        void RefreshList(List<Item> items)
        {
            label1.Text = items.Count + " items";
            listView1.Items.Clear();
            foreach (var item in items)
            {
                var a = new ListViewItem(item.TitleId);
                a.SubItems.Add(item.Region);
                a.SubItems.Add(item.TitleName);
                a.Tag = item;

                listView1.Items.Add(a);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            List<Item> itms = new List<Item>();

            foreach (var item in currentDatabase)
            {
                if (item.CompareName(textBox1.Text))
                    itms.Add(item);
            }

            RefreshList(itms);
        }

        Item currentDownload = null;

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Cancel")
            {
                currentDownloadWebClient.CancelAsync();
            }
            else
            {
                if (string.IsNullOrEmpty(Settings.instance.downloadDir) || string.IsNullOrEmpty(Settings.instance.pkgPath))
                {
                    MessageBox.Show("You don't have proper config", "Whops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Options o = new Options();
                    o.ShowDialog();
                    return;
                }

                if (!File.Exists(Settings.instance.pkgPath))
                {
                    MessageBox.Show("You missing your pkg dec", "Whops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Options o = new Options();
                    o.ShowDialog();
                    return;
                }

                if (listView1.SelectedItems.Count == 0) return;
                //button1.Enabled = false;
                button1.Text = "Cancel";
                var a = (listView1.SelectedItems[0].Tag as Item);
                DownloadFile(a);
            }
        }

        WebClient currentDownloadWebClient = null;

        private void DownloadFile(Item item)
        {
            try
            {
                currentDownload = item;
                WebClient webClient = new WebClient();
                currentDownloadWebClient = webClient;
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompleted);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                webClient.DownloadProgressChanged += (sender, e) => progressChangeForSpeed(e.BytesReceived);
                webClient.DownloadFileAsync(new Uri(item.pkg), Settings.instance.downloadDir + "\\" + item.TitleId + ".pkg");
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        DateTime lastUpdate;
        long lastBytes = 0;

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
                label3.Text = bytesPerSecond.ToString() + " KB/s";
            else
            {
                label3.Text = ((float)((float)bytesPerSecond / 1024)).ToString("0.00") + " MB/s";
            }

        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            label2.Text = ((float)((float)e.BytesReceived / (1024 * 1024))).ToString("0.00") + " MB / " + ((float)((float)e.TotalBytesToReceive / (1024 * 1024))).ToString("0.00") + " MB";
            progressBar1.Value = e.ProgressPercentage;
        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {

            button1.Text = "Download and unpack";
            label2.Text = "";
            label3.Text = "";
            progressBar1.Value = 0;
            button1.Enabled = true;
            if (!e.Cancelled)
            {
                System.Diagnostics.ProcessStartInfo a = new System.Diagnostics.ProcessStartInfo();
                a.WorkingDirectory = Settings.instance.downloadDir + "\\";
                a.FileName = string.Format("\"{0}\"", Settings.instance.pkgPath);
                a.Arguments = Settings.instance.pkgParams.ToLower().Replace("{pkgfile}", "\"" + Settings.instance.downloadDir + "\\" + currentDownload.TitleId + ".pkg\"").Replace("{zrifkey}", currentDownload.zRfi);
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = a;

                proc.Start();
            }
            else
            {
                File.Delete(Settings.instance.downloadDir + "\\" + currentDownload.TitleId + ".pkg");
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Options o = new Options();
            o.ShowDialog();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                currentDatabase = dlcsDbs;
                textBox1_TextChanged(null, null);
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                currentDatabase = gamesDbs;
                textBox1_TextChanged(null, null);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            Item itm = (listView1.SelectedItems[0].Tag as Item);

            Helpers.Renascene r = new Helpers.Renascene(itm.TitleId);

            if (r.imgUrl != null)
            {
                pictureBox1.LoadAsync(r.imgUrl);
                label5.Text = r.ToString();
            }
            else
            {
                pictureBox1.Image = null;
                label5.Text = "";
            }


        }
    }


}
