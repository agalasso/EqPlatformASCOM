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
using System.Collections.Generic;
using System.Text;
using ASCOM;

namespace ASCOM.EqPlatformAdapter
{
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

        static ASCOM.DriverAccess.Camera s_camera;
        static ASCOM.DriverAccess.Telescope s_mount;
        static ASCOM.DriverAccess.Switch s_switch;
        static short s_switchIdx;

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
            using (ASCOM.Utilities.Profile profile = new Utilities.Profile())
            {
                profile.DeviceType = "Camera";
                return profile.GetValue(CamDriverId, "cameraId", string.Empty, string.Empty);
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
            using (ASCOM.Utilities.Profile profile = new Utilities.Profile())
            {
                profile.DeviceType = "Telescope";

                string val = profile.GetValue(ScopeDriverId, "scopeId");
                if (val == null || val.Length == 0)
                    throw new ASCOM.DriverException("Missing ASCOM Telescope Device selection");
                mountDriverId = val;

                val = profile.GetValue(ScopeDriverId, "switchDriverId");
                switchDriverId = val;

                val = profile.GetValue(ScopeDriverId, "switchId");
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
