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
    internal static string AdbCommand(string name, string command, CancellationToken timeoutToken, CancellationToken cancelToken)
    {
      return ExecuteCommand($"adb --name {name} --command \"{command.Replace("\"","\\\"")}\"", timeoutToken, cancelToken);
    }

    static string ExecuteCommand(string command, int timeout, CancellationToken cancelToken, string ldConsolePath = null)
    {
      using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
      return ExecuteCommand(command, cancellationTokenSource.Token, cancelToken, ldConsolePath);
    }
    static string ExecuteCommand(string command, CancellationToken timeoutToken, CancellationToken cancelToken, string ldConsolePath = null)
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

      using (cancelToken.Register(() => process.Kill()))
      using (timeoutToken.Register(() => process.Kill())) 
        process.WaitForExit();
      cancelToken.ThrowIfCancellationRequested();
      if (timeoutToken.IsCancellationRequested) throw new LdPlayerTimeoutException(command);

      string result = process.StandardOutput.ReadToEnd();
      string err = process.StandardError.ReadToEnd();
      if (process.ExitCode != 0) throw new LdPlayerException(result, err, command);
      else if (!string.IsNullOrEmpty(err))
      {
        Console.WriteLine($"LdConsole :" + command);
        Console.WriteLine($"\t\tStandardOutput:" + result);
        Console.WriteLine($"\t\tStandardError:" + err);
        //throw new AdbException(result, err, command);
      }
      return result;
    }


    public static void QuitAll(int timeout, CancellationToken cancelToken = default)
      => ExecuteCommand("quitall", timeout, cancelToken);
    public static void QuitAll(CancellationToken timeoutToken = default, CancellationToken cancelToken = default) 
      => ExecuteCommand("quitall", timeoutToken, cancelToken);


    public static IEnumerable<string> List(int timeout, CancellationToken cancelToken = default)
    {
      string result = ExecuteCommand("list", timeout, cancelToken);
      return result.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
    }
    public static IEnumerable<string> List(CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
    {
      string result = ExecuteCommand("list", timeoutToken, cancelToken);
      return result.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
    }


    public static IEnumerable<string> RunningList(int timeout, CancellationToken cancelToken = default)
    {
      string result = ExecuteCommand("runninglist", timeout, cancelToken);
      return result.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
    }
    public static IEnumerable<string> RunningList(CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
    {
      string result = ExecuteCommand("runninglist", timeoutToken, cancelToken);
      return result.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
    }


    public static IEnumerable<LdList2> List2(int timeout, CancellationToken cancelToken = default)
    {
      using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
      return List2(cancellationTokenSource.Token, cancelToken);
    }
    public static IEnumerable<LdList2> List2(CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
    {
      string result = ExecuteCommand("list2", timeoutToken, cancelToken);
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


    public static void Copy(string from, string newName, int timeout, CancellationToken cancelToken = default)
     => ExecuteCommand($"copy --name {newName} --from {from}", timeout, cancelToken);
    public static void Copy(string from,string newName, CancellationToken timeoutToken = default, CancellationToken cancelToken = default) 
      => ExecuteCommand($"copy --name {newName} --from {from}", timeoutToken, cancelToken);


    public static void SortWnd(int timeout, CancellationToken cancelToken = default)
      => ExecuteCommand($"sortWnd", timeout, cancelToken);
    public static void SortWnd(CancellationToken timeoutToken = default, CancellationToken cancelToken = default) 
      => ExecuteCommand($"sortWnd", timeoutToken, cancelToken);
    #endregion



    public Adb Adb { get; }
    public int TimeoutDefault
    {
      get { return Adb.TimeoutDefault; }
      set { Adb.TimeoutDefault = value; }
    }

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


    public void Quit() => ExecuteCommand($"quit --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault, Adb.CancellationToken);
    public void Launch() => ExecuteCommand($"launch --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault, Adb.CancellationToken);
    public void Reboot() => ExecuteCommand($"reboot --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault, Adb.CancellationToken);
    public bool IsRunning() => ExecuteCommand($"isrunning --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault, Adb.CancellationToken).Contains("running");
    public void Remove() => ExecuteCommand($"remove --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault, Adb.CancellationToken);
    public void Rename(string newName)
    {
      ExecuteCommand($"rename --name \"{Adb.DeviceId}\" --title \"{newName}\"", Adb.TimeoutDefault, Adb.CancellationToken);
      Adb.DeviceId = newName;
    }

    public void InstallAppFile(string fileName) 
      => ExecuteCommand($"installapp --name \"{Adb.DeviceId}\" --filename {fileName}", Adb.TimeoutDefault, Adb.CancellationToken);
    public void InstallAppPackage(string pakageName) 
      => ExecuteCommand($"installapp --name \"{Adb.DeviceId}\" --packagename {pakageName}", Adb.TimeoutDefault, Adb.CancellationToken);
    public void UninstallApp(string pakageName) 
      => ExecuteCommand($"uninstallapp --name \"{Adb.DeviceId}\" --packagename {pakageName}", Adb.TimeoutDefault, Adb.CancellationToken);
    public void RunApp(string pakageName) 
      => ExecuteCommand($"runapp --name \"{Adb.DeviceId}\" --packagename {pakageName}", Adb.TimeoutDefault, Adb.CancellationToken);
    public void KillApp(string pakageName) 
      => ExecuteCommand($"killapp --name \"{Adb.DeviceId}\" --packagename {pakageName}", Adb.TimeoutDefault, Adb.CancellationToken);
    public void Locatte(double lng, double lat) 
      => ExecuteCommand($"remove --name \"{Adb.DeviceId}\" --LLI {lng},{lat}", Adb.TimeoutDefault, Adb.CancellationToken);
    public void BackupApp(string pakageName, string pcPath) 
      => ExecuteCommand($"backupapp --name \"{Adb.DeviceId}\" --packagename {pakageName} --file {pcPath}", Adb.TimeoutDefault, Adb.CancellationToken);
    public void RestoreApp(string pakageName, string pcPath) 
      => ExecuteCommand($"restoreapp --name \"{Adb.DeviceId}\" --packagename {pakageName} --file {pcPath}", Adb.TimeoutDefault, Adb.CancellationToken);

  }
}
