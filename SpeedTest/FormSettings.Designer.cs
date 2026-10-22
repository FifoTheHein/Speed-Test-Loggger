namespace SpeedTest
{
    partial class FormSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSettings));
            this.GroupDownloadURL = new SpeedTest.SectionBox();
            this.RadioAutoDownload = new System.Windows.Forms.RadioButton();
            this.RadioCustomDownload = new System.Windows.Forms.RadioButton();
            this.TextDownloadURL = new System.Windows.Forms.TextBox();
            this.GroupAPIURL = new SpeedTest.SectionBox();
            this.RadioAutoDetectIP = new System.Windows.Forms.RadioButton();
            this.RadioCustomIPDetect = new System.Windows.Forms.RadioButton();
            this.TextIPDetectURL = new System.Windows.Forms.TextBox();
            this.GroupTimeFormat = new SpeedTest.SectionBox();
            this.RadioISOFormat = new System.Windows.Forms.RadioButton();
            this.RadioEUFormat = new System.Windows.Forms.RadioButton();
            this.RadioUSFormat = new System.Windows.Forms.RadioButton();
            this.GroupSpeedUnits = new System.Windows.Forms.Panel();
            this.RadioBitsPerSecond = new System.Windows.Forms.RadioButton();
            this.RadioBytesPerSecond = new System.Windows.Forms.RadioButton();
            this.GroupLogging = new SpeedTest.SectionBox();
            this.LabelCheckConnection = new System.Windows.Forms.Label();
            this.ComboCheckConnection = new System.Windows.Forms.ComboBox();
            this.LabelLogFile = new System.Windows.Forms.Label();
            this.TextLogFile = new System.Windows.Forms.TextBox();
            this.ButtonLogFile = new System.Windows.Forms.Button();
            this.CheckAutoStart = new System.Windows.Forms.CheckBox();
            this.CheckMinimize = new System.Windows.Forms.CheckBox();
            this.ButtonOpenLog = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonOK = new System.Windows.Forms.Button();
            this.GroupDownloadURL.SuspendLayout();
            this.GroupAPIURL.SuspendLayout();
            this.GroupTimeFormat.SuspendLayout();
            this.GroupSpeedUnits.SuspendLayout();
            this.GroupLogging.SuspendLayout();
            this.SuspendLayout();
            //
            // GroupDownloadURL
            //
            this.GroupDownloadURL.Controls.Add(this.RadioAutoDownload);
            this.GroupDownloadURL.Controls.Add(this.RadioCustomDownload);
            this.GroupDownloadURL.Controls.Add(this.TextDownloadURL);
            this.GroupDownloadURL.Location = new System.Drawing.Point(16, 14);
            this.GroupDownloadURL.Name = "GroupDownloadURL";
            this.GroupDownloadURL.Size = new System.Drawing.Size(348, 84);
            this.GroupDownloadURL.TabIndex = 0;
            this.GroupDownloadURL.TabStop = false;
            this.GroupDownloadURL.Text = "Test file";
            //
            // RadioAutoDownload
            //
            this.RadioAutoDownload.AutoSize = true;
            this.RadioAutoDownload.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RadioAutoDownload.Location = new System.Drawing.Point(0, 30);
            this.RadioAutoDownload.Name = "RadioAutoDownload";
            this.RadioAutoDownload.TabIndex = 1;
            this.RadioAutoDownload.TabStop = true;
            this.RadioAutoDownload.Text = "Automatic";
            this.RadioAutoDownload.UseVisualStyleBackColor = true;
            this.RadioAutoDownload.CheckedChanged += new System.EventHandler(this.RadioAutoDownload_CheckedChanged);
            //
            // RadioCustomDownload
            //
            this.RadioCustomDownload.AutoSize = true;
            this.RadioCustomDownload.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RadioCustomDownload.Location = new System.Drawing.Point(0, 57);
            this.RadioCustomDownload.Name = "RadioCustomDownload";
            this.RadioCustomDownload.TabIndex = 2;
            this.RadioCustomDownload.TabStop = true;
            this.RadioCustomDownload.Text = "Custom";
            this.RadioCustomDownload.UseVisualStyleBackColor = true;
            //
            // TextDownloadURL
            //
            this.TextDownloadURL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextDownloadURL.Enabled = false;
            this.TextDownloadURL.Location = new System.Drawing.Point(84, 55);
            this.TextDownloadURL.Name = "TextDownloadURL";
            this.TextDownloadURL.Size = new System.Drawing.Size(264, 23);
            this.TextDownloadURL.TabIndex = 3;
            //
            // GroupAPIURL
            //
            this.GroupAPIURL.Controls.Add(this.RadioAutoDetectIP);
            this.GroupAPIURL.Controls.Add(this.RadioCustomIPDetect);
            this.GroupAPIURL.Controls.Add(this.TextIPDetectURL);
            this.GroupAPIURL.Location = new System.Drawing.Point(16, 106);
            this.GroupAPIURL.Name = "GroupAPIURL";
            this.GroupAPIURL.Size = new System.Drawing.Size(348, 84);
            this.GroupAPIURL.TabIndex = 4;
            this.GroupAPIURL.TabStop = false;
            this.GroupAPIURL.Text = "IP lookup";
            //
            // RadioAutoDetectIP
            //
            this.RadioAutoDetectIP.AutoSize = true;
            this.RadioAutoDetectIP.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RadioAutoDetectIP.Location = new System.Drawing.Point(0, 30);
            this.RadioAutoDetectIP.Name = "RadioAutoDetectIP";
            this.RadioAutoDetectIP.TabIndex = 5;
            this.RadioAutoDetectIP.TabStop = true;
            this.RadioAutoDetectIP.Text = "Automatic";
            this.RadioAutoDetectIP.UseVisualStyleBackColor = true;
            this.RadioAutoDetectIP.CheckedChanged += new System.EventHandler(this.RadioAutoDetectIP_CheckedChanged);
            //
            // RadioCustomIPDetect
            //
            this.RadioCustomIPDetect.AutoSize = true;
            this.RadioCustomIPDetect.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RadioCustomIPDetect.Location = new System.Drawing.Point(0, 57);
            this.RadioCustomIPDetect.Name = "RadioCustomIPDetect";
            this.RadioCustomIPDetect.TabIndex = 6;
            this.RadioCustomIPDetect.TabStop = true;
            this.RadioCustomIPDetect.Text = "Custom";
            this.RadioCustomIPDetect.UseVisualStyleBackColor = true;
            //
            // TextIPDetectURL
            //
            this.TextIPDetectURL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextIPDetectURL.Enabled = false;
            this.TextIPDetectURL.Location = new System.Drawing.Point(84, 55);
            this.TextIPDetectURL.Name = "TextIPDetectURL";
            this.TextIPDetectURL.Size = new System.Drawing.Size(264, 23);
            this.TextIPDetectURL.TabIndex = 7;
            //
            // GroupTimeFormat
            //
            this.GroupTimeFormat.Controls.Add(this.RadioISOFormat);
            this.GroupTimeFormat.Controls.Add(this.RadioEUFormat);
            this.GroupTimeFormat.Controls.Add(this.RadioUSFormat);
            this.GroupTimeFormat.Controls.Add(this.GroupSpeedUnits);
            this.GroupTimeFormat.Location = new System.Drawing.Point(16, 198);
            this.GroupTimeFormat.Name = "GroupTimeFormat";
            this.GroupTimeFormat.Size = new System.Drawing.Size(348, 126);
            this.GroupTimeFormat.TabIndex = 8;
            this.GroupTimeFormat.TabStop = false;
            this.GroupTimeFormat.Text = "Display";
            //
            // RadioISOFormat
            //
            this.RadioISOFormat.AutoSize = true;
            this.RadioISOFormat.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RadioISOFormat.Location = new System.Drawing.Point(0, 30);
            this.RadioISOFormat.Name = "RadioISOFormat";
            this.RadioISOFormat.TabIndex = 9;
            this.RadioISOFormat.TabStop = true;
            this.RadioISOFormat.Text = "ISO date, 24h clock";
            this.RadioISOFormat.UseVisualStyleBackColor = true;
            //
            // RadioEUFormat
            //
            this.RadioEUFormat.AutoSize = true;
            this.RadioEUFormat.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RadioEUFormat.Location = new System.Drawing.Point(0, 52);
            this.RadioEUFormat.Name = "RadioEUFormat";
            this.RadioEUFormat.TabIndex = 10;
            this.RadioEUFormat.TabStop = true;
            this.RadioEUFormat.Text = "EU date, 24h clock";
            this.RadioEUFormat.UseVisualStyleBackColor = true;
            //
            // RadioUSFormat
            //
            this.RadioUSFormat.AutoSize = true;
            this.RadioUSFormat.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RadioUSFormat.Location = new System.Drawing.Point(0, 74);
            this.RadioUSFormat.Name = "RadioUSFormat";
            this.RadioUSFormat.TabIndex = 11;
            this.RadioUSFormat.TabStop = true;
            this.RadioUSFormat.Text = "US date, 12h clock";
            this.RadioUSFormat.UseVisualStyleBackColor = true;
            //
            // GroupSpeedUnits
            //
            this.GroupSpeedUnits.Controls.Add(this.RadioBitsPerSecond);
            this.GroupSpeedUnits.Controls.Add(this.RadioBytesPerSecond);
            this.GroupSpeedUnits.Location = new System.Drawing.Point(0, 100);
            this.GroupSpeedUnits.Name = "GroupSpeedUnits";
            this.GroupSpeedUnits.Size = new System.Drawing.Size(348, 24);
            this.GroupSpeedUnits.TabIndex = 12;
            //
            // RadioBitsPerSecond
            //
            this.RadioBitsPerSecond.AutoSize = true;
            this.RadioBitsPerSecond.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RadioBitsPerSecond.Location = new System.Drawing.Point(0, 2);
            this.RadioBitsPerSecond.Name = "RadioBitsPerSecond";
            this.RadioBitsPerSecond.TabIndex = 13;
            this.RadioBitsPerSecond.TabStop = true;
            this.RadioBitsPerSecond.Text = "Bits per second (mb/s)";
            this.RadioBitsPerSecond.UseVisualStyleBackColor = true;
            //
            // RadioBytesPerSecond
            //
            this.RadioBytesPerSecond.AutoSize = true;
            this.RadioBytesPerSecond.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.RadioBytesPerSecond.Location = new System.Drawing.Point(180, 2);
            this.RadioBytesPerSecond.Name = "RadioBytesPerSecond";
            this.RadioBytesPerSecond.TabIndex = 14;
            this.RadioBytesPerSecond.TabStop = true;
            this.RadioBytesPerSecond.Text = "Bytes per second (MB/s)";
            this.RadioBytesPerSecond.UseVisualStyleBackColor = true;
            //
            // GroupLogging
            //
            this.GroupLogging.Controls.Add(this.LabelCheckConnection);
            this.GroupLogging.Controls.Add(this.ComboCheckConnection);
            this.GroupLogging.Controls.Add(this.LabelLogFile);
            this.GroupLogging.Controls.Add(this.TextLogFile);
            this.GroupLogging.Controls.Add(this.ButtonLogFile);
            this.GroupLogging.Controls.Add(this.CheckAutoStart);
            this.GroupLogging.Controls.Add(this.CheckMinimize);
            this.GroupLogging.Location = new System.Drawing.Point(16, 332);
            this.GroupLogging.Name = "GroupLogging";
            this.GroupLogging.Size = new System.Drawing.Size(348, 128);
            this.GroupLogging.TabIndex = 15;
            this.GroupLogging.TabStop = false;
            this.GroupLogging.Text = "Logging";
            //
            // LabelCheckConnection
            //
            this.LabelCheckConnection.AutoSize = true;
            this.LabelCheckConnection.Location = new System.Drawing.Point(0, 36);
            this.LabelCheckConnection.Name = "LabelCheckConnection";
            this.LabelCheckConnection.TabIndex = 16;
            this.LabelCheckConnection.Text = "Test every";
            //
            // ComboCheckConnection
            //
            this.ComboCheckConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ComboCheckConnection.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ComboCheckConnection.FormattingEnabled = true;
            this.ComboCheckConnection.Items.AddRange(new object[] {
                "15 seconds",
                "30 seconds",
                "1 minute",
                "5 minutes",
                "10 minutes",
                "30 minutes",
                "1 hour"});
            this.ComboCheckConnection.Location = new System.Drawing.Point(84, 32);
            this.ComboCheckConnection.Name = "ComboCheckConnection";
            this.ComboCheckConnection.Size = new System.Drawing.Size(140, 23);
            this.ComboCheckConnection.TabIndex = 17;
            //
            // LabelLogFile
            //
            this.LabelLogFile.AutoSize = true;
            this.LabelLogFile.Location = new System.Drawing.Point(0, 68);
            this.LabelLogFile.Name = "LabelLogFile";
            this.LabelLogFile.TabIndex = 18;
            this.LabelLogFile.Text = "Log file";
            //
            // TextLogFile
            //
            this.TextLogFile.BackColor = System.Drawing.SystemColors.Window;
            this.TextLogFile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextLogFile.Location = new System.Drawing.Point(84, 64);
            this.TextLogFile.Name = "TextLogFile";
            this.TextLogFile.ReadOnly = true;
            this.TextLogFile.Size = new System.Drawing.Size(228, 23);
            this.TextLogFile.TabIndex = 19;
            //
            // ButtonLogFile
            //
            this.ButtonLogFile.Location = new System.Drawing.Point(318, 63);
            this.ButtonLogFile.Name = "ButtonLogFile";
            this.ButtonLogFile.Size = new System.Drawing.Size(30, 25);
            this.ButtonLogFile.TabIndex = 20;
            this.ButtonLogFile.Text = "...";
            this.ButtonLogFile.Click += new System.EventHandler(this.ButtonLogFile_Click);
            //
            // CheckAutoStart
            //
            this.CheckAutoStart.AutoSize = true;
            this.CheckAutoStart.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CheckAutoStart.Location = new System.Drawing.Point(0, 100);
            this.CheckAutoStart.Name = "CheckAutoStart";
            this.CheckAutoStart.TabIndex = 21;
            this.CheckAutoStart.Text = "Start with Windows";
            this.CheckAutoStart.UseVisualStyleBackColor = true;
            //
            // CheckMinimize
            //
            this.CheckMinimize.AutoSize = true;
            this.CheckMinimize.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.CheckMinimize.Location = new System.Drawing.Point(180, 100);
            this.CheckMinimize.Name = "CheckMinimize";
            this.CheckMinimize.TabIndex = 22;
            this.CheckMinimize.Text = "Minimize to system tray";
            this.CheckMinimize.UseVisualStyleBackColor = true;
            //
            // ButtonOpenLog
            //
            this.ButtonOpenLog.Location = new System.Drawing.Point(16, 476);
            this.ButtonOpenLog.Name = "ButtonOpenLog";
            this.ButtonOpenLog.Size = new System.Drawing.Size(120, 30);
            this.ButtonOpenLog.TabIndex = 25;
            this.ButtonOpenLog.Text = "Open log file";
            this.ButtonOpenLog.Click += new System.EventHandler(this.ButtonOpenLog_Click);
            //
            // ButtonCancel
            //
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(196, 476);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(80, 30);
            this.ButtonCancel.TabIndex = 24;
            this.ButtonCancel.Text = "Cancel";
            //
            // ButtonOK
            //
            this.ButtonOK.Location = new System.Drawing.Point(284, 476);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.Size = new System.Drawing.Size(80, 30);
            this.ButtonOK.TabIndex = 23;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
            //
            // FormSettings
            //
            this.AcceptButton = this.ButtonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(380, 522);
            this.Controls.Add(this.GroupDownloadURL);
            this.Controls.Add(this.GroupAPIURL);
            this.Controls.Add(this.GroupTimeFormat);
            this.Controls.Add(this.GroupLogging);
            this.Controls.Add(this.ButtonOK);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOpenLog);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSettings";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configuration settings";
            this.GroupDownloadURL.ResumeLayout(false);
            this.GroupDownloadURL.PerformLayout();
            this.GroupAPIURL.ResumeLayout(false);
            this.GroupAPIURL.PerformLayout();
            this.GroupTimeFormat.ResumeLayout(false);
            this.GroupTimeFormat.PerformLayout();
            this.GroupSpeedUnits.ResumeLayout(false);
            this.GroupSpeedUnits.PerformLayout();
            this.GroupLogging.ResumeLayout(false);
            this.GroupLogging.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private SectionBox GroupDownloadURL;
        private System.Windows.Forms.RadioButton RadioAutoDownload;
        private System.Windows.Forms.RadioButton RadioCustomDownload;
        private System.Windows.Forms.TextBox TextDownloadURL;
        private SectionBox GroupAPIURL;
        private System.Windows.Forms.RadioButton RadioAutoDetectIP;
        private System.Windows.Forms.RadioButton RadioCustomIPDetect;
        private System.Windows.Forms.TextBox TextIPDetectURL;
        private SectionBox GroupTimeFormat;
        private System.Windows.Forms.RadioButton RadioISOFormat;
        private System.Windows.Forms.RadioButton RadioEUFormat;
        private System.Windows.Forms.RadioButton RadioUSFormat;
        private System.Windows.Forms.Panel GroupSpeedUnits;
        private System.Windows.Forms.RadioButton RadioBitsPerSecond;
        private System.Windows.Forms.RadioButton RadioBytesPerSecond;
        private SectionBox GroupLogging;
        private System.Windows.Forms.Label LabelCheckConnection;
        private System.Windows.Forms.ComboBox ComboCheckConnection;
        private System.Windows.Forms.Label LabelLogFile;
        private System.Windows.Forms.TextBox TextLogFile;
        private System.Windows.Forms.Button ButtonLogFile;
        private System.Windows.Forms.CheckBox CheckAutoStart;
        private System.Windows.Forms.CheckBox CheckMinimize;
        private System.Windows.Forms.Button ButtonOpenLog;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonOK;
    }
}
