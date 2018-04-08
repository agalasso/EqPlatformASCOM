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
            using (SetupDialogForm F = new SetupDialogForm(this))
            {
                F.ShowDialog();
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

        private void InitTransformSite()
        {
            transform.SiteElevation = m_mount.SiteElevation;
            transform.SiteLatitude = m_mount.SiteLatitude;
            transform.SiteLongitude = m_mount.SiteLongitude;
        }

        private void InitTransform()
        {
            InitTransformSite();
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
                return m_mount.EquatorialSystem;
            }
        }

        public void FindHome()
        {
            CheckConnected();
            if (Platform.IsTracking)
                throw new InvalidOperationException("cannot FindHome when platform is tracking");
            m_mount.FindHome();
        }

        public double FocalLength
        {
            get
            {
                CheckConnected();
                return m_mount.FocalLength;
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
            // guide rate is determined by the platform, no way to set or get it
            get
            {
                tl.LogMessage("GuideRateRightAscension Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateRightAscension", false);
            }
            set
            {
                tl.LogMessage("GuideRateRightAscension Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("GuideRateRightAscension", true);
            }
        }

        public bool IsPulseGuiding
        {
            get
            {
                CheckConnected();
                return Platform.IsTracking ? m_camera.IsPulseGuiding : false;
            }
        }

        public void MoveAxis(TelescopeAxes Axis, double Rate)
        {
            CheckConnected();
            if (Platform.IsTracking)
                throw new InvalidOperationException("cannot MoveAxis when platform is tracking");
            m_mount.MoveAxis(Axis, Rate);
        }

        public void Park()
        {
            CheckConnected();
            if (Platform.IsTracking)
                throw new InvalidOperationException("cannot Park when platform is tracking");
            m_mount.Park();
        }

        private void TransformGuidePulse(double raAmt, double decAmt, out double st4raAmt, out double st4decAmt)
        {
            // TODO-Thomas - here is where the transformation code can go
            //
            //  input parameters:
            //      raAmt
            //      decAmt
            //          pulse duration (milliseconds) from PHD2; one or the other of these will be zero
            //
            //  output:
            //      st4raAmt
            //      st4decAmt
            //          pulses durations (milliseconds) to send to the mount

            // scope pointing context information you may need is available as:
            double ra = Platform.RightAscension;
            double dec = Platform.Declination;
            double latitude = m_mount.SiteLatitude;
            double longitude = m_mount.SiteLongitude;
            double lst = m_mount.SiderealTime; // local apparent sidereal time
            double hourAngle = (lst - ra * 24.0 / 360.0) % 24.0;
            InitTransform(); // call this before getting alt/az
            double altitude = transform.ElevationTopocentric;
            double azimuth = transform.AzimuthTopocentric;

            // identity transform
            st4raAmt = raAmt;
            st4decAmt = decAmt;
        }

        public void PulseGuide(GuideDirections Direction, int Duration)
        {
            CheckConnected();
            if (!Platform.IsTracking)
                throw new InvalidOperationException("cannot PulseGuide when platform is not tracking");

            double ra = 0.0, dec = 0.0;

            switch (Direction)
            {
                case GuideDirections.guideEast: ra = (double)Duration; break;
                case GuideDirections.guideWest: ra = (double)-Duration; break;
                case GuideDirections.guideNorth: dec = (double)Duration; break;
                case GuideDirections.guideSouth: dec = (double)-Duration; break;
            }

            double st4ra, st4dec;
            TransformGuidePulse(ra, dec, out st4ra, out st4dec);

            // issue the RA pulse

            GuideDirections dir;
            if (st4ra >= 0.0)
                dir = GuideDirections.guideEast;
            else
                dir = GuideDirections.guideWest;

            int dur = (int)(Math.Abs(st4ra) + 0.5);
            if (dur > 0)
            {
                m_camera.PulseGuide(dir, dur);
                System.Threading.Thread.Sleep(dur + 10);
                Stopwatch stopwatch = Stopwatch.StartNew();
                while (m_camera.IsPulseGuiding)
                {
                    if (stopwatch.ElapsedMilliseconds > 5000 + dur)
                        throw new ASCOM.DriverException("timed-out waiting for pulse guide to complete");
                    System.Threading.Thread.Sleep(10);
                }            
            }

            // issue the dec pulse

            if (st4dec >= 0.0)
                dir = GuideDirections.guideNorth;
            else
                dir = GuideDirections.guideSouth;

            dur = (int)(Math.Abs(st4dec) + 0.5);
            if (dur > 0)
            {
                m_camera.PulseGuide(dir, dur);
                // return right away
            }
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
                return m_mount.RightAscensionRate;
            }
            set
            {
                CheckConnected();
                m_mount.RightAscensionRate = value;
            }
        }

        public void SetPark()
        {
            CheckConnected();
            m_mount.SetPark();
        }

        public PierSide SideOfPier
        {
            get
            {
                CheckConnected();
                return m_mount.SideOfPier;
            }
            set
            {
                CheckConnected();
                m_mount.SideOfPier = value;
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
                return m_mount.SiteElevation;
            }
            set
            {
                CheckConnected();
                m_mount.SiteElevation = value;
            }
        }

        public double SiteLatitude
        {
            get
            {
                CheckConnected();
                return m_mount.SiteLatitude;
            }
            set
            {
                CheckConnected();
                m_mount.SiteLatitude = value;
            }
        }

        public double SiteLongitude
        {
            get
            {
                CheckConnected();
                return m_mount.SiteLongitude;
            }
            set
            {
                CheckConnected();
                m_mount.SiteLongitude = value;
            }
        }

        public short SlewSettleTime
        {
            get
            {
                CheckConnected();
                return m_mount.SlewSettleTime;
            }
            set
            {
                CheckConnected();
                m_mount.SlewSettleTime = value;
            }
        }

        public void SlewToAltAz(double Azimuth, double Altitude)
        {
            CheckConnected();
            if (Platform.IsTracking)
                throw new InvalidOperationException("cannot SlewToAltAz when platform is tracking");
            m_mount.SlewToAltAz(Azimuth, Altitude);
        }

        public void SlewToAltAzAsync(double Azimuth, double Altitude)
        {
            CheckConnected();
            if (Platform.IsTracking)
                throw new InvalidOperationException("cannot SlewToAltAzAsync when platform is tracking");
            m_mount.SlewToAltAzAsync(Azimuth, Altitude);
        }

        public void SlewToCoordinates(double RightAscension, double Declination)
        {
            CheckConnected();
            if (Platform.IsTracking)
                throw new InvalidOperationException("cannot SlewToCoordinates when platform is tracking");
            m_mount.SlewToCoordinates(RightAscension, Declination);
        }

        public void SlewToCoordinatesAsync(double RightAscension, double Declination)
        {
            CheckConnected();
            if (Platform.IsTracking)
                throw new InvalidOperationException("cannot SlewToCoordinatesAsync when platform is tracking");
            m_mount.SlewToCoordinatesAsync(RightAscension, Declination);
        }

        public void SlewToTarget()
        {
            CheckConnected();
            if (Platform.IsTracking)
                throw new InvalidOperationException("cannot SlewToTarget when platform is tracking");
            m_mount.SlewToTarget();
        }

        public void SlewToTargetAsync()
        {
            CheckConnected();
            if (Platform.IsTracking)
                throw new InvalidOperationException("cannot SlewToTargetAsync when platform is tracking");
            m_mount.SlewToTargetAsync();
        }

        public bool Slewing
        {
            get
            {
                CheckConnected();
                return m_mount.Slewing;
            }
        }

        public void SyncToAltAz(double Azimuth, double Altitude)
        {
            CheckConnected();
            if (Platform.IsTracking)
            {
                InitTransformSite();
                transform.SetAzimuthElevation(Azimuth, Altitude);
                Platform.SyncToCoordinates(transform.RATopocentric, transform.DECTopocentric);
            }
            else
                m_mount.SyncToAltAz(Azimuth, Altitude);
        }

        public void SyncToCoordinates(double RightAscension, double Declination)
        {
            if (Platform.IsTracking)
                Platform.SyncToCoordinates(RightAscension, Declination);
            else
                m_mount.SyncToCoordinates(RightAscension, Declination);
        }

        public void SyncToTarget()
        {
            CheckConnected();
            if (Platform.IsTracking)
                Platform.SyncToCoordinates(m_mount.TargetRightAscension, m_mount.TargetDeclination);
            else
                m_mount.SyncToTarget();
        }

        public double TargetDeclination
        {
            get
            {
                CheckConnected();
                return m_mount.TargetDeclination;
            }
            set
            {
                CheckConnected();
                m_mount.TargetDeclination = value;
            }
        }

        public double TargetRightAscension
        {
            get
            {
                CheckConnected();
                return m_mount.TargetRightAscension;
            }
            set
            {
                CheckConnected();
                m_mount.TargetRightAscension = value;
            }
        }

        public bool Tracking
        {
            get
            {
                CheckConnected();
                return Platform.IsTracking ? true : m_mount.Tracking;
            }
            set
            {
                CheckConnected();
                if (value)
                {
                    if (Platform.IsTracking || m_mount.Tracking)
                        return;
                    m_mount.Tracking = true;
                }
                else
                {
                    if (Platform.IsTracking)
                        Platform.StopTracking();
                    m_mount.Tracking = false;
                }
            }
        }

        public DriveRates TrackingRate
        {
            get
            {
                CheckConnected();
                return Platform.IsTracking ? DriveRates.driveSidereal : m_mount.TrackingRate;
            }
            set
            {
                CheckConnected();
                if (Platform.IsTracking)
                    throw new InvalidOperationException("cannot set TrackingRate when platform is tracking");
                m_mount.TrackingRate = value;
            }
        }

        public ITrackingRates TrackingRates
        {
            get
            {
                CheckConnected();
                return m_mount.TrackingRates; // guaranteed to include sidereal rate
            }
        }

        public DateTime UTCDate
        {
            get
            {
                CheckConnected();
                return m_mount.UTCDate;
            }
            set
            {
                CheckConnected();
                m_mount.UTCDate = value;
            }
        }

        public void Unpark()
        {
            CheckConnected();
            if (Platform.IsTracking)
                throw new InvalidOperationException("cannot Unpark when platform is tracking");
            m_mount.Unpark();
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
