using System;

namespace ASCOM.EqPlatformAdapter
{
    partial class MainForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.trackPlatform = new System.Windows.Forms.CheckBox();
            this.trackMount = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.setupCam = new System.Windows.Forms.Button();
            this.chooseCam = new System.Windows.Forms.Button();
            this.camName = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.setupMount = new System.Windows.Forms.Button();
            this.chooseMount = new System.Windows.Forms.Button();
            this.mountName = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.switchIds = new System.Windows.Forms.ComboBox();
            this.setupSwitch = new System.Windows.Forms.Button();
            this.chooseSwitch = new System.Windows.Forms.Button();
            this.switchDriverName = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.trackPlatform);
            this.groupBox1.Controls.Add(this.trackMount);
            this.groupBox1.Location = new System.Drawing.Point(15, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 102);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Tracking";
            // 
            // trackPlatform
            // 
            this.trackPlatform.Appearance = System.Windows.Forms.Appearance.Button;
            this.trackPlatform.Location = new System.Drawing.Point(106, 29);
            this.trackPlatform.Name = "trackPlatform";
            this.trackPlatform.Size = new System.Drawing.Size(69, 43);
            this.trackPlatform.TabIndex = 2;
            this.trackPlatform.Text = "Platform";
            this.trackPlatform.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.trackPlatform.UseVisualStyleBackColor = true;
            // 
            // trackMount
            // 
            this.trackMount.Appearance = System.Windows.Forms.Appearance.Button;
            this.trackMount.Location = new System.Drawing.Point(17, 29);
            this.trackMount.Name = "trackMount";
            this.trackMount.Size = new System.Drawing.Size(71, 43);
            this.trackMount.TabIndex = 1;
            this.trackMount.Text = "Mount";
            this.trackMount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.trackMount.UseVisualStyleBackColor = true;
            this.trackMount.Click += new System.EventHandler(this.mountTrackingClick);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.setupCam);
            this.groupBox2.Controls.Add(this.chooseCam);
            this.groupBox2.Controls.Add(this.camName);
            this.groupBox2.Location = new System.Drawing.Point(15, 129);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(384, 80);
            this.groupBox2.TabIndex = 1;
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
            // camName
            // 
            this.camName.Location = new System.Drawing.Point(7, 20);
            this.camName.Name = "camName";
            this.camName.ReadOnly = true;
            this.camName.Size = new System.Drawing.Size(290, 20);
            this.camName.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.setupMount);
            this.groupBox3.Controls.Add(this.chooseMount);
            this.groupBox3.Controls.Add(this.mountName);
            this.groupBox3.Location = new System.Drawing.Point(15, 224);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(384, 80);
            this.groupBox3.TabIndex = 2;
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
            // mountName
            // 
            this.mountName.Location = new System.Drawing.Point(7, 20);
            this.mountName.Name = "mountName";
            this.mountName.ReadOnly = true;
            this.mountName.Size = new System.Drawing.Size(290, 20);
            this.mountName.TabIndex = 0;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.switchIds);
            this.groupBox4.Controls.Add(this.setupSwitch);
            this.groupBox4.Controls.Add(this.chooseSwitch);
            this.groupBox4.Controls.Add(this.switchDriverName);
            this.groupBox4.Location = new System.Drawing.Point(15, 319);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(384, 80);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Platform Switch";
            // 
            // switchIds
            // 
            this.switchIds.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.switchIds.FormattingEnabled = true;
            this.switchIds.Location = new System.Drawing.Point(7, 50);
            this.switchIds.Name = "switchIds";
            this.switchIds.Size = new System.Drawing.Size(290, 21);
            this.switchIds.TabIndex = 3;
            this.switchIds.SelectedIndexChanged += new System.EventHandler(this.switchId_SelectedIndexChanged);
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
            // switchDriverName
            // 
            this.switchDriverName.Location = new System.Drawing.Point(7, 20);
            this.switchDriverName.Name = "switchDriverName";
            this.switchDriverName.ReadOnly = true;
            this.switchDriverName.Size = new System.Drawing.Size(290, 20);
            this.switchDriverName.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(416, 412);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "EqPlatformAdapter Driver Server";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox trackPlatform;
        private System.Windows.Forms.CheckBox trackMount;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button setupCam;
        private System.Windows.Forms.Button chooseCam;
        private System.Windows.Forms.TextBox camName;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button setupMount;
        private System.Windows.Forms.Button chooseMount;
        private System.Windows.Forms.TextBox mountName;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button setupSwitch;
        private System.Windows.Forms.Button chooseSwitch;
        private System.Windows.Forms.TextBox switchDriverName;
        private System.Windows.Forms.ComboBox switchIds;
    }
}

