using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QrssUploader
{
    public partial class Form1 : Form
    {
        DateTime lastUpload = new DateTime(0);
        bool IsRecentlyUploaded { get { return (DateTime.UtcNow - lastUpload).Seconds < 5; } }

        public Form1()
        {
            InitializeComponent();
            btnLoad_Click(null, null);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private string Obfuscate(string message)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(message)).Trim('=');
        }

        private string DeObfuscate(string obfuscated)
        {
            try
            {
                while (obfuscated.Length % 4 != 0)
                    obfuscated += "=";
                return Encoding.UTF8.GetString(Convert.FromBase64String(obfuscated));
            }
            catch
            {
                return null;
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            tbServer.Text = Properties.Settings.Default["server"].ToString();
            tbUser.Text = Properties.Settings.Default["username"].ToString();
            tbPass.Text = DeObfuscate(Properties.Settings.Default["password"].ToString());
            tbLocal.Text = Properties.Settings.Default["localPath"].ToString();
            tbRemote.Text = Properties.Settings.Default["remotePath"].ToString();
            lblStatus2.Text = "Loaded settings from user folder.";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default["server"] = tbServer.Text;
            Properties.Settings.Default["username"] = tbUser.Text;
            Properties.Settings.Default["password"] = Obfuscate(tbPass.Text);
            Properties.Settings.Default["localPath"] = tbLocal.Text;
            Properties.Settings.Default["remotePath"] = tbRemote.Text;
            Properties.Settings.Default.Save();
            lblStatus2.Text = "Saved settings to user folder.";
        }

        private void UploadNow()
        {
            if (IsRecentlyUploaded)
                return;
            else
                lastUpload = DateTime.UtcNow;

            string localFilePath = tbLocal.Text;
            if (!File.Exists(localFilePath))
            {
                MessageBox.Show($"File does not exist:\n{localFilePath}", 
                    "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string ftpUrl = $"{tbServer.Text}{tbRemote.Text}";
            using (var client = new WebClient())
            {
                client.Credentials = new NetworkCredential(tbUser.Text, tbPass.Text);
                lblStatus2.Text = "Connecting...";
                Stopwatch sw = Stopwatch.StartNew();
                client.UploadFile(ftpUrl, localFilePath);
                lblStatus2.Text = $"{lblTime.Text}: Uploaded {Path.GetFileName(localFilePath)} in {sw.Elapsed.TotalSeconds:N2} seconds";
                lastUpload = DateTime.UtcNow;
            }

            using (StreamWriter w = File.AppendText("log.txt"))
            {
                w.WriteLine($"{DateTime.UtcNow.ToLongTimeString()}: Uploaded {ftpUrl}");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.UtcNow;
            int secIntoTenMinute = (now.Minute % 10) * 60 + now.Second;
            int secUntilNextTenMinute = 10 * 60 - secIntoTenMinute;

            lblTime.Text = $"{now.Hour:D2}:{now.Minute:D2}:{now.Second:D2} UTC";

            btnTest.Enabled = !IsRecentlyUploaded;

            if (cbAutomatic.Checked)
            {
                pbTimeIntoTenMinute.Value = secIntoTenMinute;

                if (secIntoTenMinute < nudDelay.Value)
                    lblStatus.Text = $"Delaying {nudDelay.Value - secIntoTenMinute} more seconds...";
                else if (secIntoTenMinute == nudDelay.Value)
                    UploadNow();
                else
                    lblStatus.Text = $"Next upload in {secUntilNextTenMinute} seconds...";
            }
            else
            {
                pbTimeIntoTenMinute.Value = 0;
                lblStatus.Text = "Automatic uploading disabled";
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            btnTest.Enabled = false;
            UploadNow();
        }
    }
}
