#region Using
// Imported namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SpeedTest.Properties;
#endregion
namespace SpeedTest
{
    /// <summary>The class representing the Logger Form of the application.</summary>
    public partial class FormMain : Form
    {
        #region Enums
        public enum StatusCode : int
        {
            None,
            Online,
            Offline,
        }
        #endregion
        #region Delegates
        public delegate void TimerTickCallback(object stateObject);
        #endregion
        #region Members
        // Private variables
        private string p_CurrentIP;
        private StatusCode p_CurrentStatus;
        private int p_ElapsedSeconds;
        private bool p_StartLogging;
        private DateTime p_StartupTime;
        private System.Threading.Timer p_TimerLog;
        private Uri FileUri;

        // The test in progress, if any, and when the logger's timer should cancel it for taking too long
        private CustomWebClient p_ActiveClient;
        private DateTime? p_TestDeadline;
        private const int IpLookupTimeout = 15000;
        private const int DownloadTimeout = 60000;

        #endregion
        #region Properties
        /// <summary>Gets the file name of the last download URL, or "n/a" if no download has started yet.</summary>
        private string CurrentFileName
        {
            get { return FileUri != null && FileUri.Segments.Length > 0 ? FileUri.Segments[FileUri.Segments.Length - 1] : "n/a"; }
        }
        #endregion
        #region Constructor
        /// <summary>Initializes a new instance of the FormMain class.</summary>
        public FormMain(bool startLogging)
        {
            // This call is required by the designer
            InitializeComponent();

            // Initialize private variables
            p_CurrentIP = "-";
            p_CurrentStatus = StatusCode.None;
            p_ElapsedSeconds = 0;
            p_StartLogging = startLogging;
            p_StartupTime = DateTime.Now;
            p_TimerLog = null;
            p_ActiveClient = null;
            p_TestDeadline = null;

            // Initialize FormMain properties
            this.Font = Theme.UI;

            // Initialize tray icon
            IconMain.ContextMenuStrip = MenuMain;

            // Initialize Menu/ToolBar
            MenuShowSuccessful.Checked = Settings.Default.ShowSuccessful;
            MenuShowStatusChanges.Checked = Settings.Default.ShowStatusChanges;
            MenuShowErrors.Checked = Settings.Default.ShowErrors;
            ToolBarShowSuccessful.Checked = MenuShowSuccessful.Checked;
            ToolBarShowStatusChanges.Checked = MenuShowStatusChanges.Checked;
            ToolBarShowErrors.Checked = MenuShowErrors.Checked;

            ApplyTheme();
            UpdateLoggingStatus();
        }
        #endregion
        #region Overrides
        /// <summary>Occurs before the Form is closed.</summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Stop the logger if it is running
            if (p_TimerLog != null)
            {
                // Stop the timer
                p_TimerLog.Dispose();
                p_TimerLog = null;

                // Reset elapsed seconds
                p_ElapsedSeconds = 0;

                // Cancel a test in progress
                p_ActiveClient?.CancelAsync();

                // Create a new ListViewItem object
                ListViewItem item = new ListViewItem(Global.ConvertTime(DateTime.Now)); //Time
                item.SubItems.Add(Global.ConvertBytes(0) + "/s");                       //b/s
                item.SubItems.Add(p_CurrentIP);                                              //IP
                item.SubItems.Add("-");                                                 //Message
                item.SubItems.Add("Logger Stopped");                                    //Status
                item.SubItems.Add("n/a");                                               //Filename
                item.SubItems.Add("0");                                                 //Speed
                item.ImageIndex = 0;
                item.Tag = DateTime.Now;

                // Add the ListViewItem to the log
                Global.ConnectionLog.Add(item);
            }

            // Hide tray icon
            IconMain.Visible = false;

            // Save the connection log (the status bar can't show an error any more, so tell the user directly)
            if (!Global.SaveConnectionLog(Settings.Default.LogFile))
                MessageBox.Show(this, "The connection log couldn't be saved: " + Global.LastSaveError, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

            // Update configuration settings
            Settings.Default.ShowSuccessful = MenuShowSuccessful.Checked;
            Settings.Default.ShowStatusChanges = MenuShowStatusChanges.Checked;
            Settings.Default.ShowErrors = MenuShowErrors.Checked;

            // Save configuration settings
            Settings.Default.Save();

            // Update auto-start setting
            UpdateAutoStart();

            // Continue processing the event as usual
            base.OnFormClosing(e);
        }

        /// <summary>Occurs when the window moves to a monitor with different display scaling.</summary>
        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            // WinForms rescales the controls and fonts; the column widths, icons and spacing are this app's own
            base.OnDpiChanged(e);

            ScaleColumns(e.DeviceDpiNew / (float)e.DeviceDpiOld);
            ApplyThemeSizes(e.DeviceDpiNew);
            GraphMain.Invalidate();
        }

        /// <summary>Occurs whenever the Form is first displayed.</summary>
        protected override void OnShown(EventArgs e)
        {
            // Explorer themed ListView
            Global.SetWindowTheme(ListLog.Handle, "Explorer", null);
            Global.SendMessage(ListLog.Handle, 0x1000 + 54, 0x00010000, 0x00010000); // LVS_EX_DOUBLEBUFFERED

            // Start logging
            if (p_StartLogging) MenuStartLogging_Click(this, EventArgs.Empty);

            // Continue processing the event as usual
            base.OnShown(e);
        }

        /// <summary>Procedure used to process Windows messages.</summary>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0112) // WM_SYSCOMMAND
            {
                if (m.WParam.ToInt32() == 0xf020) // SC_MINIMIZE
                {
                    if (Settings.Default.Minimize)
                    {
                        m.Result = IntPtr.Zero;
                        Visible = false;
                        IconMain.Visible = true;
                        return;
                    }
                }
            }

            // Continue processing the event as usual
            base.WndProc(ref m);
        }
        #endregion
        #region Event Handlers
        /// <summary>Occurs when the Start Logging menu item is clicked.</summary>
        private void MenuStartLogging_Click(object sender, EventArgs e)
        {
            // Update the GUI
            ToolBarStartLogging.Enabled = false;
            ToolBarStopLogging.Enabled = true;
            ToolBarTest.Enabled = true;
            MenuStartLogging.Enabled = ToolBarStartLogging.Enabled;
            MenuStopLogging.Enabled = ToolBarStopLogging.Enabled;
            MenuTest.Enabled = ToolBarTest.Enabled;
            StatusMain.Text = "Started logging at: " + Global.ConvertTime(DateTime.Now);
            UpdateLoggingStatus(true);

            // Create a new ListViewItem object
            ListViewItem item = new ListViewItem(Global.ConvertTime(DateTime.Now));//Time
            item.SubItems.Add(Global.ConvertBytes(0) + "/s");                      //b/s
            item.SubItems.Add(p_CurrentIP);                                             //   IP
            item.SubItems.Add("-");                                                //Message
            item.SubItems.Add("Logger Started");                                   //Status
            item.SubItems.Add("n/a");                                              //Filename
            item.SubItems.Add("0");                                                //Speed
            item.ImageIndex = 1;
            item.Tag = DateTime.Now;

            // Add the ListViewItem to the log
            AddLogItem(item);

            // Start the Timer
            p_TimerLog = new System.Threading.Timer(new System.Threading.TimerCallback(p_TimerLog_Tick), null, 1000, 1000);
            GetRemoteIP();
        }

        /// <summary>Occurs when the Stop Logging menu item is clicked.</summary>
        private void MenuStopLogging_Click(object sender, EventArgs e)
        {
            // Stop the Timer
            p_TimerLog.Dispose();
            p_TimerLog = null;

            p_ElapsedSeconds = 0;

            // Cancel a test in progress; its completion handler then disposes it without logging
            p_ActiveClient?.CancelAsync();

            // Update the GUI
            ToolBarStartLogging.Enabled = true;
            ToolBarStopLogging.Enabled = false;
            ToolBarTest.Enabled = false;
            MenuStartLogging.Enabled = ToolBarStartLogging.Enabled;
            MenuStopLogging.Enabled = ToolBarStopLogging.Enabled;
            MenuTest.Enabled = ToolBarTest.Enabled;
            StatusMain.Text = "Stopped logging at: " + Global.ConvertTime(DateTime.Now);
            UpdateLoggingStatus(false);

            // Create a new ListViewItem object
            ListViewItem item = new ListViewItem(Global.ConvertTime(DateTime.Now));//Time
            item.SubItems.Add(Global.ConvertBytes(0) + "/s");                      //b/s
            item.SubItems.Add(p_CurrentIP);                                             //IP
            item.SubItems.Add("-");                                                //Message
            item.SubItems.Add("Logger Stopped");                                   //Status
            item.SubItems.Add("n/a");                                              //Filename
            item.SubItems.Add("0");                                                //Speed
            item.ImageIndex = 1;
            item.Tag = DateTime.Now;

            // Add the ListViewItem to the log
            AddLogItem(item);
        }

        /// <summary>Occurs when the Test Now menu item is clicked.</summary>
        private void MenuTest_Click(object sender, EventArgs e)
        {
            GetRemoteIP();
        }

        /// <summary>Occurs when the Show Successful Logs menu item is clicked.</summary>
        private void MenuShowSuccessful_Click(object sender, EventArgs e)
        {
            MenuShowSuccessful.Checked = !MenuShowSuccessful.Checked;
            ToolBarShowSuccessful.Checked = MenuShowSuccessful.Checked;

            UpdateLog();
        }

        /// <summary>Occurs when the Show Status Changes item is clicked.</summary>
        private void MenuShowStatusChanges_Click(object sender, EventArgs e)
        {
            MenuShowStatusChanges.Checked = !MenuShowStatusChanges.Checked;
            ToolBarShowStatusChanges.Checked = MenuShowStatusChanges.Checked;

            UpdateLog();
        }

        /// <summary>Occurs when the Show Error Messages menu item is clicked.</summary>
        private void MenuShowErrors_Click(object sender, EventArgs e)
        {
            MenuShowErrors.Checked = !MenuShowErrors.Checked;
            ToolBarShowErrors.Checked = MenuShowErrors.Checked;

            UpdateLog();
        }

        /// <summary>Occurs when the Configuration Settings menu item is clicked.</summary>
        private void MenuSettings_Click(object sender, EventArgs e)
        {
            // Create a new FormSettings dialog and display it to the user
            using (FormSettings fs = new FormSettings())
            {
                if (fs.ShowDialog(this) == DialogResult.OK)
                {
                    // Apply the auto-start setting straight away, not only when the app closes
                    UpdateAutoStart();

                    // The interval, units or time format may have changed
                    UpdateLoggingStatus();
                    GraphMain.Invalidate();
                }
            }
        }

        /// <summary>Occurs when the Get Help Online menu item is clicked.</summary>
        private void MenuHelp_Click(object sender, EventArgs e)
        {
            // Navigate to online help URL
            Process.Start("https://github.com/FifoTheHein/Speed-Test-Loggger");
        }

        /// <summary>Occurs when the About Logger menu item is clicked.</summary>
        private void MenuAbout_Click(object sender, EventArgs e)
        {
            // Display an About dialog to the user
            MessageBox.Show(this, "Speed Test Loggger for Windows V1.05" + Environment.NewLine + "Open-Source (AGPL v3)" + Environment.NewLine + "Copyright © 2015 a9t9", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>Occurs when the Exit menu item is clicked.</summary>
        private void MenuExit_Click(object sender, EventArgs e)
        {
            // Exit the application
            Application.Exit();
        }

        /// <summary>Occurs when the toolbar lays out its buttons: widens the spacer so Settings, Help and About sit at the right.</summary>
        private void ToolStripMain_Layout(object sender, LayoutEventArgs e)
        {
            // Right-aligned toolbar items aren't exposed to screen readers or UI Automation in .NET Framework,
            // so every button is left-aligned and this invisible spacer fills the gap instead
            int used = 0;

            foreach (ToolStripItem item in ToolStripMain.Items)
            {
                if (item != ToolBarSpacer && item.Available) used += item.Width + item.Margin.Horizontal;
            }

            // A few pixels of slack: if the spacer takes every last pixel, the toolbar moves the last button out of view
            int width = Math.Max(0, ToolStripMain.DisplayRectangle.Width - used - ToolBarSpacer.Margin.Horizontal - Theme.Scale(this, 4));

            // Changing the width lays the toolbar out again, so only change it when it differs
            if (ToolBarSpacer.Width != width) ToolBarSpacer.Width = width;
        }

        /// <summary>Occurs when the NotifyIcon is double-clicked.</summary>
        private void IconMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Visible = true;
            IconMain.Visible = false;
        }

        /// <summary>Occurs each time the Timer ticks (Once per-second).</summary>
        private void p_TimerLog_Tick(object stateObject)
        {
            if (p_TimerLog == null) return;

            if (InvokeRequired) Invoke(new TimerTickCallback(p_TimerLog_Tick), new object[] { stateObject });
            else
            {
                // The logger may have been stopped while this tick waited for the UI thread
                if (p_TimerLog == null) return;

                // Cancel a test that has taken too long; it then completes as failed
                if (p_ActiveClient != null && p_TestDeadline.HasValue && DateTime.Now >= p_TestDeadline.Value)
                {
                    p_TestDeadline = null;
                    p_ActiveClient.CancelAsync();
                }

                // Increment elapsed seconds
                p_ElapsedSeconds++;

                // Create a new TimeSpan object
                TimeSpan ts = new TimeSpan(0, 0, 0, p_ElapsedSeconds);

                switch (Settings.Default.CheckConnection)
                {
                    case 0: // 15 Seconds
                        if (ts.TotalSeconds >= 15) GetRemoteIP();
                        break;
                    case 1: // 30 Seconds
                        if (ts.TotalSeconds >= 30) GetRemoteIP();
                        break;
                    case 2: // 1 Minute
                        if (ts.TotalMinutes >= 1) GetRemoteIP();
                        break;
                    case 3: // 5 Minutes
                        if (ts.TotalMinutes >= 5) GetRemoteIP();
                        break;
                    case 4: // 10 Minutes
                        if (ts.TotalMinutes >= 10) GetRemoteIP();
                        break;
                    case 5: // 30 Minutes
                        if (ts.TotalMinutes >= 30) GetRemoteIP();
                        break;
                    case 6: // 1 Hour
                        if (ts.TotalMinutes >= 60) GetRemoteIP();
                        break;
                    default: // An unknown setting would otherwise never test
                        if (ts.TotalSeconds >= 15) GetRemoteIP();
                        break;
                }
            }
        }

        /// <summary>Occurs each time the Timer ticks (Once per-second).</summary>
        private void TimerGraph_Tick(object sender, EventArgs e)
        {
            // Update the GUI
            GraphMain.Invalidate();
        }

        /// <summary>Occurs when the test download completes, fails or is cancelled.</summary>
        private void webClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            // Retrieve the CustomWebClient object
            CustomWebClient webClient = sender as CustomWebClient;
            if (webClient == null) return;

            // The logger was stopped while the test ran
            if (webClient != p_ActiveClient || p_TimerLog == null)
            {
                FinishTest(webClient);
                return;
            }

            // The IP address from the lookup, or null if the lookup failed
            string ip = (string)e.UserState;

            if (e.Error == null && !e.Cancelled) // Data downloaded successfully
            {
                // Update the StatusBar
                StatusMain.Text = "Connection logged, size: " + Global.ConvertBytes(webClient.AllBytesDownloaded) + " speed: " + Global.ConvertBytes(webClient.CurrentSpeed) + "/s";

                // If only the IP lookup failed, the IP address is unknown rather than changed
                LogTestResult(StatusCode.Online, ip ?? p_CurrentIP, "OK", "-", webClient.CurrentSpeed);
            }
            else if (ip == null) // Neither the IP lookup nor the test download worked
            {
                StatusMain.Text = "Unable to test speed, no connection: " + Global.ConvertTime(DateTime.Now);
                LogTestResult(StatusCode.Offline, "-", "Offline", "-", 0);
            }
            else // The IP lookup worked, so the connection is up, but the test download failed
            {
                string error = DescribeError(e);

                StatusMain.Text = "Unable to test speed (" + error + "): " + Global.ConvertTime(DateTime.Now);
                LogTestResult(StatusCode.Online, ip, "Error", error, 0);
            }

            FinishTest(webClient);
        }

        private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Retrieve the CustomWebClient object
            CustomWebClient webClient = sender as CustomWebClient;
            if (webClient == null) return;

            if (p_TimerLog == null) return;

            StatusMain.Text = "Calculating speed: " + Global.ConvertBytes(webClient.CurrentSpeed) + "/s...";
        }

        /// <summary>Occurs when an asynchronous download operation completes.</summary>
        private void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            // Retrieve the CustomWebClient object
            CustomWebClient webClient = sender as CustomWebClient;
            if (webClient == null) return;

            // The logger was stopped while the lookup ran
            if (webClient != p_ActiveClient || p_TimerLog == null)
            {
                FinishTest(webClient);
                return;
            }

            // The IP address, or null if the lookup failed. A failed lookup alone doesn't mean offline:
            // the test download still runs, so an outage of the lookup service isn't logged as an outage.
            string ip = e.Error == null && !e.Cancelled ? FindIPAddress(e.Result) : null;

            if (!TryGetDownloadUri(out Uri downloadUri))
            {
                // Settings rejects invalid URLs, but the setting could predate that check
                StatusMain.Text = "Unable to test speed: the custom download URL in Configuration Settings isn't a valid web address";
                FinishTest(webClient);
                return;
            }

            FileUri = downloadUri;
            StatusMain.Text = "Testing speed at: " + Global.ConvertTime(DateTime.Now);

            // Begin downloading the data, passing the IP to the DownloadDataCompleted event
            StartTestTimeout(DownloadTimeout);
            webClient.StartTimedDownload(FileUri, ip);
        }
        #endregion
        #region Private Methods
        /// <summary>Gives the window its look: flat toolbar, status bar and tray menu, and line icons where Windows has the icon font.</summary>
        private void ApplyTheme()
        {
            FlatRenderer renderer = new FlatRenderer();
            ToolStripMain.Renderer = renderer;
            StatusStripMain.Renderer = renderer;
            MenuMain.Renderer = renderer;

            // The list's column widths are designed at 100% display scaling, and WinForms doesn't scale them
            ScaleColumns(DeviceDpi / 96f);

            ApplyThemeSizes(DeviceDpi);
        }

        /// <summary>Sizes the icons and spacing for the specified DPI. Runs again when the window moves to a monitor with different scaling.</summary>
        private void ApplyThemeSizes(int dpi)
        {
            int S(int pixels) => (int)Math.Round(pixels * dpi / 96f);

            // Toolbar: icons, spacing, and icons falling back to the original images (same order in ImageListMain)
            ToolStripMain.ImageScalingSize = new Size(S(20), S(20));
            ToolStripMain.Padding = new Padding(S(8), S(2), S(8), S(2));

            foreach (ToolStripItem item in ToolStripMain.Items)
            {
                int padding = item.DisplayStyle == ToolStripItemDisplayStyle.Image ? S(4) : S(6);
                item.Padding = item is ToolStripSeparator ? Padding.Empty : new Padding(padding, 0, padding, 0);
                item.Margin = item is ToolStripSeparator ? new Padding(S(6), 0, S(6), 0) : new Padding(S(1), 0, S(1), 0);
            }

            SetIcon(ToolBarStartLogging, Theme.GlyphPlay, Theme.Success, 0, S(20));
            SetIcon(ToolBarStopLogging, Theme.GlyphStop, Theme.Danger, 1, S(20));
            SetIcon(ToolBarTest, Theme.GlyphRefresh, Theme.Accent, 2, S(20));
            SetIcon(ToolBarShowSuccessful, Theme.GlyphCompleted, Theme.Success, 3, S(20));
            SetIcon(ToolBarShowStatusChanges, Theme.GlyphGlobe, Theme.Accent, 4, S(20));
            SetIcon(ToolBarShowErrors, Theme.GlyphError, Theme.Danger, 5, S(20));
            SetIcon(ToolBarSettings, Theme.GlyphSettings, Theme.Text, 6, S(20));
            SetIcon(ToolBarHelp, Theme.GlyphHelp, Theme.Text, 7, S(20));
            SetIcon(ToolBarAbout, Theme.GlyphInfo, Theme.Text, 8, S(20));

            // Tray menu icons
            MenuMain.ImageScalingSize = new Size(S(16), S(16));
            MenuStartLogging.Image = Theme.Icon(Theme.GlyphPlay, S(16), Theme.Success);
            MenuStopLogging.Image = Theme.Icon(Theme.GlyphStop, S(16), Theme.Danger);
            MenuTest.Image = Theme.Icon(Theme.GlyphRefresh, S(16), Theme.Accent);
            MenuSettings.Image = Theme.Icon(Theme.GlyphSettings, S(16), Theme.Text);
            MenuHelp.Image = Theme.Icon(Theme.GlyphHelp, S(16), Theme.Text);
            MenuAbout.Image = Theme.Icon(Theme.GlyphInfo, S(16), Theme.Text);

            // Status bar spacing; the right side leaves room for the resize grip
            StatusStripMain.Padding = new Padding(S(8), S(2), S(22), S(2));

            // List icons: successful, status change, error (the ImageIndex values the log uses)
            if (Theme.HasIcons)
            {
                ImageListLog.Images.Clear();
                ImageListLog.ColorDepth = ColorDepth.Depth32Bit;
                ImageListLog.ImageSize = new Size(S(16), S(16));
                ImageListLog.Images.Add(Theme.Icon(Theme.GlyphCompleted, S(16), Theme.Success));
                ImageListLog.Images.Add(Theme.Icon(Theme.GlyphGlobe, S(16), Theme.Accent));
                ImageListLog.Images.Add(Theme.Icon(Theme.GlyphError, S(16), Theme.Danger));
            }

            UpdateLoggingStatus();
        }

        /// <summary>Multiplies the list's column widths by the specified factor.</summary>
        private void ScaleColumns(float factor)
        {
            foreach (ColumnHeader column in ListLog.Columns) column.Width = (int)Math.Round(column.Width * factor);
        }

        /// <summary>Sets a toolbar button's line icon, or the original image when Windows has no icon font.</summary>
        private void SetIcon(ToolStripItem item, char glyph, Color color, int fallbackIndex, int size)
        {
            item.Image = Theme.Icon(glyph, size, color) ?? ImageListMain.Images[fallbackIndex];
        }

        /// <summary>Shows in the status bar whether the logger is running, and how often it tests.</summary>
        private void UpdateLoggingStatus()
        {
            UpdateLoggingStatus(p_TimerLog != null);
        }

        /// <summary>Shows in the status bar whether the logger is running, and how often it tests.</summary>
        private void UpdateLoggingStatus(bool logging)
        {
            string[] intervals = { "15 seconds", "30 seconds", "1 minute", "5 minutes", "10 minutes", "30 minutes", "1 hour" };
            int interval = Settings.Default.CheckConnection;

            StatusLogging.Image = Theme.Dot(Theme.Scale(this, 10), logging ? Theme.Success : Theme.TextMuted);
            StatusLogging.Text = logging ? "Logging every " + (interval >= 0 && interval < intervals.Length ? intervals[interval] : intervals[0]) : "Not logging";
        }

        /// <summary>Adds the specified item to the connection log, shows it if its type is visible, and saves the log.</summary>
        private void AddLogItem(ListViewItem item)
        {
            // Add the ListViewItem to the log
            Global.ConnectionLog.Add(item);

            if (IsShown(item))
            {
                ListLog.Items.Insert(0, item);
                item.EnsureVisible();
            }

            // Save after every row, so an outage is on disk even if the PC loses power
            if (!Global.SaveConnectionLog(Settings.Default.LogFile))
                StatusMain.Text = "Unable to save the log: " + Global.LastSaveError;
        }

        /// <summary>Gets a value indicating whether rows of the specified item's type are shown in the list (the Show buttons on the toolbar).</summary>
        private bool IsShown(ListViewItem item)
        {
            switch (item.ImageIndex)
            {
                case 0: // Successful
                    return ToolBarShowSuccessful.Checked;
                case 1: // Status Changed
                    return ToolBarShowStatusChanges.Checked;
                default: // Error
                    return ToolBarShowErrors.Checked;
            }
        }

        /// <summary>Logs the result of a test, marking a change of IP address or of online/offline status in the Message column.</summary>
        private void LogTestResult(StatusCode newStatus, string ip, string status, string message, long speed)
        {
            int imageIndex = status.Equals("OK") ? 0 : 2;

            if (!p_CurrentIP.Equals(ip))
            {
                message = "IP Changed";
                imageIndex = 1;
                p_CurrentIP = ip;
            }

            if (!p_CurrentStatus.Equals(newStatus))
            {
                message = newStatus == StatusCode.Online ? "Online" : "Offline";
                imageIndex = newStatus == StatusCode.Online ? 1 : 2;
                p_CurrentStatus = newStatus;
            }

            // Create a new ListViewItem object
            ListViewItem item = new ListViewItem(Global.ConvertTime(DateTime.Now));//Time
            item.SubItems.Add(Global.ConvertBytes(speed) + "/s");                  //b/s
            item.SubItems.Add(p_CurrentIP);                                        //IP
            item.SubItems.Add(message);                                            //Message
            item.SubItems.Add(status);                                             //Status
            item.SubItems.Add(CurrentFileName);                                    //Filename
            item.SubItems.Add(speed.ToString(CultureInfo.InvariantCulture));       //Speed
            item.ImageIndex = imageIndex;
            item.Tag = DateTime.Now;

            // Add the ListViewItem to the log
            AddLogItem(item);
        }

        /// <summary>Ends the specified test: clears its deadline, lets the next test start, and disposes its client.</summary>
        private void FinishTest(CustomWebClient webClient)
        {
            if (webClient == p_ActiveClient)
            {
                p_TestDeadline = null;
                p_ActiveClient = null;
            }

            webClient.Dispose();
        }

        /// <summary>Sets the deadline for the current step of the test. The logger's once-a-second tick cancels the test after it.</summary>
        private void StartTestTimeout(int milliseconds)
        {
            p_TestDeadline = DateTime.Now.AddMilliseconds(milliseconds);
        }

        /// <summary>Gets the first valid IPv4 address in the specified text, or null if there isn't one (e.g. a captive portal page).</summary>
        private static string FindIPAddress(string text)
        {
            foreach (Match match in Regex.Matches(text ?? string.Empty, @"\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b"))
            {
                // The pattern also matches values like 999.1.1.1
                if (IPAddress.TryParse(match.Value, out IPAddress address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return match.Value;
            }

            return null;
        }

        /// <summary>Gets the URL of the file to time: the custom URL, or a file sized to the fastest speed logged in the last 24 hours.</summary>
        private static bool TryGetDownloadUri(out Uri uri)
        {
            if (!Settings.Default.AutoDownload) return Global.TryGetWebUri(Settings.Default.DownloadURL, out uri);

            // Only recent speeds count, so one fast result long ago doesn't pick the large file for ever,
            // and the scan stays short however long the log grows
            long maximumSpeed = 0;
            DateTime since = DateTime.Now.AddHours(-24);

            for (int i = Global.ConnectionLog.Count - 1; i >= 0 && (DateTime)Global.ConnectionLog[i].Tag > since; i--)
            {
                long speed = Global.ToInt64(Global.ConnectionLog[i].SubItems[6].Text);

                if (speed > maximumSpeed) maximumSpeed = speed;
            }

            // Below about 1 MB/s the 100 KB file is enough; above it, the 1 MB file times more accurately
            uri = new Uri(maximumSpeed < 1024000 ? "https://app.tfn.co.za/test/test100k.zip" : "https://app.tfn.co.za/test/test1mb.zip");
            return true;
        }

        /// <summary>Gets a short description of why a test download failed, for the Message column.</summary>
        private static string DescribeError(AsyncCompletedEventArgs e)
        {
            if (e.Cancelled) return "Timed out";

            WebException error = e.Error as WebException;

            if (error?.Response is HttpWebResponse response) return "HTTP " + (int)response.StatusCode;

            switch (error?.Status)
            {
                case WebExceptionStatus.NameResolutionFailure:
                    return "Server not found";
                case WebExceptionStatus.ConnectFailure:
                    return "Connection failed";
                case WebExceptionStatus.Timeout:
                    return "Timed out";
                default:
                    return "Download failed";
            }
        }
        #endregion
        #region Public Methods
        /// <summary>Starts a test: looks up the public IPv4 address, then times the test download.</summary>
        public void GetRemoteIP()
        {
            // Reset elapsed seconds
            p_ElapsedSeconds = 0;

            // Overlapping tests would share the connection, and each would measure about half the speed
            if (p_ActiveClient != null)
            {
                StatusMain.Text = "Skipped a test at " + Global.ConvertTime(DateTime.Now) + ": the previous test is still running";
                return;
            }

            string api = Settings.Default.AutoIPDetect ? "http://icanhazip.com" : Settings.Default.IPDetectURL;

            // Settings rejects invalid URLs, but the setting could predate that check
            if (!Global.TryGetWebUri(api, out Uri apiUri))
            {
                StatusMain.Text = "Unable to test speed: the custom IP lookup URL in Configuration Settings isn't a valid web address";
                return;
            }

            // Create a new WebClient object and download the string. There's no ping first: both steps are HTTP requests,
            // which also work on networks that block ping, and they run in the background instead of freezing the window.
            p_ActiveClient = new CustomWebClient();
            p_ActiveClient.DownloadDataCompleted += webClient_DownloadDataCompleted;
            p_ActiveClient.DownloadProgressChanged += webClient_DownloadProgressChanged;
            p_ActiveClient.DownloadStringCompleted += webClient_DownloadStringCompleted;

            StartTestTimeout(IpLookupTimeout);
            p_ActiveClient.DownloadStringAsync(apiUri);
        }

        /// <summary>Adds or removes the application from the Windows Startup folder.</summary>
        public void UpdateAutoStart()
        {
            try
            {
                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string startupScript = Path.Combine(startupFolder, "Speed Test Loggger.cmd");

                // Remove what older versions created. They started the exe straight from ClickOnce's versioned folder,
                // which skipped the update check and stopped working once an update removed that folder.
                string oldBatch = Path.Combine(Global.AppDataFolder, "SpeedTest.bat");
                string oldShortcut = Path.Combine(startupFolder, "SpeedTest.url");
                if (File.Exists(oldBatch)) File.Delete(oldBatch);
                if (File.Exists(oldShortcut)) File.Delete(oldShortcut);

                if (!Settings.Default.AutoStart)
                {
                    if (File.Exists(startupScript)) File.Delete(startupScript);
                    return;
                }

                // cmd.exe reads scripts in the OEM code page, which can't hold every character in a user name,
                // so write paths under the user's AppData folder as %APPDATA%
                Encoding encoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string ForScript(string path) => path.StartsWith(appData, StringComparison.OrdinalIgnoreCase) ? "%APPDATA%" + path.Substring(appData.Length) : path;
                string clickOnceShortcut = GetClickOnceShortcut();

                using (StreamWriter writer = new StreamWriter(startupScript, false, encoding))
                {
                    writer.WriteLine("@ECHO OFF");

                    if (clickOnceShortcut != null)
                    {
                        // Start through the Start Menu shortcut, so ClickOnce checks for updates first. The shortcut can't
                        // pass /start, so leave a flag file that tells the app to start logging.
                        writer.WriteLine("ECHO.> \"" + ForScript(Global.AutoStartFlagFile) + "\"");
                        writer.WriteLine("START \"\" \"" + ForScript(clickOnceShortcut) + "\"");
                    }
                    else
                    {
                        // Not installed with ClickOnce (e.g. run from Visual Studio)
                        writer.WriteLine("START \"\" \"" + Application.ExecutablePath + "\" /start");
                    }
                }
            }
            catch { }
        }

        /// <summary>Gets the path of the Start Menu shortcut ClickOnce created for this app, or null if it isn't installed with ClickOnce.</summary>
        private static string GetClickOnceShortcut()
        {
            if (!System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed) return null;

            // ClickOnce names the shortcut after the product, under a folder named after the publisher
            string name = Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".appref-ms";
            string[] shortcuts = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Programs), name, SearchOption.AllDirectories);

            return shortcuts.Length > 0 ? shortcuts[0] : null;
        }

        /// <summary>Updates the ListView when the user shows or hides log messages.</summary>
        public void UpdateLog()
        {
            // The list shows this session's rows, newest at the top. The log is in time order, so read it
            // newest first and stop at the first row from before startup.
            List<ListViewItem> items = new List<ListViewItem>();

            if (Global.ConnectionLog != null)
            {
                for (int i = Global.ConnectionLog.Count - 1; i >= 0 && (DateTime)Global.ConnectionLog[i].Tag > p_StartupTime; i--)
                {
                    if (IsShown(Global.ConnectionLog[i])) items.Add(Global.ConnectionLog[i]);
                }
            }

            // Refill the ListView in one go instead of redrawing it for every row
            ListLog.BeginUpdate();
            ListLog.Items.Clear();
            ListLog.Items.AddRange(items.ToArray());
            ListLog.EndUpdate();

            if (items.Count > 0) items[0].EnsureVisible();
        }
        #endregion
    }
}
