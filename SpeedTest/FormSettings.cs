#region Using
// Imported namespaces
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SpeedTest.Properties;
#endregion
namespace SpeedTest
{
    /// <summary>The class representing the Configuration Settings Form of the application.</summary>
    public partial class FormSettings : Form
    {
        #region Constructor
        /// <summary>Initializes a new instance of the FormSettings class.</summary>
        public FormSettings()
        {
            // This call is required by the designer
            InitializeComponent();

            // Initialize FormSettings properties
            Font = SystemFonts.MenuFont;

            // Initialize child controls
            RadioAutoDownload.Checked = Settings.Default.AutoDownload;
            RadioCustomDownload.Checked = !Settings.Default.AutoDownload;
            TextDownloadURL.Text = Settings.Default.DownloadURL;

            RadioAutoDetectIP.Checked = Settings.Default.AutoIPDetect;
            RadioCustomIPDetect.Checked = !Settings.Default.AutoIPDetect;
            TextIPDetectURL.Text = Settings.Default.IPDetectURL;

            RadioISOFormat.Checked = Settings.Default.TimeFormat == 0;
            RadioEUFormat.Checked = Settings.Default.TimeFormat == 1;
            RadioUSFormat.Checked = Settings.Default.TimeFormat == 2;

            RadioBitsPerSecond.Checked = !Settings.Default.SpeedUnits;
            RadioBytesPerSecond.Checked = Settings.Default.SpeedUnits;
            TextLogFile.Text = Settings.Default.LogFile;
            ComboCheckConnection.SelectedIndex = Settings.Default.CheckConnection;
            CheckAutoStart.Checked = Settings.Default.AutoStart;
            CheckMinimize.Checked = Settings.Default.Minimize;

            TextIPDetectURL.Enabled = !RadioAutoDetectIP.Checked;
            TextDownloadURL.Enabled = !RadioAutoDownload.Checked;
        }
        #endregion
        #region Overrides
        /// <summary>Occurs before the Form is closed.</summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Only OK applies the changes; Cancel, Esc and the close button discard them
            if (DialogResult != DialogResult.OK)
            {
                base.OnFormClosing(e);
                return;
            }

            // Update configuration settings
            Settings.Default.AutoDownload = RadioAutoDownload.Checked;
            Settings.Default.DownloadURL = TextDownloadURL.Text;

            Settings.Default.AutoIPDetect = RadioAutoDetectIP.Checked;
            Settings.Default.IPDetectURL = TextIPDetectURL.Text;

            if (RadioISOFormat.Checked) Settings.Default.TimeFormat = 0;
            else if (RadioEUFormat.Checked) Settings.Default.TimeFormat = 1;
            else if (RadioUSFormat.Checked) Settings.Default.TimeFormat = 2;

            Settings.Default.SpeedUnits = !RadioBitsPerSecond.Checked;
            Settings.Default.LogFile = TextLogFile.Text;
            Settings.Default.CheckConnection = ComboCheckConnection.SelectedIndex;
            Settings.Default.AutoStart = CheckAutoStart.Checked;
            Settings.Default.Minimize = CheckMinimize.Checked;            

            // Continue processing the event as usual
            base.OnFormClosing(e);
        }
        #endregion
        #region Child Events
        /// <summary>Occurs when the radio button Checked property changes.</summary>
        private void RadioAutoDownload_CheckedChanged(object sender, EventArgs e)
        {
            TextDownloadURL.Enabled = !RadioAutoDownload.Checked;
        }

        /// <summary>Occurs when the IP API radio button Checked property changes.</summary>
        private void RadioAutoDetectIP_CheckedChanged(object sender, EventArgs e)
        {
            TextIPDetectURL.Enabled = !RadioAutoDetectIP.Checked;
        }

        /// <summary>Occurs when the Log File button is clicked.</summary>
        private void ButtonLogFile_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.AutoUpgradeEnabled = true;
                sfd.CheckPathExists = true;
                sfd.FileName = "SpeedTestLog2.csv";
                sfd.Filter = @"Comma Separated Values (*.csv)|*.csv";
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "SpeedTest";
                sfd.Title = @"Select Log File";

                if (sfd.ShowDialog(this).Equals(DialogResult.OK)) TextLogFile.Text = sfd.FileName;
            }
        }

        /// <summary>Occurs when the OK button is clicked.</summary>
        private void ButtonOK_Click(object sender, EventArgs e)
        {
            // Custom URLs must be full http or https addresses, or the next test would fail
            if (RadioCustomDownload.Checked && !Global.TryGetWebUri(TextDownloadURL.Text, out _))
            {
                ShowInvalidUrl(TextDownloadURL, "download URL");
                return;
            }

            if (RadioCustomIPDetect.Checked && !Global.TryGetWebUri(TextIPDetectURL.Text, out _))
            {
                ShowInvalidUrl(TextIPDetectURL, "IP lookup URL");
                return;
            }

            DialogResult = DialogResult.OK;
        }

        /// <summary>Tells the user the specified URL box doesn't hold a valid address, and selects it.</summary>
        private void ShowInvalidUrl(TextBox textBox, string name)
        {
            MessageBox.Show(this, "The custom " + name + " must be a full web address, starting with http:// or https://.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            textBox.Focus();
            textBox.SelectAll();
        }

        /// <summary>Occurs when the Open Log button is clicked.</summary>
        private void ButtonOpenLog_Click(object sender, EventArgs e)
        {
            if (File.Exists(TextLogFile.Text)) Process.Start(TextLogFile.Text);
        }
        #endregion

        private void ComboCheckConnection_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
