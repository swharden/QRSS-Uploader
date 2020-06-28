using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QrssUploader
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Log($"QRSS Uploader {version.Major}.{version.Minor}", false);
            btnLoad_Click(null, null);
        }

        private void Log(string message, bool newLine = true)
        {
            Debug.WriteLine(message);
            if (newLine)
                richTextBox1.AppendText(Environment.NewLine);
            richTextBox1.AppendText(message);
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            tbServer.Text = Properties.Settings.Default.server;
            tbUsername.Text = Properties.Settings.Default.username;
            tbPassword.Text = DeObfuscate(Properties.Settings.Default.password);
            tbRemotePath.Text = Properties.Settings.Default.remotePath;
            lbLocalPaths.Items.Clear();
            lbLocalPaths.Items.AddRange(
                Properties.Settings.Default.localPaths.Split(';')
                .Where(x => string.IsNullOrWhiteSpace(x) == false)
                .ToArray());
            Log("Loaded default settings");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.server = tbServer.Text;
            Properties.Settings.Default.username = tbUsername.Text;
            Properties.Settings.Default.password = Obfuscate(tbPassword.Text);
            Properties.Settings.Default.remotePath = tbRemotePath.Text;

            string[] paths = new string[lbLocalPaths.Items.Count];
            for (int i = 0; i < paths.Length; i++)
                paths[i] = lbLocalPaths.Items[i].ToString();

            Properties.Settings.Default.localPaths = string.Join(";", paths);
            Properties.Settings.Default.Save();

            Log("Saved default settings");
        }

        private string Obfuscate(string message)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(message)).Trim('=');
        }

        private string DeObfuscate(string obfuscated)
        {
            while (obfuscated.Length % 4 != 0)
                obfuscated += "=";
            return Encoding.UTF8.GetString(Convert.FromBase64String(obfuscated));
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog diag = new OpenFileDialog();
            diag.Filter = "Images|*.jpeg;*.jpeg;*.gif;*.png;*.bmp";
            diag.Filter += "|All files (*.*)|*.*";
            diag.Title = "Select Grabber Image";
            if (diag.ShowDialog() == DialogResult.OK)
            {
                lbLocalPaths.Items.Add(diag.FileName);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lbLocalPaths.SelectedIndex >= 0)
                lbLocalPaths.Items.RemoveAt(lbLocalPaths.SelectedIndex);
        }
    }
}
