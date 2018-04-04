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

        private void mountTrackingClick(object sender, EventArgs e)
        {

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
            this.chooseCam.Enabled = !cam_connected;
            this.setupCam.Enabled = !cam_connected;

            bool mount_connected = SharedResources.MountConnected;
            this.chooseMount.Enabled = !mount_connected;
            this.setupMount.Enabled = !mount_connected;
            this.chooseSwitch.Enabled = !mount_connected;
            this.setupSwitch.Enabled = !mount_connected;
            this.switchIds.Enabled = !mount_connected;
        }
    }
}
