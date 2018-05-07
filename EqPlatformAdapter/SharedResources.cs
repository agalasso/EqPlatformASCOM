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
//
// ================
// Shared Resources
// ================
//
// This class is a container for all shared resources that may be needed
// by the drivers served by the Local Server. 
//
using System;
using System.Diagnostics;
using System.Text;
using ASCOM;

namespace ASCOM.EqPlatformAdapter
{
    public enum TrackingStates
    {
        AtStart,
        Tracking,
        Stopped
    }

    internal class Settings : IDisposable
    {
        private ASCOM.Utilities.Profile profile = new ASCOM.Utilities.Profile();

        internal Settings()
        {
            profile.DeviceType = "Telescope";
        }

        public void Dispose()
        {
            if (profile != null)
            {
                profile.Dispose();
                profile = null;
            }
            GC.SuppressFinalize(this);
        }

        public string Get(string name, string defaultval)
        {
            return profile.GetValue(SharedResources.ScopeDriverId, name, string.Empty, defaultval);
        }

        public string Get(string name)
        {
            return Get(name, String.Empty);
        }

        public void Set(string name, string val)
        {
            profile.WriteValue(SharedResources.ScopeDriverId, name, val);
        }
    }

    public class Platform
    {
        private static double SIDEREAL_DAY_SECONDS = (23.0 * 60.0 + 56.0) * 60.0 + 4.0905;
        private static double SIDEREAL_DEGREES_PER_SECOND = 360.0 / SIDEREAL_DAY_SECONDS;

        private TrackingStates m_state;
        private double m_stroke; // full stroke time in seconds

        // coordinates at the time tracking started
        double m_ra;
        double m_dec;
        internal Stopwatch m_swatch; // elapsed tracking time

        internal Platform()
        {
            m_swatch = new Stopwatch();

            Init();
        }

        internal string Dump()
        {
            return this.IsTracking ?
                String.Format("Platform state = {0} RA = {1:F4} Dec = {2:F3} Elapsed = {3:F1} Rem = {4:F1}",
                    m_state.ToString(),
                    m_ra, m_dec,
                    this.TrackingTimeElapsed, this.TimeRemaining) :
                String.Format("Platform state = {0} RA = ? Dec = ? Elapsed = {1:F1} Rem = {2:F1}",
                    m_state.ToString(),
                    this.TrackingTimeElapsed, this.TimeRemaining);
        }

        public void Init()
        {
            m_state = TrackingStates.AtStart;

            using (Settings settings = new Settings())
            {
                string s = settings.Get("strokeDegrees", "22.0");
                double val = 22.0;
                Double.TryParse(s, out val);
                StrokeDegrees = val;
            }

            m_swatch.Reset();
        }

        public TrackingStates TrackingState
        {
            get
            {
                return m_state;
            }
        }

        public bool IsTracking
        {
            get
            {
                return m_state == TrackingStates.Tracking;
            }
        }

        public double RightAscension
        {
            get
            {
                Debug.Assert(IsTracking);
                return m_ra;
            }
        }

        public double Declination
        {
            get
            {
                Debug.Assert(IsTracking);
                return m_dec;
            }
        }

        public void SyncToCoordinates(double ra, double dec)
        {
            Debug.Assert(IsTracking);
            m_ra = ra;
            m_dec = dec;
        }

        public double StrokeSeconds
        {
            get
            {
                return m_stroke;
            }
        }

        public double StrokeDegrees
        {
            get
            {
                return m_stroke * SIDEREAL_DEGREES_PER_SECOND;
            }
            set
            {
                m_stroke = value / SIDEREAL_DEGREES_PER_SECOND;
            }
        }

        public double TrackingTimeElapsed
        {
            get
            {
                return m_swatch.ElapsedMilliseconds / 1000.0;
            }
        }

        public double TimeRemaining
        {
            get
            {
                double rem = m_stroke - m_swatch.ElapsedMilliseconds / 1000.0;
                return rem > 0.0 ? rem : 0.0;
            }
        }

        private void DoStart()
        {
            // grab the starting coordinates
            m_ra = SharedResources.s_mount.RightAscension;
            m_dec = SharedResources.s_mount.Declination;

            // stop mount tracking
            SharedResources.s_mount.Tracking = false;

            // start platform tracking
            if (SharedResources.s_switch != null)
                SharedResources.s_switch.SetSwitch(SharedResources.s_switchIdx, true);

            // start the tracking timer
            m_swatch.Start();

            m_state = TrackingStates.Tracking;
        }

        public void StartTracking()
        {
            if (TrackingState != TrackingStates.AtStart)
                throw new ASCOM.InvalidOperationException("cannot start tracking unless platform is at start position");

            DoStart();

            Server.MainForm.UpdateState();
        }

        public void StopTracking()
        {
            if (TrackingState != TrackingStates.Tracking)
                return;

            // stop platform tracking
            if (SharedResources.s_switch != null)
                SharedResources.s_switch.SetSwitch(SharedResources.s_switchIdx, false);

            // stop the tracking timer
            m_swatch.Stop();

            // start mount tracking
            SharedResources.s_mount.Tracking = true;

            // assume RA and Dec stay fixed during equatorial tracking, sync mount to starting RA/Dec
            SharedResources.s_mount.SyncToCoordinates(m_ra, m_dec);

            m_state = TrackingStates.Stopped;

            Server.MainForm.UpdateState();
        }

        public void ResumeTracking()
        {
            if (TrackingState != TrackingStates.Stopped)
                throw new ASCOM.InvalidOperationException("cannot resume tracking unless platform is paused");

            DoStart();

            Server.MainForm.UpdateState();
        }

        private static double norm(double val, double start, double end)
        {
            double range = end - start;
            double ofs = val - start;
            return val - Math.Floor(ofs / range) * range;
        }

        private static double norm_ra(double val)
        {
            return norm(val, 0.0, 24.0);
        }

        public void Reset()
        {
            if (m_state == TrackingStates.AtStart)
                return;

            double ra, dec;

            if (m_state == TrackingStates.Stopped)
            {
                // mount is tracking, grab RA and dec from the mount
                ra = SharedResources.s_mount.RightAscension;
                dec = SharedResources.s_mount.Declination;
            }
            else
            {
                // platform is tracking, use the saved coordinates
                ra = m_ra;
                dec = m_dec;

                // stop platform tracking
                if (SharedResources.s_switch != null)
                    SharedResources.s_switch.SetSwitch(SharedResources.s_switchIdx, false);

                // start mount tracking
                SharedResources.s_mount.Tracking = true;
            }

            m_swatch.Stop();

            double delta_ra = m_swatch.ElapsedMilliseconds / 1000.0 * SIDEREAL_DEGREES_PER_SECOND * 24.0 / 360.0;

            SharedResources.s_mount.SyncToCoordinates(norm_ra(ra - delta_ra), dec);

            // reset the tracking timer
            m_swatch.Reset();

            m_state = TrackingStates.AtStart;

            Server.MainForm.UpdateState();
        }
    }

    /// <summary>
    /// The resources shared by all drivers and devices, in this example it's a serial port with a shared SendMessage method
    /// an idea for locking the message and handling connecting is given.
    /// In reality extensive changes will probably be needed.
    /// Multiple drivers means that several applications connect to the same hardware device, aka a hub.
    /// Multiple devices means that there are more than one instance of the hardware, such as two focusers.
    /// In this case there needs to be multiple instances of the hardware connector, each with it's own connection count.
    /// </summary>
    public static class SharedResources
    {
        public static string CamDriverId = "ASCOM.EqPlatformAdatper.Camera"; // fixme - get this from driver
        public static string ScopeDriverId = "ASCOM.EqPlatformAdatper.Telescope"; // fixme - get this from driver

        // object used for locking to prevent multiple drivers accessing common code at the same time
        private static readonly object lockObject = new object();

        static internal ASCOM.DriverAccess.Camera s_camera;
        static internal ASCOM.DriverAccess.Telescope s_mount;
        static internal ASCOM.DriverAccess.Switch s_switch;
        static internal short s_switchIdx;

        static public readonly Platform s_platform = new Platform();

        static ASCOM.Utilities.TraceLogger s_tl;
        static uint s_tl_cnt;

        static uint s_cam_connections; // number of connections to (outer) Camera device
        static uint s_mount_connections; // number of connections to (outer) Telescope device

        private static uint CamUseCnt
        {
            get
            {
                // camera and mount connections both reference the camera
                return s_cam_connections + s_mount_connections;
            }
        }

        private static uint MountUseCnt
        {
            get
            {
                return s_mount_connections;
            }
        }

        public static ASCOM.Utilities.TraceLogger GetTraceLogger()
        {
            lock (lockObject)
            {
                if (s_tl == null)
                {
                    s_tl = new ASCOM.Utilities.TraceLogger("", "EqPlatformAdapter");

                    using (Settings settings = new Settings())
                    {
                        string s = settings.Get("traceOn", false.ToString());
                        bool trace = false;
                        Boolean.TryParse(s, out trace);
                        s_tl.Enabled = trace;
                    }
                }
                ++s_tl_cnt;
                return s_tl;
            }
        }

        public static void PutTraceLogger()
        {
            lock (lockObject)
            {
                if (--s_tl_cnt == 0)
                {
                    s_tl.Enabled = false;
                    s_tl.Dispose();
                    s_tl = null;
                }
            }
        }

        public static void EnableLogging(bool val)
        {
            lock (lockObject)
            {
                if (s_tl != null && s_tl.Enabled != val)
                {
                    if (!val)
                        s_tl.LogMessage("Server", "Logging disabled");

                    s_tl.Enabled = val;

                    if (val)
                        s_tl.LogMessage("Server", "Logging enabled");
                }
            }

            using (Settings settings = new Settings())
            {
                settings.Set("traceOn", val.ToString());
            }
        }

        private static string GetCamDriverId()
        {
            using (Settings settings = new Settings())
            {
                return settings.Get("cameraId");
            }
        }

        private static void InstantiateCamera()
        {
            string cameraDriverId = GetCamDriverId();
            if (cameraDriverId.Length == 0)
                throw new InvalidOperationException("Missing camera selection");
            s_camera = new ASCOM.DriverAccess.Camera(cameraDriverId);
        }

        public static void FreeCamera()
        {
            lock (lockObject)
            {
                Debug.Assert(CamUseCnt == 0);
                if (s_camera != null)
                {
                    s_camera.Dispose();
                    s_camera = null;
                }
            }
        }

        private static void ConnectCameraInner()
        {
            s_camera.Connected = true;
            try
            {
                if (!s_camera.CanPulseGuide)
                    throw new ASCOM.DriverException("The selected camera does not support pulse guiding");
            }
            catch (Exception)
            {
                try
                {
                    s_camera.Connected = false;
                }
                catch (Exception)
                {
                    // ignore
                }
                s_camera.Dispose();
                s_camera = null;
                throw;
            }
        }

        public static ASCOM.DriverAccess.Camera ConnectCamera()
        {
            bool updateForm = false;

            lock (lockObject)
            {
                if (s_cam_connections > 0)
                    throw new ASCOM.DriverException("multiple camera connections not allowed");

                if (s_camera == null)
                    InstantiateCamera();

                if (CamUseCnt == 0)
                {
                    ConnectCameraInner();
                    updateForm = true;
                }

                ++s_cam_connections;
            }

            if (updateForm)
                Server.MainForm.UpdateState();

            return s_camera;
        }

        public static bool CameraConnected
        {
            get
            {
                lock (lockObject)
                {
                    return s_cam_connections > 0;
                }
            }
        }

        private static void DisconnectCameraInner()
        {
            try
            {
                s_camera.Connected = false;
            }
            catch (Exception)
            {
                // ignore it
            }
        }

        public static void DisconnectCamera()
        {
            lock (lockObject)
            {
                if (s_cam_connections == 0)
                    throw new InvalidOperationException("disconnecting camera when not connected");

                if (--s_cam_connections == 0 && CamUseCnt == 0)
                    DisconnectCameraInner();
            }

            Server.MainForm.UpdateState();
        }

        private static void GetMountDriverIds(out string mountDriverId, out string switchDriverId, out short switchIdx)
        {
            using (Settings settings = new Settings())
            {
                string val = settings.Get("scopeId");
                if (String.IsNullOrEmpty(val))
                    throw new ASCOM.DriverException("Missing ASCOM Telescope Device selection");
                mountDriverId = val;

                val = settings.Get("switchDriverId");
                switchDriverId = val;

                val = settings.Get("switchId");
                switchIdx = 0;
                Int16.TryParse(val, out switchIdx);
            }
        }

        private static void InstantiateMount()
        {
            string mountDriverId, switchDriverId;
            short switchIdx;
            GetMountDriverIds(out mountDriverId, out switchDriverId, out switchIdx);

            if (mountDriverId.Length == 0)
                throw new InvalidOperationException("Missing mount selection");

            ASCOM.DriverAccess.Telescope mount = null;
            ASCOM.DriverAccess.Switch sw = null;
            try
            {
                mount = new ASCOM.DriverAccess.Telescope(mountDriverId);
                if (switchDriverId.Length > 0)
                {
                    sw = new ASCOM.DriverAccess.Switch(switchDriverId);
                }
            }
            catch (Exception)
            {
                mount.Dispose();
                if (sw != null)
                    sw.Dispose();
                throw;
            }

            s_mount = mount;
            s_switch = sw;
            s_switchIdx = switchIdx;
        }

        public static void FreeMount()
        {
            lock (lockObject)
            {
                Debug.Assert(MountUseCnt == 0);

                if (s_mount != null)
                {
                    s_mount.Dispose();
                    s_mount = null;
                }

                if (s_switch != null)
                {
                    s_switch.Dispose();
                    s_switch = null;
                }
            }
        }

        static void ConnectMountInner()
        {
            s_mount.Connected = true;
            try
            {
                if (s_switch != null)
                    s_switch.Connected = true;
            }
            catch (Exception)
            {
                try
                {
                    s_mount.Connected = false;
                }
                catch (Exception)
                {
                    // ignore it
                }
                throw;
            }
        }

        public static ASCOM.DriverAccess.Telescope ConnectMount(out ASCOM.DriverAccess.Camera outCam)
        {
            bool updateForm = false;

            lock (lockObject)
            {
                if (s_mount == null)
                    InstantiateMount();

                if (s_camera == null)
                    InstantiateCamera();

                if (MountUseCnt == 0)
                {
                    ConnectMountInner();
                    try
                    {
                        if (s_cam_connections == 0)
                            ConnectCameraInner();
                    }
                    catch (Exception)
                    {
                        DisconnectMountInner();
                        throw;
                    }
                    updateForm = true;
                }

                ++s_mount_connections;
            }

            if (updateForm)
                Server.MainForm.UpdateState();

            outCam = s_camera;
            return s_mount;
        }

        public static bool MountConnected
        {
            get
            {
                lock (lockObject)
                {
                    return s_mount_connections > 0;
                }
            }
        }

        private static void DisconnectMountInner()
        {
            try
            {
                s_mount.Connected = false;
            }
            catch (Exception)
            {
                // ignore it
            }

            if (s_switch != null)
            {
                try
                {
                    s_switch.Connected = false;
                }
                catch (Exception)
                {
                    // ignore it
                }
            }
        }

        public static void DisconnectMount()
        {
            lock (lockObject)
            {
                if (s_mount_connections == 0)
                    throw new InvalidOperationException("disconnecting mount when not connected");

                if (--s_mount_connections == 0)
                {
                    DisconnectMountInner();
                    if (CamUseCnt == 0)
                        DisconnectCameraInner();
                    s_platform.Init();
                }
            }

            Server.MainForm.UpdateState();
        }

        public static void SetupCamera()
        {
            lock (lockObject)
            {
                if (s_camera == null)
                    InstantiateCamera();
                s_camera.SetupDialog();
            }
        }

        public static void SetupMount()
        {
            lock (lockObject)
            {
                if (s_mount == null)
                    InstantiateMount();
                s_mount.SetupDialog();
            }
        }

        public static void SetupSwitch()
        {
            lock (lockObject)
            {
                if (s_mount == null)
                    InstantiateMount();
                if (s_switch != null)
                    s_switch.SetupDialog();
            }
        }
    }
}
