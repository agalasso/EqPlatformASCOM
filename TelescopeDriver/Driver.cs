//tabs=4
// --------------------------------------------------------------------------------
// TODO fill in this information for your driver, then remove this line!
//
// ASCOM Telescope driver for EqPlatformAdapter
//
// Description:	Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam 
//				nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam 
//				erat, sed diam voluptua. At vero eos et accusam et justo duo 
//				dolores et ea rebum. Stet clita kasd gubergren, no sea takimata 
//				sanctus est Lorem ipsum dolor sit amet.
//
// Implements:	ASCOM Telescope interface version: <To be completed by driver developer>
// Author:		(XXX) Your N. Here <your@email.here>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// dd-mmm-yyyy	XXX	6.0.0	Initial edit, created from ASCOM driver template
// --------------------------------------------------------------------------------
//


// This is used to define code in the template that is specific to one class implementation
// unused code canbe deleted and this definition removed.
#define Telescope

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.Transform;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;
using System.Globalization;
using System.Collections;

namespace ASCOM.EqPlatformAdapter
{
    //
    // Your driver's DeviceID is ASCOM.EqPlatformAdapter.Telescope
    //
    // The Guid attribute sets the CLSID for ASCOM.EqPlatformAdapter.Telescope
    // The ClassInterface/None addribute prevents an empty interface called
    // _EqPlatformAdapter from being created and used as the [default] interface
    //
    // TODO Replace the not implemented exceptions with code to implement the function or
    // throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM Telescope Driver for EqPlatformAdapter.
    /// </summary>
    [Guid("31df09a7-634e-4a1e-8e5e-0e00efe0ff8e")]
    [ProgId("ASCOM.EqPlatformAdatper.Telescope")]
    [ServedClassName("EqPlatformAdapter Telescope")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Telescope : ReferenceCountedObjectBase, ITelescopeV3
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        internal static string driverID = "ASCOM.EqPlatformAdapter.Telescope";
        // TODO Change the descriptive string for your driver then remove this line
        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        private static string driverDescription = "ASCOM Telescope Driver for Equatorial Platform Adapter.";

        internal static string traceStateProfileName = "Trace Level";
        internal static string traceStateDefault = "false";

        /// <summary>
        /// Private variable to hold the connected state
        /// </summary>
        private ASCOM.DriverAccess.Telescope m_mount;
        private ASCOM.DriverAccess.Camera m_camera;

        /// <summary>
        /// Private variable to hold an ASCOM Utilities object
        /// </summary>
        private Util utilities;

        /// <summary>
        /// Private variable to hold an ASCOM AstroUtilities object to provide the Range method
        /// </summary>
        private AstroUtils astroUtilities;
        private Transform transform;

        /// <summary>
        /// Variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        internal TraceLogger tl;

        private Platform Platform
        {
            get
            {
                return SharedResources.s_platform;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EqPlatformAdapter"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public Telescope()
        {
            driverID = Marshal.GenerateProgIdForType(this.GetType());

            tl = SharedResources.GetTraceLogger();

            ReadProfile(); // Read device configuration from the ASCOM Profile store

            tl.LogMessage("Telescope", "Starting initialisation");

            utilities = new Util(); //Initialise util object
            astroUtilities = new AstroUtils(); // Initialise astro utilities object
            transform = new Transform();

            tl.LogMessage("Telescope", "Completed initialisation");
        }


        //
        // PUBLIC COM INTERFACE ITelescopeV3 IMPLEMENTATION
        //

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            // consider only showing the setup dialog if not connected
            // or call a different dialog if connected
            if (IsConnected)
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");

            using (SetupDialogForm F = new SetupDialogForm(this))
            {
                var result = F.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    WriteProfile(); // Persist device configuration values to the ASCOM Profile store
                }
            }
        }

        public ArrayList SupportedActions
        {
            get
            {
                CheckConnected();
                return m_mount.SupportedActions;
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            CheckConnected();
            return m_mount.Action(actionName, actionParameters);
        }

        public void CommandBlind(string command, bool raw)
        {
            CheckConnected();
            m_mount.CommandBlind(command, raw);
        }

        public bool CommandBool(string command, bool raw)
        {
            CheckConnected();
            return m_mount.CommandBool(command, raw);
        }

        public string CommandString(string command, bool raw)
        {
            CheckConnected();
            return m_mount.CommandString(command, raw);
        }

        public void Dispose()
        {
            // Clean up the tracelogger and util objects
            if (m_mount != null)
            {
                SharedResources.DisconnectMount();
                m_mount = null;
                m_camera = null;
            }

            SharedResources.PutTraceLogger();
            tl = null;

            utilities.Dispose();
            utilities = null;
            astroUtilities.Dispose();
            astroUtilities = null;
            transform.Dispose();
            transform = null;
        }

        public bool Connected
        {
            get
            {
                LogMessage("Connected", "Get {0}", IsConnected);
                return IsConnected;
            }
            set
            {
                tl.LogMessage("Connected", "Set {0}", value);
                if (value == IsConnected)
                    return;

                if (value)
                {
                    m_mount = SharedResources.ConnectMount(out m_camera);
                }
                else
                {
                    SharedResources.DisconnectMount();
                    m_mount = null;
                }
            }
        }

        public string Description
        {
            get
            {
                return driverDescription;
            }
        }

        public string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverInfo = driverDescription + " Version: " + String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                return driverInfo;
            }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                return driverVersion;
            }
        }

        public short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                return Convert.ToInt16("3");
            }
        }

        public string Name
        {
            get
            {
                string name = "EQ Platform Adapter driver Telescope Device";
                return name;
            }
        }

        #endregion

        #region ITelescope Implementation
        public void AbortSlew()
        {
            CheckConnected();
            m_mount.AbortSlew();
        }

        public AlignmentModes AlignmentMode
        {
            get
            {
                CheckConnected();
                return m_mount.AlignmentMode;
            }
        }

        private void InitTransform()
        {
            transform.SiteElevation = m_mount.SiteElevation;
            transform.SiteLatitude = m_mount.SiteLatitude;
            transform.SiteLongitude = m_mount.SiteLongitude;
            transform.SetTopocentric(Platform.RightAscension, Platform.Declination);
        }

        public double Altitude
        {
            get
            {
                CheckConnected();
                if (Platform.IsTracking)
                {
                    InitTransform();
                    return transform.ElevationTopocentric;
                }
                else
                {
                    return m_mount.Altitude;
                }
            }
        }

        public double ApertureArea
        {
            get
            {
                CheckConnected();
                return m_mount.ApertureArea;
            }
        }

        public double ApertureDiameter
        {
            get
            {
                CheckConnected();
                return m_mount.ApertureDiameter;
            }
        }

        public bool AtHome
        {
            get
            {
                CheckConnected();
                return !Platform.IsTracking && m_mount.AtHome;
            }
        }

        public bool AtPark
        {
            get
            {
                CheckConnected();
                return !Platform.IsTracking && m_mount.AtPark;
            }
        }

        public IAxisRates AxisRates(TelescopeAxes Axis)
        {
            CheckConnected();
            // TODO: should this do anything different?
            return m_mount.AxisRates(Axis);
        }

        public double Azimuth
        {
            get
            {
                CheckConnected();
                if (Platform.IsTracking)
                {
                    InitTransform();
                    return transform.AzimuthTopocentric;
                }
                else
                {
                    return m_mount.Azimuth;
                }
            }
        }

        public bool CanFindHome
        {
            get
            {
                CheckConnected();
                return m_mount.CanFindHome;
            }
        }

        public bool CanMoveAxis(TelescopeAxes Axis)
        {
            CheckConnected();
            // TODO: should this return false when tracking?
            return m_mount.CanMoveAxis(Axis);
        }

        public bool CanPark
        {
            get
            {
                CheckConnected();
                return m_mount.CanPark;
            }
        }

        public bool CanPulseGuide
        {
            get
            {
                CheckConnected();
                return m_camera.CanPulseGuide;
            }
        }

        public bool CanSetDeclinationRate
        {
            get
            {
                CheckConnected();
                return m_mount.CanSetDeclinationRate;
            }
        }

        public bool CanSetGuideRates
        {
            get
            {
                return false;
            }
        }

        public bool CanSetPark
        {
            get
            {
                CheckConnected();
                return m_mount.CanSetPark;
            }
        }

        public bool CanSetPierSide
        {
            get
            {
                CheckConnected();
                return m_mount.CanSetPierSide;
            }
        }

        public bool CanSetRightAscensionRate
        {
            get
            {
                CheckConnected();
                return m_mount.CanSetRightAscensionRate;
            }
        }

        public bool CanSetTracking
        {
            get
            {
                CheckConnected();
                return m_mount.CanSetTracking;
            }
        }

        public bool CanSlew
        {
            get
            {
                CheckConnected();
                return m_mount.CanSlew;
            }
        }

        public bool CanSlewAltAz
        {
            get
            {
                CheckConnected();
                return m_mount.CanSlewAltAz;
            }
        }

        public bool CanSlewAltAzAsync
        {
            get
            {
                CheckConnected();
                return m_mount.CanSlewAltAzAsync;
            }
        }

        public bool CanSlewAsync
        {
            get
            {
                CheckConnected();
                return m_mount.CanSlewAsync;
            }
        }

        public bool CanSync
        {
            get
            {
                CheckConnected();
                return m_mount.CanSync;
            }
        }

        public bool CanSyncAltAz
        {
            get
            {
                CheckConnected();
                return m_mount.CanSyncAltAz;
            }
        }

        public bool CanUnpark
        {
            get
            {
                CheckConnected();
                return m_mount.CanUnpark;
            }
        }

        public double Declination
        {
            get
            {
                CheckConnected();
                return Platform.IsTracking ?
                    Platform.Declination :
                    m_mount.Declination;
            }
        }

        public double DeclinationRate
        {
            get
            {
                CheckConnected();
                return m_mount.DeclinationRate;
            }
            set
            {
                CheckConnected();
                m_mount.DeclinationRate = value;
            }
        }

        public PierSide DestinationSideOfPier(double RightAscension, double Declination)
        {
            CheckConnected();
            return m_mount.DestinationSideOfPier(RightAscension, Declination);
        }

        public bool DoesRefraction
        {
            get
            {
                CheckConnected();
                return m_mount.DoesRefraction;
            }
            set
            {
                CheckConnected();
                m_mount.DoesRefraction = value;
            }
        }

        public EquatorialCoordinateType EquatorialSystem
        {
            get
            {
                CheckConnected();
                EquatorialCoordinateType equatorialSystem = EquatorialCoordinateType.equLocalTopocentric;
                tl.LogMessage("DeclinationRate", "Get - " + equatorialSystem.ToString());
                return equatorialSystem;
            }
        }

        public void FindHome()
        {
            CheckConnected();
            tl.LogMessage("FindHome", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("FindHome");
        }

        public double FocalLength
        {
            get
            {
                CheckConnected();
                tl.LogMessage("FocalLength Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("FocalLength", false);
            }
        }

        public double GuideRateDeclination
        {
            get
            {
                CheckConnected();
                tl.LogMessage("GuideRateDeclination Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateDeclination", false);
            }
            set
            {
                CheckConnected();
                tl.LogMessage("GuideRateDeclination Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateDeclination", true);
            }
        }

        public double GuideRateRightAscension
        {
            get
            {
                CheckConnected();
                tl.LogMessage("GuideRateRightAscension Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateRightAscension", false);
            }
            set
            {
                CheckConnected();
                tl.LogMessage("GuideRateRightAscension Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateRightAscension", true);
            }
        }

        public bool IsPulseGuiding
        {
            get
            {
                CheckConnected();
                tl.LogMessage("IsPulseGuiding Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("IsPulseGuiding", false);
            }
        }

        public void MoveAxis(TelescopeAxes Axis, double Rate)
        {
            CheckConnected();
            tl.LogMessage("MoveAxis", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("MoveAxis");
        }

        public void Park()
        {
            CheckConnected();
            tl.LogMessage("Park", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("Park");
        }

        public void PulseGuide(GuideDirections Direction, int Duration)
        {
            CheckConnected();
            tl.LogMessage("PulseGuide", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("PulseGuide");
        }

        public double RightAscension
        {
            get
            {
                CheckConnected();
                return Platform.IsTracking ?
                    Platform.RightAscension :
                    m_mount.RightAscension;
            }
        }

        public double RightAscensionRate
        {
            get
            {
                CheckConnected();
                double rightAscensionRate = 0.0;
                tl.LogMessage("RightAscensionRate", "Get - " + rightAscensionRate.ToString());
                return rightAscensionRate;
            }
            set
            {
                CheckConnected();
                tl.LogMessage("RightAscensionRate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("RightAscensionRate", true);
            }
        }

        public void SetPark()
        {
            CheckConnected();
            tl.LogMessage("SetPark", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SetPark");
        }

        public PierSide SideOfPier
        {
            get
            {
                CheckConnected();
                tl.LogMessage("SideOfPier Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SideOfPier", false);
            }
            set
            {
                CheckConnected();
                tl.LogMessage("SideOfPier Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SideOfPier", true);
            }
        }

        public double SiderealTime
        {
            get
            {
                CheckConnected();
                return m_mount.SiderealTime;
            }
        }

        public double SiteElevation
        {
            get
            {
                CheckConnected();
                tl.LogMessage("SiteElevation Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteElevation", false);
            }
            set
            {
                CheckConnected();
                tl.LogMessage("SiteElevation Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteElevation", true);
            }
        }

        public double SiteLatitude
        {
            get
            {
                CheckConnected();
                tl.LogMessage("SiteLatitude Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteLatitude", false);
            }
            set
            {
                CheckConnected();
                tl.LogMessage("SiteLatitude Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteLatitude", true);
            }
        }

        public double SiteLongitude
        {
            get
            {
                CheckConnected();
                tl.LogMessage("SiteLongitude Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteLongitude", false);
            }
            set
            {
                CheckConnected();
                tl.LogMessage("SiteLongitude Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SiteLongitude", true);
            }
        }

        public short SlewSettleTime
        {
            get
            {
                CheckConnected();
                tl.LogMessage("SlewSettleTime Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SlewSettleTime", false);
            }
            set
            {
                CheckConnected();
                tl.LogMessage("SlewSettleTime Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("SlewSettleTime", true);
            }
        }

        public void SlewToAltAz(double Azimuth, double Altitude)
        {
            CheckConnected();
            tl.LogMessage("SlewToAltAz", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAz");
        }

        public void SlewToAltAzAsync(double Azimuth, double Altitude)
        {
            CheckConnected();
            tl.LogMessage("SlewToAltAzAsync", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToAltAzAsync");
        }

        public void SlewToCoordinates(double RightAscension, double Declination)
        {
            CheckConnected();
            tl.LogMessage("SlewToCoordinates", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToCoordinates");
        }

        public void SlewToCoordinatesAsync(double RightAscension, double Declination)
        {
            CheckConnected();
            tl.LogMessage("SlewToCoordinatesAsync", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToCoordinatesAsync");
        }

        public void SlewToTarget()
        {
            CheckConnected();
            tl.LogMessage("SlewToTarget", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToTarget");
        }

        public void SlewToTargetAsync()
        {
            CheckConnected();
            tl.LogMessage("SlewToTargetAsync", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SlewToTargetAsync");
        }

        public bool Slewing
        {
            get
            {
                CheckConnected();
                tl.LogMessage("Slewing Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Slewing", false);
            }
        }

        public void SyncToAltAz(double Azimuth, double Altitude)
        {
            CheckConnected();
            tl.LogMessage("SyncToAltAz", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SyncToAltAz");
        }

        public void SyncToCoordinates(double RightAscension, double Declination)
        {
            CheckConnected();
            tl.LogMessage("SyncToCoordinates", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SyncToCoordinates");
        }

        public void SyncToTarget()
        {
            CheckConnected();
            tl.LogMessage("SyncToTarget", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("SyncToTarget");
        }

        public double TargetDeclination
        {
            get
            {
                CheckConnected();
                tl.LogMessage("TargetDeclination Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("TargetDeclination", false);
            }
            set
            {
                CheckConnected();
                tl.LogMessage("TargetDeclination Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("TargetDeclination", true);
            }
        }

        public double TargetRightAscension
        {
            get
            {
                CheckConnected();
                tl.LogMessage("TargetRightAscension Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("TargetRightAscension", false);
            }
            set
            {
                CheckConnected();
                tl.LogMessage("TargetRightAscension Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("TargetRightAscension", true);
            }
        }

        public bool Tracking
        {
            get
            {
                CheckConnected();
                bool tracking = true;
                tl.LogMessage("Tracking", "Get - " + tracking.ToString());
                return tracking;
            }
            set
            {
                CheckConnected();
                tl.LogMessage("Tracking Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Tracking", true);
            }
        }

        public DriveRates TrackingRate
        {
            get
            {
                CheckConnected();
                tl.LogMessage("TrackingRate Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("TrackingRate", false);
            }
            set
            {
                CheckConnected();
                tl.LogMessage("TrackingRate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("TrackingRate", true);
            }
        }

        public ITrackingRates TrackingRates
        {
            get
            {
                CheckConnected();
                ITrackingRates trackingRates = new TrackingRates();
                tl.LogMessage("TrackingRates", "Get - ");
                foreach (DriveRates driveRate in trackingRates)
                {
                    tl.LogMessage("TrackingRates", "Get - " + driveRate.ToString());
                }
                return trackingRates;
            }
        }

        public DateTime UTCDate
        {
            get
            {
                CheckConnected();
                DateTime utcDate = DateTime.UtcNow;
                tl.LogMessage("TrackingRates", "Get - " + String.Format("MM/dd/yy HH:mm:ss", utcDate));
                return utcDate;
            }
            set
            {
                CheckConnected();
                tl.LogMessage("UTCDate Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("UTCDate", true);
            }
        }

        public void Unpark()
        {
            CheckConnected();
            tl.LogMessage("Unpark", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("Unpark");
        }

        #endregion

        #region Private properties and methods
        // here are some useful properties and methods that can be used as required
        // to help with driver development

        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private bool IsConnected
        {
            get
            {
                return m_mount != null;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        private void CheckConnected()
        {
            CheckConnected("Not connected");
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Telescope";
                tl.Enabled = Convert.ToBoolean(driverProfile.GetValue(driverID, traceStateProfileName, string.Empty, traceStateDefault));
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Telescope";
                driverProfile.WriteValue(driverID, traceStateProfileName, tl.Enabled.ToString());
            }
        }

        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        internal void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            tl.LogMessage(identifier, msg);
        }
        #endregion
    }
}
