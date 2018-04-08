using System;
using System.Windows.Forms;

namespace ASCOM.EqPlatformAdapter
{
    public partial class Form1 : Form
    {
        private ASCOM.DriverAccess.Telescope driver;

        public Form1()
        {
            InitializeComponent();
            SetUIState();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsConnected)
                driver.Connected = false;

            Properties.Settings.Default.Save();
        }

        private void buttonChoose_Click(object sender, EventArgs e)
        {
            string choice = ASCOM.DriverAccess.Telescope.Choose(Properties.Settings.Default.DriverId);
            if (choice != Properties.Settings.Default.DriverId)
            {
                Properties.Settings.Default.DriverId = choice;
                if (driver != null)
                {
                    driver.Dispose();
                    driver = null;
                }
            }
            SetUIState();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.Connected = false;
            }
            else
            {
                try
                {
                    if (driver == null)
                        driver = new ASCOM.DriverAccess.Telescope(Properties.Settings.Default.DriverId);
                    driver.Connected = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not connect: " + ex.Message);
                }
            }
            SetUIState();
        }

        private void SetUIState()
        {
            buttonConnect.Enabled = !string.IsNullOrEmpty(Properties.Settings.Default.DriverId);
            buttonChoose.Enabled = !IsConnected;
            buttonConnect.Text = IsConnected ? "Disconnect" : "Connect";
        }

        private bool IsConnected
        {
            get
            {
                return ((this.driver != null) && (driver.Connected == true));
            }
        }
    }
}
