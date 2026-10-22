#region Using
// Imported namespaces
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SpeedTest.Properties;
using Microsoft.Win32;
using System.Security.Policy;
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
            InvalidURL,
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

            // Initialize FormMain properties
            this.ClientSize = new Size(660, 480);
            this.Font = SystemFonts.MenuFont;

            // Initialize tray icon
            IconMain.ContextMenu = MenuMain;

            // Initialize Menu/ToolBar
            MenuShowSuccessful.Checked = Settings.Default.ShowSuccessful;
            MenuShowStatusChanges.Checked = Settings.Default.ShowStatusChanges;
            MenuShowErrors.Checked = Settings.Default.ShowErrors;
            ToolBarShowSuccessful.Pushed = MenuShowSuccessful.Checked;
            ToolBarShowStatusChanges.Pushed = MenuShowStatusChanges.Checked;
            ToolBarShowErrors.Pushed = MenuShowErrors.Checked;
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

        /// <summary>Occurs whenever the Form is first displayed.</summary>
        protected override void OnShown(EventArgs e)
        {
            // Explorer themed ListView
            Global.SetWindowTheme(ListLog.Handle, "Explorer", null);
            Global.SendMessage(ListLog.Handle, 0x1000 + 54, 0x00010000, 0x00010000); // LVS_EX_DOUBLEBUFFERED

            // Extend frame if Desktop Composition (Aero) is enabled
            if (Global.CompositionEnabled && !Global.IsWindows8)
            {
                BackColor = Color.Black;
                ContainerMain.BackColor = SystemColors.Control;
                
                Global.ExtendFrame(this.Handle, new System.Drawing.Printing.Margins(0, 0, ToolBarMain.Height, 0));

                //if (Global.IsWindows8) Global.SetWindowTheme(ToolBarMain.Handle, "TrayNotifyComposited", null);
                Global.SetWindowTheme(ToolBarMain.Handle, "Media", null);
            }

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

            // Update the GUI
            ToolBarStartLogging.Enabled = true;
            ToolBarStopLogging.Enabled = false;
            ToolBarTest.Enabled = false;
            MenuStartLogging.Enabled = ToolBarStartLogging.Enabled;
            MenuStopLogging.Enabled = ToolBarStopLogging.Enabled;
            MenuTest.Enabled = ToolBarTest.Enabled;
            StatusMain.Text = "Stopped logging at: " + Global.ConvertTime(DateTime.Now);

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
            ToolBarShowSuccessful.Pushed = MenuShowSuccessful.Checked;

            UpdateLog();
        }

        /// <summary>Occurs when the Show Status Changes item is clicked.</summary>
        private void MenuShowStatusChanges_Click(object sender, EventArgs e)
        {
            MenuShowStatusChanges.Checked = !MenuShowStatusChanges.Checked;
            ToolBarShowStatusChanges.Pushed = MenuShowStatusChanges.Checked;

            UpdateLog();
        }

        /// <summary>Occurs when the Show Error Messages menu item is clicked.</summary>
        private void MenuShowErrors_Click(object sender, EventArgs e)
        {
            MenuShowErrors.Checked = !MenuShowErrors.Checked;
            ToolBarShowErrors.Pushed = MenuShowErrors.Checked;

            UpdateLog();
        }

        /// <summary>Occurs when the Configuration Settings menu item is clicked.</summary>
        private void MenuSettings_Click(object sender, EventArgs e)
        {
            // Create a new FormSettings dialog and display it to the user
            using (FormSettings fs = new FormSettings())
            {
                // Apply the auto-start setting straight away, not only when the app closes
                if (fs.ShowDialog(this) == DialogResult.OK) UpdateAutoStart();
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
            MessageBox.Show(this, "Speed Test Loggger for Windows V1.04.1" + Environment.NewLine + "Open-Source (AGPL v3)" + Environment.NewLine + "Copyright © 2015 a9t9", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>Occurs when the Exit menu item is clicked.</summary>
        private void MenuExit_Click(object sender, EventArgs e)
        {
            // Exit the application
            Application.Exit();
        }

        /// <summary>Occurs when a ToolBarButton on the ToolBar is clicked.</summary>
        private void ToolBarMain_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            switch (e.Button.ImageIndex)
            {
                case 0: // Start Logging
                    MenuStartLogging_Click(this, EventArgs.Empty);
                    break;
                case 1: // Stop Logging
                    MenuStopLogging_Click(this, EventArgs.Empty);
                    break;
                case 2: // Test Now
                    MenuTest_Click(this, EventArgs.Empty);
                    break;
                case 3: // Show Successful Logs
                    MenuShowSuccessful_Click(this, EventArgs.Empty);
                    break;
                case 4: // Show Status Changes
                    MenuShowStatusChanges_Click(this, EventArgs.Empty);
                    break;
                case 5: // Show Error Messages
                    MenuShowErrors_Click(this, EventArgs.Empty);
                    break;
                case 6: // Configuration Settings
                    MenuSettings_Click(this, EventArgs.Empty);
                    break;
                case 7: // Get Help Online
                    MenuHelp_Click(this, EventArgs.Empty);
                    break;
                case 8: // About Logger
                    MenuAbout_Click(this, EventArgs.Empty);
                    break;
            }
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
                        
                }
            }
        }

        /// <summary>Occurs each time the Timer ticks (Once per-second).</summary>
        private void TimerGraph_Tick(object sender, EventArgs e)
        {
            // Update the GUI
            GraphMain.Invalidate();
        }

        private void webClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            // Retrieve the CustomWebClient object
            CustomWebClient webClient = sender as CustomWebClient;
            if (webClient == null) return;

            if (p_TimerLog == null) return;

            // Retrieve the IP address
            string ip = (string)e.UserState;

            // Local variables
            string message = "-";
            StatusCode newStatus = StatusCode.Online;
            int imageIndex = 0;
            ListViewItem item = null;

            if (e.Error == null) // Data downloaded successfully
            {
                // Update the StatusBar
                StatusMain.Text = "Connection logged, size: " + Global.ConvertBytes(webClient.AllBytesDownloaded) + " speed: " + Global.ConvertBytes(webClient.CurrentSpeed) + "/s";

                if (!p_CurrentIP.Equals(ip))
                {
                    message = "IP Changed";
                    imageIndex = 1;
                    p_CurrentIP = ip;
                }

                if (!p_CurrentStatus.Equals(newStatus) && !p_CurrentStatus.Equals(StatusCode.Online))
                {
                    message = "Online";
                    imageIndex = 0;
                    p_CurrentStatus = newStatus;
                }

                if (!p_CurrentStatus.Equals(newStatus) && !p_CurrentStatus.Equals(StatusCode.Offline))
                {
                    message = "Offline";
                    imageIndex = 2;
                    p_CurrentStatus = newStatus;
                }

                // Create a new ListViewItem object
                item = new ListViewItem(Global.ConvertTime(DateTime.Now));            //Time
                item.SubItems.Add(Global.ConvertBytes(webClient.CurrentSpeed) + "/s");//b/s
                item.SubItems.Add(p_CurrentIP);                                            //IP
                item.SubItems.Add(message);                                                //Message
                item.SubItems.Add("OK");                                              //Status
                item.SubItems.Add(CurrentFileName);                                          //Filename
                item.SubItems.Add(webClient.CurrentSpeed.ToString());                 //Speed
                item.ImageIndex = imageIndex;
                item.Tag = DateTime.Now;
            }
            else // An error occured downloading the data
            {
                // Update the StatusBar
                StatusMain.Text = "Unable to test speed: " + Global.ConvertTime(DateTime.Now);

                // Set status to Offline
                newStatus = StatusCode.InvalidURL;
                imageIndex = 2;
                //ip = "-";

                if (!p_CurrentIP.Equals(ip))
                {
                    message = "IP Changed";
                    imageIndex = 1;
                    p_CurrentIP = ip;
                }

                if (!p_CurrentStatus.Equals(newStatus))
                {
                    message = "Invalid URL";
                    imageIndex = 2;
                    p_CurrentStatus = newStatus;
                }

                // Create a new ListViewItem object
                item = new ListViewItem(Global.ConvertTime(DateTime.Now));//Time
                item.SubItems.Add(Global.ConvertBytes(0) + "/s");         //b/s
                item.SubItems.Add(p_CurrentIP);                                //IP
                item.SubItems.Add(message);                                    //Message
                item.SubItems.Add("Error");                               //Status
                item.SubItems.Add("n/a");                                 //Filename
                item.SubItems.Add("0");                                   //Speed
                item.ImageIndex = imageIndex;
                item.Tag = DateTime.Now;
            }

            // Add the ListViewItem to the log
            if (item != null) AddLogItem(item);

            // Dispose the CustomWebClient object
            webClient.Dispose();
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

            // Local variables
            string ip = "-";
            string message = "-";
            StatusCode newStatus = StatusCode.Online;
            int imageIndex = 0;
            // Regex to match IP address
            Regex regex = new Regex(@"(?:[0-9]{1,3}\.){3}[0-9]{1,3}");

            if (e.Error == null && regex.IsMatch(e.Result)) // String downloaded successfully and contains IP address
            {
                // Get the first instance of an IP in the result using regex
                Match match = regex.Match(e.Result);

                // Remove line breaks from the IP address
                ip = match.Value;

                StatusMain.Text = "Testing speed at: " + Global.ConvertTime(DateTime.Now);

                // Begin downloading the data, passing the IP to the DownloadDataCompleted event
                if (Settings.Default.AutoDownload)
                {
                    string url = string.Empty;
                    long maximumSpeed = 0;

                    foreach (ListViewItem item in Global.ConnectionLog)
                    {
                        long speed = Global.ToInt64(item.SubItems[6].Text);

                        if (speed > maximumSpeed) maximumSpeed = speed;
                    }

                    // Auto-select URL based on maximum download speed
                    if (maximumSpeed < 1024000)
                    {
                        // Download 100k file
                        url = "https://app.tfn.co.za/test/test100k.zip";
                    }
                    //else if (maximumSpeed < 1024000 * 5)
                    //{
                    //    // Download 1 MB file
                    //    url = "https://app.tfn.co.za/test/test1mb.zip";
                    //}
                    else
                    {
                        // Download 1 MB file
                        url = "https://app.tfn.co.za/test/test1mb.zip";
                    }

                    FileUri = new Uri(url);

                    webClient.DownloadDataAsync(FileUri, ip);
                }
                else if (Global.TryGetWebUri(Settings.Default.DownloadURL, out Uri downloadUri))
                {
                    FileUri = downloadUri;
                    webClient.DownloadDataAsync(FileUri, ip);
                }
                else
                {
                    // Settings rejects invalid URLs, but the setting could predate that check
                    StatusMain.Text = "Unable to test speed: the custom download URL in Configuration Settings isn't a valid web address";
                    webClient.Dispose();
                }
            }
            else // An error occured downloading the string
            {
                StatusMain.Text = "Unable to test speed: " + Global.ConvertTime(DateTime.Now);

                // Set status to Offline
                newStatus = StatusCode.Offline;
                imageIndex = 2;
                ip = "-";

                if (!p_CurrentIP.Equals(ip))
                {
                    message = "IP Changed";
                    imageIndex = 1;
                    p_CurrentIP = ip;
                }

                if (!p_CurrentStatus.Equals(newStatus))
                {
                    message = "Offline";
                    imageIndex = 2;
                    p_CurrentStatus = newStatus;
                }

                // Create a new ListViewItem object
                ListViewItem item = new ListViewItem(Global.ConvertTime(DateTime.Now));//Time
                item.SubItems.Add(Global.ConvertBytes(0) + "/s");                      //b/s
                item.SubItems.Add(p_CurrentIP);                                             //IP
                item.SubItems.Add(message);                                                 //Message
                item.SubItems.Add("Offline");                                          //Status
                item.SubItems.Add(CurrentFileName);                                           //Filename
                item.SubItems.Add("0");                                                //Speed
                item.ImageIndex = imageIndex;
                item.Tag = DateTime.Now;

                // Add the ListViewItem to the log
                AddLogItem(item);

                // Dispose the CustomWebClient object
                webClient.Dispose();
            }
        }
        #endregion
        #region Private Methods
        /// <summary>Adds the specified item to the connection log, shows it if its type is visible, and saves the log.</summary>
        private void AddLogItem(ListViewItem item)
        {
            // Add the ListViewItem to the log
            Global.ConnectionLog.Add(item);

            bool visible;
            switch (item.ImageIndex)
            {
                case 0: // Successful
                    visible = ToolBarShowSuccessful.Pushed;
                    break;
                case 1: // Status Changed
                    visible = ToolBarShowStatusChanges.Pushed;
                    break;
                default: // Error
                    visible = ToolBarShowErrors.Pushed;
                    break;
            }

            if (visible)
            {
                ListLog.Items.Insert(0, item);
                item.EnsureVisible();
            }

            // Save after every row, so an outage is on disk even if the PC loses power
            if (!Global.SaveConnectionLog(Settings.Default.LogFile))
                StatusMain.Text = "Unable to save the log: " + Global.LastSaveError;
        }
        #endregion
        #region Public Methods
        /// <summary>Retrieves the remote IPv4 address for this system from icanhazip.com.</summary>
        public void GetRemoteIP()
        {
            // Reset elapsed seconds
            p_ElapsedSeconds = 0;

            if (Global.IsOnline)
            {
                string api = string.Empty;
                if (Settings.Default.AutoIPDetect)
                {
                    api = "http://icanhazip.com";
                }
                else
                {
                    api = Settings.Default.IPDetectURL;
                }

                // Settings rejects invalid URLs, but the setting could predate that check
                if (!Global.TryGetWebUri(api, out Uri apiUri))
                {
                    StatusMain.Text = "Unable to test speed: the custom IP lookup URL in Configuration Settings isn't a valid web address";
                    return;
                }

                // Create a new WebClient object and download the string
                CustomWebClient webClient = new CustomWebClient();
                webClient.DownloadDataCompleted += webClient_DownloadDataCompleted;
                webClient.DownloadProgressChanged += webClient_DownloadProgressChanged;
                webClient.DownloadStringCompleted += webClient_DownloadStringCompleted;
                webClient.DownloadStringAsync(apiUri);
            }
            else
            {
                // Local variables
                StatusCode newStatus = StatusCode.Offline;
                int imageIndex = 2;
                string ip = "-";
                string message = "-";

                // Set status to Offline
                StatusMain.Text = "Unable to test speed: " + Global.ConvertTime(DateTime.Now);

                if (!p_CurrentIP.Equals(ip))
                {
                    message = "IP Changed";
                    imageIndex = 1;
                    p_CurrentIP = ip;
                }

                if (!p_CurrentStatus.Equals(newStatus))
                {
                    message = "Offline";
                    imageIndex = 2;
                    p_CurrentStatus = newStatus;
                }

                // Create a new ListViewItem object
                ListViewItem item = new ListViewItem(Global.ConvertTime(DateTime.Now));//Time
                item.SubItems.Add(Global.ConvertBytes(0) + "/s");                      //b/s
                item.SubItems.Add(p_CurrentIP);                                             //IP
                item.SubItems.Add(message);                                                 //Message
                item.SubItems.Add("Offline");                                          //Status
                item.SubItems.Add(CurrentFileName);                                           //Filename
                item.SubItems.Add("0");                                                //Speed
                item.ImageIndex = imageIndex;
                item.Tag = DateTime.Now;

                // Add the ListViewItem to the log
                AddLogItem(item);
            }
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
            // Clear the ListView
            ListLog.Items.Clear();

            if (Global.ConnectionLog != null)
            {
                foreach (ListViewItem item in Global.ConnectionLog)
                {
                    DateTime time = (DateTime)item.Tag;

                    if (time > p_StartupTime)
                    {
                        switch (item.ImageIndex)
                        {
                            case 0: // Successful
                                if (ToolBarShowSuccessful.Pushed)
                                {
                                    ListLog.Items.Insert(0, item);
                                    item.EnsureVisible();
                                }
                                break;
                            case 1: // Status Changed
                                if (ToolBarShowStatusChanges.Pushed)
                                {
                                    ListLog.Items.Insert(0, item);
                                    item.EnsureVisible();
                                }
                                break;
                            case 2: // Error
                                if (ToolBarShowErrors.Pushed)
                                {
                                    ListLog.Items.Insert(0, item);
                                    item.EnsureVisible();
                                }
                                break;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
