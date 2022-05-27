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
        /// <exception cref="FileNotFoundException"></exception>
        public static ProcessCommand BuildAdbCommand(string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments)) throw new ArgumentNullException(nameof(arguments));
            if (!File.Exists(AdbPath)) throw new FileNotFoundException("can't find adb");
            return new ProcessCommand()
            {
                ExecuteFile = AdbPath,
                Arguments = arguments,
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public static ProcessCommand BuildCmdAdbCommand(string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments)) throw new ArgumentNullException(nameof(arguments));
            if (!File.Exists(AdbPath)) throw new FileNotFoundException("can't find adb");
            return new ProcessCommand()
            {
                ExecuteFile = "cmd.exe",
                Arguments = $"\"{AdbPath}\" {arguments}",
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<ProcessResult> KillServerAsync(CancellationToken cancellationToken = default)
            => BuildAdbCommand("kill-server").ExecuteAsync(cancellationToken);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        public static ProcessResult KillServer(CancellationToken cancellationToken = default)
           => BuildAdbCommand("kill-server").Execute(cancellationToken);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<ProcessResult> StartServerAsync(CancellationToken cancellationToken = default)
            => BuildAdbCommand("start-server").ExecuteAsync(cancellationToken);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static ProcessResult StartServer(CancellationToken cancellationToken = default)
            => BuildAdbCommand("start-server").Execute(cancellationToken);

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
        public string DeviceId { get; }

        /// <summary>
        /// 
        /// </summary>
        public event LogCallback LogCommand;

        /// <summary>
        /// 
        /// </summary>
        public LdPlayer LdPlayer { get; internal set; }

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
        /// <exception cref="FileNotFoundException"></exception>
        public ProcessCommand BuildAdbDeviceCommand(string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments)) throw new ArgumentNullException(nameof(arguments));
            if(IsLd)
            {
                return LdPlayer.BuildLdconsoleDeviceAdbCommand(arguments);                
            }
            else
            {
                if (!File.Exists(AdbPath)) throw new FileNotFoundException("can't find adb");
                return new ProcessCommand()
                {
                    ExecuteFile = AdbPath,
                    Arguments = $"-s {DeviceId} {arguments.Trim()}",
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public ProcessCommand BuildCmdAdbDeviceCommand(string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments)) throw new ArgumentNullException(nameof(arguments));
            if (!File.Exists(AdbPath)) throw new FileNotFoundException("can't find adb");
            return new ProcessCommand()
            {
                ExecuteFile = "cmd.exe",
                Arguments = $"\"{AdbPath}\" -s {DeviceId} {arguments.Trim()}",
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        public ProcessResult Root(CancellationToken cancellationToken = default)
           => BuildAdbDeviceCommand("root").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> RootAsync(CancellationToken cancellationToken = default)
            => BuildAdbDeviceCommand("root").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        public ProcessResult UnRoot(CancellationToken cancellationToken = default)
           => BuildAdbDeviceCommand("unroot").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> UnRootAsync(CancellationToken cancellationToken = default)
            => BuildAdbDeviceCommand("unroot").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult WaitFor(WaitForType type, CancellationToken cancellationToken = default)
            => BuildAdbDeviceCommand($"wait-for-{type.ToString().ToLower()}").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> WaitForAsync(WaitForType type, CancellationToken cancellationToken = default)
            => BuildAdbDeviceCommand($"wait-for-{type.ToString().ToLower()}").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        /// <param name="androidPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult PushFile(string pcPath, string androidPath, CancellationToken cancellationToken = default)
            => BuildAdbDeviceCommand($"push \"{pcPath}\" \"{androidPath}\"").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        /// <param name="androidPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> PushFileAsync(string pcPath, string androidPath, CancellationToken cancellationToken = default)
            => BuildAdbDeviceCommand($"push \"{pcPath}\" \"{androidPath}\"").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        /// <param name="androidPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult PullFile(string androidPath, string pcPath, CancellationToken cancellationToken = default)
            => BuildAdbDeviceCommand($"pull \"{androidPath}\" \"{pcPath}\"").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        /// <param name="androidPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> PullFileAsync(string androidPath, string pcPath, CancellationToken cancellationToken = default)
            => BuildAdbDeviceCommand($"pull \"{androidPath}\" \"{pcPath}\"").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult InstallApk(string pcPath, CancellationToken cancellationToken = default)
            => BuildAdbDeviceCommand($"install \"{pcPath}\"").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        /// <param name="cancellationToken"></param>
        public Task<ProcessResult> InstallApkAsync(string pcPath, CancellationToken cancellationToken = default)
            => BuildAdbDeviceCommand($"install \"{pcPath}\"").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult UpdateApk(string pcPath, CancellationToken cancellationToken = default)
            => BuildAdbDeviceCommand($"install -r \"{pcPath}\"").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> UpdateApkAsync(string pcPath, CancellationToken cancellationToken = default)
            => BuildAdbDeviceCommand($"install -r \"{pcPath}\"").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult UnInstallApk(string packageName, CancellationToken cancellationToken = default)
            => BuildAdbDeviceCommand($"uninstall {packageName}").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> UnInstallApkAsync(string packageName, CancellationToken cancellationToken = default)
            => BuildAdbDeviceCommand($"uninstall {packageName}").ExecuteAsync(cancellationToken, true);


        /// <summary>
        /// 
        /// </summary>
        public AdbShell Shell { get; }
        #endregion
    }
}