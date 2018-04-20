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
#define Camera

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

using ASCOM;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;
using System.Globalization;
using System.Collections;

namespace ASCOM.EqPlatformAdapter
{
    //
    // Your driver's DeviceID is ASCOM.EqPlatformAdapter.Camera
    //
    // The Guid attribute sets the CLSID for ASCOM.EqPlatformAdapter.Camera
    // The ClassInterface/None addribute prevents an empty interface called
    // _EqPlatformAdapter from being created and used as the [default] interface
    //
    // TODO Replace the not implemented exceptions with code to implement the function or
    // throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM Camera Driver for EqPlatformAdapter.
    /// </summary>
    [Guid("24c02ae0-1b72-4aa6-8686-00fa5c07041b")]
    [ProgId("ASCOM.EqPlatformAdatper.Camera")]
    [ServedClassName("EqPlatformAdapter Camera")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Camera : ReferenceCountedObjectBase, ICameraV2, IDisposable
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        internal static string driverID = "ASCOM.EqPlatformAdapter.Camera";
        // TODO Change the descriptive string for your driver then remove this line
        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        private static string driverDescription = "ASCOM Camera Driver for Equatorial Platform Adapter.";

        /// <summary>
        /// Private variable to hold the connected state
        /// </summary>
        private ASCOM.DriverAccess.Camera m_camera;

        /// <summary>
        /// Variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        internal TraceLogger tl;

        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="EqPlatformAdapter"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public Camera()
        {
            driverID = Marshal.GenerateProgIdForType(this.GetType());

            tl = SharedResources.GetTraceLogger();
            tl.LogMessage("Camera", "Starting initialisation");
            tl.LogMessage("Camera", "Completed initialisation");
        }


        //
        // PUBLIC COM INTERFACE ICameraV2 IMPLEMENTATION
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
                return m_camera.SupportedActions;
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            CheckConnected();
            return m_camera.Action(actionName, actionParameters);
        }

        public void CommandBlind(string command, bool raw)
        {
            CheckConnected();
            m_camera.CommandBlind(command, raw);
        }

        public bool CommandBool(string command, bool raw)
        {
            CheckConnected();
            return m_camera.CommandBool(command, raw);
        }

        public string CommandString(string command, bool raw)
        {
            CheckConnected();
            return m_camera.CommandString(command, raw);
        }

        ~Camera()
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
                // free managed resources here
            }

            if (m_camera != null)
            {
                SharedResources.DisconnectCamera();
                m_camera = null;
            }

            SharedResources.PutTraceLogger();
            tl = null;

            disposed = true;
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
                    m_camera = SharedResources.ConnectCamera();
                }
                else
                {
                    SharedResources.DisconnectCamera();
                    m_camera = null;
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
                return Convert.ToInt16("2");
            }
        }

        public string Name
        {
            get
            {
                string name = "EQ Platform Adapter driver Camera Device";
                return name;
            }
        }

        #endregion

        #region ICamera Implementation

        public void AbortExposure()
        {
            if (IsConnected)
                m_camera.AbortExposure();
        }

        public short BayerOffsetX
        {
            get
            {
                CheckConnected();
                return m_camera.BayerOffsetX;
            }
        }

        public short BayerOffsetY
        {
            get
            {
                CheckConnected();
                return m_camera.BayerOffsetY;
            }
        }

        public short BinX
        {
            get
            {
                CheckConnected();
                return m_camera.BinX;
            }
            set
            {
                CheckConnected();
                m_camera.BinX = value;
            }
        }

        public short BinY
        {
            get
            {
                CheckConnected();
                return m_camera.BinY;
            }
            set
            {
                CheckConnected();
                m_camera.BinY = value;
            }
        }

        public double CCDTemperature
        {
            get
            {
                CheckConnected();
                return m_camera.CCDTemperature;
            }
        }

        public CameraStates CameraState
        {
            get
            {
                CheckConnected();
                return m_camera.CameraState;
            }
        }

        public int CameraXSize
        {
            get
            {
                CheckConnected();
                return m_camera.CameraXSize;
            }
        }

        public int CameraYSize
        {
            get
            {
                CheckConnected();
                return m_camera.CameraYSize;
            }
        }

        public bool CanAbortExposure
        {
            get
            {
                CheckConnected();
                return m_camera.CanAbortExposure;
            }
        }

        public bool CanAsymmetricBin
        {
            get
            {
                CheckConnected();
                return m_camera.CanAsymmetricBin;
            }
        }

        public bool CanFastReadout
        {
            get
            {
                CheckConnected();
                return m_camera.CanFastReadout;
            }
        }

        public bool CanGetCoolerPower
        {
            get
            {
                CheckConnected();
                return m_camera.CanGetCoolerPower;
            }
        }

        public bool CanPulseGuide
        {
            get
            {
                // explicitly return false .. we don't want any pulse guide commands delegated to the camera
                // interfering with the guide commands being issued via the Telescope device
                tl.LogMessage("CanPulseGuide Get", false.ToString());
                return false;
            }
        }

        public bool CanSetCCDTemperature
        {
            get
            {
                CheckConnected();
                return m_camera.CanSetCCDTemperature;
            }
        }

        public bool CanStopExposure
        {
            get
            {
                CheckConnected();
                return m_camera.CanStopExposure;
            }
        }

        public bool CoolerOn
        {
            get
            {
                CheckConnected();
                return m_camera.CoolerOn;
            }
            set
            {
                CheckConnected();
                m_camera.CoolerOn = value;
            }
        }

        public double CoolerPower
        {
            get
            {
                CheckConnected();
                return m_camera.CoolerPower;
            }
        }

        public double ElectronsPerADU
        {
            get
            {
                CheckConnected();
                return m_camera.ElectronsPerADU;
            }
        }

        public double ExposureMax
        {
            get
            {
                CheckConnected();
                return m_camera.ExposureMax;
            }
        }

        public double ExposureMin
        {
            get
            {
                CheckConnected();
                return m_camera.ExposureMin;
            }
        }

        public double ExposureResolution
        {
            get
            {
                CheckConnected();
                return m_camera.ExposureResolution;
            }
        }

        public bool FastReadout
        {
            get
            {
                CheckConnected();
                return m_camera.FastReadout;
            }
            set
            {
                CheckConnected();
                m_camera.FastReadout = value;
            }
        }

        public double FullWellCapacity
        {
            get
            {
                CheckConnected();
                return m_camera.FullWellCapacity;
            }
        }

        public short Gain
        {
            get
            {
                CheckConnected();
                return m_camera.Gain;
            }
            set
            {
                CheckConnected();
                m_camera.Gain = value;
            }
        }

        public short GainMax
        {
            get
            {
                CheckConnected();
                return m_camera.GainMax;
            }
        }

        public short GainMin
        {
            get
            {
                CheckConnected();
                return m_camera.GainMin;
            }
        }

        public ArrayList Gains
        {
            get
            {
                CheckConnected();
                return m_camera.Gains;
            }
        }

        public bool HasShutter
        {
            get
            {
                CheckConnected();
                return m_camera.HasShutter;
            }
        }

        public double HeatSinkTemperature
        {
            get
            {
                CheckConnected();
                return m_camera.HeatSinkTemperature;
            }
        }

        public object ImageArray
        {
            get
            {
                CheckConnected();
                return m_camera.ImageArray;
            }
        }

        public object ImageArrayVariant
        {
            get
            {
                CheckConnected();
                return m_camera.ImageArrayVariant;
            }
        }

        public bool ImageReady
        {
            get
            {
                CheckConnected();
                return m_camera.ImageReady;
            }
        }

        public bool IsPulseGuiding
        {
            get
            {
                // not implemented (no pulse guiding via the camera interface)
                tl.LogMessage("IsPulseGuiding Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("IsPulseGuiding", false);
            }
        }

        public double LastExposureDuration
        {
            get
            {
                CheckConnected();
                return m_camera.LastExposureDuration;
            }
        }

        public string LastExposureStartTime
        {
            get
            {
                CheckConnected();
                return m_camera.LastExposureStartTime;
            }
        }

        public int MaxADU
        {
            get
            {
                CheckConnected();
                return m_camera.MaxADU;
            }
        }

        public short MaxBinX
        {
            get
            {
                CheckConnected();
                return m_camera.MaxBinX;
            }
        }

        public short MaxBinY
        {
            get
            {
                CheckConnected();
                return m_camera.MaxBinY;
            }
        }

        public int NumX
        {
            get
            {
                CheckConnected();
                return m_camera.NumX;
            }
            set
            {
                CheckConnected();
                m_camera.NumX = value;
            }
        }

        public int NumY
        {
            get
            {
                CheckConnected();
                return m_camera.NumY;
            }
            set
            {
                CheckConnected();
                m_camera.NumY = value;
            }
        }

        public short PercentCompleted
        {
            get
            {
                CheckConnected();
                return m_camera.PercentCompleted;
            }
        }

        public double PixelSizeX
        {
            get
            {
                CheckConnected();
                return m_camera.PixelSizeX;
            }
        }

        public double PixelSizeY
        {
            get
            {
                CheckConnected();
                return m_camera.PixelSizeY;
            }
        }

        public void PulseGuide(GuideDirections Direction, int Duration)
        {
            tl.LogMessage("PulseGuide", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("PulseGuide");
        }

        public short ReadoutMode
        {
            get
            {
                CheckConnected();
                return m_camera.ReadoutMode;
            }
            set
            {
                CheckConnected();
                m_camera.ReadoutMode = value;
            }
        }

        public ArrayList ReadoutModes
        {
            get
            {
                CheckConnected();
                return m_camera.ReadoutModes;
            }
        }

        public string SensorName
        {
            get
            {
                CheckConnected();
                return m_camera.SensorName;
            }
        }

        public SensorType SensorType
        {
            get
            {
                CheckConnected();
                return m_camera.SensorType;
            }
        }

        public double SetCCDTemperature
        {
            get
            {
                CheckConnected();
                return m_camera.SetCCDTemperature;
            }
            set
            {
                CheckConnected();
                m_camera.SetCCDTemperature = value;
            }
        }

        public void StartExposure(double Duration, bool Light)
        {
            CheckConnected();
            m_camera.StartExposure(Duration, Light);
        }

        public int StartX
        {
            get
            {
                CheckConnected();
                return m_camera.StartX;
            }
            set
            {
                CheckConnected();
                m_camera.StartX = value;
            }
        }

        public int StartY
        {
            get
            {
                CheckConnected();
                return m_camera.StartY;
            }
            set
            {
                CheckConnected();
                m_camera.StartY = value;
            }
        }

        public void StopExposure()
        {
            CheckConnected();
            m_camera.StopExposure();
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
                return m_camera != null;
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
