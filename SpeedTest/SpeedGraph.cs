#region Using
// Imported namespaces
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using SpeedTest.Properties;
#endregion
namespace SpeedTest
{
    /// <summary>Custom user control that displays a graph representing download speeds.</summary>
    public partial class SpeedGraph : UserControl
    {
        #region Members
        // Private variables
        private long p_MaxBits;
        private long p_MaxBytes;

        // One step on the speed axis, matching the MB (or mb) that Global.ConvertBytes shows in the log
        private const long BytesPerMB = 1024 * 1024;
        #endregion
        #region Properties
        /// <summary>Gets the maximum number of bits downloaded according to the connection log.</summary>
        public long MaxBits
        {
            get { return p_MaxBits; }
        }

        /// <summary>Gets the maximum number of bytes downloaded according to the connection log.</summary>
        public long MaxBytes
        {
            get { return p_MaxBytes; }
        }
        #endregion
        #region Constructor
        /// <summary>Initializes a new instance of the SpeedGraph class.</summary>
        public SpeedGraph()
        {
            // This call is required by the designer
            InitializeComponent();

            // Initialize private variables
            p_MaxBits = BytesPerMB * 2;
            p_MaxBytes = BytesPerMB * 2;

            // Initialize SpeedGraph properties
            this.Font = new Font(SystemFonts.MenuFont.FontFamily, 8.0f);
            this.ResizeRedraw = true;

            // The graph repaints every second, so draw off-screen first to stop it flickering
            this.DoubleBuffered = true;

            // Initialize child controls
            ComboTimeScale.Font = SystemFonts.MenuFont;
            ComboTimeScale.SelectedIndex = Settings.Default.TimeScale;
        }
        #endregion
        #region Overrides
        /// <summary>Occurs when the SpeedGraph is redrawn.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Local variables
            int speed = 0;
            int time = 0;

            // Scale the graph to the visible log entries before drawing the axis or any points
            UpdateMaxSpeeds();

            // Adjust Graphics object properties
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Draw the background
            e.Graphics.FillRectangle(SystemBrushes.Window, 40, 20, Width - 60, Height - 80);

            // Draw the border
            e.Graphics.DrawRectangle(SystemPens.ControlDark, 40, 20, Width - 60, Height - 80);

            // Create a new SolidBrush object
            using (SolidBrush b = new SolidBrush(ForeColor))
            {
                if (Height > 80)
                {
                    // Local variables
                    int totalTime = 0;
                    int counter = 0;
                    int speedCounter = 0;
                    bool flag = true;
                    bool speedFlag = true;

                    switch (Settings.Default.TimeScale)
                    {
                        case 0: // 10 Minutes
                            totalTime = 10;
                            break;
                        case 1: // 30 Minutes
                            totalTime = 30;
                            break;
                        case 2: // 1 Hour
                            totalTime = 60;
                            break;
                        case 3: // 12 Hours
                            totalTime = 12;
                            break;
                        case 4: // 24 Hours
                            totalTime = 24;
                            break;
                    }

                    // Draw the Timeline
                    for (float f = Width - 20.0f; f >= 39.0f; f -= Global.Percent(1, totalTime, Width - 60.0f))
                    {
                        if (f < Width - 20.0f && f > 41.0f) e.Graphics.DrawLine(SystemPens.Control, f, 21, f, Height - 60);
                        e.Graphics.DrawLine(SystemPens.ControlDark, f, Height - 60, f, Height - 56);
                        if (Width > 320 && flag)
                        {
                            DateTime time2 = DateTime.Now;

                            switch (Settings.Default.TimeScale)
                            {
                                case 0: // 10 Minutes
                                    time2 = DateTime.Now.Subtract(new TimeSpan(0, 0, time, 0));
                                    break;
                                case 1: // 30 Minutes
                                    time2 = DateTime.Now.Subtract(new TimeSpan(0, 0, time, 0));
                                    break;
                                case 2: // 1 Hour
                                    time2 = DateTime.Now.Subtract(new TimeSpan(0, 0, time, 0));
                                    break;
                                case 3: // 12 Hours
                                    time2 = DateTime.Now.Subtract(new TimeSpan(0, time, 0, 0));
                                    break;
                                case 4: // 24 Hours
                                    time2 = DateTime.Now.Subtract(new TimeSpan(0, time, 0, 0));
                                    break;
                            }

                            if (time.ToString().Length > 1) e.Graphics.DrawString(time2.ToString("HH:mm"), Font, b, f - 16, Height - 56);
                            else e.Graphics.DrawString(time2.ToString("HH:mm"), Font, b, f - 13, Height - 56);
                        }
                        time += 1;

                        if (Settings.Default.TimeScale == 2)
                        {
                            counter++;

                            if (counter == 4 || counter == 8)
                            {
                                flag = true;
                            }
                            else flag = false;

                            if (counter == 8) counter = 0;
                        }
                        else flag = !flag;
                    }

                    // Draw the speed units
                    if (Settings.Default.SpeedUnits)
                    {
                        for (float f = Height - 60.0f; f >= 19.0f; f -= Global.Percent(BytesPerMB, p_MaxBytes, Height - 80.0f))
                        {
                            if (f < Height - 60.0f && f > 21.0f) e.Graphics.DrawLine(SystemPens.Control, 41, f, Width - 21, f);
                            e.Graphics.DrawLine(SystemPens.ControlDark, 36, f, 40, f);
                            if (Height > 120 && speedFlag) e.Graphics.DrawString(speed.ToString(), Font, b, 26, f - 7);
                            speed += 1;
                            speedCounter++;

                            if (p_MaxBytes > BytesPerMB * 60)
                            {
                                if (speedCounter == 6)
                                {
                                    speedCounter = 0;
                                    speedFlag = true;
                                }
                                else speedFlag = false;
                            }
                            else if (p_MaxBytes > BytesPerMB * 40)
                            {
                                if (speedCounter == 4)
                                {
                                    speedCounter = 0;
                                    speedFlag = true;
                                }
                                else speedFlag = false;
                            }
                            else if (p_MaxBytes > BytesPerMB * 20)
                            {
                                if (speedCounter == 2)
                                {
                                    speedCounter = 0;
                                    speedFlag = true;
                                }
                                else speedFlag = false;
                            }
                            else if (p_MaxBytes > BytesPerMB * 10)
                            {
                                speedCounter = 0;
                                speedFlag = !speedFlag;
                            }
                            else
                            {
                                speedCounter = 0;
                                speedFlag = true;
                            }
                        }
                    }
                    else
                    {
                        for (float f = Height - 60.0f; f >= 19.0f; f -= Global.Percent(BytesPerMB, p_MaxBits, Height - 80.0f))
                        {
                            if (f < Height - 60.0f && f > 21.0f) e.Graphics.DrawLine(SystemPens.Control, 41, f, Width - 21, f);
                            e.Graphics.DrawLine(SystemPens.ControlDark, 36, f, 40, f);
                            if (Height > 120 && speedFlag) e.Graphics.DrawString(speed.ToString(), Font, b, 26, f - 7);
                            speed += 1;
                            speedCounter++;

                            if (p_MaxBits > BytesPerMB * 60)
                            {
                                if (speedCounter == 6)
                                {
                                    speedCounter = 0;
                                    speedFlag = true;
                                }
                                else speedFlag = false;
                            }
                            else if (p_MaxBits > BytesPerMB * 40)
                            {
                                if (speedCounter == 4)
                                {
                                    speedCounter = 0;
                                    speedFlag = true;
                                }
                                else speedFlag = false;
                            }
                            else if (p_MaxBits > BytesPerMB * 20)
                            {
                                if (speedCounter == 2)
                                {
                                    speedCounter = 0;
                                    speedFlag = true;
                                }
                                else speedFlag = false;
                            }
                            else if (p_MaxBits > BytesPerMB * 10)
                            {
                                speedCounter = 0;
                                speedFlag = !speedFlag;
                            }
                            else
                            {
                                speedCounter = 0;
                                speedFlag = true;
                            }
                        }
                    }

                    // Create a new StringFormat object
                    using (StringFormat sf = (StringFormat)StringFormat.GenericTypographic.Clone())
                    {
                        // Set vertical text direction
                        sf.FormatFlags = StringFormatFlags.DirectionVertical;

                        if (Height > 120)
                        {
                            if (Settings.Default.SpeedUnits) e.Graphics.DrawString("Speed (MB)", Font, b, new PointF(6, (Height - 94) / 2), sf);
                            else e.Graphics.DrawString("Speed (mb)", Font, b, new PointF(6, (Height - 94) / 2), sf);
                        }

                        if (Width > 320) e.Graphics.DrawString("Timeline:", SystemFonts.MenuFont, b, Width - 186, Height - 32);
                    }

                    // Draw the color keys
                    if (Height > 30)
                    {
                        e.Graphics.FillRectangle(Brushes.Orange, 6, Height - 30, 122, 14);
                        e.Graphics.DrawString("No Internet Connection", Font, b, 6, Height - 30);

                        e.Graphics.FillRectangle(Brushes.Yellow, 134, Height - 30, 104, 14);
                        e.Graphics.DrawString("Logger Not Running", Font, b, 134, Height - 30);
                    }

                    if (Global.ConnectionLog != null)
                    {
                        // Local variables
                        int onlineCount = 0;
                        long lastItemSpeed = 0;
                        if (Global.ConnectionLog.Count > 0) 
                            lastItemSpeed = Global.ToInt64(Global.ConnectionLog[Global.ConnectionLog.Count - 1].SubItems[6].Text);

                        // Bits instead of bytes
                        if (!Settings.Default.SpeedUnits) lastItemSpeed *= 8;
                        
                        List<PointF> points = new List<PointF>();
                        List<PointF> loggerPoints = new List<PointF>();
                        List<PointF> offlinePoints = new List<PointF>();
                        PointF lastPoint = new PointF();
                        PointF firstOfflinePoint = new PointF();

                        if (Settings.Default.SpeedUnits) lastPoint = new PointF(Width - 21, (Height - 61) - Global.Percent(lastItemSpeed, p_MaxBytes, Height - 80));
                        else lastPoint = new PointF(Width - 21, (Height - 61) - Global.Percent(lastItemSpeed, p_MaxBits, Height - 80));

                        points.Add(new PointF(Width - 20, Height - 60));

                        if (Settings.Default.SpeedUnits) points.Add(new PointF(Width - 20, (Height - 61) - Global.Percent(lastItemSpeed, p_MaxBytes, Height - 80)));
                        else points.Add(new PointF(Width - 20, (Height - 61) - Global.Percent(lastItemSpeed, p_MaxBits, Height - 80)));

                        // Local variables
                        long elapsedTime = GetTimelineStart();
                        long now = Global.ToUnixTime(DateTime.Now);
                        long total = now - elapsedTime;

                        // Draw the download speed, newest first
                        for (int i = Global.ConnectionLog.Count - 1; i >= 0; i--)
                        {
                            // Local variables
                            long itemTime = Global.ToUnixTime((DateTime)Global.ConnectionLog[i].Tag);
                            long complete = itemTime - elapsedTime;

                            // The log is in time order, so every older row is off the left edge too.
                            // Stopping here keeps repaints fast however long the log grows.
                            if (complete <= 0) break;

                            long itemSpeed = Global.ToInt64(Global.ConnectionLog[i].SubItems[6].Text);

                            // Bits instead of bytes
                            if (!Settings.Default.SpeedUnits) itemSpeed *= 8;

                            float x = Global.Percent(complete, total, Width - 61) + 40;

                            if (Settings.Default.SpeedUnits) e.Graphics.DrawLine(Pens.Green, lastPoint.X, lastPoint.Y, x, (Height - 61) - Global.Percent(itemSpeed, p_MaxBytes, Height - 80));
                            else e.Graphics.DrawLine(Pens.Green, lastPoint.X, lastPoint.Y, x, (Height - 61) - Global.Percent(itemSpeed, p_MaxBits, Height - 80));

                            // Local variables
                            string change = GetConnectionChange(Global.ConnectionLog[i]);

                            if (IsLoggerEvent(Global.ConnectionLog[i], "Logger Started"))
                            {
                                loggerPoints.Add(new PointF(x, 20));
                                loggerPoints.Add(new PointF(x, Height - 60));

                                e.Graphics.DrawLine(Pens.Yellow, x, 21, x, Height - 60);
                            }
                            else if (IsLoggerEvent(Global.ConnectionLog[i], "Logger Stopped"))
                            {
                                loggerPoints.Add(new PointF(x, Height - 60));
                                loggerPoints.Add(new PointF(x, 20));

                                using (SolidBrush b2 = new SolidBrush(Color.FromArgb(100, Color.Yellow)))
                                {
                                    e.Graphics.FillPolygon(b2, loggerPoints.ToArray());
                                }
                                loggerPoints.Clear();

                                e.Graphics.DrawLine(Pens.Yellow, x, 21, x, Height - 60);
                            }
                            if (change.Equals("Online"))
                            {
                                if (offlinePoints.Count == 0)
                                {
                                    offlinePoints.Add(new PointF(x, 20));
                                    offlinePoints.Add(new PointF(x, Height - 60));
                                }
                                else
                                {
                                    offlinePoints.Clear();
                                }

                                onlineCount++;
                            }
                            else if (change.Equals("Offline"))
                            {
                                if (offlinePoints.Count == 2)
                                {
                                    offlinePoints.Add(new PointF(x, Height - 60));
                                    offlinePoints.Add(new PointF(x, 20));

                                    using (SolidBrush b2 = new SolidBrush(Color.FromArgb(100, Color.Orange)))
                                    {
                                        e.Graphics.FillPolygon(b2, offlinePoints.ToArray());
                                    }
                                    offlinePoints.Clear();
                                }

                                if (onlineCount == 0) firstOfflinePoint = new PointF(x, Height - 60);
                            }

                            if (Settings.Default.SpeedUnits) lastPoint = new PointF(x, (Height - 61) - Global.Percent(itemSpeed, p_MaxBytes, Height - 80));
                            else lastPoint = new PointF(x, (Height - 61) - Global.Percent(itemSpeed, p_MaxBits, Height - 80));

                            points.Add(lastPoint);
                        }

                        if (!firstOfflinePoint.X.Equals(0) && !firstOfflinePoint.Y.Equals(0))
                        {
                            offlinePoints.Clear();
                            offlinePoints.Add(new PointF(firstOfflinePoint.X, 20));
                            offlinePoints.Add(new PointF(Width - 20, 20));
                            offlinePoints.Add(new PointF(Width - 20, Height - 60));
                            offlinePoints.Add(new PointF(firstOfflinePoint.X, Height - 60));

                            using (SolidBrush b2 = new SolidBrush(Color.FromArgb(100, Color.Orange)))
                            {
                                e.Graphics.FillPolygon(b2, offlinePoints.ToArray());
                            }

                        }

                        e.Graphics.DrawLine(Pens.Green, 40, lastPoint.Y, lastPoint.X, lastPoint.Y);

                        points.Add(new PointF(40, lastPoint.Y));
                        points.Add(new PointF(40, Height - 60));

                        // Fill in the download speed
                        using (SolidBrush b2 = new SolidBrush(Color.FromArgb(100, Color.Green)))
                        {
                            e.Graphics.FillPolygon(b2, points.ToArray());
                        }
                    }
                }
            }

            // Continue processing the event as usual
            base.OnPaint(e);
        }

        /// <summary>Occurs when the SpeedGraph is resized.</summary>
        protected override void OnResize(EventArgs e)
        {
            // Set the Timeline combo box visibility
            ComboTimeScale.Visible = Width > 260 && Height > 80;

            // continue processing the event as usual
            base.OnResize(e);
        }
        #endregion
        #region Private Methods
        /// <summary>Gets the Unix time at the left edge of the graph for the selected time scale.</summary>
        private static long GetTimelineStart()
        {
            switch (Settings.Default.TimeScale)
            {
                case 0: // 10 Minutes
                    return Global.ToUnixTime(DateTime.Now.Subtract(new TimeSpan(0, 0, 10, 0)));
                case 1: // 30 Minutes
                    return Global.ToUnixTime(DateTime.Now.Subtract(new TimeSpan(0, 0, 30, 0)));
                case 2: // 1 Hour
                    return Global.ToUnixTime(DateTime.Now.Subtract(new TimeSpan(0, 1, 0, 0)));
                case 3: // 12 Hours
                    return Global.ToUnixTime(DateTime.Now.Subtract(new TimeSpan(0, 12, 0, 0)));
                case 4: // 24 Hours
                    return Global.ToUnixTime(DateTime.Now.Subtract(new TimeSpan(0, 24, 0, 0)));
                default:
                    return 0;
            }
        }

        /// <summary>Sets the maximum speeds to the fastest log entry on the graph, so every point and the axis share one scale.</summary>
        private void UpdateMaxSpeeds()
        {
            // Local variables
            long maxBytes = 0;
            long timelineStart = GetTimelineStart();

            if (Global.ConnectionLog != null)
            {
                // Newest first, stopping at the first row before the timeline. The newest row always counts,
                // because it's drawn at the right edge even when it's older than the timeline.
                for (int i = Global.ConnectionLog.Count - 1; i >= 0; i--)
                {
                    if (i < Global.ConnectionLog.Count - 1 && Global.ToUnixTime((DateTime)Global.ConnectionLog[i].Tag) <= timelineStart) break;

                    maxBytes = Math.Max(maxBytes, Global.ToInt64(Global.ConnectionLog[i].SubItems[6].Text));
                }
            }

            // Never scale below 2 MB (or 2 mb)
            p_MaxBytes = Math.Max(BytesPerMB * 2, maxBytes);
            p_MaxBits = Math.Max(BytesPerMB * 2, maxBytes * 8);
        }

        /// <summary>Gets the connection change logged in the specified row: "Online", "Offline", "IP Changed" or "-".</summary>
        private static string GetConnectionChange(ListViewItem item)
        {
            string message = item.SubItems[3].Text;
            string status = item.SubItems[4].Text;

            // Rows logged before the column reorder hold the result (OK, Error, Offline) in Message and the change in Status
            bool legacy = message.Equals("OK") || message.Equals("Error") || (message.Equals("Offline") && !status.Equals("Offline"));

            return legacy ? status : message;
        }

        /// <summary>Gets a value indicating whether the specified row records the specified logger event, in either column.</summary>
        private static bool IsLoggerEvent(ListViewItem item, string loggerEvent)
        {
            return item.SubItems[3].Text.Equals(loggerEvent) || item.SubItems[4].Text.Equals(loggerEvent);
        }
        #endregion
        #region Event Handlers
        /// <summary>Occurs when the selected index changes in the Timeline combo box.</summary>
        private void ComboTimeScale_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update configuration settings
            Settings.Default.TimeScale = ComboTimeScale.SelectedIndex;

            // Re-draw the control
            Invalidate();
        }
        #endregion
    }
}
