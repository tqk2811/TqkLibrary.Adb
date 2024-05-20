using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TqkLibrary.AdbDotNet.Classes;

namespace TqkLibrary.AdbDotNet
{
    /// <summary>
    /// 
    /// </summary>
    public class AdbShell
    {
        readonly Adb adb;
        internal AdbShell(Adb adb)
        {
            this.adb = adb ?? throw new ArgumentNullException(nameof(adb));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ProcessCommand BuildShellCommand(string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments)) throw new ArgumentNullException(nameof(arguments));
            return adb.BuildAdbDeviceCommand($"shell {arguments.Trim()}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ProcessCommand Reboot() => BuildShellCommand("reboot");

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ProcessCommand Shutdown() => BuildShellCommand("reboot -p");

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ProcessCommand RebootRecovery() => BuildShellCommand("reboot-recovery");

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ProcessCommand RebootBootLoader() => BuildShellCommand("reboot-bootloader");

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ProcessCommand FastBoot() => BuildShellCommand("fastboot");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="androidPath"></param>
        /// <returns></returns>
        public ProcessCommand DeleteFile(string androidPath) => BuildShellCommand($"rm \"{androidPath}\"");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adbShellAMStart"></param>
        /// <returns></returns>
        public ProcessCommand OpenApk(AdbShellAMStart adbShellAMStart) => BuildShellCommand($"am start {adbShellAMStart.GetCommand()}");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public ProcessCommand DisableApk(string packageName) => BuildShellCommand($"pm disable {packageName}");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public ProcessCommand EnableApk(string packageName) => BuildShellCommand($"pm enable {packageName}");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public ProcessCommand ForceStopApk(string packageName) => BuildShellCommand($"am force-stop {packageName}");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public ProcessCommand ClearApk(string packageName) => BuildShellCommand($"pm clear {packageName}");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public ProcessCommand SetProxy(string proxy) => BuildShellCommand($"settings put global http_proxy {proxy}");

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ProcessCommand ClearProxy() => SetProxy(":0");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public ProcessCommand Tap(Point point) => BuildShellCommand($"input tap {point.X} {point.Y}");

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ProcessCommand Swipe(Point from, Point to, int duration) => BuildShellCommand($"input swipe {from.X} {from.Y} {to.X} {to.Y} {duration}");

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ProcessCommand LongPress(Point point, int duration) => Swipe(point, point, duration);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ProcessCommand Key(ADBKeyEvent key) => BuildShellCommand($"input keyevent {(int)key}");

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ProcessCommand InputText(string text) => BuildShellCommand($"input text {text.AdbCharEscape()}");





        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public IEnumerable<string> DumpsysPackage(CancellationToken cancellationToken = default)
            => BuildShellCommand($"dumpsys package")
            .Execute(cancellationToken, true)
            .Stdout().Split('\r').Where(x => !string.IsNullOrWhiteSpace(x));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> DumpsysPackageAsync(CancellationToken cancellationToken = default)
            => (await BuildShellCommand($"dumpsys package")
                    .ExecuteAsync(cancellationToken, true)
                    .StdoutAsync().ConfigureAwait(false))
            .Split('\r').Where(x => !string.IsNullOrWhiteSpace(x));



    }
}
