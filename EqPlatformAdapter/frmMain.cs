using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ASCOM.EqPlatformAdapter
{
    public partial class MainForm : Form
    {
        delegate void SetTextCallback(string text);

        public MainForm()
        {
            InitializeComponent();
        }

        private string DeviceName(string deviceType, string driverId)
        {
            if (driverId.Length == 0)
                return String.Empty;

            using (ASCOM.Utilities.Profile profile = new ASCOM.Utilities.Profile())
            {
                profile.DeviceType = deviceType;
                return profile.GetValue(driverId, string.Empty, string.Empty, driverId);
            }
        }

        private void chooseCam_Click(object sender, EventArgs e)
        {
            string camId = settings.Get("cameraId");

            using (ASCOM.Utilities.Chooser chooser = new Utilities.Chooser())
            {
                chooser.DeviceType = "Camera";
                string val = chooser.Choose(camId);
                if (!String.IsNullOrEmpty(val) && val != camId)
                {
                    if (val == SharedResources.CamDriverId)
                        return; // reject choice of this driver!
                    settings.Set("cameraId", val);
                    camName.Text = DeviceName("Camera", val);
                    SharedResources.FreeCamera();
                }
            }
        }

        private void LoadSwitches()
        {
            this.switchIds.Items.Clear();

            string switchDriverId = settings.Get("switchDriverId");

            if (switchDriverId.Length == 0)
                return;

            using (ASCOM.DriverAccess.Switch sw = new ASCOM.DriverAccess.Switch(switchDriverId))
            {
                sw.Connected = true;
                for (short n = 0; n < sw.MaxSwitch; n++)
                {
                    string name = sw.GetSwitchName(n);
                    this.switchIds.Items.Add(name);
                }
            }

            short switchId = 0;
            Int16.TryParse(settings.Get("switchId", "0"), out switchId);
            this.switchIds.SelectedIndex = switchId;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            string stroke = settings.Get("strokeDegrees");
            if (stroke.Length != 0)
            {
                double val = 22.0;
                Double.TryParse(stroke, out val);
                this.stroke.Value = (decimal) val;
            }

            string s = settings.Get("traceOn", false.ToString());
            bool trace = false;
            Boolean.TryParse(s, out trace);
            cbTrace.Checked = trace;

            camName.Text = DeviceName("Camera", settings.Get("cameraId"));
            mountName.Text = DeviceName("Telescope", settings.Get("scopeId"));
            switchDriverName.Text = DeviceName("Switch", settings.Get("switchDriverId"));

            LoadSwitches();
        }

        private void chooseMount_Click(object sender, EventArgs e)
        {
            string scopeId = settings.Get("scopeId");

            using (ASCOM.Utilities.Chooser chooser = new Utilities.Chooser())
            {
                chooser.DeviceType = "Telescope";
                string val = chooser.Choose(scopeId);
                if (!String.IsNullOrEmpty(val) && val != scopeId)
                {
                    if (val == SharedResources.ScopeDriverId)
                        return; // reject choice of this driver!
                    settings.Set("scopeId", val);
                    mountName.Text = DeviceName("Telescope", val);
                    SharedResources.FreeMount();
                }
            }
        }

        private void chooseSwitch_Click(object sender, EventArgs e)
        {
            string switchDriverId = settings.Get("switchDriverId");

            using (ASCOM.Utilities.Chooser chooser = new Utilities.Chooser())
            {
                chooser.DeviceType = "Switch";
                string val = chooser.Choose(switchDriverId);
                if (!String.IsNullOrEmpty(val) && val != switchDriverId)
                {
                    settings.Set("switchDriverId", val);
                    switchDriverName.Text = DeviceName("Switch", val);
                    SharedResources.FreeMount();
                }
            }

            LoadSwitches();
        }

        private void setupMount_Click(object sender, EventArgs e)
        {
            SharedResources.SetupMount();
        }

        private void switchId_SelectedIndexChanged(object sender, EventArgs e)
        {
            short switchId = (short)this.switchIds.SelectedIndex;
            settings.Set("switchId", switchId.ToString());
        }

        private void setupCam_Click(object sender, EventArgs e)
        {
            SharedResources.SetupCamera();
        }

        private void switchSetup_Click(object sender, EventArgs e)
        {
            SharedResources.SetupSwitch();
        }

        public void UpdateState()
        {
            bool cam_connected = SharedResources.CameraConnected;
            bool mount_connected = SharedResources.MountConnected;

            bool enable_cam = !cam_connected && !mount_connected;
            this.chooseCam.Enabled = enable_cam;
            this.setupCam.Enabled = enable_cam;

            bool enable_mount = !mount_connected;
            this.chooseMount.Enabled = enable_mount;
            this.setupMount.Enabled = enable_mount;
            this.chooseSwitch.Enabled = enable_mount;
            this.setupSwitch.Enabled = enable_mount;
            this.switchIds.Enabled = enable_mount;

            Platform platform = SharedResources.s_platform;

            stroke.Enabled = !mount_connected;

            if (!mount_connected)
            {
                btnStart.Enabled = false;
                btnPause.Enabled = false;
                btnResume.Enabled = false;
                btnReset.Enabled = false;
                platformStatus.Clear();
                statusTimer.Enabled = false;
                return;
            }

            switch (platform.TrackingState)
            {
                case TrackingStates.AtStart:
                    btnStart.Enabled = true;
                    btnPause.Enabled = false;
                    btnResume.Enabled = false;
                    btnReset.Enabled = false;
                    platformStatus.Text = "At Start";
                    statusTimer.Enabled = false;
                    break;
                case TrackingStates.Stopped:
                    btnStart.Enabled = false;
                    btnPause.Enabled = false;
                    btnResume.Enabled = platform.TimeRemaining > 0.0;
                    btnReset.Enabled = true;
                    platformStatus.Text = String.Format("Paused, {0:F0}s remaining", platform.TimeRemaining);
                    statusTimer.Enabled = false;
                    break;
                case TrackingStates.Tracking:
                    btnStart.Enabled = false;
                    btnPause.Enabled = true;
                    btnResume.Enabled = false;
                    btnReset.Enabled = true;
                    platformStatus.Text = String.Format("Tracking, {0:F0}s remaining", platform.TimeRemaining);
                    statusTimer.Enabled = true;
                    break;
            }
        }

        private void statusTimer_Tick(object sender, EventArgs e)
        {
            Platform platform = SharedResources.s_platform;

            if (platform.TimeRemaining <= 0.0)
            {
                platform.StopTracking(); // will update state
                return;
            }

            UpdateState();
        }

        private void stroke_ValueChanged(object sender, EventArgs e)
        {
            SharedResources.s_platform.StrokeDegrees = (double) stroke.Value;
            settings.Set("strokeDegrees", String.Format("{0:F1}", stroke.Value));
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            SharedResources.s_platform.StartTracking();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            SharedResources.s_platform.StopTracking();
        }

        private void btnResume_Click(object sender, EventArgs e)
        {
            SharedResources.s_platform.ResumeTracking();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            SharedResources.s_platform.Reset();
        }

        private void cbTrace_CheckedChanged(object sender, EventArgs e)
        {
            bool enable = cbTrace.Checked;
            SharedResources.EnableLogging(enable);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Server.ObjectsCount != 0)
            {
                string msg = String.Format("There are {0} client application(s) currently connected to the server. Are you sure you want to exit?", Server.ObjectsCount);
                if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
