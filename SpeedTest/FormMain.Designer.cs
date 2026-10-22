namespace SpeedTest
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.ContainerMain = new System.Windows.Forms.SplitContainer();
            this.GraphMain = new SpeedTest.SpeedGraph();
            this.ListLog = new System.Windows.Forms.ListView();
            this.ColumnTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnSpeed = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnIP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnMessage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ImageListLog = new System.Windows.Forms.ImageList(this.components);
            this.StatusStripMain = new System.Windows.Forms.StatusStrip();
            this.StatusLogging = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusMain = new System.Windows.Forms.ToolStripStatusLabel();
            this.IconMain = new System.Windows.Forms.NotifyIcon(this.components);
            this.MenuMain = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuStartLogging = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuStopLogging = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuTest = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuShowSuccessful = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuShowStatusChanges = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuShowErrors = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMain = new System.Windows.Forms.ToolStrip();
            this.ToolBarStartLogging = new System.Windows.Forms.ToolStripButton();
            this.ToolBarStopLogging = new System.Windows.Forms.ToolStripButton();
            this.ToolBarTest = new System.Windows.Forms.ToolStripButton();
            this.ToolBarSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolBarShowSuccessful = new System.Windows.Forms.ToolStripButton();
            this.ToolBarShowStatusChanges = new System.Windows.Forms.ToolStripButton();
            this.ToolBarShowErrors = new System.Windows.Forms.ToolStripButton();
            this.ToolBarAbout = new System.Windows.Forms.ToolStripButton();
            this.ToolBarHelp = new System.Windows.Forms.ToolStripButton();
            this.ToolBarSettings = new System.Windows.Forms.ToolStripButton();
            this.ImageListMain = new System.Windows.Forms.ImageList(this.components);
            this.TimerGraph = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.ContainerMain)).BeginInit();
            this.ContainerMain.Panel1.SuspendLayout();
            this.ContainerMain.Panel2.SuspendLayout();
            this.ContainerMain.SuspendLayout();
            this.StatusStripMain.SuspendLayout();
            this.MenuMain.SuspendLayout();
            this.ToolStripMain.SuspendLayout();
            this.SuspendLayout();
            //
            // ContainerMain
            //
            this.ContainerMain.BackColor = System.Drawing.Color.White;
            this.ContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ContainerMain.Location = new System.Drawing.Point(0, 40);
            this.ContainerMain.Name = "ContainerMain";
            this.ContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // ContainerMain.Panel1
            //
            this.ContainerMain.Panel1.Controls.Add(this.GraphMain);
            this.ContainerMain.Panel1MinSize = 0;
            //
            // ContainerMain.Panel2
            //
            this.ContainerMain.Panel2.Controls.Add(this.ListLog);
            this.ContainerMain.Panel2MinSize = 0;
            this.ContainerMain.Size = new System.Drawing.Size(922, 537);
            this.ContainerMain.SplitterDistance = 300;
            this.ContainerMain.SplitterWidth = 5;
            this.ContainerMain.TabIndex = 0;
            //
            // GraphMain
            //
            this.GraphMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GraphMain.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.GraphMain.Location = new System.Drawing.Point(0, 0);
            this.GraphMain.Name = "GraphMain";
            this.GraphMain.Size = new System.Drawing.Size(922, 300);
            this.GraphMain.TabIndex = 2;
            //
            // ListLog
            //
            this.ListLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ListLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnTime,
            this.ColumnSpeed,
            this.ColumnIP,
            this.ColumnMessage,
            this.ColumnStatus,
            this.ColumnFileName});
            this.ListLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListLog.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ListLog.FullRowSelect = true;
            this.ListLog.HideSelection = false;
            this.ListLog.Location = new System.Drawing.Point(0, 0);
            this.ListLog.Name = "ListLog";
            this.ListLog.Size = new System.Drawing.Size(922, 232);
            this.ListLog.SmallImageList = this.ImageListLog;
            this.ListLog.TabIndex = 2;
            this.ListLog.UseCompatibleStateImageBehavior = false;
            this.ListLog.View = System.Windows.Forms.View.Details;
            //
            // ColumnTime
            //
            this.ColumnTime.Text = "Time";
            this.ColumnTime.Width = 170;
            //
            // ColumnSpeed
            //
            this.ColumnSpeed.Text = "Speed";
            this.ColumnSpeed.Width = 100;
            //
            // ColumnIP
            //
            this.ColumnIP.Text = "IP";
            this.ColumnIP.Width = 120;
            //
            // ColumnMessage
            //
            this.ColumnMessage.Text = "Message";
            this.ColumnMessage.Width = 120;
            //
            // ColumnStatus
            //
            this.ColumnStatus.Text = "Status";
            this.ColumnStatus.Width = 110;
            //
            // ColumnFileName
            //
            this.ColumnFileName.Text = "Test file";
            this.ColumnFileName.Width = 260;
            //
            // ImageListLog
            //
            this.ImageListLog.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageListLog.ImageStream")));
            this.ImageListLog.TransparentColor = System.Drawing.Color.Transparent;
            this.ImageListLog.Images.SetKeyName(0, "navigate_check.png");
            this.ImageListLog.Images.SetKeyName(1, "earth_network.png");
            this.ImageListLog.Images.SetKeyName(2, "delete2.png");
            //
            // StatusStripMain
            //
            this.StatusStripMain.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.StatusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLogging,
            this.StatusMain});
            this.StatusStripMain.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.StatusStripMain.Location = new System.Drawing.Point(0, 577);
            this.StatusStripMain.Name = "StatusStripMain";
            this.StatusStripMain.Padding = new System.Windows.Forms.Padding(8, 2, 22, 2);
            this.StatusStripMain.Size = new System.Drawing.Size(922, 26);
            this.StatusStripMain.TabIndex = 1;
            //
            // StatusLogging
            //
            this.StatusLogging.Name = "StatusLogging";
            this.StatusLogging.Size = new System.Drawing.Size(86, 17);
            this.StatusLogging.Text = "Not logging";
            //
            // StatusMain
            //
            this.StatusMain.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.StatusMain.Name = "StatusMain";
            this.StatusMain.Size = new System.Drawing.Size(40, 17);
            this.StatusMain.Text = "Ready";
            //
            // IconMain
            //
            this.IconMain.Icon = ((System.Drawing.Icon)(resources.GetObject("IconMain.Icon")));
            this.IconMain.Text = "Speed Test Loggger";
            this.IconMain.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.IconMain_MouseDoubleClick);
            //
            // MenuMain
            //
            this.MenuMain.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MenuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuStartLogging,
            this.MenuStopLogging,
            this.MenuTest,
            this.MenuSeparator1,
            this.MenuShowSuccessful,
            this.MenuShowStatusChanges,
            this.MenuShowErrors,
            this.MenuSeparator2,
            this.MenuSettings,
            this.MenuHelp,
            this.MenuAbout,
            this.MenuSeparator3,
            this.MenuExit});
            this.MenuMain.Name = "MenuMain";
            this.MenuMain.Size = new System.Drawing.Size(220, 242);
            //
            // MenuStartLogging
            //
            this.MenuStartLogging.Name = "MenuStartLogging";
            this.MenuStartLogging.Text = "Start logging";
            this.MenuStartLogging.Click += new System.EventHandler(this.MenuStartLogging_Click);
            //
            // MenuStopLogging
            //
            this.MenuStopLogging.Enabled = false;
            this.MenuStopLogging.Name = "MenuStopLogging";
            this.MenuStopLogging.Text = "Stop logging";
            this.MenuStopLogging.Click += new System.EventHandler(this.MenuStopLogging_Click);
            //
            // MenuTest
            //
            this.MenuTest.Enabled = false;
            this.MenuTest.Name = "MenuTest";
            this.MenuTest.Text = "Test now";
            this.MenuTest.Click += new System.EventHandler(this.MenuTest_Click);
            //
            // MenuSeparator1
            //
            this.MenuSeparator1.Name = "MenuSeparator1";
            //
            // MenuShowSuccessful
            //
            this.MenuShowSuccessful.Checked = true;
            this.MenuShowSuccessful.Name = "MenuShowSuccessful";
            this.MenuShowSuccessful.Text = "Show successful tests";
            this.MenuShowSuccessful.Click += new System.EventHandler(this.MenuShowSuccessful_Click);
            //
            // MenuShowStatusChanges
            //
            this.MenuShowStatusChanges.Checked = true;
            this.MenuShowStatusChanges.Name = "MenuShowStatusChanges";
            this.MenuShowStatusChanges.Text = "Show status changes";
            this.MenuShowStatusChanges.Click += new System.EventHandler(this.MenuShowStatusChanges_Click);
            //
            // MenuShowErrors
            //
            this.MenuShowErrors.Checked = true;
            this.MenuShowErrors.Name = "MenuShowErrors";
            this.MenuShowErrors.Text = "Show errors";
            this.MenuShowErrors.Click += new System.EventHandler(this.MenuShowErrors_Click);
            //
            // MenuSeparator2
            //
            this.MenuSeparator2.Name = "MenuSeparator2";
            //
            // MenuSettings
            //
            this.MenuSettings.Name = "MenuSettings";
            this.MenuSettings.Text = "Settings...";
            this.MenuSettings.Click += new System.EventHandler(this.MenuSettings_Click);
            //
            // MenuHelp
            //
            this.MenuHelp.Name = "MenuHelp";
            this.MenuHelp.Text = "Help online...";
            this.MenuHelp.Click += new System.EventHandler(this.MenuHelp_Click);
            //
            // MenuAbout
            //
            this.MenuAbout.Name = "MenuAbout";
            this.MenuAbout.Text = "About Speed Test Loggger...";
            this.MenuAbout.Click += new System.EventHandler(this.MenuAbout_Click);
            //
            // MenuSeparator3
            //
            this.MenuSeparator3.Name = "MenuSeparator3";
            //
            // MenuExit
            //
            this.MenuExit.Name = "MenuExit";
            this.MenuExit.Text = "Exit";
            this.MenuExit.Click += new System.EventHandler(this.MenuExit_Click);
            //
            // ToolStripMain
            //
            this.ToolStripMain.AutoSize = false;
            this.ToolStripMain.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ToolStripMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ToolStripMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ToolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolBarStartLogging,
            this.ToolBarStopLogging,
            this.ToolBarTest,
            this.ToolBarSeparator1,
            this.ToolBarShowSuccessful,
            this.ToolBarShowStatusChanges,
            this.ToolBarShowErrors,
            this.ToolBarAbout,
            this.ToolBarHelp,
            this.ToolBarSettings});
            this.ToolStripMain.Location = new System.Drawing.Point(0, 0);
            this.ToolStripMain.Name = "ToolStripMain";
            this.ToolStripMain.Padding = new System.Windows.Forms.Padding(8, 2, 8, 2);
            this.ToolStripMain.Size = new System.Drawing.Size(922, 40);
            this.ToolStripMain.TabIndex = 2;
            //
            // ToolBarStartLogging
            //
            this.ToolBarStartLogging.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ToolBarStartLogging.Name = "ToolBarStartLogging";
            this.ToolBarStartLogging.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ToolBarStartLogging.Text = "Start";
            this.ToolBarStartLogging.ToolTipText = "Start logging";
            this.ToolBarStartLogging.Click += new System.EventHandler(this.MenuStartLogging_Click);
            //
            // ToolBarStopLogging
            //
            this.ToolBarStopLogging.Enabled = false;
            this.ToolBarStopLogging.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ToolBarStopLogging.Name = "ToolBarStopLogging";
            this.ToolBarStopLogging.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ToolBarStopLogging.Text = "Stop";
            this.ToolBarStopLogging.ToolTipText = "Stop logging";
            this.ToolBarStopLogging.Click += new System.EventHandler(this.MenuStopLogging_Click);
            //
            // ToolBarTest
            //
            this.ToolBarTest.Enabled = false;
            this.ToolBarTest.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ToolBarTest.Name = "ToolBarTest";
            this.ToolBarTest.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ToolBarTest.Text = "Test now";
            this.ToolBarTest.ToolTipText = "Test the connection now";
            this.ToolBarTest.Click += new System.EventHandler(this.MenuTest_Click);
            //
            // ToolBarSeparator1
            //
            this.ToolBarSeparator1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ToolBarSeparator1.Name = "ToolBarSeparator1";
            //
            // ToolBarShowSuccessful
            //
            this.ToolBarShowSuccessful.Checked = true;
            this.ToolBarShowSuccessful.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ToolBarShowSuccessful.Name = "ToolBarShowSuccessful";
            this.ToolBarShowSuccessful.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ToolBarShowSuccessful.Text = "Successful";
            this.ToolBarShowSuccessful.ToolTipText = "Show successful tests in the list";
            this.ToolBarShowSuccessful.Click += new System.EventHandler(this.MenuShowSuccessful_Click);
            //
            // ToolBarShowStatusChanges
            //
            this.ToolBarShowStatusChanges.Checked = true;
            this.ToolBarShowStatusChanges.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ToolBarShowStatusChanges.Name = "ToolBarShowStatusChanges";
            this.ToolBarShowStatusChanges.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ToolBarShowStatusChanges.Text = "Status changes";
            this.ToolBarShowStatusChanges.ToolTipText = "Show status changes in the list";
            this.ToolBarShowStatusChanges.Click += new System.EventHandler(this.MenuShowStatusChanges_Click);
            //
            // ToolBarShowErrors
            //
            this.ToolBarShowErrors.Checked = true;
            this.ToolBarShowErrors.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ToolBarShowErrors.Name = "ToolBarShowErrors";
            this.ToolBarShowErrors.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ToolBarShowErrors.Text = "Errors";
            this.ToolBarShowErrors.ToolTipText = "Show errors in the list";
            this.ToolBarShowErrors.Click += new System.EventHandler(this.MenuShowErrors_Click);
            //
            // ToolBarAbout
            //
            this.ToolBarAbout.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.ToolBarAbout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolBarAbout.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ToolBarAbout.Name = "ToolBarAbout";
            this.ToolBarAbout.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ToolBarAbout.Text = "About";
            this.ToolBarAbout.ToolTipText = "About Speed Test Loggger";
            this.ToolBarAbout.Click += new System.EventHandler(this.MenuAbout_Click);
            //
            // ToolBarHelp
            //
            this.ToolBarHelp.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.ToolBarHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.ToolBarHelp.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ToolBarHelp.Name = "ToolBarHelp";
            this.ToolBarHelp.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ToolBarHelp.Text = "Help";
            this.ToolBarHelp.ToolTipText = "Help online";
            this.ToolBarHelp.Click += new System.EventHandler(this.MenuHelp_Click);
            //
            // ToolBarSettings
            //
            this.ToolBarSettings.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.ToolBarSettings.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ToolBarSettings.Name = "ToolBarSettings";
            this.ToolBarSettings.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.ToolBarSettings.Text = "Settings";
            this.ToolBarSettings.ToolTipText = "Configuration settings";
            this.ToolBarSettings.Click += new System.EventHandler(this.MenuSettings_Click);
            //
            // ImageListMain
            //
            this.ImageListMain.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageListMain.ImageStream")));
            this.ImageListMain.TransparentColor = System.Drawing.Color.Transparent;
            this.ImageListMain.Images.SetKeyName(0, "AppImage.png");
            this.ImageListMain.Images.SetKeyName(1, "stop_red.png");
            this.ImageListMain.Images.SetKeyName(2, "arrow_down_green.png");
            this.ImageListMain.Images.SetKeyName(3, "navigate_check.png");
            this.ImageListMain.Images.SetKeyName(4, "earth_network.png");
            this.ImageListMain.Images.SetKeyName(5, "delete2.png");
            this.ImageListMain.Images.SetKeyName(6, "wrench.png");
            this.ImageListMain.Images.SetKeyName(7, "help_earth.png");
            this.ImageListMain.Images.SetKeyName(8, "about.png");
            //
            // TimerGraph
            //
            this.TimerGraph.Enabled = true;
            this.TimerGraph.Interval = 1000;
            this.TimerGraph.Tick += new System.EventHandler(this.TimerGraph_Tick);
            //
            // FormMain
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(922, 603);
            this.Controls.Add(this.ContainerMain);
            this.Controls.Add(this.StatusStripMain);
            this.Controls.Add(this.ToolStripMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(940, 650);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Speed Test Loggger for Windows";
            this.ContainerMain.Panel1.ResumeLayout(false);
            this.ContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ContainerMain)).EndInit();
            this.ContainerMain.ResumeLayout(false);
            this.StatusStripMain.ResumeLayout(false);
            this.StatusStripMain.PerformLayout();
            this.MenuMain.ResumeLayout(false);
            this.ToolStripMain.ResumeLayout(false);
            this.ToolStripMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer ContainerMain;
        private System.Windows.Forms.ColumnHeader ColumnTime;
        private System.Windows.Forms.ColumnHeader ColumnSpeed;
        private System.Windows.Forms.ColumnHeader ColumnIP;
        private System.Windows.Forms.ColumnHeader ColumnMessage;
        private System.Windows.Forms.ColumnHeader ColumnStatus;
        private System.Windows.Forms.ImageList ImageListLog;
        private System.Windows.Forms.NotifyIcon IconMain;
        private System.Windows.Forms.ImageList ImageListMain;
        private System.Windows.Forms.ContextMenuStrip MenuMain;
        private System.Windows.Forms.ToolStripMenuItem MenuStartLogging;
        private System.Windows.Forms.ToolStripMenuItem MenuStopLogging;
        private System.Windows.Forms.ToolStripMenuItem MenuTest;
        private System.Windows.Forms.ToolStripSeparator MenuSeparator1;
        private System.Windows.Forms.ToolStripMenuItem MenuShowSuccessful;
        private System.Windows.Forms.ToolStripMenuItem MenuShowStatusChanges;
        private System.Windows.Forms.ToolStripMenuItem MenuShowErrors;
        private System.Windows.Forms.ToolStripSeparator MenuSeparator2;
        private System.Windows.Forms.ToolStripMenuItem MenuSettings;
        private System.Windows.Forms.ToolStripMenuItem MenuHelp;
        private System.Windows.Forms.ToolStripMenuItem MenuAbout;
        private System.Windows.Forms.ToolStripSeparator MenuSeparator3;
        private System.Windows.Forms.ToolStripMenuItem MenuExit;
        private System.Windows.Forms.ToolStrip ToolStripMain;
        private System.Windows.Forms.ToolStripButton ToolBarStartLogging;
        private System.Windows.Forms.ToolStripButton ToolBarStopLogging;
        private System.Windows.Forms.ToolStripButton ToolBarTest;
        private System.Windows.Forms.ToolStripSeparator ToolBarSeparator1;
        private System.Windows.Forms.ToolStripButton ToolBarShowSuccessful;
        private System.Windows.Forms.ToolStripButton ToolBarShowStatusChanges;
        private System.Windows.Forms.ToolStripButton ToolBarShowErrors;
        private System.Windows.Forms.ToolStripButton ToolBarSettings;
        private System.Windows.Forms.ToolStripButton ToolBarHelp;
        private System.Windows.Forms.ToolStripButton ToolBarAbout;
        private System.Windows.Forms.StatusStrip StatusStripMain;
        private System.Windows.Forms.ToolStripStatusLabel StatusLogging;
        private System.Windows.Forms.ToolStripStatusLabel StatusMain;
        private SpeedGraph GraphMain;
        private System.Windows.Forms.ListView ListLog;
        private System.Windows.Forms.Timer TimerGraph;
        private System.Windows.Forms.ColumnHeader ColumnFileName;
    }
}
