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
    public partial class Options : Form
    {


        public Options()
        {
            InitializeComponent();
        }

        private void Options_Load(object sender, EventArgs e)
        {
            textDownload.Text = Settings.instance.downloadDir;
            textPKGPath.Text = Settings.instance.pkgPath;
            textParams.Text = Settings.instance.pkgParams;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    textDownload.Text = fbd.SelectedPath;
                    Settings.instance.downloadDir = textDownload.Text;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var fbd = new OpenFileDialog())
            {
                fbd.Filter = "|*.exe";

                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                {
                    textPKGPath.Text = fbd.FileName;
                    Settings.instance.pkgPath = textPKGPath.Text;
                }
            }
        }

        private void Options_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.instance.pkgParams = textParams.Text;
            Settings.instance.Store();
        }


    }
}
