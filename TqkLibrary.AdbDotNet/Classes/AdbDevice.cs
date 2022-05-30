using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.AdbDotNet.Classes
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAdbDevice
    {
        /// <summary>
        /// 
        /// </summary>
        string DeviceId { get; }
        /// <summary>
        /// 
        /// </summary>
        DeviceState DeviceState { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AdbDevice : IAdbDevice
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        public AdbDevice(string line)
        {
            var parts = line.Split('\t');
            DeviceId = parts[0].Trim();
            DeviceState = (DeviceState)Enum.Parse(typeof(DeviceState), parts[1].Trim(), true);
        }
        /// <summary>
        /// 
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DeviceState DeviceState { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return DeviceId;
        }
    }
}
