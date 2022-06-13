using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.AdbDotNet.LdPlayers
{
    /// <summary>
    /// 
    /// </summary>
    public class LdPlayer
    {
        internal static string _LdConsolePath = "ldconsole.exe";
        /// <summary>
        /// 
        /// </summary>
        public static string LdConsolePath
        {
            get { return _LdConsolePath; }
            set { if (File.Exists(value)) _LdConsolePath = value; else throw new FileNotFoundException(value); }
        }
        /// <summary>
        /// 
        /// </summary>
        public event LogCallback LogCommand;

        #region Static func

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static ProcessCommand BuildLdconsoleCommand(string action, string arguments = null)
        {
            if (string.IsNullOrWhiteSpace(action)) throw new ArgumentNullException(nameof(action));
            return new ProcessCommand()
            {
                ExecuteFile = LdConsolePath,
                Arguments = $"{action} {arguments}".Trim(),
            };
        }


        public static ProcessResult QuitAll(CancellationToken cancellationToken = default)
            => BuildLdconsoleCommand("quitall").Execute(cancellationToken, true);
        public static Task<ProcessResult> QuitAllAsync(CancellationToken cancellationToken = default)
            => BuildLdconsoleCommand("quitall").ExecuteAsync(cancellationToken, true);

        public static IEnumerable<string> List(CancellationToken cancellationToken = default)
            => BuildLdconsoleCommand("list").Execute(cancellationToken, true).Stdout()
            .Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
        public static Task<IEnumerable<string>> ListAsync(CancellationToken cancellationToken = default)
            => BuildLdconsoleCommand("list").ExecuteAsync(cancellationToken, true).StdoutAsync()
            .ContinueWith(x => x.Result.Split('\n').Select(y => y.Trim()).Where(y => !string.IsNullOrWhiteSpace(y)));

        public static IEnumerable<string> RunningList(CancellationToken cancellationToken = default)
            => BuildLdconsoleCommand("runninglist").Execute(cancellationToken, true).Stdout()
            .Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
        public static Task<IEnumerable<string>> RunningListAsync(CancellationToken cancellationToken = default)
            => BuildLdconsoleCommand("runninglist").ExecuteAsync(cancellationToken, true).StdoutAsync()
            .ContinueWith(x => x.Result.Split('\n').Select(y => y.Trim()).Where(y => !string.IsNullOrWhiteSpace(y)));

        public static IEnumerable<LdList2> List2(CancellationToken cancellationToken = default)
            => BuildLdconsoleCommand("list2").Execute(cancellationToken, true).Stdout()
            .Split('\n')
            .Select(x =>
            {
                var splits = x.Trim().Split(',');
                if (splits.Length == 7)
                {
                    return new LdList2()
                    {
                        Index = int.Parse(splits[0]),
                        Title = splits[1],
                        TopWindowHandle = new IntPtr(int.Parse(splits[2])),
                        BindWindowHandle = new IntPtr(int.Parse(splits[3])),
                        AndroidStarted = int.Parse(splits[4]) == 1,
                        ProcessId = int.Parse(splits[5]),
                        ProcessIdOfVbox = int.Parse(splits[6])
                    };
                }
                else return null;
            })
            .Where(x => x != null);
        public static Task<IEnumerable<LdList2>> List2Async(CancellationToken cancellationToken = default)
            => BuildLdconsoleCommand("list2").ExecuteAsync(cancellationToken, true).StdoutAsync()
            .ContinueWith(x => x.Result
                .Split('\n')
                .Select(x =>
                {
                    var splits = x.Trim().Split(',');
                    if (splits.Length == 7)
                    {
                        return new LdList2()
                        {
                            Index = int.Parse(splits[0]),
                            Title = splits[1],
                            TopWindowHandle = new IntPtr(int.Parse(splits[2])),
                            BindWindowHandle = new IntPtr(int.Parse(splits[3])),
                            AndroidStarted = int.Parse(splits[4]) == 1,
                            ProcessId = int.Parse(splits[5]),
                            ProcessIdOfVbox = int.Parse(splits[6])
                        };
                    }
                    else return null;
                })
                .Where(x => x != null));

        public static ProcessResult Copy(LdList2 from, string newName, CancellationToken cancellationToken = default)
            => BuildLdconsoleCommand("copy", $"--name \"{newName}\" --from {from.Index}").Execute(cancellationToken, true);
        public static Task<ProcessResult> CopyAsync(LdList2 from, string newName, CancellationToken cancellationToken = default)
            => BuildLdconsoleCommand("copy", $"--name \"{newName}\" --from {from.Index}").ExecuteAsync(cancellationToken, true);

        public static ProcessResult SortWnd(CancellationToken cancellationToken = default)
            => BuildLdconsoleCommand("sortWnd").Execute(cancellationToken, true);
        public static Task<ProcessResult> SortWndAsync(CancellationToken cancellationToken = default)
            => BuildLdconsoleCommand("sortWnd").ExecuteAsync(cancellationToken, true);

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #endregion


        /// <summary>
        /// 
        /// </summary>
        public Adb Adb { get; }
        /// <summary>
        /// 
        /// </summary>
        public LdList2 LdList2 { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ldList2"></param>
        public LdPlayer(LdList2 ldList2)
        {
            this.LdList2 = ldList2 ?? throw new ArgumentNullException(nameof(ldList2));
            this.Adb = new Adb(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ldList2"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void UpdateLdList2(LdList2 ldList2)
        {
            this.LdList2 = ldList2 ?? throw new ArgumentNullException(nameof(ldList2));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public ProcessCommand BuildLdconsoleDeviceCommand(string action, string arguments = null)
        {
            if (string.IsNullOrWhiteSpace(action)) throw new ArgumentNullException(nameof(action));
            var command = new ProcessCommand()
            {
                ExecuteFile = LdConsolePath,
                Arguments = $"{action} --index {LdList2.Index} {arguments}".Trim(),
            };
            command.CommandLogEvent += (l) => LogCommand?.Invoke($"ldconsole {l}");
            return command;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adbArguments"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public ProcessCommand BuildLdconsoleDeviceAdbCommand(string adbArguments)
        {
            return BuildLdconsoleDeviceCommand("adb", $"--command \"{adbArguments.Trim().WindowCharEscape()}\"");
        }




#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ProcessResult Quit(CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("quit").Execute(cancellationToken, true);
        public Task<ProcessResult> QuitAsync(CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("quit").ExecuteAsync(cancellationToken, true);

        public ProcessResult Launch(CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("launch").Execute(cancellationToken, true);
        public Task<ProcessResult> LaunchAsync(CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("launch").ExecuteAsync(cancellationToken, true);

        public ProcessResult Reboot(CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("reboot").Execute(cancellationToken, true);
        public Task<ProcessResult> RebootAsync(CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("reboot").ExecuteAsync(cancellationToken, true);

        public bool IsRunning(CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("isrunning").Execute(cancellationToken, true).Stdout().Contains("running");
        public Task<bool> IsRunningAsync(CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("isrunning").ExecuteAsync(cancellationToken, true).StdoutAsync().ContinueWith(t => t.Result.Contains("running"));

        public ProcessResult Remove(CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("remove").Execute(cancellationToken, true);
        public Task<ProcessResult> RemoveAsync(CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("remove").ExecuteAsync(cancellationToken, true);

        public ProcessResult Rename(string newName, CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("rename", $"--title \"{newName}\"").Execute(cancellationToken, true);
        public Task<ProcessResult> RenameAsync(string newName, CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("rename", $"--title \"{newName}\"").ExecuteAsync(cancellationToken, true);

        public ProcessResult InstallAppFile(string fileName, CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("installapp", $"--filename \"{fileName}\"").Execute(cancellationToken, true);
        public Task<ProcessResult> InstallAppFileAsync(string fileName, CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("installapp", $"--filename \"{fileName}\"").ExecuteAsync(cancellationToken, true);

        public ProcessResult InstallAppPackage(string pakageName, CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("installapp", $"--packagename {pakageName}").Execute(cancellationToken, true);
        public Task<ProcessResult> InstallAppPackageAsync(string fileName, CancellationToken cancellationToken = default)
           => BuildLdconsoleDeviceCommand("installapp", $"--packagename \"{fileName}\"").ExecuteAsync(cancellationToken, true);

        public ProcessResult UninstallApp(string pakageName, CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("uninstallapp", $"--packagename {pakageName}").Execute(cancellationToken, true);
        public Task<ProcessResult> UninstallAppAsync(string fileName, CancellationToken cancellationToken = default)
           => BuildLdconsoleDeviceCommand("uninstallapp", $"--packagename \"{fileName}\"").ExecuteAsync(cancellationToken, true);

        public ProcessResult RunApp(string pakageName, CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("runapp", $"--packagename {pakageName}").Execute(cancellationToken, true);
        public Task<ProcessResult> RunAppAsync(string fileName, CancellationToken cancellationToken = default)
           => BuildLdconsoleDeviceCommand("runapp", $"--packagename \"{fileName}\"").ExecuteAsync(cancellationToken, true);

        public ProcessResult KillApp(string pakageName, CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("killapp", $"--packagename {pakageName}").Execute(cancellationToken, true);
        public Task<ProcessResult> KillAppAsync(string fileName, CancellationToken cancellationToken = default)
           => BuildLdconsoleDeviceCommand("killapp", $"--packagename \"{fileName}\"").ExecuteAsync(cancellationToken, true);

        public ProcessResult Locatte(double lng, double lat, CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("locate", $"--LLI {lng},{lat}").Execute(cancellationToken, true);
        public Task<ProcessResult> LocatteAsync(double lng, double lat, CancellationToken cancellationToken = default)
           => BuildLdconsoleDeviceCommand("locate", $"--LLI {lng},{lat}").ExecuteAsync(cancellationToken, true);

        public ProcessResult BackupApp(string pakageName, string pcPath, CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("backupapp", $"--packagename {pakageName} --file \"{pcPath}\"").Execute(cancellationToken, true);
        public Task<ProcessResult> BackupAppAsync(string pakageName, string pcPath, CancellationToken cancellationToken = default)
           => BuildLdconsoleDeviceCommand("backupapp", $"--packagename {pakageName} --file \"{pcPath}\"").ExecuteAsync(cancellationToken, true);

        public ProcessResult RestoreApp(string pakageName, string pcPath, CancellationToken cancellationToken = default)
            => BuildLdconsoleDeviceCommand("restoreapp", $"--packagename {pakageName} --file \"{pcPath}\"").Execute(cancellationToken, true);
        public Task<ProcessResult> RestoreAppAsync(string pakageName, string pcPath, CancellationToken cancellationToken = default)
           => BuildLdconsoleDeviceCommand("restoreapp", $"--packagename {pakageName} --file \"{pcPath}\"").ExecuteAsync(cancellationToken, true);

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// 
    /// </summary>
    //public class LdModify
    //{
    //    public int Width { get; set; }
    //    public int Height { get; set; }
    //    public int Dpi { get; set; }
    //    public int Cpu { get; set; }
    //    public int Memory { get; set; }
    //    public string ManuFacturer { get; set; }
    //    public string Model { get; set; }
    //    public int Pnumber { get; set; }
    //    public int? Imei { get; set; }
    //    public int? Imsi { get; set; }
    //    public int? SimSerial { get; set; }
    //    public string Androidid { get; set; }
    //    public string Mac { get; set; }
    //}
}
