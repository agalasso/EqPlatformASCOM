/*
 * This file is part of EqPlatform Adapter ASCOM Driver.
 * 
 * Copyright 2018 Andy Galasso <andy.galasso@gmail.com>
 * 
 *  EqPlatform Adapter ASCOM Driver is free software: you can redistribute it
 *  and/or modify it under the terms of the GNU Lesser General Public License
 *  as published by the Free Software Foundation, either version 3 of the License,
 *  or (at your option) any later version.
 *  
 *  EqPlatform Adapter ASCOM Driver is distributed in the hope that it will be
 *  useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General
 *  Public License for more details.
 *  
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with EqPlatform Adapter ASCOM Driver.  If not, see
 *  <http://www.gnu.org/licenses/>.
 */

// This is used to define code in the template that is specific to one class implementation
// unused code can be deleted and this definition removed.
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
    public class Telescope : ReferenceCountedObjectBase, ITelescopeV3, IDisposable
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

        private bool disposed = false;

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
            Server.MainForm.Activate();
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

        ~Telescope()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // cleanup managed resources (none to cleanup for now)
            }

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

            disposed = true;
        }

        public bool Connected
        {
            get
            {
                tl.LogMessage("Connected", String.Format("Get {0}", IsConnected));
                return IsConnected;
            }
            set
            {
                tl.LogMessage("Connected", String.Format("Set {0}", value));
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

        // another test line of code

        public void Park()
        {
            CheckConnected();
            if (Platform.IsTracking)
                throw new InvalidOperationException("cannot Park when platform is tracking");
            m_mount.Park();
        }

        // this is a test line of code that I added in Visual Studio

        private void TransformGuidePulse(double raAmt, double decAmt, out double st4raAmt, out double st4decAmt)
        {
            
            // Start of Motion Control Adapter

            //Input Parameters
            double Pi = Math.PI;
            double DegRad = Pi / 180;
            double RadDeg = 180 / Pi;

            double Adec = 0.020;                        //Declination Step Angle (Deg)
            double Aasc = 0.020;                        //Right Ascension Step Angle (Deg)
            double Aps = 11.000;                         //Platform Start Angle (Deg)
            double Tet = Platform.TrackingTimeElapsed;  //Elapsed Tracking Time (Sec)
            double Lat = m_mount.SiteLatitude;          //Observation Site Latitude (Deg)
            double Lon = m_mount.SiteLongitude;         //Observation Site Longitude (Deg)
            double Lmst = m_mount.SiderealTime;         //Local Mean Sidereal Time (Hour)
            double Malt = m_mount.Altitude;             //Scope Mount Altitude (Deg)
            double Mazm = m_mount.Azimuth;              //Scope Mount Azimuth (Deg)
            double Pdec = Platform.Declination;         //Platform Declination (Deg)
            double Pasc = Platform.RightAscension;      //Platform Right Ascension (Hour)

            InitTransform();
            double Oalt = transform.ElevationTopocentric;   //Object Initial Altitude (Deg)
            double Oazm = transform.AzimuthTopocentric;     //Object Initial Azimuth (Deg)

            //Output Parameters
            double Tpt = 0;     //Platform Tracking Time (Min)
            double Apt = 0;     //Platform Tilt Angle (Deg)
            double Gmst = 0;    //Greenwich Mean Sidereal Time (Hour)
            double Odec = 0;    //Object Declination (Deg)
            double Oasc = 0;    //Object Right Ascension (Hour)

            double Olha = 0;    //Object Actual Local Hour (Hour)
            double Slha = 0;    //Scope Initial Local Hour (Hour)
            double Salt = 0;    //Scope Initial Altitude (Deg)
            double Sazm = 0;    //Scope Initial Azimuth (Deg)

            double Plha = 0;    //Platform Initial Local Hour (Hour)
            double Palt = 0;    //Platform Initial Altitude (Deg)
            double Pazm = 0;    //Platform Initial Azimuth (Deg)

            double Slhad = 0;    //Scope Dec Move Local Hour (Hour)
            double Saltd = 0;    //Scope Dec Move Altitude (Deg)
            double Sazmd = 0;    //Scope Dec Move Azimuth (Deg)

            double Slhar = 0;   //Scope RA Move Local Hour (Hour)
            double Saltr = 0;   //Scope RA Move Altitude(Deg)
            double Sazmr = 0;   //Scope RA Move Azimuth (Deg)

            double Altdc = 0;   //Correct Dec Move Altitude Delta (Deg)
            double Azmdc = 0;   //Correct Dec Move Azimuth Delta (Deg)
            double Altrc = 0;   //Correct RA Move Altitude Delta (Deg)
            double Azmrc = 0;   //Correct RA Move Azimuth Delta (Deg)

            double Altd = 0;    //Actual Dec Move Azimuth Delta (Deg)
            double Azmd = 0;    //Actual Dec Move Azimuth Delta (Deg)
            double Altr = 0;    //Actual RA Move Altitude Delta (Deg)
            double Azmr = 0;    //Actual RA Move Azimuth Delta (Deg)

            double Dalt = 0;    //Dec Move Angle (Deg)
            double Dazm = 0;    //RA Move Angle (Deg) 

            double Kdd = 0;     //PHD Dec to EQP Dec Move Factor (Deg)
            double Kdr = 0;     //PHD Dec to EQP RA Move Factor (Deg)
            double Krd = 0;     //PHD RA to EQP Dec Move Factor (Deg)
            double Krr = 0;     //PHD RA to EQP RA Move Factor (Deg)

            double Kddx = 0;     //PHD Dec to EQP Dec Move Factor (Deg)
            double Kdrx = 0;     //PHD Dec to EQP RA Move Factor (Deg)
            double Krdx = 0;     //PHD RA to EQP Dec Move Factor (Deg)
            double Krrx = 0;     //PHD RA to EQP RA Move Factor (Deg)

            double Kmax = 1.00; //Maximum Correction Factor
            double Cmax = 2000; //Maximum Command Limit (mSec)
            double Gmax = 0;    //Maximum Guiding Calculation (mSec)

            //Platform Tilt Angle (Deg)
            Tpt = Tet / 60;
            Apt = Aps - Tpt * 15 / 60;

            //Greenwich Mean Sidereal Time (Hour)
            if (Lmst - Lon / 15 > 24)
            { Gmst = Lmst - Lon / 15 - 24; }
            else
            if (Lmst - Lon / 15 < 0)
            { Gmst = Lmst - Lon / 15 + 24; }
            else
            { Gmst = Lmst - Lon / 15; }

            //Object Declination (Deg)
            Odec = RadDeg * Math.Asin(Math.Cos(Oazm * DegRad) * Math.Cos(Lat * DegRad) * Math.Cos(Oalt * DegRad) + Math.Sin(Lat * DegRad) * Math.Sin(Oalt * DegRad));

            //Object Right Ascension (Deg)
            if (Oazm < 180)
            { Oasc = Lmst + (12 / Pi) * Math.Acos((Math.Sin(Oalt * DegRad) - Math.Sin(Lat * DegRad) * Math.Sin(Odec * DegRad)) / (Math.Cos(Lat * DegRad) * Math.Cos(Odec * DegRad))); }
            else
            { Oasc = Lmst - (12 / Pi) * Math.Acos((Math.Sin(Oalt * DegRad) - Math.Sin(Lat * DegRad) * Math.Sin(Odec * DegRad)) / (Math.Cos(Lat * DegRad) * Math.Cos(Odec * DegRad))); }
            if (Oasc > 24) Oasc = Oasc - 24;
            if (Oasc < 0) Oasc = Oasc + 24;

            //Object Actual Local Hour (Hour)
            if (Lmst - Oasc > 24)
            { Olha = Lmst - Oasc - 24; }
            else
                if (Lmst - Oasc < 0)
                { Olha = Lmst - Oasc + 24; }
                else
                { Olha = Lmst - Oasc; }

            //Platform Initial Local Hour, Altitude, Azimuth Angles (Deg)
            Plha = Lmst - Oasc + Apt / 15;
            if (Plha > 24)
            { Plha = Plha - 24; }
            if (Plha < 0)
            { Plha = Plha + 24; }

            Palt = RadDeg * Math.Asin(Math.Sin(Lat * DegRad) * Math.Sin(Odec * DegRad) + Math.Cos(Lat * DegRad) * Math.Cos(Odec * DegRad) * Math.Cos(15 * Plha * DegRad));

            if (Plha > 12)
            { Pazm = RadDeg * Math.Acos((Math.Sin(Odec * DegRad) - Math.Sin(Lat * DegRad) * Math.Sin(Palt * DegRad)) / (Math.Cos(Lat * DegRad) * Math.Cos(Palt * DegRad))); }
            else
            { Pazm = 360 - RadDeg * Math.Acos((Math.Sin(Odec * DegRad) - Math.Sin(Lat * DegRad) * Math.Sin(Palt * DegRad)) / (Math.Cos(Lat * DegRad) * Math.Cos(Palt * DegRad))); }

            //Scope Initial Local Hour, Altitude, Azimuth (Deg)
            Slha = Lmst - Oasc + Apt / 15;
            if (Slha > 24)
            { Slha = Slha - 24; }
            if (Slha < 0)
            { Slha = Slha + 24; }

            Salt = RadDeg * Math.Asin(Math.Sin(Lat * DegRad) * Math.Sin(Odec * DegRad) + Math.Cos(Lat * DegRad) * Math.Cos(Odec * DegRad) * Math.Cos(15 * Slha * DegRad));

            if (Slha > 12)
            { Sazm = RadDeg * Math.Acos((Math.Sin(Odec * DegRad) - Math.Sin(Lat * DegRad) * Math.Sin(Salt * DegRad)) / (Math.Cos(Lat * DegRad) * Math.Cos(Salt * DegRad))); }
            else
            { Sazm = 360 - RadDeg * Math.Acos((Math.Sin(Odec * DegRad) - Math.Sin(Lat * DegRad) * Math.Sin(Salt * DegRad)) / (Math.Cos(Lat * DegRad) * Math.Cos(Salt * DegRad))); }

            //Scope Declination Move Local Hour, Altitude, Azimuth Angles (Deg)
            Slhad = Slha;

            Saltd = RadDeg * Math.Asin(Math.Sin((Lat + Adec) * DegRad) * Math.Sin(Odec * DegRad) + Math.Cos((Lat + Adec) * DegRad) * Math.Cos(Odec * DegRad) * Math.Cos(15 * Slhad * DegRad));

            if (Slhad > 12)
            { Sazmd = RadDeg * Math.Acos((Math.Sin(Odec * DegRad) - Math.Sin((Lat + Adec) * DegRad) * Math.Sin(Saltd * DegRad)) / (Math.Cos((Lat + Adec) * DegRad) * Math.Cos(Saltd * DegRad))); }
            else
            { Sazmd = 360 - RadDeg * Math.Acos((Math.Sin(Odec * DegRad) - Math.Sin((Lat + Adec) * DegRad) * Math.Sin(Saltd * DegRad)) / (Math.Cos((Lat + Adec) * DegRad) * Math.Cos(Saltd * DegRad))); }

            //Scope Right Ascension Move Local Hour, Altitude, Azimuth Angles (Deg)
            Slhar = Lmst - Oasc + (Apt + Aasc) / 15;
            if (Slhar > 24)
            { Slhar = Slhar - 24; }
            if (Slhar < 0)
            { Slhar = Slhar + 24; }

            Saltr = RadDeg * Math.Asin(Math.Sin(Lat * DegRad) * Math.Sin(Odec * DegRad) + Math.Cos(Lat * DegRad) * Math.Cos(Odec * DegRad) * Math.Cos(15 * Slhar * DegRad));

            if (Slhar > 12)
            { Sazmr = RadDeg * Math.Acos((Math.Sin(Odec * DegRad) - Math.Sin(Lat * DegRad) * Math.Sin(Saltr * DegRad)) / (Math.Cos(Lat * DegRad) * Math.Cos(Saltr * DegRad))); }
            else
            { Sazmr = 360 - RadDeg * Math.Acos((Math.Sin(Odec * DegRad) - Math.Sin(Lat * DegRad) * Math.Sin(Saltr * DegRad)) / (Math.Cos(Lat * DegRad) * Math.Cos(Saltr * DegRad))); }
       
            //Correct Scope Altitude and Azimuth Movements
            Altdc = Adec * Math.Cos(Sazm * DegRad);
            Azmdc = Adec * Math.Sin(Sazm * DegRad);
            Altrc = Aasc * Math.Cos((Sazm - 90) * DegRad);
            Azmrc = Aasc * Math.Sin((Sazm - 90) * DegRad);

            //Actual Scope Altitude and Azimuth Moves (Deg)
            Altd = Saltd - Salt;
            Azmd = Sazmd - Sazm;
            Altr = Saltr - Salt;
            Azmr = Sazmr - Sazm;

            //Altitude and Azimuth Move Angles (Deg)
            if (Azmd == 0)
                Dalt = 270;
            if (Azmd > 0)
                Dalt = 180 - (180 / Pi) * Math.Atan(Altd / Azmd);
            else
                if ((180 / Pi) * Math.Atan(Altd / Azmd) > 0)
                Dalt = 360 - (180 / Pi) * Math.Atan(Altd / Azmd);
            else
                Dalt = -(180 / Pi) * Math.Atan(Altd / Azmd);
            if (Azmr == 0)
                Dazm = 270;
            if (Azmr > 0)
                Dazm = 180 - (180 / Pi) * Math.Atan(Altr / Azmr);
            else
                if ((180 / Pi) * Math.Atan(Altr / Azmr) > 0)
                Dazm = 360 - (180 / Pi) * Math.Atan(Altr / Azmr);
            else
                Dazm = -(180 / Pi) * Math.Atan(Altr / Azmr);

            //PHD to EQP Correction Factors
            if (Altd * Azmr - Azmd * Altr == 0)
                Kdd = 0;
            else
                Kdd = (Altdc * Azmr - Azmdc * Altr) / (Altd * Azmr - Azmd * Altr);
            if (Altr * Azmd - Azmr * Altd == 0)
                Kdr = 0;
            else
                Kdr = (Altdc * Azmd - Azmdc * Altd) / (Altr * Azmd - Azmr * Altd);
            if (Altd * Azmr - Azmd * Altr == 0)
                Krd = 0;
            else
                Krd = (Altrc * Azmr - Azmrc * Altr) / (Altd * Azmr - Azmd * Altr);
            if (Altr * Azmd - Azmr * Altd == 0)
                Krr = 0;
            else
                Krr = (Altrc * Azmd - Azmrc * Altd) / (Altr * Azmd - Azmr * Altd);

            //Limit Correction Factors
            Kddx = Kdd * Math.Min(1, Math.Min(Kmax / Math.Abs(Kdd), Kmax / Math.Abs(Kdr)));
            Kdrx = Kdr * Math.Min(1, Math.Min(Kmax / Math.Abs(Kdd), Kmax / Math.Abs(Kdr)));
            Krdx = Krd * Math.Min(1, Math.Min(Kmax / Math.Abs(Krd), Kmax / Math.Abs(Krr)));
            Krrx = Krr * Math.Min(1, Math.Min(Kmax / Math.Abs(Krd), Kmax / Math.Abs(Krr)));

            st4decAmt = Kddx * decAmt + Krdx * raAmt;
            st4raAmt = Kdrx * decAmt + Krrx * raAmt;
            
            //Limit Command Levels
            if (Math.Abs(st4decAmt) > Math.Abs(st4raAmt))
            {
                if (Math.Abs(st4decAmt) > Cmax)
                    Gmax = Cmax;
                else
                    Gmax = Math.Abs(st4decAmt);
                if (st4decAmt > Gmax)
                { st4raAmt = Gmax * st4raAmt / st4decAmt; st4decAmt = Gmax; }
                if (st4decAmt < -Gmax)
                { st4raAmt = -Gmax * st4raAmt / st4decAmt; st4decAmt = -Gmax; }
            }
            else
            {
                if (Math.Abs(st4raAmt) > Cmax)
                    Gmax = Cmax;
                else
                    Gmax = Math.Abs(st4raAmt);
                if (st4raAmt > Gmax)
                { st4decAmt = Gmax * st4decAmt / st4raAmt; st4raAmt = Gmax; }
                if (st4raAmt < -Gmax)
                { st4decAmt = -Gmax * st4decAmt / st4raAmt; st4raAmt = -Gmax; }
            }

            //Log Parameters
            tl.LogMessage("TransformGuidePulse", String.Format("log: Lmst={0:F3} hour Apt={1:F3} deg Tet={2:F3} sec", Lmst, Apt, Tet));
            tl.LogMessage("TransformGuidePulse", String.Format("log: Pdec={0:F3} deg Pasc={1:F3} hour Odec={2:F3} deg Oasc={3:F3} hour", Pdec, Pasc, Odec, Oasc));
            tl.LogMessage("TransformGuidePulse", String.Format("log: Malt={0:F3} deg Mazm={1:F3} deg Oalt={2:F3} deg Oazm={3:F3} deg", Malt, Mazm, Oalt, Oazm));
            tl.LogMessage("TransformGuidePulse", String.Format("log: Plha={0:F3} hour Slha={1:F3} hour Slhad={2:F3} hour Slhar={3:F3} hour", Plha, Slha, Slhad, Slhar));
            tl.LogMessage("TransformGuidePulse", String.Format("log: Palt={0:F3} deg Salt={1:F3} deg Saltd={2:F3} deg Saltr={3:F3} deg", Palt, Salt, Saltd, Saltr));
            tl.LogMessage("TransformGuidePulse", String.Format("log: Pazm={0:F3} deg Sazm={1:F3} deg Sazmd={2:F3} deg Sazmr={3:F3} deg", Pazm, Sazm, Sazmd, Sazmr));
            tl.LogMessage("TransformGuidePulse", String.Format("log: Altdc={0:F4} hour Azmdc={1:F4} Altrc={2:F4} Azmrc={3:F4}", Altdc, Azmdc, Altrc, Azmrc));
            tl.LogMessage("TransformGuidePulse", String.Format("log: Altd={0:F4} hour Azmd={1:F4} Altr={2:F4} Azmr={3:F4}", Altd, Azmd, Altr, Azmr));
            tl.LogMessage("TransformGuidePulse", String.Format("log: Kdd={0:F3} Kdr={1:F3} Krd={2:F3} Krr={3:F3}", Kdd, Kdr, Krd, Krr));
            tl.LogMessage("TransformGuidePulse", String.Format("log: Kddx={0:F3} Kdrx={1:F3} Krdx={2:F3} Krrx={3:F3}", Kddx, Kdrx, Krdx, Krrx));
            tl.LogMessage("TransformGuidePulse", String.Format("log: decAmt={0:F0} ms raAmt={1:F0} ms st4decAmt={2:F0} ms st4raAmt ={3:F0} ms", decAmt, raAmt, st4decAmt, st4raAmt));

            //End of Motion Control Adapter
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
                tl.LogMessage("TransformGuidePulse", String.Format("log: dir={0} dur={1} ms Tet={2:F3} sec", dir, dur, Platform.TrackingTimeElapsed));
                System.Threading.Thread.Sleep(dur + 10);
                Stopwatch stopwatch = Stopwatch.StartNew();
                while (m_camera.IsPulseGuiding)
                {
                    if (stopwatch.ElapsedMilliseconds > 5000 + dur)
                        throw new ASCOM.DriverException("timed-out waiting for pulse guide to complete");
                    System.Threading.Thread.Sleep(10);
                }
                tl.LogMessage("TransformGuidePulse", String.Format("log: dir={0} dur={1} ms Tet={2:F3} sec", dir, dur, Platform.TrackingTimeElapsed));
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
                tl.LogMessage("TransformGuidePulse", String.Format("log: dir={0} dur={1} ms Tet={2:F3} sec", dir, dur, Platform.TrackingTimeElapsed));
                System.Threading.Thread.Sleep(dur + 10);
                Stopwatch stopwatch = Stopwatch.StartNew();
                while (m_camera.IsPulseGuiding)
                {
                    if (stopwatch.ElapsedMilliseconds > 5000 + dur)
                        throw new ASCOM.DriverException("timed-out waiting for pulse guide to complete");
                    System.Threading.Thread.Sleep(10);
                }
                tl.LogMessage("TransformGuidePulse", String.Format("log: dir={0} dur={1} ms Tet={2:F3} sec", dir, dur, Platform.TrackingTimeElapsed));
            }
        
        // issue the resulting RA and Dec pulses by interleaving

            //GuideDirections ra_dir = st4ra >= 0.0 ? GuideDirections.guideEast : GuideDirections.guideWest;
            //GuideDirections dec_dir = st4dec >= 0.0 ? GuideDirections.guideNorth : GuideDirections.guideSouth;

            //int ra_tot = (int)(Math.Abs(st4ra) + 0.5);
            //int dec_tot = (int)(Math.Abs(st4dec) + 0.5);

            //const int nominal_pulse_ms = 500; // nominal pulse duration    TODO: make configurable?

            //int steps = Math.Max(1, Math.Min(ra_tot, dec_tot) / nominal_pulse_ms);

            //int ra_step = (ra_tot + steps - 1) / steps;
            //int dec_step = (dec_tot + steps - 1) / steps;
            //tl.LogMessage("Output", String.Format("log: ra_tot={0:F0} ms dec_tot={1:F0} ms ra_step={2:F0} ms dec_step={3:F0} ms", ra_tot, dec_tot, ra_step, dec_step));
            //while (ra_tot > 0 || dec_tot > 0)
            //{
                //{
                    //int ra_dur = Math.Min(ra_step, ra_tot);
                    //ra_tot -= ra_dur;

                    //if (ra_dur > 0)
                    //{
                        //m_camera.PulseGuide(ra_dir, ra_dur);
                        //tl.LogMessage("Output", String.Format("log: ra_dir={0:F0} ra_dur={1:F0} ms", ra_dir, ra_dur));
                        //System.Threading.Thread.Sleep(ra_dur + 10);
                        //Stopwatch stopwatch = Stopwatch.StartNew();
                        //while (m_camera.IsPulseGuiding)
                        //{
                            //if (stopwatch.ElapsedMilliseconds > 5000 + ra_dur)
                                //throw new ASCOM.DriverException("timed-out waiting for pulse guide to complete");
                            //System.Threading.Thread.Sleep(10);
                        //}
                    //}
                //}

                //{
                    //int dec_dur = Math.Min(dec_step, dec_tot);
                    //dec_tot -= dec_dur;

                    //if (dec_dur > 0)
                    //{
                        //m_camera.PulseGuide(dec_dir, dec_dur);
                        //tl.LogMessage("Output", String.Format("log: dec_dir={0:F0} dec_dur={1:F0} ms", dec_dir, dec_dur));
                        //System.Threading.Thread.Sleep(dec_dur + 10);
                        //Stopwatch stopwatch = Stopwatch.StartNew();
                        //while (m_camera.IsPulseGuiding)
                        //{
                            //if (stopwatch.ElapsedMilliseconds > 5000 + dec_dur)
                                //throw new ASCOM.DriverException("timed-out waiting for pulse guide to complete");
                            //System.Threading.Thread.Sleep(10);
                        //}
                    //}
                //}
            //}
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
