using System;

namespace ASCOM.EqPlatformAdapter
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private Settings settings = new Settings();

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                if (settings != null)
                {
                    settings.Dispose();
                    settings = null;
                }
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
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.stroke = new System.Windows.Forms.NumericUpDown();
            this.camName = new System.Windows.Forms.TextBox();
            this.mountName = new System.Windows.Forms.TextBox();
            this.switchIds = new System.Windows.Forms.ComboBox();
            this.switchDriverName = new System.Windows.Forms.TextBox();
            this.statusTimer = new System.Windows.Forms.Timer(this.components);
            this.topPanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.expand = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnResume = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.platformStatus = new System.Windows.Forms.TextBox();
            this.settingsPanel = new System.Windows.Forms.Panel();
            this.cbTrace = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.setupCam = new System.Windows.Forms.Button();
            this.chooseCam = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.setupMount = new System.Windows.Forms.Button();
            this.chooseMount = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.setupSwitch = new System.Windows.Forms.Button();
            this.chooseSwitch = new System.Windows.Forms.Button();
            this.clearSwitch = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.stroke)).BeginInit();
            this.topPanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.settingsPanel.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // stroke
            // 
            this.stroke.DecimalPlaces = 1;
            this.stroke.Location = new System.Drawing.Point(145, 11);
            this.stroke.Maximum = new decimal(new int[] {
            99,
            0,
            0,
            0});
            this.stroke.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.stroke.Name = "stroke";
            this.stroke.Size = new System.Drawing.Size(48, 20);
            this.stroke.TabIndex = 10;
            this.toolTip1.SetToolTip(this.stroke, "Full platform stroke, in degrees");
            this.stroke.Value = new decimal(new int[] {
            22,
            0,
            0,
            0});
            this.stroke.ValueChanged += new System.EventHandler(this.stroke_ValueChanged);
            // 
            // camName
            // 
            this.camName.Location = new System.Drawing.Point(7, 20);
            this.camName.Name = "camName";
            this.camName.ReadOnly = true;
            this.camName.Size = new System.Drawing.Size(290, 20);
            this.camName.TabIndex = 0;
            this.toolTip1.SetToolTip(this.camName, "ASCOM Camera device selection.  The camera guide port should be connected to the " +
        "equatorial platform.");
            // 
            // mountName
            // 
            this.mountName.Location = new System.Drawing.Point(7, 20);
            this.mountName.Name = "mountName";
            this.mountName.ReadOnly = true;
            this.mountName.Size = new System.Drawing.Size(290, 20);
            this.mountName.TabIndex = 0;
            this.toolTip1.SetToolTip(this.mountName, "ASCOM Mount selection. Select the ASCOM driver for the alt-az telescope mount.");
            // 
            // switchIds
            // 
            this.switchIds.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.switchIds.FormattingEnabled = true;
            this.switchIds.Location = new System.Drawing.Point(7, 50);
            this.switchIds.Name = "switchIds";
            this.switchIds.Size = new System.Drawing.Size(290, 21);
            this.switchIds.TabIndex = 3;
            this.toolTip1.SetToolTip(this.switchIds, "For ASCOM drivers that can control multiple switches, select the switch that cont" +
        "rols the platform tracking.");
            this.switchIds.SelectedIndexChanged += new System.EventHandler(this.switchId_SelectedIndexChanged);
            // 
            // switchDriverName
            // 
            this.switchDriverName.Location = new System.Drawing.Point(7, 20);
            this.switchDriverName.Name = "switchDriverName";
            this.switchDriverName.ReadOnly = true;
            this.switchDriverName.Size = new System.Drawing.Size(263, 20);
            this.switchDriverName.TabIndex = 0;
            this.toolTip1.SetToolTip(this.switchDriverName, "ASCOM Switch driver selection. Select the ASCOM driver  that controls platform tr" +
        "acking.");
            // 
            // statusTimer
            // 
            this.statusTimer.Interval = 500;
            this.statusTimer.Tick += new System.EventHandler(this.statusTimer_Tick);
            // 
            // topPanel
            // 
            this.topPanel.AutoSize = true;
            this.topPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.topPanel.Controls.Add(this.tableLayoutPanel1);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(402, 142);
            this.topPanel.TabIndex = 9;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.expand, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(402, 142);
            this.tableLayoutPanel1.TabIndex = 10;
            // 
            // expand
            // 
            this.expand.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.expand.Location = new System.Drawing.Point(163, 119);
            this.expand.Name = "expand";
            this.expand.Size = new System.Drawing.Size(75, 20);
            this.expand.TabIndex = 9;
            this.expand.Text = "?";
            this.expand.UseVisualStyleBackColor = true;
            this.expand.Click += new System.EventHandler(this.expand_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.groupBox1.Controls.Add(this.btnReset);
            this.groupBox1.Controls.Add(this.btnResume);
            this.groupBox1.Controls.Add(this.btnPause);
            this.groupBox1.Controls.Add(this.btnStart);
            this.groupBox1.Controls.Add(this.platformStatus);
            this.groupBox1.Location = new System.Drawing.Point(19, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(364, 110);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Platform Tracking";
            // 
            // btnReset
            // 
            this.btnReset.Enabled = false;
            this.btnReset.Location = new System.Drawing.Point(278, 26);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(71, 41);
            this.btnReset.TabIndex = 9;
            this.btnReset.Text = "Rese&t";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnResume
            // 
            this.btnResume.Enabled = false;
            this.btnResume.Location = new System.Drawing.Point(191, 26);
            this.btnResume.Name = "btnResume";
            this.btnResume.Size = new System.Drawing.Size(71, 41);
            this.btnResume.TabIndex = 8;
            this.btnResume.Text = "&Resume";
            this.btnResume.UseVisualStyleBackColor = true;
            this.btnResume.Click += new System.EventHandler(this.btnResume_Click);
            // 
            // btnPause
            // 
            this.btnPause.Enabled = false;
            this.btnPause.Location = new System.Drawing.Point(104, 26);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(71, 41);
            this.btnPause.TabIndex = 7;
            this.btnPause.Text = "&Pause";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnStart
            // 
            this.btnStart.Enabled = false;
            this.btnStart.Location = new System.Drawing.Point(17, 26);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(71, 41);
            this.btnStart.TabIndex = 6;
            this.btnStart.Text = "&Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // platformStatus
            // 
            this.platformStatus.Location = new System.Drawing.Point(37, 77);
            this.platformStatus.Name = "platformStatus";
            this.platformStatus.ReadOnly = true;
            this.platformStatus.Size = new System.Drawing.Size(290, 20);
            this.platformStatus.TabIndex = 5;
            // 
            // settingsPanel
            // 
            this.settingsPanel.AutoSize = true;
            this.settingsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.settingsPanel.Controls.Add(this.cbTrace);
            this.settingsPanel.Controls.Add(this.label1);
            this.settingsPanel.Controls.Add(this.stroke);
            this.settingsPanel.Controls.Add(this.groupBox2);
            this.settingsPanel.Controls.Add(this.groupBox3);
            this.settingsPanel.Controls.Add(this.groupBox4);
            this.settingsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.settingsPanel.Location = new System.Drawing.Point(0, 142);
            this.settingsPanel.MinimumSize = new System.Drawing.Size(0, 330);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new System.Drawing.Size(402, 330);
            this.settingsPanel.TabIndex = 10;
            this.settingsPanel.Visible = false;
            // 
            // cbTrace
            // 
            this.cbTrace.AutoSize = true;
            this.cbTrace.Location = new System.Drawing.Point(298, 14);
            this.cbTrace.Name = "cbTrace";
            this.cbTrace.Size = new System.Drawing.Size(69, 17);
            this.cbTrace.TabIndex = 12;
            this.cbTrace.Text = "Trace on";
            this.cbTrace.UseVisualStyleBackColor = true;
            this.cbTrace.CheckedChanged += new System.EventHandler(this.cbTrace_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Platform stroke (degrees)";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.setupCam);
            this.groupBox2.Controls.Add(this.chooseCam);
            this.groupBox2.Controls.Add(this.camName);
            this.groupBox2.Location = new System.Drawing.Point(11, 50);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(384, 80);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Camera";
            // 
            // setupCam
            // 
            this.setupCam.Location = new System.Drawing.Point(303, 48);
            this.setupCam.Name = "setupCam";
            this.setupCam.Size = new System.Drawing.Size(75, 23);
            this.setupCam.TabIndex = 2;
            this.setupCam.Text = "Setup";
            this.setupCam.UseVisualStyleBackColor = true;
            this.setupCam.Click += new System.EventHandler(this.setupCam_Click);
            // 
            // chooseCam
            // 
            this.chooseCam.Location = new System.Drawing.Point(303, 19);
            this.chooseCam.Name = "chooseCam";
            this.chooseCam.Size = new System.Drawing.Size(75, 23);
            this.chooseCam.TabIndex = 1;
            this.chooseCam.Text = "Choose";
            this.chooseCam.UseVisualStyleBackColor = true;
            this.chooseCam.Click += new System.EventHandler(this.chooseCam_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.setupMount);
            this.groupBox3.Controls.Add(this.chooseMount);
            this.groupBox3.Controls.Add(this.mountName);
            this.groupBox3.Location = new System.Drawing.Point(11, 145);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(384, 80);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Mount";
            // 
            // setupMount
            // 
            this.setupMount.Location = new System.Drawing.Point(303, 48);
            this.setupMount.Name = "setupMount";
            this.setupMount.Size = new System.Drawing.Size(75, 23);
            this.setupMount.TabIndex = 2;
            this.setupMount.Text = "Setup";
            this.setupMount.UseVisualStyleBackColor = true;
            this.setupMount.Click += new System.EventHandler(this.setupMount_Click);
            // 
            // chooseMount
            // 
            this.chooseMount.Location = new System.Drawing.Point(303, 19);
            this.chooseMount.Name = "chooseMount";
            this.chooseMount.Size = new System.Drawing.Size(75, 23);
            this.chooseMount.TabIndex = 1;
            this.chooseMount.Text = "Choose";
            this.chooseMount.UseVisualStyleBackColor = true;
            this.chooseMount.Click += new System.EventHandler(this.chooseMount_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.clearSwitch);
            this.groupBox4.Controls.Add(this.switchIds);
            this.groupBox4.Controls.Add(this.setupSwitch);
            this.groupBox4.Controls.Add(this.chooseSwitch);
            this.groupBox4.Controls.Add(this.switchDriverName);
            this.groupBox4.Location = new System.Drawing.Point(11, 240);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(384, 80);
            this.groupBox4.TabIndex = 9;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Platform Switch";
            // 
            // setupSwitch
            // 
            this.setupSwitch.Location = new System.Drawing.Point(303, 48);
            this.setupSwitch.Name = "setupSwitch";
            this.setupSwitch.Size = new System.Drawing.Size(75, 23);
            this.setupSwitch.TabIndex = 2;
            this.setupSwitch.Text = "Setup";
            this.setupSwitch.UseVisualStyleBackColor = true;
            this.setupSwitch.Click += new System.EventHandler(this.switchSetup_Click);
            // 
            // chooseSwitch
            // 
            this.chooseSwitch.Location = new System.Drawing.Point(303, 19);
            this.chooseSwitch.Name = "chooseSwitch";
            this.chooseSwitch.Size = new System.Drawing.Size(75, 23);
            this.chooseSwitch.TabIndex = 1;
            this.chooseSwitch.Text = "Choose";
            this.chooseSwitch.UseVisualStyleBackColor = true;
            this.chooseSwitch.Click += new System.EventHandler(this.chooseSwitch_Click);
            // 
            // clearSwitch
            // 
            this.clearSwitch.Location = new System.Drawing.Point(276, 19);
            this.clearSwitch.Name = "clearSwitch";
            this.clearSwitch.Size = new System.Drawing.Size(21, 23);
            this.clearSwitch.TabIndex = 4;
            this.clearSwitch.Text = "x";
            this.toolTip1.SetToolTip(this.clearSwitch, "Clear platform switch selection");
            this.clearSwitch.UseVisualStyleBackColor = true;
            this.clearSwitch.Click += new System.EventHandler(this.clearSwitch_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(402, 473);
            this.Controls.Add(this.settingsPanel);
            this.Controls.Add(this.topPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(418, 38);
            this.Name = "MainForm";
            this.Text = "EqPlatformAdapter Driver Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.stroke)).EndInit();
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.settingsPanel.ResumeLayout(false);
            this.settingsPanel.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer statusTimer;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnResume;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox platformStatus;
        private System.Windows.Forms.Button expand;
        private System.Windows.Forms.Panel settingsPanel;
        private System.Windows.Forms.CheckBox cbTrace;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown stroke;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button setupCam;
        private System.Windows.Forms.Button chooseCam;
        private System.Windows.Forms.TextBox camName;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button setupMount;
        private System.Windows.Forms.Button chooseMount;
        private System.Windows.Forms.TextBox mountName;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ComboBox switchIds;
        private System.Windows.Forms.Button setupSwitch;
        private System.Windows.Forms.Button chooseSwitch;
        private System.Windows.Forms.TextBox switchDriverName;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button clearSwitch;
    }
}

