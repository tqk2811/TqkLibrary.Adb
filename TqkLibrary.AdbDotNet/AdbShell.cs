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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult Reboot(CancellationToken cancellationToken = default)
            => BuildShellCommand("reboot").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> RebootAsync(CancellationToken cancellationToken = default)
            => BuildShellCommand("reboot").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult Shutdown(CancellationToken cancellationToken = default)
            => BuildShellCommand("reboot -p").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> ShutdownAsync(CancellationToken cancellationToken = default)
            => BuildShellCommand("reboot -p").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult RebootRecovery(CancellationToken cancellationToken = default)
            => BuildShellCommand("reboot-recovery").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> RebootRecoveryAsync(CancellationToken cancellationToken = default)
            => BuildShellCommand("reboot-recovery").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult RebootBootLoader(CancellationToken cancellationToken = default)
            => BuildShellCommand("reboot-bootloader").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> RebootBootLoaderAsync(CancellationToken cancellationToken = default)
            => BuildShellCommand("reboot-bootloader").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult FastBoot(CancellationToken cancellationToken = default)
            => BuildShellCommand("fastboot").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> FastBootAsync(CancellationToken cancellationToken = default)
            => BuildShellCommand("fastboot").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="androidPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult DeleteFile(string androidPath, CancellationToken cancellationToken = default)
            => BuildShellCommand($"rm \"{androidPath}\"").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="androidPath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> DeleteFileAsync(string androidPath, CancellationToken cancellationToken = default)
            => BuildShellCommand($"rm \"{androidPath}\"").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adbShellAMStart"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult OpenApk(AdbShellAMStart adbShellAMStart, CancellationToken cancellationToken = default)
            => BuildShellCommand($"am start {adbShellAMStart.GetCommand()}").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adbShellAMStart"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> OpenApkAsync(AdbShellAMStart adbShellAMStart, CancellationToken cancellationToken = default)
            => BuildShellCommand($"am start {adbShellAMStart.GetCommand()}").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult DisableApk(string packageName, CancellationToken cancellationToken = default)
            => BuildShellCommand($"pm disable {packageName}").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> DisableApkAsync(string packageName, CancellationToken cancellationToken = default)
            => BuildShellCommand($"pm disable {packageName}").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult EnableApk(string packageName, CancellationToken cancellationToken = default)
            => BuildShellCommand($"pm enable {packageName}").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> EnableApkAsync(string packageName, CancellationToken cancellationToken = default)
            => BuildShellCommand($"pm enable {packageName}").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult ForceStopApk(string packageName, CancellationToken cancellationToken = default)
            => BuildShellCommand($"am force-stop {packageName}").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> ForceStopApkAsync(string packageName, CancellationToken cancellationToken = default)
            => BuildShellCommand($"am force-stop {packageName}").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult ClearApk(string packageName, CancellationToken cancellationToken = default)
            => BuildShellCommand($"pm clear {packageName}").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> ClearApkAsync(string packageName, CancellationToken cancellationToken = default)
            => BuildShellCommand($"pm clear {packageName}").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult SetProxy(string proxy, CancellationToken cancellationToken = default)
            => BuildShellCommand($"settings put global http_proxy {proxy}").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> SetProxyAsync(string proxy, CancellationToken cancellationToken = default)
            => BuildShellCommand($"settings put global http_proxy {proxy}").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult ClearProxy(CancellationToken cancellationToken = default)
            => SetProxy(":0", cancellationToken);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> ClearProxyAsync(CancellationToken cancellationToken = default)
            => SetProxyAsync(":0", cancellationToken);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ProcessResult Tap(Point point, CancellationToken cancellationToken = default)
            => BuildShellCommand($"input tap {point.X} {point.Y}").Execute(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessResult> TapAsync(Point point, CancellationToken cancellationToken = default)
            => BuildShellCommand($"input tap {point.X} {point.Y}").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ProcessResult Swipe(Point from, Point to, int duration, CancellationToken cancellationToken = default)
            => BuildShellCommand($"input swipe {from.X} {from.Y} {to.X} {to.Y} {duration}").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<ProcessResult> SwipeAsync(Point from, Point to, int duration, CancellationToken cancellationToken = default)
            => BuildShellCommand($"input swipe {from.X} {from.Y} {to.X} {to.Y} {duration}").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ProcessResult LongPress(Point point, int duration, CancellationToken cancellationToken = default)
            => Swipe(point, point, duration, cancellationToken);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<ProcessResult> LongPressAsync(Point point, int duration, CancellationToken cancellationToken = default)
            => SwipeAsync(point, point, duration, cancellationToken);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ProcessResult Key(ADBKeyEvent key, CancellationToken cancellationToken = default)
            => BuildShellCommand($"input keyevent {(int)key}").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<ProcessResult> KeyAsync(ADBKeyEvent key, CancellationToken cancellationToken = default)
            => BuildShellCommand($"input keyevent {(int)key}").ExecuteAsync(cancellationToken, true);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ProcessResult InputText(string text, CancellationToken cancellationToken = default)
            => BuildShellCommand($"input text {text.AdbCharEscape()}").Execute(cancellationToken, true);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<ProcessResult> InputTextAsync(string text, CancellationToken cancellationToken = default)
            => BuildShellCommand($"input text {text.AdbCharEscape()}").ExecuteAsync(cancellationToken, true);





        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public IEnumerable<string> DumpsysPackage(CancellationToken cancellationToken = default)
            => BuildShellCommand($"dumpsys package")
            .Execute(cancellationToken, true)
            .Stdout().Split('\r').Where(x => !string.IsNullOrWhiteSpace(x));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> DumpsysPackageAsync(CancellationToken cancellationToken = default)
            => (await BuildShellCommand($"dumpsys package")
            .ExecuteAsync(cancellationToken, true)
            .StdoutAsync().ConfigureAwait(false)).Split('\r').Where(x => !string.IsNullOrWhiteSpace(x));



    }
}
