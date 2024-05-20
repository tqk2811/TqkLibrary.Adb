using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.AdbDotNet.Classes;
using TqkLibrary.AdbDotNet.LdPlayers;

namespace TqkLibrary.AdbDotNet
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="log"></param>
    public delegate void LogCallback(string log);
    /// <summary>
    /// 
    /// </summary>
    public class Adb
    {
        #region Static
        private static string _AdbPath = "adb.exe";
        /// <summary>
        /// 
        /// </summary>
        public static string AdbPath
        {
            get { return _AdbPath; }
            set
            {
                if (File.Exists(value)) _AdbPath = value;
                else throw new FileNotFoundException(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ProcessCommand BuildAdbCommand(string arguments)
        {
            return new ProcessCommand(AdbPath, arguments);
        }

        /// <summary>
        /// 
        /// </summary>
        public static ProcessCommand KillServer() => BuildAdbCommand("kill-server");

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ProcessCommand StartServer() => BuildAdbCommand("start-server");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<IAdbDevice>> DevicesAsync(CancellationToken cancellationToken = default)
        {
            string stdout = await BuildAdbCommand("devices").ExecuteAsync(cancellationToken).StdoutAsync().ConfigureAwait(false);
            var lines = Regex.Split(stdout, "\r\n").Skip(1).Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
            return lines.Select(x => new AdbDevice(x));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static IEnumerable<IAdbDevice> Devices(CancellationToken cancellationToken = default)
        {
            string stdout = BuildAdbCommand("devices").Execute(cancellationToken).Stdout();
            var lines = Regex.Split(stdout, "\r\n").Skip(1).Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
            return lines.Select(x => new AdbDevice(x));
        }


        #endregion



        /// <summary>
        /// 
        /// </summary>
        public string? DeviceId { get; }

        /// <summary>
        /// 
        /// </summary>
        public event LogCallback? LogCommand;

        /// <summary>
        /// 
        /// </summary>
        public LdPlayer? LdPlayer { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsLd { get { return LdPlayer != null; } }
        internal Adb(LdPlayer ldPlayer) : this()
        {
            this.LdPlayer = ldPlayer ?? throw new ArgumentNullException(nameof(ldPlayer));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Adb(string deviceId) : this()
        {
            if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentNullException(nameof(deviceId));
            this.DeviceId = deviceId;
        }

        internal Adb()
        {
            Shell = new AdbShell(this);
        }







        #region Device

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ProcessCommand BuildAdbDeviceCommand(string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments)) throw new ArgumentNullException(nameof(arguments));
            if (IsLd)
            {
                var command = LdPlayer!.BuildLdconsoleDeviceAdbCommand(arguments);
                return command;
            }
            else
            {
                var command = new ProcessCommand(AdbPath, $"-s {DeviceId} {arguments.Trim()}");
                command.CommandLogEvent += (l) => LogCommand?.Invoke($"adb {l}");
                return command;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ProcessCommand Root() => BuildAdbDeviceCommand("root");

        /// <summary>
        /// 
        /// </summary>
        public ProcessCommand UnRoot() => BuildAdbDeviceCommand("unroot");

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ProcessCommand WaitFor(WaitForType type) => BuildAdbDeviceCommand($"wait-for-{type.ToString().ToLower()}");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        /// <param name="androidPath"></param>
        /// <returns></returns>
        public ProcessCommand PushFile(string pcPath, string androidPath) => BuildAdbDeviceCommand($"push \"{pcPath}\" \"{androidPath}\"");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        /// <param name="androidPath"></param>
        /// <returns></returns>
        public ProcessCommand PullFile(string androidPath, string pcPath) => BuildAdbDeviceCommand($"pull \"{androidPath}\" \"{pcPath}\"");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        /// <returns></returns>
        public ProcessCommand InstallApk(string pcPath) => BuildAdbDeviceCommand($"install \"{pcPath}\"");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        /// <returns></returns>
        public ProcessCommand UpdateApk(string pcPath) => BuildAdbDeviceCommand($"install -r \"{pcPath}\"");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public ProcessCommand UnInstallApk(string packageName) => BuildAdbDeviceCommand($"uninstall {packageName}");

        /// <summary>
        /// 
        /// </summary>
        public AdbShell Shell { get; }
        #endregion
    }
}