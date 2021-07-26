using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.Adb.LdPlayer
{
  public class LdProp
  {

  }
  public class LdPlayer : IDisposable
  {
    internal static string _LdConsolePath = "ldconsole.exe";
    public static string LdConsolePath
    {
      get { return _LdConsolePath; }
      set { if (File.Exists(value)) _LdConsolePath = value; else throw new FileNotFoundException(value); }
    }


    #region Static func
    internal static string AdbCommand(string name, string command, CancellationToken cancellationToken)
    {
      return ExecuteCommand($"adb --name {name} --command \"{command.Replace("\"","\\\"")}\"", cancellationToken);
    }

    static string ExecuteCommand(string command, CancellationToken cancellationToken, string ldConsolePath = null)
    {
      using Process process = new Process();
      process.StartInfo.FileName = string.IsNullOrEmpty(ldConsolePath) ? _LdConsolePath : ldConsolePath;
      process.StartInfo.WorkingDirectory = new FileInfo(process.StartInfo.FileName).DirectoryName;
      process.StartInfo.Arguments = command;
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.RedirectStandardInput = true;
      process.Start();

      using (cancellationToken.Register(() => process.Kill())) process.WaitForExit();
      cancellationToken.ThrowIfCancellationRequested();

      string result = process.StandardOutput.ReadToEnd();
      string err = process.StandardError.ReadToEnd();
      if (!string.IsNullOrEmpty(err))
      {
        Console.WriteLine($"LdConsole :" + command);
        Console.WriteLine($"\t\tStandardOutput:" + result);
        Console.WriteLine($"\t\tStandardError:" + err);
        //throw new AdbException(result, err, command);
      }
      return result;
    }

    public static void QuitAll(CancellationToken cancellationToken = default) => ExecuteCommand("quitall", cancellationToken);

    public static IEnumerable<string> List(CancellationToken cancellationToken = default)
    {
      string result = ExecuteCommand("list", cancellationToken);
      return result.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
    }

    public static IEnumerable<string> RunningList(CancellationToken cancellationToken = default)
    {
      string result = ExecuteCommand("runninglist", cancellationToken);
      return result.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
    }

    public static IEnumerable<LdList2> List2(CancellationToken cancellationToken = default)
    {
      string result = ExecuteCommand("list2", cancellationToken);
      return result
        .Split('\n')
        .Select(x => x.Trim())
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Select(x =>
      {
        var splits = x.Split(',');
        if (splits.Length == 7)
        {
          return new LdList2()
          {
            Index = int.Parse(splits[0]),
            Title = splits[1],
            TopWindowHandle = int.Parse(splits[2]),
            BindWindowHandle = int.Parse(splits[3]),
            AndroidStarted = int.Parse(splits[4]),
            ProcessId = int.Parse(splits[5]),
            ProcessIdOfVbox = int.Parse(splits[6])
          };
        }
        else return null;
      })
        .Where(x => x != null);
    }

    public static void Copy(string from,string newName, CancellationToken cancellationToken = default) => ExecuteCommand($"copy --name {newName} --from {from}", cancellationToken);

    public static void SortWnd(CancellationToken cancellationToken = default) => ExecuteCommand($"sortWnd", cancellationToken);
    #endregion



    public Adb Adb { get; }

    public LdPlayer(string name)
    {
      this.Adb = new Adb(name);
    }
    public void Stop()
    {
      Adb.Stop();
    }
    public void Dispose()
    {
      Adb.Dispose();
    }


    public void Quit() => ExecuteCommand($"quit --name \"{Adb.DeviceId}\"", Adb.CancellationToken);
    public void Launch() => ExecuteCommand($"launch --name \"{Adb.DeviceId}\"", Adb.CancellationToken);
    public void Reboot() => ExecuteCommand($"reboot --name \"{Adb.DeviceId}\"", Adb.CancellationToken);
    public bool IsRunning() => ExecuteCommand($"isrunning --name \"{Adb.DeviceId}\"", Adb.CancellationToken).Contains("running");
    public void Remove() => ExecuteCommand($"remove --name \"{Adb.DeviceId}\"", Adb.CancellationToken);
    public void Rename(string newName)
    {
      ExecuteCommand($"rename --name \"{Adb.DeviceId}\" --title \"{newName}\"", Adb.CancellationToken);
      Adb.DeviceId = newName;
    }

    public void InstallAppFile(string fileName) => ExecuteCommand($"installapp --name \"{Adb.DeviceId}\" --filename {fileName}", Adb.CancellationToken);
    public void InstallAppPackage(string pakageName) => ExecuteCommand($"installapp --name \"{Adb.DeviceId}\" --packagename {pakageName}", Adb.CancellationToken);
    public void UninstallApp(string pakageName) => ExecuteCommand($"uninstallapp --name \"{Adb.DeviceId}\" --packagename {pakageName}", Adb.CancellationToken);
    public void RunApp(string pakageName) => ExecuteCommand($"runapp --name \"{Adb.DeviceId}\" --packagename {pakageName}", Adb.CancellationToken);
    public void KillApp(string pakageName) => ExecuteCommand($"killapp --name \"{Adb.DeviceId}\" --packagename {pakageName}", Adb.CancellationToken);
    public void Locatte(double lng, double lat) => ExecuteCommand($"remove --name \"{Adb.DeviceId}\" --LLI {lng},{lat}", Adb.CancellationToken);

    public void BackupApp(string pakageName, string pcPath) => ExecuteCommand($"backupapp --name \"{Adb.DeviceId}\" --packagename {pakageName} --file {pcPath}", Adb.CancellationToken);
    public void RestoreApp(string pakageName, string pcPath) => ExecuteCommand($"restoreapp --name \"{Adb.DeviceId}\" --packagename {pakageName} --file {pcPath}", Adb.CancellationToken);

  }
}
