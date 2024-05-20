using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.AdbDotNet.Classes;

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
        public string? Title { get; set; }
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
        public override string? ToString()
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<IAdbDevice>> GetAdbDeviceIdAsync(CancellationToken cancellationToken = default)
        {
            int port0 = 5554 + Index * 2;
            int port1 = port0 + 1;
            string p0 = port0.ToString();
            string p1 = port1.ToString();
            IEnumerable<IAdbDevice> devices = await Adb.DevicesAsync(cancellationToken);
            return devices
                .Where(x => x.DeviceState == DeviceState.Device && (x.DeviceId.EndsWith(p0.ToString()) || x.DeviceId.EndsWith(p1.ToString())));
        }

        /// <summary>
        /// 
        /// </summary>
        public async Task TryConnectAsync(int? timeout = null, CancellationToken cancellationToken = default)
        {
            int port0 = 5554 + Index * 2;
            int port1 = port0 + 1;
            string? command = null;
            if (PortInUse(port0).Count() > 0)
                command = $"connect 127.0.0.1:{port0}";
            else if (PortInUse(port1).Count() > 0)
                command = $"connect 127.0.0.1:{port1}";
            if (!string.IsNullOrWhiteSpace(command))
            {
                _ = await Adb.BuildAdbCommand(command!).WithTimeout(timeout, true).ExecuteAsync(cancellationToken);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj is LdList2 ldList2)
            {
                return ldList2.Index == Index;
            }
            return base.Equals(obj);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        static IEnumerable<IPEndPoint> PortInUse(params int[] ports)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
            return ipEndPoints.Where(x => ports.Any(y => x.Port == y));
        }
    }
}
