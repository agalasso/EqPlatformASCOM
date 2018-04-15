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

        public void Park()
        {
            CheckConnected();
            if (Platform.IsTracking)
                throw new InvalidOperationException("cannot Park when platform is tracking");
            m_mount.Park();
        }

        private void TransformGuidePulse(double raAmt, double decAmt, out double st4raAmt, out double st4decAmt)
        {
            //  input parameters:
            //      raAmt
            //      decAmt
            //          pulse duration (milliseconds) from PHD2; one or the other of these will be zero
            //
            //  output:
            //      st4raAmt
            //      st4decAmt
            //          pulses durations (milliseconds) to send to the mount

            tl.LogMessage("TransformGuidePulse", String.Format("input raAmt={0:F1} ms decAmt={1:F1} ms", raAmt, decAmt));

            //Input Parameters
            double Pi = Math.PI;
            double DegtoRad = Pi / 180;
            double Aasc = 1;    //Right Ascension Step Angle (Deg)
            double Adec = 1;    //Declination Step Angle (Deg)

            double Lat = m_mount.SiteLatitude;      //Observation Site Latitude (Deg)
            double Lon = m_mount.SiteLongitude;     //Observation Site Longitude (Deg)
            double Odec = Platform.Declination;     //Object Declination (Deg)
            double Oasc = Platform.RightAscension;  //Object Right Ascension (Deg)

            InitTransform();
            double Oalt = transform.ElevationTopocentric;   //Object Altitude (Deg)
            double Oazm = transform.AzimuthTopocentric;     //Object Azimuth (Deg)

            double hourAngle = (m_mount.SiderealTime - Platform.RightAscension) % 24.0;
            double Olha = hourAngle;    //Object Local Hour (Deg)

            //Output Parameters
            double Altdc = 0; //Correct Dec Move Altitude
            double Azmdc = 0; //Correct Dec Move Azimuth
            double Altrc = 0; //Correct RA Move Altitude
            double Azmrc = 0; //Correct RA Move Azimuth

            double Naltd = 0; //Original Dec Move Altitude(Deg)
            double Caltd = 0; //Correct Dec Move Altitude(Deg)
            double Paltd = 0; //Adjusted Dec Move Altitude(Deg)
            double Kaltd = 0; //Dec Move Altitude Correction (Deg)

            double Nazmd = 0; //Original Dec Move Azimuth(Deg)
            double Cazmd = 0; //Correct Dec Move Azimuth (Deg)
            double Yaltd = 0; //Adjusted Dec Move Altitude(Deg)
            double Pazmd = 0; //Adjusted Dec Move Azimuth (Deg)
            double Kazmd = 0; //Dec Move Azimuth Correction (Deg)

            double Nlhar = 0; //Original RA Move Local Hour (Deg)
            double Naltr = 0; //Original RA Move Altitude (Deg)
            double Caltr = 0; //Correct RA Move Altitude(Deg)
            double Plhar = 0; //Adjusted RA Move Local Hour (Deg)
            double Paltr = 0; //Adjusted RA Move Altitude(Deg)
            double Kaltr = 0; //RA Move Altitude Correction (Deg)

            double Nazmr = 0; //Original RA Move Azimuth(Deg)
            double Cazmr = 0; //Correct RA Move Azimuth (Deg)
            double Xlhar = 0; //Adjusted Local Hour (Deg)
            double Xaltr = 0; //Adjusted RA Move Altitude(Deg)
            double Pazmr = 0; //Adjusted RA Move Azimuth (Deg)
            double Kazmr = 0; //RA Move Azimuth Correction (Deg)

            double Altr = 0; //Actual RA Move Altitude
            double Azmr = 0; //Actual Dec Move Altitude
            double Altd = 0; //Actual RA Move Azimuth
            double Azmd = 0; //Actual Dec Move Azimuth

            //Correct Platform Altitude and Azimuth Movements
            Altdc = Adec * Math.Cos(Oazm * Pi / 180);
            Azmdc = Aasc * Math.Cos((Oazm + 90) * Pi / 180);
            Altrc = Adec * Math.Sin(Oazm * Pi / 180);
            Azmrc = Aasc * Math.Sin((Oazm + 90) * Pi / 180);

            //Original, Correct, and Adjusted Declination Move Altitude (Deg)
            Naltd = (180 / Pi) * Math.Asin(Math.Sin((Lat + Adec) * Pi / 180) * Math.Sin(Odec * Pi / 180) + 
            Math.Cos((Lat + Adec) * Pi / 180) * Math.Cos(Odec * Pi / 180) * Math.Cos(Olha * Pi / 180));
            Caltd = Oalt + Altdc;
            Paltd = Oalt;
            if (Paltd < Caltd)
            {
                while (Paltd < Caltd + 0.0001)
                {
                    Paltd = (180 / Pi) * Math.Asin(Math.Sin((Lat + Kaltd * Adec) * Pi / 180) * Math.Sin(Odec * Pi / 180) + 
                    Math.Cos((Lat + Kaltd * Adec) * Pi / 180) * Math.Cos(Odec * Pi / 180) *  Math.Cos(Olha * Pi / 180));
                    Kaltd = Kaltd + 0.001;
                }
            }
            else
            {
                while (Paltd > Caltd - 0.0001)
                {
                    Paltd = (180 / Pi) * Math.Asin(Math.Sin((Lat + Kaltd * Adec) * Pi / 180) * Math.Sin(Odec * Pi / 180) + 
                    Math.Cos((Lat + Kaltd * Adec) * Pi / 180) * Math.Cos(Odec * Pi / 180) * Math.Cos(Olha * Pi / 180));
                    Kaltd = Kaltd - 0.001;
                }
            }

            //Original, Correct, and Adjusted Declination Move Azimuth (Deg)
            if (Olha > 12)
            {
                Nazmd = (180 / Pi) * Math.Acos((Math.Sin(Odec * Pi / 180) - Math.Sin((Lat + Adec) * Pi / 180) * Math.Sin(Naltd * Pi / 180)) /
                (Math.Cos((Lat + Adec) * Pi / 180) * Math.Cos(Naltd * Pi / 180)));
            }
            else
            {
                Nazmd = (180 / Pi) * Math.Acos((Math.Sin(Odec * Pi / 180) - Math.Sin((Lat + Adec) * Pi / 180) * Math.Sin(Naltd * Pi / 180)) /
                (Math.Cos((Lat + Adec) * Pi / 180) * Math.Cos(Naltd * Pi / 180)));
            }
            Cazmd = Oazm + Azmdc;
            Pazmd = Oazm;
            Yaltd = Oalt;
            if (Pazmd < Cazmd)
            {
                while (Pazmd < Cazmd + 0.0001)
                {
                    Yaltd = (180 / Pi) * Math.Asin(Math.Sin((Lat + Kazmd * Adec) * Pi / 180) * Math.Sin(Odec * Pi / 180) + 
                    Math.Cos((Lat + Kazmd * Adec) * Pi / 180) * Math.Cos(Odec * Pi / 180) * Math.Cos(Olha * Pi / 180));
                    if (Olha > 12)
                    {
                        Pazmd = (180 / Pi) * Math.Acos((Math.Sin(Odec * Pi / 180) - Math.Sin((Lat + Kazmd * Adec) * Pi / 180) * Math.Sin(Yaltd * Pi / 180)) / 
                        (Math.Cos((Lat + Kazmd * Adec) * Pi / 180) * Math.Cos(Yaltd * Pi / 180)));
                    }
                    else
                    {
                        Pazmd = (180 / Pi) * Math.Acos((Math.Sin(Odec * Pi / 180) - Math.Sin((Lat + Kazmd * Adec) * Pi / 180) * Math.Sin(Yaltd * Pi / 180)) / 
                        (Math.Cos((Lat + Kazmd * Adec) * Pi / 180) * Math.Cos(Yaltd * Pi / 180)));
                    }
                    Kazmd = Kazmd + 0.001;
                }
            }
            else
            {
                while (Pazmd > Cazmd - 0.0001)
                    {
                    Yaltd = (180 / Pi) * Math.Asin(Math.Sin((Lat + Kazmd * Adec) * Pi / 180) * Math.Sin(Odec * Pi / 180) + 
                    Math.Cos((Lat + Kazmd * Adec) * Pi / 180) * Math.Cos(Odec * Pi / 180) * Math.Cos(Olha * Pi / 180));
                    if (Olha > 12)
                    {
                        Pazmd = (180 / Pi) * Math.Acos((Math.Sin(Odec * Pi / 180) - Math.Sin((Lat + Kazmd * Adec) * Pi / 180) * Math.Sin(Yaltd * Pi / 180)) / 
                        (Math.Cos((Lat + Kazmd * Adec) * Pi / 180) * Math.Cos(Yaltd * Pi / 180)));
                    }
                    else
                    {
                        Pazmd = (180 / Pi) * Math.Acos((Math.Sin(Odec * Pi / 180) - Math.Sin((Lat + Kazmd * Adec) * Pi / 180) * Math.Sin(Yaltd * Pi / 180)) / 
                        (Math.Cos((Lat + Kazmd * Adec) * Pi / 180) * Math.Cos(Yaltd * Pi / 180)));
                    }
                    Kazmd = Kazmd - 0.001;
                }
            }

            //Original, Correct, and Adjusted Right Ascension Move Altitude(Deg)
            if (15 * Olha + Aasc < 360)
            {
                Nlhar = 15 * Olha + Aasc + 720;
            }
            else
            {
                if (15 * Olha + Aasc < 0)
                {
                    Nlhar = 15 * Olha + Aasc + 360;
                }
                else
                {
                    Nlhar = 15 * Olha + Aasc;
                }
            }
            Naltr = (180 / Pi) * Math.Asin(Math.Sin(Lat * Pi / 180) * Math.Sin(Odec * Pi / 180) + Math.Cos(Lat * Pi / 180) * 
            Math.Cos(Odec * Pi / 180) * Math.Cos(Nlhar * Pi / 180));
            Caltr = Oalt + Altrc;
            Paltr = Oalt;
            if (Paltr < Caltr)
            {
                while (Paltr < Caltr + 0.0001)
                {
                    if (15 * Olha + Kaltr * Aasc < 360)
                    {
                        Plhar = 15 * Olha + Kaltr * Aasc + 720;
                    }
                else
                {
                    if (15 * Olha + Kaltr * Aasc < 0)
                    {
                        Plhar = 15 * Olha + Kaltr * Aasc + 360;
                    }
                    else
                    {
                        Plhar = 15 * Olha + Kaltr * Aasc;
                    }
                }
                    Paltr = (180 / Pi) * Math.Asin(Math.Sin(Lat * Pi / 180) * Math.Sin(Odec * Pi / 180) + Math.Cos(Lat * Pi / 180) *
                    Math.Cos(Odec * Pi / 180) * Math.Cos(Plhar * Pi / 180));
                    Kaltr = Kaltr + 0.001;
                }
            }
            else
            {
                while (Paltr > Caltr - 0.0001)
                {
                    if (15 * Olha + Kaltr * Aasc < 360)
                    {
                        Plhar = 15 * Olha + Kaltr * Aasc + 720;
                    }
                    else
                    {
                        if (15 * Olha + Kaltr * Aasc < 0)
                        {
                            Plhar = 15 * Olha + Kaltr * Aasc + 360;
                        }
                        else
                        {
                            Plhar = 15 * Olha + Kaltr * Aasc;
                        }
                    }
                    Paltr = (180 / Pi) * Math.Asin(Math.Sin(Lat * Pi / 180) * Math.Sin(Odec * Pi / 180) + Math.Cos(Lat * Pi / 180) *
                    Math.Cos(Odec * Pi / 180) * Math.Cos(Plhar * Pi / 180));
                    Kaltr = Kaltr - 0.001;
                }
            }

            //Original, Correct, and Adjusted Right Ascension Move Azimuth (Deg)
            if (Olha > 12)
            {
                Nazmr = (180 / Pi) * Math.Acos((Math.Sin(Odec * Pi / 180) - Math.Sin(Lat * Pi / 180) * Math.Sin(Naltr * Pi / 180)) / 
                (Math.Cos(Lat * Pi / 180) * Math.Cos(Naltr * Pi / 180)));
            }
            else
            {
                Nazmr = (180 / Pi) * Math.Acos((Math.Sin(Odec * Pi / 180) - Math.Sin(Lat * Pi / 180) * Math.Sin(Naltr * Pi / 180)) / 
                (Math.Cos(Lat * Pi / 180) * Math.Cos(Naltr * Pi / 180)));
            }
            Cazmr = Oazm + Azmrc;
            Pazmr = Nazmr;
            if (Pazmr < Cazmr)
            {
                while (Pazmr < Cazmr + 0.0001)
                {
                    if (15 * Olha + Kazmr * Aasc < 360)
                    {
                        Xlhar = 15 * Olha + Kazmr * Aasc + 720;
                    }
                    else
                    {
                        if (15 * Olha + Kazmr * Aasc < 0)
                        {
                            Xlhar = 15 * Olha + Kazmr * Aasc + 360;
                        }
                        else
                        {
                            Xlhar = 15 * Olha + Kazmr * Aasc;
                        }
                    }
                    Xaltr = (180 / Pi) * Math.Asin(Math.Sin(Lat * Pi / 180) * Math.Sin(Odec * Pi / 180) + Math.Cos(Lat * Pi / 180) *
                    Math.Cos(Odec * Pi / 180) * Math.Cos(Xlhar * Pi / 180));
                    if (Xlhar > 12)
                    {
                        Pazmr = (180 / Pi) * Math.Acos((Math.Sin(Odec * Pi / 180) - Math.Sin(Lat * Pi / 180) * Math.Sin(Xaltr * Pi / 180)) /
                        (Math.Cos(Lat * Pi / 180) * Math.Cos(Xaltr * Pi / 180)));
                    }
                    else
                    {
                        Pazmr = (180 / Pi) * Math.Acos((Math.Sin(Odec * Pi / 180) - Math.Sin(Lat * Pi / 180) * Math.Sin(Xaltr * Pi /180)) / 
                        (Math.Cos(Lat * Pi / 180) * Math.Cos(Xaltr * Pi / 180)));
                    }
                    Kazmr = Kazmr + 0.001;
                }
            }
            else
            {
                while (Pazmr > Cazmr - 0.0001)
                {
                    if (15 * Olha + Kazmr * Aasc < 360)
                    {
                        Xlhar = 15 * Olha + Kazmr * Aasc + 720;
                    }
                    else
                    {
                        if (15 * Olha + Kazmr * Aasc < 0)
                        {
                            Xlhar = 15 * Olha + Kazmr * Aasc + 360;
                        }
                        else
                        {
                            Xlhar = 15 * Olha + Kazmr * Aasc;
                        }
                    }
                    Xaltr = (180 / Pi) * Math.Asin(Math.Sin(Lat * Pi / 180) * Math.Sin(Odec * Pi / 180) + Math.Cos(Lat * Pi / 180) *
                    Math.Cos(Odec * Pi / 180) * Math.Cos(Xlhar * Pi / 180));
                    if (Xlhar > 12)
                    {
                        Pazmr = (180 / Pi) * Math.Acos((Math.Sin(Odec * Pi / 180) - Math.Sin(Lat * Pi / 180) * Math.Sin(Xaltr * Pi / 180)) /
                        (Math.Cos(Lat * Pi / 180) * Math.Cos(Xaltr * Pi / 180)));
                    }
                    else
                    {
                        Pazmr = (180 / Pi) * Math.Acos((Math.Sin(Odec * Pi /180) - Math.Sin(Lat * Pi / 180) * Math.Sin(Xaltr * Pi / 180)) / 
                        (Math.Cos(Lat * Pi / 180) * Math.Cos(Xaltr * Pi / 180)));
                    }
                    Kazmr = Kazmr - 0.001;
                }
            }

            //Adjusted Platform Altitude and Azimuth Moves (Deg)

            Altr = Paltr - Oalt;
            Azmr = Pazmr - Oazm;
            Altd = Paltd - Oalt;
            Azmd = Pazmd - Oazm;

            //Equatorial Mount to Equatorial Platform Motion Commands
            st4raAmt = Kazmr * raAmt + Kazmd * decAmt;
            st4decAmt = Kaltr * raAmt + Kaltd * decAmt;

            tl.LogMessage("TransformGuidePulse", String.Format("output: st4raAmt={0:F1} ms st4decAmt={1:F1} ms", st4raAmt, st4decAmt));
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
