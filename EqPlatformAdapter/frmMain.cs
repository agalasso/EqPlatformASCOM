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

        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        private void chooseCam_Click(object sender, EventArgs e)
        {
            using (ASCOM.Utilities.Profile profile = new Utilities.Profile())
            {
                profile.DeviceType = "Camera";
                string camId = profile.GetValue(SharedResources.CamDriverId, "cameraId", string.Empty, string.Empty);

                using (ASCOM.Utilities.Chooser chooser = new Utilities.Chooser())
                {
                    chooser.DeviceType = "Camera";
                    string val = chooser.Choose(camId);
                    if (val != null && val.Length != 0 && val != camId)
                    {
                        if (val == SharedResources.CamDriverId)
                            return; // reject choice of this driver!
                        string camName = profile.GetValue(val, string.Empty, string.Empty, val);
                        profile.WriteValue(SharedResources.CamDriverId, "cameraId", val);
                        profile.WriteValue(SharedResources.CamDriverId, "cameraName", camName);
                        this.camName.Text = camName;
                        SharedResources.FreeCamera();
                    }
                }
            }
        }

        private void LoadSwitches()
        {
            this.switchIds.Items.Clear();

            using (ASCOM.Utilities.Profile profile = new Utilities.Profile())
            {
                profile.DeviceType = "Telescope";
                string switchDriverId = profile.GetValue(SharedResources.ScopeDriverId, "switchDriverId", string.Empty, string.Empty);

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
                Int16.TryParse(profile.GetValue(SharedResources.ScopeDriverId, "switchId", string.Empty, "0"), out switchId);
                this.switchIds.SelectedIndex = switchId;
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            using (ASCOM.Utilities.Profile profile = new Utilities.Profile())
            {
                profile.DeviceType = "Telescope";
                string stroke = profile.GetValue(SharedResources.ScopeDriverId, "strokeDegrees");
                if (stroke != null && stroke.Length != 0)
                {
                    double val = 22.0;
                    Double.TryParse(stroke, out val);
                    this.stroke.Value = (decimal) val;
                }

                profile.DeviceType = "Camera";
                string camId = profile.GetValue(SharedResources.CamDriverId, "cameraId");
                if (camId != null && camId.Length != 0)
                {
                    string camName = profile.GetValue(camId, string.Empty, string.Empty, camId);
                    this.camName.Text = camName;
                }

                profile.DeviceType = "Telescope";
                string scopeId = profile.GetValue(SharedResources.ScopeDriverId, "scopeId");
                if (scopeId != null && scopeId.Length != 0)
                {
                    string scopeName = profile.GetValue(scopeId, string.Empty, string.Empty, scopeId);
                    this.mountName.Text = scopeName;
                }

                profile.DeviceType = "Telescope";
                string switchDriverId = profile.GetValue(SharedResources.ScopeDriverId, "switchDriverId");
                if (switchDriverId != null && switchDriverId.Length != 0)
                {
                    profile.DeviceType = "Switch";
                    string switchDriverName = profile.GetValue(switchDriverId, string.Empty, string.Empty, switchDriverId);
                    this.switchDriverName.Text = switchDriverName;
                }
            }

            LoadSwitches();
        }

        private void chooseMount_Click(object sender, EventArgs e)
        {
            using (ASCOM.Utilities.Profile profile = new Utilities.Profile())
            {
                profile.DeviceType = "Telescope";
                string scopeId = profile.GetValue(SharedResources.ScopeDriverId, "scopeId", string.Empty, string.Empty);

                using (ASCOM.Utilities.Chooser chooser = new Utilities.Chooser())
                {
                    chooser.DeviceType = "Telescope";
                    string val = chooser.Choose(scopeId);
                    if (val != null && val.Length != 0 && val != scopeId)
                    {
                        if (val == SharedResources.ScopeDriverId)
                            return; // reject choice of this driver!
                        string scopeName = profile.GetValue(val, string.Empty, string.Empty, val);
                        profile.WriteValue(SharedResources.ScopeDriverId, "scopeId", val);
                        profile.WriteValue(SharedResources.ScopeDriverId, "scopeName", scopeName);
                        this.mountName.Text = scopeName;
                        SharedResources.FreeMount();
                    }
                }
            }
        }

        private void chooseSwitch_Click(object sender, EventArgs e)
        {
            using (ASCOM.Utilities.Profile profile = new Utilities.Profile())
            {
                profile.DeviceType = "Telescope";
                string switchDriverId = profile.GetValue(SharedResources.ScopeDriverId, "switchDriverId", string.Empty, string.Empty);

                using (ASCOM.Utilities.Chooser chooser = new Utilities.Chooser())
                {
                    chooser.DeviceType = "Switch";
                    string val = chooser.Choose(switchDriverId);
                    if (val != null && val.Length != 0 && val != switchDriverId)
                    {
                        profile.DeviceType = "Switch";
                        string switchDriverName = profile.GetValue(val, string.Empty, string.Empty, val);
                        profile.DeviceType = "Telescope";
                        profile.WriteValue(SharedResources.ScopeDriverId, "switchDriverId", val);
                        profile.WriteValue(SharedResources.ScopeDriverId, "switchDriverName", switchDriverName);
                        this.switchDriverName.Text = switchDriverName;
                        SharedResources.FreeMount();
                    }
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
            using (ASCOM.Utilities.Profile profile = new Utilities.Profile())
            {
                profile.DeviceType = "Telescope";
                short switchId = (short)this.switchIds.SelectedIndex;
                profile.WriteValue(SharedResources.ScopeDriverId, "switchId", switchId.ToString());
            }
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
            using (Settings s = new Settings())
            {
                s.Set("strokeDegrees", String.Format("{0:F1}", stroke.Value));
            }
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
    }
}
