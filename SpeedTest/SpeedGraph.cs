#region Using
// Imported namespaces
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
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

        // Space around the plot: the summary row above it, time labels and the key below it
        private const int PlotLeft = 56;
        private const int PlotTop = 96;
        private const int PlotRight = 24;
        private const int PlotBottom = 56;
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
            this.BackColor = Theme.Background;
            this.Font = Theme.Small;
            this.ResizeRedraw = true;

            // The graph repaints every second, so draw off-screen first to stop it flickering
            this.DoubleBuffered = true;

            // Initialize child controls
            ComboTimeScale.Font = Theme.UI;
            ComboTimeScale.SelectedIndex = Settings.Default.TimeScale;
        }
        #endregion
        #region Overrides
        /// <summary>Occurs when the SpeedGraph is redrawn.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.Background);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle plot = new Rectangle(PlotLeft, PlotTop, Width - PlotLeft - PlotRight, Height - PlotTop - PlotBottom);

            if (plot.Width > 60 && plot.Height > 40)
            {
                // Scale the graph to the visible log entries before drawing the axis or any points
                UpdateMaxSpeeds();

                bool bytes = Settings.Default.SpeedUnits;
                long timelineStart = GetTimelineStart();
                long now = Global.ToUnixTime(DateTime.Now);
                double span = Math.Max(1, now - timelineStart);

                // The rows on the timeline, oldest first. The log is in time order, so reading it newest first
                // and stopping at the first row before the timeline keeps repaints fast however long it grows.
                List<ListViewItem> rows = new List<ListViewItem>();

                if (Global.ConnectionLog != null)
                {
                    for (int i = Global.ConnectionLog.Count - 1; i >= 0 && Global.ToUnixTime((DateTime)Global.ConnectionLog[i].Tag) > timelineStart; i--)
                        rows.Add(Global.ConnectionLog[i]);
                }

                rows.Reverse();

                // Speed axis: a round top value, with about four steps up to it
                double maxValue = (bytes ? p_MaxBytes : p_MaxBits) / (double)BytesPerMB;
                double step = NiceStep(maxValue / 4);
                double axisMax = Math.Ceiling(maxValue / step - 0.0001) * step;

                Func<long, float> x = time => plot.Left + (float)((time - timelineStart) / span * plot.Width);
                Func<long, float> y = speed => plot.Bottom - (float)((bytes ? speed : speed * 8.0) / BytesPerMB / axisMax * plot.Height);

                DrawSummary(g, rows);
                DrawBands(g, plot, rows, x);
                DrawGrid(g, plot, step, axisMax, bytes, timelineStart, now, x);

                if (rows.Count == 0) DrawEmpty(g, plot);
                else DrawSpeed(g, plot, rows, x, y);

                DrawKey(g);
            }

            // Continue processing the event as usual
            base.OnPaint(e);
        }

        /// <summary>Occurs when the SpeedGraph is resized.</summary>
        protected override void OnResize(EventArgs e)
        {
            // Keep the Timeline combo box at the top right, beside the summary
            ComboTimeScale.Location = new Point(Width - ComboTimeScale.Width - PlotRight, 22);
            ComboTimeScale.Visible = Width > 420 && Height > 120;

            // continue processing the event as usual
            base.OnResize(e);
        }
        #endregion
        #region Drawing
        /// <summary>Draws the summary row: the last result, the average and fastest speeds, and the number of outages on the timeline.</summary>
        private void DrawSummary(Graphics g, List<ListViewItem> rows)
        {
            long last = -1, sum = 0, fastest = 0;
            int count = 0, outages = 0;
            DateTime lastTime = DateTime.MinValue;

            foreach (ListViewItem row in rows)
            {
                long speed = Global.ToInt64(row.SubItems[6].Text);

                if (speed > 0)
                {
                    last = speed;
                    lastTime = (DateTime)row.Tag;
                    sum += speed;
                    count++;
                    fastest = Math.Max(fastest, speed);
                }

                if (GetConnectionChange(row).Equals("Offline")) outages++;
            }

            int left = 20;
            left = DrawStat(g, left, last >= 0 ? "Last test, " + lastTime.ToString("HH:mm") : "Last test", last >= 0 ? (long?)last : null, Theme.BigValue, Theme.Text);
            left = DrawStat(g, left, "Average", count > 0 ? (long?)(sum / count) : null, Theme.Value, Theme.Text);
            left = DrawStat(g, left, "Fastest", count > 0 ? (long?)fastest : null, Theme.Value, Theme.Text);

            // Outages: a count, not a speed
            TextRenderer.DrawText(g, "Outages", Theme.Small, new Point(left, 14), Theme.TextSecondary, TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, outages.ToString(), Theme.Value, new Point(left, 37), outages > 0 ? Theme.Danger : Theme.Text, TextFormatFlags.NoPadding);
        }

        /// <summary>Draws one summary figure: a caption, the speed and its unit. Returns where the next figure starts.</summary>
        private static int DrawStat(Graphics g, int left, string caption, long? speed, Font font, Color color)
        {
            const TextFormatFlags flags = TextFormatFlags.NoPadding;
            string number = "–", unit = string.Empty;

            if (speed.HasValue)
            {
                // ConvertBytes gives e.g. "33.42 mb": show the number large and the unit small
                string text = Global.ConvertBytes(speed.Value);
                int space = text.LastIndexOf(' ');
                number = text.Substring(0, space);
                unit = text.Substring(space + 1) + "/s";
            }

            int top = font == Theme.BigValue ? 30 : 37;
            Size numberSize = TextRenderer.MeasureText(g, number, font, Size.Empty, flags);
            Size unitSize = TextRenderer.MeasureText(g, unit, Theme.Small, Size.Empty, flags);

            TextRenderer.DrawText(g, caption, Theme.Small, new Point(left, 14), Theme.TextSecondary, flags);
            TextRenderer.DrawText(g, number, font, new Point(left, top), color, flags);
            TextRenderer.DrawText(g, unit, Theme.Small, new Point(left + numberSize.Width + 3, top + numberSize.Height - unitSize.Height - 2), Theme.TextSecondary, flags);

            int captionWidth = TextRenderer.MeasureText(g, caption, Theme.Small, Size.Empty, flags).Width;
            return left + Math.Max(captionWidth, numberSize.Width + 3 + unitSize.Width) + 32;
        }

        /// <summary>Shades the times with no connection, and the times the logger wasn't running.</summary>
        private static void DrawBands(Graphics g, Rectangle plot, List<ListViewItem> rows, Func<long, float> x)
        {
            // Walk newest first. Going back in time, an "Online" (or a logger stop) ends an outage band
            // that an older "Offline" starts, and a "Logger Started" ends a logger-off band that an older "Logger Stopped" starts.
            float? onlineAt = null, startedAt = null;
            bool stateKnown = false, loggerEventSeen = false;

            using (SolidBrush offline = new SolidBrush(Theme.DangerLight))
            using (SolidBrush loggerOff = new SolidBrush(Theme.NeutralBand))
            {
                for (int i = rows.Count - 1; i >= 0; i--)
                {
                    float rowX = x(Global.ToUnixTime((DateTime)rows[i].Tag));

                    if (IsLoggerEvent(rows[i], "Logger Started"))
                    {
                        startedAt = rowX;
                        loggerEventSeen = true;

                        // Before a start, the connection's state is unknown until the previous session's own rows
                        onlineAt = null;
                        stateKnown = true;
                    }
                    else if (IsLoggerEvent(rows[i], "Logger Stopped"))
                    {
                        // Off until the next start, or until now if this is the newest logger event
                        float right = startedAt ?? (loggerEventSeen ? rowX : plot.Right);
                        FillBand(g, loggerOff, plot, rowX, right);
                        startedAt = null;
                        loggerEventSeen = true;

                        // An outage still open when the logger stopped ends here
                        onlineAt = rowX;
                        stateKnown = true;
                    }

                    string change = GetConnectionChange(rows[i]);

                    if (change.Equals("Online"))
                    {
                        onlineAt = rowX;
                        stateKnown = true;
                    }
                    else if (change.Equals("Offline"))
                    {
                        // Ends at the next "Online", or now if nothing newer is known
                        if (onlineAt.HasValue || !stateKnown) FillBand(g, offline, plot, rowX, onlineAt ?? plot.Right);
                        onlineAt = null;
                        stateKnown = true;
                    }
                }

                // A start with no stop before it on the timeline: the logger was off until then
                if (startedAt.HasValue) FillBand(g, loggerOff, plot, plot.Left, startedAt.Value);
            }
        }

        /// <summary>Fills a band across the full height of the plot, between two x positions.</summary>
        private static void FillBand(Graphics g, Brush brush, Rectangle plot, float left, float right)
        {
            left = Math.Max(plot.Left, left);
            right = Math.Min(plot.Right, right);

            if (right > left) g.FillRectangle(brush, left, plot.Top, right - left, plot.Height);
        }

        /// <summary>Draws the gridlines with speed labels on the left and time labels below.</summary>
        private void DrawGrid(Graphics g, Rectangle plot, double step, double axisMax, bool bytes, long timelineStart, long now, Func<long, float> x)
        {
            const TextFormatFlags right = TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;

            using (Pen grid = new Pen(Theme.Border))
            using (Pen baseline = new Pen(Color.FromArgb(200, 200, 200)))
            {
                // Speed: a gridline and label at every step
                for (double value = 0; value <= axisMax + step / 2; value += step)
                {
                    float lineY = plot.Bottom - (float)(value / axisMax * plot.Height);

                    g.DrawLine(value == 0 ? baseline : grid, plot.Left, lineY, plot.Right, lineY);
                    TextRenderer.DrawText(g, value.ToString("0.##"), Theme.Small, new Rectangle(0, (int)lineY - 8, plot.Left - 10, 16), Theme.TextMuted, right);
                }

                TextRenderer.DrawText(g, bytes ? "MB/s" : "mb/s", Theme.Small, new Rectangle(0, plot.Top - 26, plot.Left - 10, 16), Theme.TextMuted, right);

                // Time: labels on round times (every 2 minutes on the 10-minute timeline, and so on), plus "Now" at the right edge
                int[] stepSeconds = { 120, 300, 600, 7200, 14400 };
                int timeStep = stepSeconds[Math.Max(0, Math.Min(stepSeconds.Length - 1, Settings.Default.TimeScale))];
                long offset = (long)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalSeconds;
                long tick = (now + offset) / timeStep * timeStep - offset;
                int labelY = plot.Bottom + 6;

                TextRenderer.DrawText(g, "Now", Theme.Small, new Rectangle(plot.Right - 60, labelY, 60, 16), Theme.TextMuted, TextFormatFlags.Right | TextFormatFlags.NoPadding);

                for (; tick > timelineStart; tick -= timeStep)
                {
                    float tickX = x(tick);

                    // Leave room for "Now"
                    if (plot.Right - tickX < 44) continue;

                    g.DrawLine(grid, tickX, plot.Top, tickX, plot.Bottom);
                    TextRenderer.DrawText(g, Global.FromUnixTime(tick).ToString("HH:mm"), Theme.Small, new Rectangle((int)tickX - 30, labelY, 60, 16), Theme.TextMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
                }
            }
        }

        /// <summary>Draws the download speed as a line with a light fill under it, and a dot for each test when there are few.</summary>
        private static void DrawSpeed(Graphics g, Rectangle plot, List<ListViewItem> rows, Func<long, float> x, Func<long, float> y)
        {
            PointF[] points = rows.Select(row => new PointF(x(Global.ToUnixTime((DateTime)row.Tag)), y(Global.ToInt64(row.SubItems[6].Text)))).ToArray();

            if (points.Length > 1)
            {
                PointF[] area = points.Concat(new[] { new PointF(points[points.Length - 1].X, plot.Bottom), new PointF(points[0].X, plot.Bottom) }).ToArray();

                using (SolidBrush fill = new SolidBrush(Color.FromArgb(40, Theme.Accent)))
                using (Pen line = new Pen(Theme.Accent, 2f) { LineJoin = LineJoin.Round })
                {
                    g.FillPolygon(fill, area);
                    g.DrawLines(line, points);
                }
            }

            if (points.Length <= 60)
            {
                using (SolidBrush dot = new SolidBrush(Theme.Accent))
                {
                    foreach (PointF point in points) g.FillEllipse(dot, point.X - 2.5f, point.Y - 2.5f, 5, 5);
                }
            }
        }

        /// <summary>Explains an empty timeline, instead of showing a blank graph.</summary>
        private void DrawEmpty(Graphics g, Rectangle plot)
        {
            const TextFormatFlags centre = TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding;
            string timeline = ComboTimeScale.SelectedIndex >= 0 ? ComboTimeScale.Text.ToLower() : "timeline";
            int middle = plot.Top + plot.Height / 2;

            TextRenderer.DrawText(g, "No tests in the last " + timeline, Theme.Heading, new Rectangle(plot.Left, middle - 20, plot.Width, 20), Theme.Text, centre);
            TextRenderer.DrawText(g, "Start logging, or choose a longer timeline at the top right.", Theme.Small, new Rectangle(plot.Left, middle + 2, plot.Width, 18), Theme.TextSecondary, centre);
        }

        /// <summary>Draws the key under the graph.</summary>
        private void DrawKey(Graphics g)
        {
            int left = 20, top = Height - 24;

            left = DrawKeyItem(g, left, top, Theme.Accent, "Download speed");
            left = DrawKeyItem(g, left, top, Theme.DangerLight, "No connection");
            DrawKeyItem(g, left, top, Theme.NeutralBand, "Logger not running");
        }

        /// <summary>Draws one key entry: a small rounded swatch and its label. Returns where the next entry starts.</summary>
        private static int DrawKeyItem(Graphics g, int left, int top, Color color, string label)
        {
            using (GraphicsPath swatch = Theme.RoundedRectangle(new RectangleF(left, top + 3, 10, 10), 2))
            using (SolidBrush brush = new SolidBrush(color))
            using (Pen border = new Pen(Color.FromArgb(40, 0, 0, 0)))
            {
                g.FillPath(brush, swatch);
                g.DrawPath(border, swatch);
            }

            TextRenderer.DrawText(g, label, Theme.Small, new Point(left + 16, top), Theme.TextSecondary, TextFormatFlags.NoPadding);
            return left + 16 + TextRenderer.MeasureText(g, label, Theme.Small, Size.Empty, TextFormatFlags.NoPadding).Width + 20;
        }
        #endregion
        #region Private Methods
        /// <summary>Gets a round step for an axis: 1, 2, 2.5 or 5 times a power of ten, at least the specified value.</summary>
        private static double NiceStep(double rough)
        {
            double power = Math.Pow(10, Math.Floor(Math.Log10(rough)));
            double fraction = rough / power;

            return (fraction <= 1 ? 1 : fraction <= 2 ? 2 : fraction <= 2.5 ? 2.5 : fraction <= 5 ? 5 : 10) * power;
        }

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
                // Newest first, stopping at the first row before the timeline
                for (int i = Global.ConnectionLog.Count - 1; i >= 0 && Global.ToUnixTime((DateTime)Global.ConnectionLog[i].Tag) > timelineStart; i--)
                {
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
