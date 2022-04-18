using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

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
    public class Adb : IDisposable
    {
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
        public bool IsEnableLogDelay { get; set; } = false;
        /// <summary>
        /// 
        /// </summary>
        public int TimeoutDefault { get; set; } = 30000;
        private readonly string adbPath;
        private readonly Random rd = new Random();
        
        internal CancellationTokenSource TokenSource = new CancellationTokenSource();
        internal CancellationTokenSource TokenSource2 = new CancellationTokenSource();//continue work cleanup after TokenSource stop
        
        /// <summary>
        /// 
        /// </summary>
        public CancellationToken CancellationToken { get { return TokenSource.Token; } }

        internal CancellationToken Token { get { return TokenSource.IsCancellationRequested ? TokenSource2.Token : TokenSource.Token; } }
        /// <summary>
        /// 
        /// </summary>
        public string DeviceId { get; internal set; }
        /// <summary>
        /// 
        /// </summary>
        public event LogCallback LogCommand;

        /// <summary>
        /// 
        /// </summary>
        public bool IsLd { get; private set; } = false;
        internal Adb(string ldName)
        {
            IsLd = true;
            DeviceId = ldName;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="adbPath"></param>
        public Adb(string deviceId = null, string adbPath = null)
        {
            this.DeviceId = deviceId;
            this.adbPath = adbPath;
            if (File.Exists(adbPath)) _AdbPath = adbPath;
        }

        #region Static
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        public static void KillServer(int timeout = 30000) => ExecuteCommand("kill-server", timeout);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        public static void StartServer(int timeout = 30000) => ExecuteCommand("start-server", timeout);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IEnumerable<string> Devices(DeviceState state = DeviceState.All)
        {
            string input = ExecuteCommand("devices");
            var lines = Regex.Split(input, "\r\n").Skip(1).Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
            if (state != DeviceState.All)
            {
                var states = state.ToString().ToLower().Split(',').Select(x => x.Trim()).ToList();
                return lines.Where(x => states.Any(y => x.EndsWith(y))).Select(x => x.Split('\t').First().Trim());
            }
            else return lines;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeout"></param>
        /// <param name="cancelToken"></param>
        /// <param name="adbPath"></param>
        /// <returns></returns>
        public static string ExecuteCommand(string command, int timeout = 30000, CancellationToken cancelToken = default, string adbPath = null)
        {
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
            return ExecuteCommand(command, cancellationTokenSource.Token, cancelToken, adbPath);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeoutToken"></param>
        /// <param name="cancelToken"></param>
        /// <param name="adbPath"></param>
        /// <returns></returns>
        /// <exception cref="AdbTimeoutException"></exception>
        /// <exception cref="AdbException"></exception>
        public static string ExecuteCommand(string command, CancellationToken timeoutToken, CancellationToken cancelToken, string adbPath = null)
        {
            using Process process = new Process();
            process.StartInfo.FileName = string.IsNullOrEmpty(adbPath) ? AdbPath : adbPath;
            process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            process.StartInfo.Arguments = command;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.Start();

            string result = string.Empty;
            using (cancelToken.Register(() => process.Kill()))
            using (timeoutToken.Register(() => process.Kill()))
            {
                result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
            cancelToken.ThrowIfCancellationRequested();
            if (timeoutToken.IsCancellationRequested) throw new AdbTimeoutException(command);

            //string result = process.StandardOutput.ReadToEnd();
            string err = process.StandardError.ReadToEnd().Trim();
            if (process.ExitCode < 0) throw new AdbException(result, err, command, process.ExitCode);
            else if (!string.IsNullOrEmpty(err))
            {
                Console.WriteLine($"AdbCommand:" + command);
                Console.WriteLine($"\t\tStandardOutput:" + result);
                Console.WriteLine($"\t\tStandardError:" + err);
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeoutToken"></param>
        /// <param name="cancelToken"></param>
        /// <param name="adbPath"></param>
        /// <returns></returns>
        /// <exception cref="AdbTimeoutException"></exception>
        /// <exception cref="AdbException"></exception>
        public static MemoryStream ExecuteCommandBuffer(string command, CancellationToken timeoutToken = default, CancellationToken cancelToken = default, string adbPath = null)
        {
            using Process process = new Process();
            process.StartInfo.FileName = string.IsNullOrEmpty(adbPath) ? AdbPath : adbPath;
            process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            process.StartInfo.Arguments = command;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.Start();
            MemoryStream memoryStream = new MemoryStream();
            using (cancelToken.Register(() => process.Kill()))
            using (timeoutToken.Register(() => process.Kill()))
            {
                process.StandardOutput.BaseStream.CopyTo(memoryStream);
                process.WaitForExit();
            }
            cancelToken.ThrowIfCancellationRequested();
            if (timeoutToken.IsCancellationRequested) throw new AdbTimeoutException(command);

            string err = process.StandardError.ReadToEnd().Trim();
            if (process.ExitCode < 0) throw new AdbException(string.Empty, err, command, process.ExitCode);
            else if (!string.IsNullOrEmpty(err))
            {
                Console.WriteLine($"AdbCommand:" + command);
                Console.WriteLine($"\t\tStandardError:" + err);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="adbPath"></param>
        /// <returns></returns>
        public static Process ExecuteCommandProcess(string command, string adbPath)
        {
            Process process = new Process();
            process.StartInfo.FileName = string.IsNullOrEmpty(adbPath) ? AdbPath : adbPath;
            process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            process.StartInfo.Arguments = command;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.Start();
            return process;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeout"></param>
        /// <param name="cancelToken"></param>
        /// <param name="adbPath"></param>
        /// <returns></returns>
        public static string ExecuteCommandCmd(string command, int timeout = 30000, CancellationToken cancelToken = default, string adbPath = null)
        {
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
            return ExecuteCommandCmd(command, cancellationTokenSource.Token, cancelToken, adbPath);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeoutToken"></param>
        /// <param name="cancelToken"></param>
        /// <param name="adbPath"></param>
        /// <returns></returns>
        /// <exception cref="AdbTimeoutException"></exception>
        /// <exception cref="AdbException"></exception>
        public static string ExecuteCommandCmd(string command, CancellationToken timeoutToken, CancellationToken cancelToken, string adbPath = null)
        {
            using Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;

            process.Start();

            process.StandardInput.WriteLine($"\"{(string.IsNullOrEmpty(adbPath) ? AdbPath : adbPath)}\" {command}");
            process.StandardInput.Flush();
            process.StandardInput.Close();

            using (cancelToken.Register(() => process.Kill()))
            using (timeoutToken.Register(() => process.Kill()))
                process.WaitForExit();
            cancelToken.ThrowIfCancellationRequested();
            if (timeoutToken.IsCancellationRequested) throw new AdbTimeoutException(command);

            string result = process.StandardOutput.ReadToEnd();
            string err = process.StandardError.ReadToEnd().Trim();
            if (process.ExitCode < 0) throw new AdbException(result, err, command, process.ExitCode);
            else if (!string.IsNullOrEmpty(err))
            {
                Console.WriteLine($"AdbCommand (cmd):" + command);
                Console.WriteLine($"\t\tStandardOutput:" + result);
                Console.WriteLine($"\t\tStandardError:" + err);
            }
            return result;
        }

        #endregion

        #region mainMethod
        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (!TokenSource.IsCancellationRequested) TokenSource.Cancel();
            else TokenSource2.Cancel();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Delay(int value)
        {
            if (IsEnableLogDelay) LogCommand?.Invoke($"Delay {value}ms");
            Task.Delay(value, Token).Wait();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Task DelayAsync(int value)
        {
            if (IsEnableLogDelay) LogCommand?.Invoke($"Delay {value}ms");
            return Task.Delay(value, Token);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void Delay(int min, int max) => Delay(rd.Next(min, max));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public Task DelayAsync(int min, int max) => DelayAsync(rd.Next(min, max));

        /// <summary>
        /// /
        /// </summary>
        public void Dispose()
        {
            TokenSource.Dispose();
            TokenSource2.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeoutToken"></param>
        /// <returns></returns>
        public string AdbCommand(string command, CancellationToken timeoutToken = default)
        {
            CancellationToken.ThrowIfCancellationRequested();
            LogCommand?.Invoke("adb " + command);
            if (IsLd)
            {
                return LdPlayer.LdPlayer.AdbCommand(DeviceId, command, timeoutToken, this.CancellationToken);
            }
            else
            {
                string adbLocation = string.IsNullOrEmpty(adbPath) ? AdbPath : adbPath;
                string commands = string.IsNullOrEmpty(DeviceId) ? command : $"-s {DeviceId} {command}";
                return ExecuteCommand(commands, timeoutToken, Token, adbLocation);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public string AdbCommand(string command, int timeout)
        {
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
            return AdbCommand(command, cancellationTokenSource.Token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeoutToken"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public string AdbCommandCmd(string command, CancellationToken timeoutToken = default)
        {
            CancellationToken.ThrowIfCancellationRequested();
            if (IsLd)
            {
                throw new NotSupportedException("AdbCommandCmd in Ldplayer Mode");
            }
            else
            {
                string adbLocation = string.IsNullOrEmpty(adbPath) ? AdbPath : adbPath;
                string commands = string.IsNullOrEmpty(DeviceId) ? command : $"-s {DeviceId} {command}";
                LogCommand?.Invoke(commands);
                return ExecuteCommandCmd(commands, timeoutToken, Token, adbLocation);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public string AdbCommandCmd(string command, int timeout)
        {
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
            return AdbCommandCmd(command, cancellationTokenSource.Token);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public Process ExecuteCommandProcess(string command)
        {
            CancellationToken.ThrowIfCancellationRequested();
            if (IsLd)
            {
                throw new NotSupportedException("AdbCommandCmd in Ldplayer Mode");
            }
            else
            {
                string adbLocation = string.IsNullOrEmpty(adbPath) ? AdbPath : adbPath;
                string commands = string.IsNullOrEmpty(DeviceId) ? command : $"-s {DeviceId} {command}";
                LogCommand?.Invoke(commands);
                return ExecuteCommandProcess(commands, adbLocation);
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void Root() => AdbCommand("root");
        /// <summary>
        /// 
        /// </summary>
        public void UnRoot() => AdbCommand("unroot");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="timeout"></param>
        public void WaitFor(WaitForType type, int timeout = 120000) => AdbCommand($"wait-for-{type.ToString().ToLower()}", timeout);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Task WaitForAsync(WaitForType type, int timeout = 120000)
          => Task.Run(() => AdbCommand($"wait-for-{type.ToString().ToLower()}", timeout));
        /// <summary>
        /// 
        /// </summary>
        public void Shutdown() => AdbCommand("shell reboot -p");
        /// <summary>
        /// 
        /// </summary>
        public void Reboot() => AdbCommand("shell reboot");
        /// <summary>
        /// 
        /// </summary>
        public void RebootRecovery() => AdbCommand("shell reboot-recovery");
        /// <summary>
        /// 
        /// </summary>
        public void RebootBootLoader() => AdbCommand("shell reboot-bootloader");
        /// <summary>
        /// 
        /// </summary>
        public void FastBoot() => AdbCommand("shell fastboot");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        /// <param name="androidPath"></param>
        public void PushFile(string pcPath, string androidPath) => AdbCommand($"push \"{pcPath}\" \"{androidPath}\"");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        /// <param name="androidPath"></param>
        /// <param name="timeout"></param>
        public void PushFile(string pcPath, string androidPath, int timeout) => AdbCommand($"push \"{pcPath}\" \"{androidPath}\"", timeout);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="androidPath"></param>
        /// <param name="pcPath"></param>
        public void PullFile(string androidPath, string pcPath) => AdbCommand($"pull \"{androidPath}\" \"{pcPath}\"");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="androidPath"></param>
        /// <param name="pcPath"></param>
        /// <param name="timeout"></param>
        public void PullFile(string androidPath, string pcPath, int timeout) => AdbCommand($"pull \"{androidPath}\" \"{pcPath}\"", timeout);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="androidPath"></param>
        public void DeleteFile(string androidPath) => AdbCommand($"shell rm \"{androidPath}\"");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        public void InstallApk(string pcPath) => AdbCommand($"install \"{pcPath}\"");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        /// <param name="timeout"></param>
        public void InstallApk(string pcPath, int timeout = 30000) => AdbCommand($"install \"{pcPath}\"", timeout);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        public void UpdateApk(string pcPath) => AdbCommand($"install -r \"{pcPath}\"");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcPath"></param>
        /// <param name="timeout"></param>
        public void UpdateApk(string pcPath, int timeout = 30000) => AdbCommand($"install -r \"{pcPath}\"", timeout);

        /// am [start|instrument]<br/>
        /// am start[-a <action>] [-d <data_uri>][-t <mime_type>] [-c <category> [-c <category>] ...][-e <extra_key> <extra_value>[-e <extra_key> <extra_value> ...][-n <component>][-D][<uri>]
        /// am instrument[-e <arg_name> <arg_value>] [-p <prof_file>] [-w] <component>
        /// https://developer.android.com/studio/command-line/adb#IntentSpec
        /// <summary>
        /// Example: com.google.android.gms/.accountsettings.mg.ui.main.MainActivity<br/>
        /// OpenApk("com.google.android.gms/.accountsettings.mg.ui.main.MainActivity");<br/>
        /// </summary>
        public void OpenApk(string n) => AdbCommand($"shell am start -n {n}");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="d"></param>
        public void OpenApk(string n, string d) => AdbCommand($"shell am start -d {d} -n {n}");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="d"></param>
        /// <param name="a"></param>
        public void OpenApk(string n, string d, string a) => AdbCommand($"shell am start -d {d} -n {n} -a {a}");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adbShellAMStart"></param>
        public void OpenApk(AdbShellAMStart adbShellAMStart) => AdbCommand($"shell am start {adbShellAMStart.GetCommand()}");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        public void DisableApk(string packageName) => AdbCommand($"shell pm disable {packageName}");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        public void EnableApk(string packageName) => AdbCommand($"shell pm enable {packageName}");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        public void UnInstallApk(string packageName) => AdbCommand($"uninstall {packageName}");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        public void ForceStopApk(string packageName) => AdbCommand($"shell am force-stop {packageName}");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        public void ClearApk(string packageName) => AdbCommand($"shell pm clear {packageName}");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="proxy"></param>
        /// <param name="timeout"></param>
        public void SetProxy(string proxy, int timeout) => AdbCommand($"shell settings put global http_proxy {proxy}", timeout);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="proxy"></param>
        public void SetProxy(string proxy) => AdbCommand($"shell settings put global http_proxy {proxy}", TimeoutDefault);
        /// <summary>
        /// 
        /// </summary>
        public void ClearProxy() => AdbCommand("shell settings put global http_proxy :0");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="deleteInAndroid"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public Bitmap ScreenShot(string FilePath = null, bool deleteInAndroid = true)
        {
            bool IsDelete = false;
            if (string.IsNullOrEmpty(FilePath))
            {
                FilePath = Directory.GetCurrentDirectory() + "\\" + Guid.NewGuid().ToString() + ".png";
                IsDelete = true;
            }
            string androidPath = $"/sdcard/{Guid.NewGuid()}.png";
            AdbCommand($"shell screencap -p \"{androidPath}\"");
            PullFile(androidPath, FilePath);
            if (deleteInAndroid) DeleteFile(androidPath);
            if (File.Exists(FilePath))
            {
                try
                {
                    byte[] buff = File.ReadAllBytes(FilePath);
                    MemoryStream memoryStream = new MemoryStream(buff);
                    return (Bitmap)Bitmap.FromStream(memoryStream);
                }
                finally
                {
                    if (IsDelete) try { File.Delete(FilePath); } catch (Exception) { }
                }
            }
            throw new FileNotFoundException(FilePath);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Bitmap ScreenShot2()
        {
            string args = string.Empty;
            if (string.IsNullOrEmpty(DeviceId)) args = "exec-out screencap -p";
            else args = $"-s {DeviceId} exec-out screencap -p";
            var stream = ExecuteCommandBuffer(args);
            return (Bitmap)Bitmap.FromStream(stream);
        }


        Point? point = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="force"></param>
        /// <returns></returns>
        /// <exception cref="AdbException"></exception>
        public Point GetScreenResolution(bool force = false)
        {
            if (point == null || force)
            {
                Regex regex = new Regex("(?<=mCurrentDisplayRect=Rect\\().*?(?=\\))", RegexOptions.Multiline);
                string result = AdbCommandCmd("shell dumpsys display | Find \"mCurrentDisplayRect\"");//
                Match match = regex.Match(result);
                if (match.Success)
                {
                    result = match.Value;

                    result = result.Substring(result.IndexOf("- ") + 2);
                    string[] temp = result.Split(',');

                    int x = Convert.ToInt32(temp[0].Trim());
                    int y = Convert.ToInt32(temp[1].Trim());
                    point = new Point(x, y);
                    return point.Value;
                }
                throw new AdbException(result, "shell dumpsys display failed", "shell dumpsys display | Find \"mCurrentDisplayRect\"", 0);
            }
            else return point.Value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public IEnumerable<string> ListActivities(string packageName)
        {
            string result = AdbCommandCmd($"shell dumpsys package | Find \"{packageName}/\" | Find \"Activity\"");//shell dumpsys package | Find "com.km.karaoke/" | Find "Activity"
            var lines = result.Split('\r');
            return lines.Select(x => x.Trim());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="count"></param>

        public void Tap(Point point, int count = 1) => Tap(point.X, point.Y, count);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="count"></param>
        public void Tap(int x, int y, int count = 1)
        {
            for (int i = 0; i < count; i++) AdbCommand($"shell input tap {x} {y}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="count"></param>
        public void TapByPercent(double x, double y, int count = 1)
        {
            var resolution = GetScreenResolution();
            int X = (int)(x * resolution.X);
            int Y = (int)(y * resolution.Y);
            Tap(X, Y, count);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        public void Swipe(Point from, Point to, int duration = 100) => Swipe(from.X, from.Y, to.X, to.Y, duration);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="duration"></param>
        public void Swipe(int x1, int y1, int x2, int y2, int duration = 100) => AdbCommand($"shell input swipe {x1} {y1} {x2} {y2} {duration}");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="duration"></param>

        public void SwipeByPercent(double x1, double y1, double x2, double y2, int duration = 100)
        {
            var resolution = GetScreenResolution();

            int X1 = (int)(x1 * resolution.X);
            int Y1 = (int)(y1 * resolution.Y);
            int X2 = (int)(x2 * resolution.X);
            int Y2 = (int)(y2 * resolution.Y);

            Swipe(X1, Y1, X2, Y2, duration);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="duration"></param>
        public void LongPress(Point point, int duration = 100) => LongPress(point.X, point.Y, duration);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="duration"></param>
        public void LongPress(int x, int y, int duration = 100) => AdbCommand($"shell input swipe {x} {y} {x} {y} {duration}");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public void Key(ADBKeyEvent key) => AdbCommand($"shell input keyevent {(int)key}");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyCode"></param>
        public void Key(int keyCode) => AdbCommand($"shell input keyevent {keyCode}");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void InputText(string text)
        {
            string text_fix = text.AdbCharEscape();
            AdbCommand($"shell input text \"{text_fix}\"");
        }
        /// <summary>
        /// 
        /// </summary>
        public void PlanModeON()
        {
            AdbCommand("settings put global airplane_mode_on 1");
            AdbCommand("am broadcast -a android.intent.action.AIRPLANE_MODE");
        }
        /// <summary>
        /// 
        /// </summary>
        public void PlanModeOFF()
        {
            AdbCommand("settings put global airplane_mode_on 0");
            AdbCommand("am broadcast -a android.intent.action.AIRPLANE_MODE");
        }
    }
}