#region Using
// Imported namespaces
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;
using SpeedTest.Properties;
#endregion
namespace SpeedTest
{
    /// <summary>The main class containing global objects and the entry point of the application.</summary>
    public static class Global
    {
        #region Enums
        /// <summary>Returned by the GetThemeMargins function to define the margins of windows that have visual styles applied.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Margins
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;

            public Margins(System.Drawing.Printing.Margins margins)
            {
                cxLeftWidth = margins.Left;
                cxRightWidth = margins.Right;
                cyTopHeight = margins.Top;
                cyBottomHeight = margins.Bottom;
            }

            public Margins(int left, int right, int top, int bottom)
            {
                cxLeftWidth = left;
                cxRightWidth = right;
                cyTopHeight = top;
                cyBottomHeight = bottom;
            }
        }
        #endregion
        #region Members
        // Global variables
        private static FormMain _gFormMain;
        private static List<ListViewItem> _gConnectionLog;

        // Connection log load and save state
        private static bool _gLogLoadFailed;
        private static int _gSkippedLogRows;
        private static bool _gLogBackedUp;
        private static string _gLastSaveError;
        #endregion
        #region Properties
        /// <summary>Gets a value indicating whether Desktop Composition is enabled on the system.</summary>
        public static bool CompositionEnabled
        {
            get
            {
                if (Environment.OSVersion.Version.Major < 6) return false;

                var result = false;

                DwmIsCompositionEnabled(ref result);

                return result;
            }
        }

        /// <summary>Gets the main Form associated with this application.</summary>
        public static FormMain FormMain => _gFormMain;

        /// <summary>Gets the connection log associated with this application.</summary>
        public static List<ListViewItem> ConnectionLog => _gConnectionLog;

        /// <summary>Gets the error from the last save of the connection log, or null if it succeeded.</summary>
        public static string LastSaveError => _gLastSaveError;

        /// <summary>Gets a value determining if this computer is currently connected to the network.</summary>
        public static bool IsOnline
        {
            get
            {
                try
                {
                    var myPing = new Ping();
                    var host = "google.com";
                    var buffer = new byte[32];
                    var pingOptions = new PingOptions();
                    var reply = myPing.Send(host, 1000, buffer, pingOptions);

                    return reply != null && reply.Status == IPStatus.Success;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>Gets the folder that holds the connection log and the auto-start flag.</summary>
        public static string AppDataFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpeedTest");

        /// <summary>Gets the file the Windows Startup script creates, so the app knows to start logging. A ClickOnce shortcut can't pass /start.</summary>
        public static string AutoStartFlagFile => Path.Combine(AppDataFolder, "autostart.flag");

        /// <summary>Gets a value determining if this computer is currently running Windows 8.</summary>
        public static bool IsWindows8 => Environment.OSVersion.Version.Major.Equals(6) && Environment.OSVersion.Version.Minor.Equals(2);

        #endregion
        #region Entry Point
        /// <summary>The main entry point of the application.</summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Start logging straight away when started with /start, or from the Windows Startup folder
            bool startLogging = Array.Exists(args, a => a.Equals("/start", StringComparison.OrdinalIgnoreCase)) || ConsumeAutoStartFlag();

            // Enable visual styles for the application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize global variables
            _gFormMain = new FormMain(startLogging);
            _gConnectionLog = new List<ListViewItem>();

            // Initialize settings
            if (string.IsNullOrEmpty(Settings.Default.DownloadURL))
                Settings.Default.DownloadURL = "https://app.tfn.co.za/test/test100k.zip";
            if (string.IsNullOrEmpty(Settings.Default.IPDetectURL))
                Settings.Default.IPDetectURL = "http://icanhazip.com";
            if (string.IsNullOrEmpty(Settings.Default.LogFile)) Settings.Default.LogFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "SpeedTest" + Path.DirectorySeparatorChar + "SpeedTestLog2.csv";
            if (Settings.Default.LogFile.ToLower().StartsWith(Application.StartupPath.ToLower())) Settings.Default.LogFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "SpeedTest" + Path.DirectorySeparatorChar + "ConnectionLog2.csv";

            // Create SpeedTest folder
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "SpeedTest")) Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "SpeedTest");

            // Load the connection log
            LoadConnectionLog(Settings.Default.LogFile);

            // Begin running the application
            Application.Run(_gFormMain);
        }
        #endregion
        #region External Methods
        // ReSharper disable once StringLiteralTypo
        [DllImport("dwmapi.dll")]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMargins);

        // ReSharper disable once StringLiteralTypo
        [DllImport("dwmapi.dll")]
        public static extern void DwmIsCompositionEnabled(ref bool pfEnabled);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("UXTheme.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SetWindowTheme(IntPtr hWnd, string textSubAppName, string textSubIdList);
        #endregion
        #region Public Methods
        /// <summary>Converts the specified bytes value into a human-readable string.</summary>
        public static string ConvertBytes(long bytes)
        {
            return ConvertBytes(bytes, CultureInfo.CurrentCulture, "###,###,##0.00");
        }

        /// <summary>Converts the specified bytes value into a human-readable string, using the specified culture and number format.</summary>
        private static string ConvertBytes(long bytes, IFormatProvider provider, string format)
        {
            // Bytes or bits, depending on the configured speed units
            string[] units = Settings.Default.SpeedUnits ? new[] { "B", "KB", "MB", "GB", "TB" } : new[] { "b", "kb", "mb", "gb", "tb" };
            double num = Settings.Default.SpeedUnits ? bytes : bytes * 8.0;
            int unit = 0;

            while (Math.Abs(num) >= 1024.0 && unit < units.Length - 1)
            {
                num /= 1024.0;
                unit++;
            }

            return Math.Round(num, 2).ToString(format, provider) + " " + units[unit];
        }

        /// <summary>Converts the specified DateTime object into a human-readable string.</summary>
        public static string ConvertTime(DateTime time)
        {
            switch (Settings.Default.TimeFormat)
            {
                case 0: // ISO Format
                    return time.ToString("yyyy/MM/dd H:mm:ss");
                case 1: // EU Format
                    return time.ToString("dd/MM/yyyy H:mm:ss");
                case 2: // US Format
                    return time.ToString("MM/dd/yyyy hh:mm:ss tt");
                default:
                    return time.ToShortDateString() + " " + time.ToShortTimeString();
            }
        }

        /// <summary>Extends the window frame into the client area.</summary>
        public static void ExtendFrame(IntPtr handle, System.Drawing.Printing.Margins margins)
        {
            var m = new Margins(margins);
            DwmExtendFrameIntoClientArea(handle, ref m);
        }

        /// <summary>Converts a Unix Timestamp value into a DateTime object.</summary>
        public static DateTime FromUnixTime(long time)
        {
            if (time == 0) return DateTime.Now;

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return epoch.AddSeconds(Convert.ToDouble(time)).ToLocalTime();
        }

        /// <summary>Loads the connection log from the specified CSV file.</summary>
        public static void LoadConnectionLog(string fileName)
        {
            try
            {
                if (File.Exists(fileName)) _gConnectionLog.AddRange(ReadLogRows(fileName));
            }
            catch
            {
                // The file exists but couldn't be read (e.g. it's locked). SaveConnectionLog reads it again before overwriting it.
                _gLogLoadFailed = true;
            }
        }

        /// <summary>Reads the rows of the specified connection log CSV file, counting any malformed rows it skips.</summary>
        private static List<ListViewItem> ReadLogRows(string fileName)
        {
            var rows = new List<ListViewItem>();

            using (var reader = new StreamReader(fileName))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    // Skip blank lines and the header row
                    if (string.IsNullOrWhiteSpace(line) || line.ToLower().StartsWith("index")) continue;

                    var item = ParseLogRow(line);

                    if (item != null) rows.Add(item);
                    else _gSkippedLogRows++;
                }
            }

            return rows;
        }

        /// <summary>Parses one row of the connection log CSV file, or returns null if the row is malformed.</summary>
        private static ListViewItem ParseLogRow(string line)
        {
            try
            {
                // Keep empty entries so columns stay aligned (e.g. an empty Filename)
                var s = line.Split(',');

                if (s.Length < 9) return null;

                // Only Speed(S) can contain commas (older versions formatted it with the PC's culture, e.g. "512,34 kb/s"),
                // so read the first three columns from the front and the last five from the back
                var last = s.Length - 5;
                var time = FromUnixTime(ToInt64(s[1]));
                var imageIndex = ToInt32(s[0]);
                var speed = ToInt64(s[last + 4]);

                var item = new ListViewItem(ConvertTime(time)) // Time (String)
                {
                    ImageIndex = imageIndex >= 0 && imageIndex <= 2 ? imageIndex : 0, // Index
                    Tag = time // Time (DateTime)
                };
                item.SubItems.Add(ConvertBytes(speed) + "/s"); // Speed (String), rebuilt from Speed(B)
                item.SubItems.Add(s[last]); // IP
                item.SubItems.Add(s[last + 1]); // Message
                item.SubItems.Add(s[last + 2]); // Status
                item.SubItems.Add(s[last + 3]); // Filename
                item.SubItems.Add(speed.ToString(CultureInfo.InvariantCulture)); // Speed (Bytes)

                return item;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Converts the specified text into an absolute http or https Uri. Returns false if it isn't one.</summary>
        public static bool TryGetWebUri(string text, out Uri uri)
        {
            return Uri.TryCreate((text ?? string.Empty).Trim(), UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>Deletes the auto-start flag file, and returns true if the Startup script created it in the last few minutes.</summary>
        private static bool ConsumeAutoStartFlag()
        {
            try
            {
                if (!File.Exists(AutoStartFlagFile)) return false;

                // Ignore a flag left over from a start that never reached the app
                bool recent = DateTime.Now - File.GetLastWriteTime(AutoStartFlagFile) < TimeSpan.FromMinutes(10);
                File.Delete(AutoStartFlagFile);

                return recent;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Calculates a percentage (0.0-max%) given the specified values.</summary>
        public static float Percent(float complete, float total, float max = 100.0f)
        {
            if (complete <= 0.0f || total <= 0.0f) return 0.0f;

            return (complete / total) * max;
        }

        /// <summary>Calculates a percentage (0-max%) given the specified values.</summary>
        public static int Percent(int complete, int total, int max = 100)
        {
            if (complete == 0 || total == 0) return 0;

            var result = Convert.ToDouble(complete) / Convert.ToDouble(total);
            result *= Convert.ToDouble(max);

            return Convert.ToInt32(result);
        }

        /// <summary>Saves the connection log in CSV file format. Returns false if it couldn't be saved; LastSaveError then says why.</summary>
        public static bool SaveConnectionLog(string fileName)
        {
            try
            {
                var folder = Path.GetDirectoryName(fileName);
                if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);

                // If the log couldn't be read at startup, read it now and put its rows before this session's.
                // If it still can't be read, nothing is saved and the log stays in memory.
                if (_gLogLoadFailed)
                {
                    if (File.Exists(fileName)) _gConnectionLog.InsertRange(0, ReadLogRows(fileName));
                    _gLogLoadFailed = false;
                }

                // Keep a copy of a log with malformed rows before the first save drops them
                if (_gSkippedLogRows > 0 && !_gLogBackedUp && File.Exists(fileName))
                {
                    File.Copy(fileName, fileName + "." + DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + ".bak");
                }
                _gLogBackedUp = true;

                // Write to a temporary file first, so a crash or power cut mid-save can't destroy the log
                var tempFile = fileName + ".tmp";

                using (var writer = new StreamWriter(tempFile))
                {
                    writer.WriteLine("Index,Time(U),Time(S),Speed(S),IP,Message,Status,Filename,Speed(B)");

                    foreach (var item in _gConnectionLog)
                    {
                        var time = (DateTime)item.Tag;
                        var speed = ToInt64(item.SubItems[6].Text);

                        writer.WriteLine(item.ImageIndex + "," +                                                    // Index
                            ToUnixTime(time) + "," +                                                                // Time (Unix Timestamp)
                            CsvField(ConvertTime(time)) + "," +                                                     // Time (String)
                            ConvertBytes(speed, CultureInfo.InvariantCulture, "0.00") + "/s," +                     // Speed (String), never with commas
                            CsvField(item.SubItems[2].Text) + "," +                                                 // IP
                            CsvField(item.SubItems[3].Text) + "," +                                                 // Message
                            CsvField(item.SubItems[4].Text) + "," +                                                 // Status
                            CsvField(item.SubItems[5].Text) + "," +                                                 // Filename
                            speed.ToString(CultureInfo.InvariantCulture));                                          // Speed (Bytes)
                    }
                }

                if (File.Exists(fileName)) File.Replace(tempFile, fileName, null);
                else File.Move(tempFile, fileName);

                _gLastSaveError = null;
                return true;
            }
            catch (Exception ex)
            {
                _gLastSaveError = ex.Message;
                return false;
            }
        }

        /// <summary>Makes the specified text safe for one column of the CSV log by removing commas.</summary>
        private static string CsvField(string text)
        {
            return string.IsNullOrEmpty(text) ? "n/a" : text.Replace(',', ' ');
        }

        /// <summary>Calculates a scalar value (0.0-1.0) given the specified values.</summary>
        public static float Scalar(float complete, float total)
        {
            if (complete <= 0.0f || total <= 0.0f) return 0.0f;

            return complete / total;
        }

        /// <summary>Converts a string value into a Int32 value.</summary>
        public static int ToInt32(string s)
        {
            return int.TryParse(s, out var result) ? result : 0;
        }

        /// <summary>Converts a string value into a Int64 value.</summary>
        public static long ToInt64(string s)
        {
            return long.TryParse(s, out var result) ? result : 0;
        }

        /// <summary>Converts a DateTime object into a Unix Timestamp value.</summary>
        public static long ToUnixTime(DateTime time)
        {
            return Convert.ToInt64((time - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).TotalSeconds);
        }
        #endregion
    }
}
