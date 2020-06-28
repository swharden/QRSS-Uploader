using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QrssUploader
{
    public partial class TenMinTimer : UserControl
    {

        public TenMinTimer()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e) => UpdateDisplay(false);
        private void cbEnable_CheckedChanged(object sender, EventArgs e) => UpdateDisplay(true);
        private void nudDelay_ValueChanged(object sender, EventArgs e) => UpdateDisplay(true);

        private DateTime RoundTenMinutes(DateTime dt, bool roundUp = false)
        {
            dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
            int minutesOver = dt.Minute % 10;
            dt = dt.AddMinutes(-minutesOver);
            if (roundUp)
                dt = dt.AddMinutes(10);
            return dt;
        }

        private string GetStamp(DateTime dt) =>
            $"{dt.Hour:D2}:{dt.Minute:D2}:{dt.Second:D2}";

        private void UpdateDisplay(bool force)
        {
            // determine what second it is and if it is novel
            DateTime timeNow = DateTime.UtcNow;
            string thisSecond = GetStamp(timeNow);
            if (thisSecond == lblTimeNow.Text && force == false)
                return;
            else
                lblTimeNow.Text = thisSecond;

            // determine when next upload should be
            DateTime timeNextUpload = RoundTenMinutes(timeNow, true);
            timeNextUpload = timeNextUpload.AddSeconds((int)nudDelay.Value);
            if ((timeNextUpload - timeNow).TotalSeconds > 600)
                timeNextUpload = timeNextUpload.AddMinutes(-10);
            lblTimeNext.Text = GetStamp(timeNextUpload);
            int secondsRemaining = (int)(timeNextUpload - timeNow).TotalSeconds;
            progress.Value = (cbEnable.Checked) ? 600 - secondsRemaining : 0;

            // decide whether to upload now
            bool isTenMinute = timeNow.Minute % 10 == 0;
            bool isCorrectSecond = timeNow.Second == nudDelay.Value;
            if (isTenMinute && isCorrectSecond)
                Upload();
        }

        private void Upload()
        {
            Console.WriteLine($"{DateTime.UtcNow} UPLOADING!");
        }
    }
}
