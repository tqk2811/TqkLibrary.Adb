using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace TqkLibrary.AdbDotNet.LdPlayers
{
    /// <summary>
    /// 
    /// </summary>
    public class LdList2
    {
        /// <summary>
        /// 
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IntPtr TopWindowHandle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IntPtr BindWindowHandle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool AndroidStarted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ProcessId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ProcessIdOfVbox { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int DPI { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Title;

        }
        /// <summary>
        /// 
        /// </summary>
        public bool IsLdClosed
        {
            get { return ProcessId == -1 && ProcessIdOfVbox == -1 && TopWindowHandle == IntPtr.Zero && BindWindowHandle == IntPtr.Zero && !AndroidStarted; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAdbDeviceId()
        {
            int port0 = 5554 + Index * 2;
            int port1 = port0 + 1;
            string p0 = port0.ToString();
            string p1 = port1.ToString();
            return Adb.Devices()
                .Where(x => x.DeviceState == DeviceState.Device && (x.DeviceId.EndsWith(p0.ToString()) || x.DeviceId.EndsWith(p1.ToString())))
                .Select(x => x.DeviceId);
        }

        /// <summary>
        /// 
        /// </summary>
        public void TryConnect(bool force = false)
        {
            int port0 = 5554 + Index * 2;
            int port1 = port0 + 1;

            if (force || PortInUse(port0).Count() > 0)
                Adb.BuildAdbCommand($"connect 127.0.0.1:{port0}").Execute();

            if (force || PortInUse(port1).Count() > 0)
                Adb.BuildAdbCommand($"connect 127.0.0.1:{port1}").Execute();

        }

        static IEnumerable<IPEndPoint> PortInUse(params int[] ports)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
            return ipEndPoints.Where(x => ports.Any(y => x.Port == y));
        }
    }
}
