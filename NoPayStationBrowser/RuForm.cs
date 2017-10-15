using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using Microsoft.Win32;

namespace NoPayStationBrowser
{
    public partial class NoPayStationBrowser : Form
    {

        List<Item> database = new List<Item>();

        public NoPayStationBrowser()
        {
            InitializeComponent();
            new Settings();

            if (Settings.instance.pkgPath == null)
            {
                MessageBox.Show("You need to perform initial configuration", "Whops!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Options o = new Options();
                o.ShowDialog();
            }

            //Console.WriteLine(Settings.instance.pkgParams);
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
            WebClient wc = new WebClient();

            // http://msdn.microsoft.com/en-us/library/ms144200.aspx
            string priceList = wc.DownloadString(
                new Uri("https://docs.google.com/spreadsheets/d/18PTwQP7mlwZH1smpycHsxbEwpJnT8IwFP7YZWQT7ZSs/export?format=tsv&id=18PTwQP7mlwZH1smpycHsxbEwpJnT8IwFP7YZWQT7ZSs&gid=1180017671"));

            wc.Dispose(); // Free resources

            priceList = Encoding.UTF8.GetString(Encoding.Default.GetBytes(priceList));

            string[] lines = priceList.Split(new string[] {
        "\r\n",
        "\n\r", // Rarely ever used, but be prepared
        "\n", // Match this and
        "\r" }, // this last
            StringSplitOptions.None);

            for (int i = 1; i < lines.Length; i++)
            {
                var a = lines[i].Split('\t');
                var itm = new Item(a[0], a[1], a[2], a[3], a[4]);
                if (!itm.zRfi.ToLower().Contains("missing") && itm.pkg.ToLower().Contains("http://"))
                    database.Add(itm);
            }


            database = database.OrderBy(i => i.TitleName).ToList();

            RefreshList(database);

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

            foreach (var item in database)
            {
                if (item.CompareName(textBox1.Text))
                    itms.Add(item);
            }

            RefreshList(itms);
        }
        Item currentDownload = null;

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.instance.downloadDir) || string.IsNullOrEmpty(Settings.instance.pkgPath))
            {
                MessageBox.Show("You don't have proper config", "Whops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Options o = new Options();
                o.ShowDialog();
                return;
            }

            if (listView1.SelectedItems.Count == 0) return;
            button1.Enabled = false;
            button1.Text = "Downloading...";
            var a = (listView1.SelectedItems[0].Tag as Item);
            DownloadFile(a);
        }


        private void DownloadFile(Item item)
        {
            try
            {
                currentDownload = item;
                WebClient webClient = new WebClient();
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

            System.Diagnostics.ProcessStartInfo a = new System.Diagnostics.ProcessStartInfo();
            a.WorkingDirectory = Settings.instance.downloadDir + "\\";
            a.FileName = "CMD.exe";
            a.Arguments = "/C " + Settings.instance.pkgPath + " " + Settings.instance.pkgParams.ToLower().Replace("{pkgfile}", Settings.instance.downloadDir + "\\" + currentDownload.TitleId + ".pkg").Replace("{zrifkey}", currentDownload.zRfi);
            // MessageBox.Show(a.Arguments);
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = a;

            proc.Start();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Options o = new Options();
            o.ShowDialog();
        }
    }


    class Item
    {
        public string TitleId, Region, TitleName, zRfi, pkg;

        public Item(string TitleId, string Region, string TitleName, string pkg, string zrif)
        {
            this.TitleId = TitleId;
            this.Region = Region;
            this.TitleName = TitleName;
            this.pkg = pkg;
            this.zRfi = zrif;
        }

        public bool CompareName(string name)
        {
            name = name.ToLower();

            if (this.TitleId.ToLower().Contains(name)) return true;
            if (this.TitleName.ToLower().Contains(name)) return true;
            return false;
        }
    }

    public class Settings
    {
        const string userRoot = "HKEY_CURRENT_USER\\SOFTWARE";
        const string subkey = "NoPayStationBrowser";
        const string keyName = userRoot + "\\" + subkey;

        public static Settings instance;

        public string defaultRegion;
        public string downloadDir;
        public string pkgPath;
        public string pkgParams;

        public Settings()
        {
            instance = this;
            defaultRegion = Registry.GetValue(keyName, "region", "ALL")?.ToString();
            downloadDir = Registry.GetValue(keyName, "downloadDir", "")?.ToString();
            pkgPath = Registry.GetValue(keyName, "pkgPath", "")?.ToString();
            pkgParams = Registry.GetValue(keyName, "pkgParams", "{pkgFile} --make-dirs=ux --license='{zRifKey}'")?.ToString();

            if (pkgParams == null) pkgParams = "{pkgFile} --make-dirs=ux --license=\"{zRifKey}\"";
            if (defaultRegion == null) defaultRegion = "ALL";


        }

        public void Store()
        {
            if (downloadDir != null)
                Registry.SetValue(keyName, "downloadDir", downloadDir);
            if (pkgPath != null)
                Registry.SetValue(keyName, "pkgPath", pkgPath);
            if (pkgParams != null)
                Registry.SetValue(keyName, "pkgParams", pkgParams);
        }

    }
}
