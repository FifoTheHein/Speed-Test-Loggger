#region Using
// Imported namespaces
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
#endregion
namespace SpeedTest
{
    /// <summary>The app's colours, fonts and line icons, in the style of Windows 11.</summary>
    public static class Theme
    {
        #region Colours
        public static readonly Color Background = Color.White;
        public static readonly Color Surface = Color.FromArgb(249, 249, 249);
        public static readonly Color Border = Color.FromArgb(229, 229, 229);
        public static readonly Color Text = Color.FromArgb(27, 27, 27);
        public static readonly Color TextSecondary = Color.FromArgb(96, 96, 96);
        public static readonly Color TextMuted = Color.FromArgb(138, 138, 138);
        public static readonly Color Accent = Color.FromArgb(0, 103, 192);
        public static readonly Color AccentDark = Color.FromArgb(0, 62, 146);
        public static readonly Color AccentLight = Color.FromArgb(224, 238, 249);
        public static readonly Color AccentLightHover = Color.FromArgb(207, 228, 245);
        public static readonly Color Hover = Color.FromArgb(240, 240, 240);
        public static readonly Color Pressed = Color.FromArgb(229, 229, 229);
        public static readonly Color Success = Color.FromArgb(15, 123, 15);
        public static readonly Color Danger = Color.FromArgb(196, 43, 28);
        public static readonly Color DangerLight = Color.FromArgb(253, 231, 233);
        public static readonly Color NeutralBand = Color.FromArgb(238, 238, 238);
        #endregion
        #region Fonts
        public static readonly Font UI = new Font("Segoe UI", 9f);
        public static readonly Font Small = new Font("Segoe UI", 8.25f);
        public static readonly Font Heading = new Font("Segoe UI Semibold", 9f);
        public static readonly Font Value = new Font("Segoe UI Semibold", 12f);
        public static readonly Font BigValue = new Font("Segoe UI Semibold", 18f);
        #endregion
        #region Icon glyphs
        // Code points in Segoe Fluent Icons (Windows 11) and Segoe MDL2 Assets (Windows 10), which share them
        public const char GlyphPlay = '';
        public const char GlyphStop = '';
        public const char GlyphRefresh = '';
        public const char GlyphCompleted = '';
        public const char GlyphGlobe = '';
        public const char GlyphError = '';
        public const char GlyphSettings = '';
        public const char GlyphHelp = '';
        public const char GlyphInfo = '';
        public const char GlyphFolder = '';
        public const char GlyphDocument = '';
        public const char GlyphCheck = '';

        // The icon font, or null on Windows versions without one (then callers keep their bitmap icons)
        private static readonly string p_IconFont = new[] { "Segoe Fluent Icons", "Segoe MDL2 Assets" }
            .FirstOrDefault(name => new InstalledFontCollection().Families.Any(f => f.Name == name));
        #endregion
        #region Public Methods
        /// <summary>Gets a value indicating whether line icons can be drawn on this PC.</summary>
        public static bool HasIcons => p_IconFont != null;

        /// <summary>Converts a size designed at 100% display scaling (96 DPI) into pixels at the specified control's current DPI.</summary>
        public static int Scale(Control control, int pixels)
        {
            return (int)Math.Round(pixels * control.DeviceDpi / 96f);
        }

        /// <summary>Converts a size designed at 100% display scaling (96 DPI) into pixels at the specified control's current DPI.</summary>
        public static float Scale(Control control, float pixels)
        {
            return pixels * control.DeviceDpi / 96f;
        }

        /// <summary>Draws the specified icon glyph as a square bitmap of the specified size and colour, or returns null if there's no icon font.</summary>
        public static Bitmap Icon(char glyph, int size, Color color)
        {
            if (p_IconFont == null) return null;

            Bitmap bitmap = new Bitmap(size, size);

            using (Graphics g = Graphics.FromImage(bitmap))
            using (Font font = new Font(p_IconFont, size * 0.8f, GraphicsUnit.Pixel))
            using (SolidBrush brush = new SolidBrush(color))
            using (StringFormat format = new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.DrawString(glyph.ToString(), font, brush, new RectangleF(0, 0, size, size), format);
            }

            return bitmap;
        }

        /// <summary>Draws a filled circle of the specified size and colour, for status dots.</summary>
        public static Bitmap Dot(int size, Color color)
        {
            Bitmap bitmap = new Bitmap(size, size);

            using (Graphics g = Graphics.FromImage(bitmap))
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillEllipse(brush, 1, 1, size - 3, size - 3);
            }

            return bitmap;
        }

        /// <summary>Gets a rectangle with rounded corners of the specified radius.</summary>
        public static GraphicsPath RoundedRectangle(RectangleF bounds, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float d = radius * 2;

            path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }

        /// <summary>Styles the specified button flat: an accent fill for the main action, or a light outline.</summary>
        public static void StyleButton(Button button, bool primary)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.Font = UI;
            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;
            button.BackColor = primary ? Accent : Background;
            button.ForeColor = primary ? Color.White : Text;
            button.FlatAppearance.BorderColor = primary ? Accent : Color.FromArgb(209, 209, 209);
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = primary ? AccentDark : Hover;
            button.FlatAppearance.MouseDownBackColor = primary ? AccentDark : Pressed;
        }
        #endregion
    }

    /// <summary>Renders toolbars, status bars and menus flat, with rounded hover and checked highlights.</summary>
    public class FlatRenderer : ToolStripProfessionalRenderer
    {
        /// <summary>Initializes a new instance of the FlatRenderer class.</summary>
        public FlatRenderer()
        {
            RoundedEdges = false;
        }

        /// <summary>Fills the toolbar, status bar or menu background.</summary>
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(e.ToolStrip is ToolStripDropDown ? Theme.Background : Theme.Surface))
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
        }

        /// <summary>Draws a hairline: around menus, above the status bar and below the toolbar.</summary>
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using (Pen pen = new Pen(Theme.Border))
            {
                if (e.ToolStrip is ToolStripDropDown) e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
                else if (e.ToolStrip is StatusStrip) e.Graphics.DrawLine(pen, 0, 0, e.ToolStrip.Width, 0);
                else e.Graphics.DrawLine(pen, 0, e.ToolStrip.Height - 1, e.ToolStrip.Width, e.ToolStrip.Height - 1);
            }
        }

        /// <summary>Highlights a toolbar button that's hovered, pressed or switched on.</summary>
        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            ToolStripButton button = e.Item as ToolStripButton;
            bool on = button != null && button.Checked;
            Color? fill = null;

            if (e.Item.Pressed) fill = on ? Theme.AccentLightHover : Theme.Pressed;
            else if (on) fill = e.Item.Selected ? Theme.AccentLightHover : Theme.AccentLight;
            else if (e.Item.Selected && e.Item.Enabled) fill = Theme.Hover;

            if (fill.HasValue) FillRounded(e.Graphics, new RectangleF(1, 2, e.Item.Width - 2, e.Item.Height - 4), fill.Value, Theme.Scale(e.ToolStrip, 4f));
        }

        /// <summary>Highlights a hovered menu item.</summary>
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.Item.Selected && e.Item.Enabled) FillRounded(e.Graphics, new RectangleF(4, 1, e.Item.Width - 8, e.Item.Height - 2), Theme.Hover, Theme.Scale(e.ToolStrip, 4f));
        }

        /// <summary>Leaves the menu's image column white instead of grey.</summary>
        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
        }

        /// <summary>Draws a separator as a short hairline.</summary>
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            int inset = Theme.Scale(e.ToolStrip, 6);

            using (Pen pen = new Pen(Theme.Border))
            {
                if (e.Vertical) e.Graphics.DrawLine(pen, e.Item.Width / 2, inset, e.Item.Width / 2, e.Item.Height - inset);
                else e.Graphics.DrawLine(pen, Theme.Scale(e.ToolStrip, 28), e.Item.Height / 2, e.Item.Width - Theme.Scale(e.ToolStrip, 8), e.Item.Height / 2);
            }
        }

        /// <summary>Draws a menu item's check mark as an accent line icon.</summary>
        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            int size = Theme.Scale(e.ToolStrip, 16);
            Bitmap check = Theme.Icon(Theme.GlyphCheck, size, Theme.Accent);

            if (check == null) base.OnRenderItemCheck(e);
            else using (check) e.Graphics.DrawImage(check, e.ImageRectangle.X + (e.ImageRectangle.Width - size) / 2, e.ImageRectangle.Y + (e.ImageRectangle.Height - size) / 2);
        }

        /// <summary>Colours item text: accent when switched on, grey when disabled or in the status bar.</summary>
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            ToolStripButton button = e.Item as ToolStripButton;

            if (!e.Item.Enabled) e.TextColor = Theme.TextMuted;
            else if (button != null && button.Checked) e.TextColor = Theme.AccentDark;
            else if (e.ToolStrip is StatusStrip) e.TextColor = Theme.TextSecondary;
            else e.TextColor = Theme.Text;

            base.OnRenderItemText(e);
        }

        /// <summary>Fills a rounded rectangle with the specified colour.</summary>
        private static void FillRounded(Graphics g, RectangleF bounds, Color color, float radius)
        {
            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = Theme.RoundedRectangle(bounds, radius))
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillPath(brush, path);
            }

            g.SmoothingMode = previous;
        }
    }

    /// <summary>A settings section: a heading with a hairline under it, instead of a boxed group.</summary>
    public class SectionBox : GroupBox
    {
        /// <summary>Initializes a new instance of the SectionBox class.</summary>
        public SectionBox()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        }

        /// <summary>Draws the heading and the hairline. Child controls draw themselves.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            TextRenderer.DrawText(e.Graphics, Text, Theme.Heading, new Point(0, 0), Theme.Text);

            using (Pen pen = new Pen(Theme.Border))
            {
                int lineY = Theme.Scale(this, 22);
                e.Graphics.DrawLine(pen, 0, lineY, Width, lineY);
            }
        }
    }
}
