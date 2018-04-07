//
// ================
// Shared Resources
// ================
//
// This class is a container for all shared resources that may be needed
// by the drivers served by the Local Server. 
//
// NOTES:
//
//	* ALL DECLARATIONS MUST BE STATIC HERE!! INSTANCES OF THIS CLASS MUST NEVER BE CREATED!
//
// Written by:	Bob Denny	29-May-2007
// Modified by Chris Rowland and Peter Simpson to hamdle multiple hardware devices March 2011
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
        internal Stopwatch m_swatch; // elpsaed tracking time

        internal Platform()
        {
            m_swatch = new Stopwatch();

            Init();
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

            double delta_ra = m_swatch.ElapsedMilliseconds / 1000.0 * SIDEREAL_DEGREES_PER_SECOND;

            SharedResources.s_mount.SyncToCoordinates(ra - delta_ra, dec);

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

        static internal Platform s_platform = new Platform();

        static ASCOM.Utilities.TraceLogger s_tl;
        static uint s_tl_cnt;

        static uint s_cam_connections; // number of connections to (outer) Camera device
        static uint s_mount_connections; // number of connections to (outer) Telescope device

        public static ASCOM.Utilities.TraceLogger GetTraceLogger()
        {
            lock (lockObject)
            {
                if (s_tl == null)
                {
                    s_tl = new ASCOM.Utilities.TraceLogger("", "EqPlatformAdapter");
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
            s_camera = new ASCOM.DriverAccess.Camera(cameraDriverId);
            s_cam_connections = 0;
        }

        public static void FreeCamera()
        {
            lock (lockObject)
            {
                s_camera.Dispose();
                s_camera = null;
                s_cam_connections = 0;
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

                if (s_cam_connections == 0)
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

        public static void DisconnectCamera()
        {
            lock (lockObject)
            {
                if (s_cam_connections == 0)
                    throw new InvalidOperationException("disconnecting camera when not connected");

                if (--s_cam_connections == 0)
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
            }

            Server.MainForm.UpdateState();
        }

        private static void GetMountDriverIds(out string mountDriverId, out string switchDriverId, out short switchIdx)
        {
            using (Settings settings = new Settings())
            {
                string val = settings.Get("scopeId");
                if (val == null || val.Length == 0)
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
            s_mount_connections = 0;
            s_switch = sw;
            s_switchIdx = switchIdx;
        }

        public static void FreeMount()
        {
            lock (lockObject)
            {
                s_mount.Dispose();
                s_mount = null;

                if (s_switch != null)
                {
                    s_switch.Dispose();
                    s_switch = null;
                }

                s_mount_connections = 0;
            }
        }

        public static ASCOM.DriverAccess.Telescope ConnectMount(out ASCOM.DriverAccess.Switch outSw, out short outSwitchIdx)
        {
            bool updateForm = false;

            lock (lockObject)
            {
                if (s_mount == null)
                    InstantiateMount();

                if (s_mount_connections == 0)
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
                    updateForm = true;
                }

                ++s_mount_connections;
            }

            if (updateForm)
                Server.MainForm.UpdateState();

            outSw = s_switch;
            outSwitchIdx = s_switchIdx;
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

        public static void DisconnectMount()
        {
            lock (lockObject)
            {
                if (s_mount_connections == 0)
                    throw new InvalidOperationException("disconnecting mount when not connected");

                if (--s_mount_connections == 0)
                {
                    s_platform.Init();

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
