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
            => List2Async(cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        public static Task<IEnumerable<LdList2>> List2Async(CancellationToken cancellationToken = default)
            => BuildLdconsoleCommand("list2").ExecuteAsync(cancellationToken, true).StdoutAsync()
            .ContinueWith(x => x.Result
                .Split('\n')
                .Select(x =>
                {
                    var splits = x.Trim().Split(',');
                    if (splits.Length == 7 || splits.Length == 10)
                    {
                        LdList2 ldList2 = new LdList2()
                        {
                            Index = int.Parse(splits[0]),
                            Title = splits[1],
                            TopWindowHandle = new IntPtr(int.Parse(splits[2])),
                            BindWindowHandle = new IntPtr(int.Parse(splits[3])),
                            AndroidStarted = int.Parse(splits[4]) == 1,
                            ProcessId = int.Parse(splits[5]),
                            ProcessIdOfVbox = int.Parse(splits[6])
                        };
                        if(splits.Length == 10)
                        {
                            ldList2.Width = int.Parse(splits[7]);
                            ldList2.Height = int.Parse(splits[8]);
                            ldList2.DPI = int.Parse(splits[9]);
                        }
                        return ldList2;
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
        public ProcessCommand BuildLdconsoleDeviceAdbCommand(string adbArguments)
        {
            return BuildLdconsoleDeviceCommand("adb", $"--command \"{adbArguments.Trim().WindowCharEscape()}\"");
        }




#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ProcessCommand Quit() => BuildLdconsoleDeviceCommand("quit");
        public ProcessCommand Launch() => BuildLdconsoleDeviceCommand("launch");
        public ProcessCommand Reboot() => BuildLdconsoleDeviceCommand("reboot");
        public ProcessCommand IsRunningBuild() => BuildLdconsoleDeviceCommand("isrunning");
        public bool IsRunning(CancellationToken cancellationToken = default)
            => IsRunningBuild().Execute(cancellationToken, true).Stdout().Contains("running");
        public Task<bool> IsRunningAsync(CancellationToken cancellationToken = default)
            => IsRunningBuild().ExecuteAsync(cancellationToken, true).StdoutAsync().ContinueWith(t => t.Result.Contains("running"));
        public ProcessCommand Remove() => BuildLdconsoleDeviceCommand("remove");
        public ProcessCommand Rename(string newName) => BuildLdconsoleDeviceCommand("rename", $"--title \"{newName}\"");
        public ProcessCommand InstallAppFile(string fileName) => BuildLdconsoleDeviceCommand("installapp", $"--filename \"{fileName}\"");
        public ProcessCommand InstallAppPackage(string pakageName) => BuildLdconsoleDeviceCommand("installapp", $"--packagename {pakageName}");
        public ProcessCommand UninstallApp(string pakageName) => BuildLdconsoleDeviceCommand("uninstallapp", $"--packagename {pakageName}");
        public ProcessCommand RunApp(string pakageName) => BuildLdconsoleDeviceCommand("runapp", $"--packagename {pakageName}");

        public ProcessCommand KillApp(string pakageName) => BuildLdconsoleDeviceCommand("killapp", $"--packagename {pakageName}");
        public ProcessCommand Locatte(double lng, double lat) => BuildLdconsoleDeviceCommand("locate", $"--LLI {lng},{lat}");
        public ProcessCommand BackupApp(string pakageName, string pcPath) => BuildLdconsoleDeviceCommand("backupapp", $"--packagename {pakageName} --file \"{pcPath}\"");
        public ProcessCommand RestoreApp(string pakageName, string pcPath) => BuildLdconsoleDeviceCommand("restoreapp", $"--packagename {pakageName} --file \"{pcPath}\"");

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
