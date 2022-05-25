//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace TqkLibrary.AdbDotNet.LdPlayer
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    public class LdPlayer : IDisposable
//    {
//        internal static string _LdConsolePath = "ldconsole.exe";
//        /// <summary>
//        /// 
//        /// </summary>
//        public static string LdConsolePath
//        {
//            get { return _LdConsolePath; }
//            set { if (File.Exists(value)) _LdConsolePath = value; else throw new FileNotFoundException(value); }
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        public event LogCallback LogCommand;

//        #region Static func
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="name"></param>
//        /// <param name="command"></param>
//        /// <param name="timeoutToken"></param>
//        /// <param name="cancelToken"></param>
//        /// <returns></returns>
//        public static string AdbCommand(string name, string command, CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
//        {
//            //https://stackoverflow.com/a/16018942/5034139
//            command = command.WindowCharEscape();
//            return ExecuteCommand($"adb --name \"{name}\" --command \"{command}\"", timeoutToken, cancelToken);
//        }

//        static string ExecuteCommand(string command, int timeout, CancellationToken cancelToken, string ldConsolePath = null)
//        {
//            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
//            return ExecuteCommand(command, cancellationTokenSource.Token, cancelToken, ldConsolePath);
//        }
//        static string ExecuteCommand(string command, CancellationToken timeoutToken, CancellationToken cancelToken, string ldConsolePath = null)
//        {
//            using Process process = new Process();
//            process.StartInfo.FileName = string.IsNullOrEmpty(ldConsolePath) ? _LdConsolePath : ldConsolePath;
//            process.StartInfo.WorkingDirectory = new FileInfo(process.StartInfo.FileName).DirectoryName;
//            process.StartInfo.Arguments = command;
//            process.StartInfo.CreateNoWindow = true;
//            process.StartInfo.UseShellExecute = false;
//            process.StartInfo.RedirectStandardOutput = true;
//            process.StartInfo.RedirectStandardError = true;
//            process.StartInfo.RedirectStandardInput = true;
//            process.Start();

//            string result = string.Empty;
//            using (cancelToken.Register(() => process.Kill()))
//            using (timeoutToken.Register(() => process.Kill()))
//            {
//                result = process.StandardOutput.ReadToEnd();
//                process.WaitForExit();
//            }
//            cancelToken.ThrowIfCancellationRequested();
//            if (timeoutToken.IsCancellationRequested) throw new LdPlayerTimeoutException(command);

//            //string result = process.StandardOutput.ReadToEnd();
//            string err = process.StandardError.ReadToEnd();
//            if (process.ExitCode < 0) throw new LdPlayerException(result, err, command, process.ExitCode);
//            else if (!string.IsNullOrEmpty(err))
//            {
//                Console.WriteLine($"LdConsole :" + command);
//                Console.WriteLine($"\t\tStandardOutput:" + result);
//                Console.WriteLine($"\t\tStandardError:" + err);
//                //throw new AdbException(result, err, command);
//            }
//            return result;
//        }
//        string _ExecuteCommand(string command, CancellationToken timeoutToken)
//        {
//            LogCommand?.Invoke($"ldconsole {command}");
//            return ExecuteCommand(command, timeoutToken, Adb.Token);
//        }
//        string _ExecuteCommand(string command, int timeout)
//        {
//            LogCommand?.Invoke($"ldconsole {command}");
//            return ExecuteCommand(command, timeout, Adb.Token);
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="timeout"></param>
//        /// <param name="cancelToken"></param>
//        public static void QuitAll(int timeout, CancellationToken cancelToken = default)
//          => ExecuteCommand("quitall", timeout, cancelToken);
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="timeoutToken"></param>
//        /// <param name="cancelToken"></param>
//        public static void QuitAll(CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
//          => ExecuteCommand("quitall", timeoutToken, cancelToken);

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="timeout"></param>
//        /// <param name="cancelToken"></param>
//        /// <returns></returns>
//        public static IEnumerable<string> List(int timeout, CancellationToken cancelToken = default)
//        {
//            string result = ExecuteCommand("list", timeout, cancelToken);
//            return result.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="timeoutToken"></param>
//        /// <param name="cancelToken"></param>
//        /// <returns></returns>
//        public static IEnumerable<string> List(CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
//        {
//            string result = ExecuteCommand("list", timeoutToken, cancelToken);
//            return result.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="timeout"></param>
//        /// <param name="cancelToken"></param>
//        /// <returns></returns>
//        public static IEnumerable<string> RunningList(int timeout, CancellationToken cancelToken = default)
//        {
//            string result = ExecuteCommand("runninglist", timeout, cancelToken);
//            return result.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="timeoutToken"></param>
//        /// <param name="cancelToken"></param>
//        /// <returns></returns>
//        public static IEnumerable<string> RunningList(CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
//        {
//            string result = ExecuteCommand("runninglist", timeoutToken, cancelToken);
//            return result.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="timeout"></param>
//        /// <param name="cancelToken"></param>
//        /// <returns></returns>
//        public static IEnumerable<LdList2> List2(int timeout, CancellationToken cancelToken = default)
//        {
//            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
//            return List2(cancellationTokenSource.Token, cancelToken);
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="timeoutToken"></param>
//        /// <param name="cancelToken"></param>
//        /// <returns></returns>
//        public static IEnumerable<LdList2> List2(CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
//        {
//            string result = ExecuteCommand("list2", timeoutToken, cancelToken);
//            return result
//              .Split('\n')
//              .Select(x =>
//            {
//                var splits = x.Trim().Split(',');
//                if (splits.Length == 7)
//                {
//                    return new LdList2()
//                    {
//                        Index = int.Parse(splits[0]),
//                        Title = splits[1],
//                        TopWindowHandle = new IntPtr(int.Parse(splits[2])),
//                        BindWindowHandle = new IntPtr(int.Parse(splits[3])),
//                        AndroidStarted = int.Parse(splits[4]) == 1,
//                        ProcessId = int.Parse(splits[5]),
//                        ProcessIdOfVbox = int.Parse(splits[6])
//                    };
//                }
//                else return null;
//            })
//              .Where(x => x != null);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="from"></param>
//        /// <param name="newName"></param>
//        /// <param name="timeout"></param>
//        /// <param name="cancelToken"></param>
//        public static void Copy(string from, string newName, int timeout, CancellationToken cancelToken = default)
//         => ExecuteCommand($"copy --name \"{newName}\" --from \"{from}\"", timeout, cancelToken);
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="from"></param>
//        /// <param name="newName"></param>
//        /// <param name="timeoutToken"></param>
//        /// <param name="cancelToken"></param>
//        public static void Copy(string from, string newName, CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
//          => ExecuteCommand($"copy --name \"{newName}\" --from \"{from}\"", timeoutToken, cancelToken);
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="name"></param>
//        /// <param name="timeoutToken"></param>
//        /// <param name="cancelToken"></param>
//        public static void Remove(string name, CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
//          => ExecuteCommand($"remove --name \"{name}\"", timeoutToken, cancelToken);
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="name"></param>
//        /// <param name="timeoutToken"></param>
//        /// <param name="cancelToken"></param>
//        public static void Quit(string name, CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
//          => ExecuteCommand($"quit --name \"{name}\"", timeoutToken, cancelToken);
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="name"></param>
//        /// <param name="timeoutToken"></param>
//        /// <param name="cancelToken"></param>
//        public static void Launch(string name, CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
//          => ExecuteCommand($"launch --name \"{name}\"", timeoutToken, cancelToken);

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="timeout"></param>
//        /// <param name="cancelToken"></param>
//        public static void SortWnd(int timeout, CancellationToken cancelToken = default)
//          => ExecuteCommand($"sortWnd", timeout, cancelToken);
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="timeoutToken"></param>
//        /// <param name="cancelToken"></param>
//        public static void SortWnd(CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
//          => ExecuteCommand($"sortWnd", timeoutToken, cancelToken);
//        #endregion


//        /// <summary>
//        /// 
//        /// </summary>
//        public Adb Adb { get; }
//        /// <summary>
//        /// 
//        /// </summary>
//        public int TimeoutDefault
//        {
//            get { return Adb.TimeoutDefault; }
//            set { Adb.TimeoutDefault = value; }
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="name"></param>
//        public LdPlayer(string name)
//        {
//            this.Adb = new Adb(name, true);
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        public void Stop()
//        {
//            Adb.Stop();
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        public void Dispose()
//        {
//            Adb.Dispose();
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        public void Quit() => _ExecuteCommand($"quit --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault);
//        /// <summary>
//        /// 
//        /// </summary>
//        public void Launch() => _ExecuteCommand($"launch --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault);
//        /// <summary>
//        /// 
//        /// </summary>
//        public void Reboot() => _ExecuteCommand($"reboot --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault);
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <returns></returns>
//        public bool IsRunning() => _ExecuteCommand($"isrunning --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault).Contains("running");
//        /// <summary>
//        /// 
//        /// </summary>
//        public void Remove() => _ExecuteCommand($"remove --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault);
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="timeoutToken"></param>
//        /// <returns></returns>
//        public bool RemoveUntillRemoved(CancellationToken timeoutToken = default)
//        {
//            while (List2().Any(x => Adb.DeviceId.Equals(x.Title)))
//            {
//                if (timeoutToken.IsCancellationRequested) return false;
//                _ExecuteCommand($"remove --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault);
//                Adb.Delay(500);
//            }
//            return true;
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="newName"></param>
//        public void Rename(string newName)
//        {
//            _ExecuteCommand($"rename --name \"{Adb.DeviceId}\" --title \"{newName}\"", Adb.TimeoutDefault);
//            Adb.DeviceId = newName;
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="newName"></param>
//        /// <param name="timeoutToken"></param>
//        /// <returns></returns>
//        public bool RenameUntilRenamed(string newName, CancellationToken timeoutToken = default)
//        {
//            while (List2().Any(x => Adb.DeviceId.Equals(x.Title)))
//            {
//                if (timeoutToken.IsCancellationRequested) return false;
//                _ExecuteCommand($"rename --name \"{Adb.DeviceId}\" --title \"{newName}\"", Adb.TimeoutDefault);
//                Adb.Delay(500);
//            }
//            Adb.DeviceId = newName;
//            return true;
//        }
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="fileName"></param>
//        public void InstallAppFile(string fileName)
//          => _ExecuteCommand($"installapp --name \"{Adb.DeviceId}\" --filename {fileName}", Adb.TimeoutDefault);
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pakageName"></param>
//        public void InstallAppPackage(string pakageName)
//          => _ExecuteCommand($"installapp --name \"{Adb.DeviceId}\" --packagename {pakageName}", Adb.TimeoutDefault);
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pakageName"></param>
//        public void UninstallApp(string pakageName)
//          => _ExecuteCommand($"uninstallapp --name \"{Adb.DeviceId}\" --packagename {pakageName}", Adb.TimeoutDefault);
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pakageName"></param>
//        public void RunApp(string pakageName)
//          => _ExecuteCommand($"runapp --name \"{Adb.DeviceId}\" --packagename {pakageName}", Adb.TimeoutDefault);
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pakageName"></param>
//        public void KillApp(string pakageName)
//          => _ExecuteCommand($"killapp --name \"{Adb.DeviceId}\" --packagename {pakageName}", Adb.TimeoutDefault);
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="lng"></param>
//        /// <param name="lat"></param>
//        public void Locatte(double lng, double lat)
//          => _ExecuteCommand($"remove --name \"{Adb.DeviceId}\" --LLI {lng},{lat}", Adb.TimeoutDefault);
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pakageName"></param>
//        /// <param name="pcPath"></param>
//        public void BackupApp(string pakageName, string pcPath)
//          => _ExecuteCommand($"backupapp --name \"{Adb.DeviceId}\" --packagename {pakageName} --file \"{pcPath}\"", Adb.TimeoutDefault);
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pakageName"></param>
//        /// <param name="pcPath"></param>
//        public void RestoreApp(string pakageName, string pcPath)
//          => _ExecuteCommand($"restoreapp --name \"{Adb.DeviceId}\" --packagename {pakageName} --file \"{pcPath}\"", Adb.TimeoutDefault);

//    }
//}
