using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        HashSet<string> regions = new HashSet<string>();

        List<DownloadWorker> downloads = new List<DownloadWorker>();

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
                {
                    dbs.Add(itm);
                    regions.Add(itm.Region);
                }
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


        private void button1_Click(object sender, EventArgs e)
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
            var a = (listView1.SelectedItems[0].Tag as Item);


            foreach (var d in downloads)
                if (d.currentDownload == a)
                    return; //already downloading


            DownloadWorker dw = new DownloadWorker(a);
            listViewEx1.Items.Add(dw.lvi);
            listViewEx1.AddEmbeddedControl(dw.progress, 3, listViewEx1.Items.Count - 1);
            downloads.Add(dw);

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

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listViewEx1.SelectedItems.Count == 0) return;
            (listViewEx1.SelectedItems[0].Tag as DownloadWorker).Cancel();
            (listViewEx1.SelectedItems[0].Tag as DownloadWorker).DeletePkg();

        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (listViewEx1.SelectedItems.Count == 0) return;
            (listViewEx1.SelectedItems[0].Tag as DownloadWorker).DeletePkg();

        }

        private void retryUnpackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewEx1.SelectedItems.Count == 0) return;

            (listViewEx1.SelectedItems[0].Tag as DownloadWorker).Unpack();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int workingThreads = 0;
            foreach (var dw in downloads)
            {
                if (!dw.isCompleted && dw.isRunning)
                    workingThreads++;
            }

            if (workingThreads < 2)
            {
                foreach (var dw in downloads)
                {
                    if (!dw.isCompleted && !dw.isRunning && !dw.isCanceled)
                    {
                        dw.Start();
                        break;
                    }
                }
            }
        }

        private void clearCompletedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<DownloadWorker> toDel = new List<DownloadWorker>();
            List<ListViewItem> toDelLVI = new List<ListViewItem>();

            foreach (var i in downloads)
            {
                if (i.isCompleted || i.isCanceled)
                {
                    toDel.Add(i);
                }
            }


            foreach (ListViewItem i in listViewEx1.Items)
            {
                if (toDel.Contains(i.Tag as DownloadWorker))
                    toDelLVI.Add(i);

            }

            foreach (var i in toDel)
                downloads.Remove(i);
            toDel.Clear();

            foreach (var i in toDelLVI)
                listViewEx1.Items.Remove(i);
            toDelLVI.Clear();


        }
    }


}
