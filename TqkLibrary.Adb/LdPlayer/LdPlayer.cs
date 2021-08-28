using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.AdbDotNet.LdPlayer
{
  public class LdPlayer : IDisposable
  {
    internal static string _LdConsolePath = "ldconsole.exe";
    public static string LdConsolePath
    {
      get { return _LdConsolePath; }
      set { if (File.Exists(value)) _LdConsolePath = value; else throw new FileNotFoundException(value); }
    }
    public event LogCallback LogCommand;

    #region Static func
    public static string AdbCommand(string name, string command, CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
    {
      //https://stackoverflow.com/a/16018942/5034139
      command = command.WindowCharEscape();
      return ExecuteCommand($"adb --name \"{name}\" --command \"{command}\"", timeoutToken, cancelToken);
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

      string result = string.Empty;
      using (cancelToken.Register(() => process.Kill()))
      using (timeoutToken.Register(() => process.Kill()))
      {
        result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
      }
      cancelToken.ThrowIfCancellationRequested();
      if (timeoutToken.IsCancellationRequested) throw new LdPlayerTimeoutException(command);

      //string result = process.StandardOutput.ReadToEnd();
      string err = process.StandardError.ReadToEnd();
      if (process.ExitCode < 0) throw new LdPlayerException(result, err, command, process.ExitCode);
      else if (!string.IsNullOrEmpty(err))
      {
        Console.WriteLine($"LdConsole :" + command);
        Console.WriteLine($"\t\tStandardOutput:" + result);
        Console.WriteLine($"\t\tStandardError:" + err);
        //throw new AdbException(result, err, command);
      }
      return result;
    }
    string _ExecuteCommand(string command, CancellationToken timeoutToken)
    {
      LogCommand?.Invoke($"ldconsole {command}");
      return ExecuteCommand(command, timeoutToken, Adb.Token);
    }
    string _ExecuteCommand(string command, int timeout)
    {
      LogCommand?.Invoke($"ldconsole {command}");
      return ExecuteCommand(command,timeout, Adb.Token);
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
        .Select(x =>
      {
        var splits = x.Trim().Split(',');
        if (splits.Length == 7)
        {
          return new LdList2()
          {
            Index = int.Parse(splits[0]),
            Title = splits[1],
            TopWindowHandle = int.Parse(splits[2]),
            BindWindowHandle = int.Parse(splits[3]),
            AndroidStarted = int.Parse(splits[4]) == 1,
            ProcessId = int.Parse(splits[5]),
            ProcessIdOfVbox = int.Parse(splits[6])
          };
        }
        else return null;
      })
        .Where(x => x != null);
    }


    public static void Copy(string from, string newName, int timeout, CancellationToken cancelToken = default)
     => ExecuteCommand($"copy --name \"{newName}\" --from \"{from}\"", timeout, cancelToken);
    public static void Copy(string from,string newName, CancellationToken timeoutToken = default, CancellationToken cancelToken = default) 
      => ExecuteCommand($"copy --name \"{newName}\" --from \"{from}\"", timeoutToken, cancelToken);

    public static void Remove(string name, CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
      => ExecuteCommand($"remove --name \"{name}\"", timeoutToken, cancelToken);
    public static void Quit(string name, CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
      => ExecuteCommand($"quit --name \"{name}\"", timeoutToken, cancelToken);
    public static void Launch(string name, CancellationToken timeoutToken = default, CancellationToken cancelToken = default)
      => ExecuteCommand($"launch --name \"{name}\"", timeoutToken, cancelToken);


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


    public void Quit() => _ExecuteCommand($"quit --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault);
    public void Launch() => _ExecuteCommand($"launch --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault);
    public void Reboot() => _ExecuteCommand($"reboot --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault);
    public bool IsRunning() => _ExecuteCommand($"isrunning --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault).Contains("running");
    public void Remove() => _ExecuteCommand($"remove --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault);
    public bool RemoveUntillRemoved(CancellationToken timeoutToken = default)
    {
      while (List2().Any(x => Adb.DeviceId.Equals(x.Title)))
      {
        if (timeoutToken.IsCancellationRequested) return false;
        _ExecuteCommand($"remove --name \"{Adb.DeviceId}\"", Adb.TimeoutDefault);
        Adb.Delay(500);
      }
      return true;
    }

    public void Rename(string newName)
    {
      _ExecuteCommand($"rename --name \"{Adb.DeviceId}\" --title \"{newName}\"", Adb.TimeoutDefault);
      Adb.DeviceId = newName;
    }

    public bool RenameUntilRenamed(string newName, CancellationToken timeoutToken = default)
    {
      while(List2().Any(x => Adb.DeviceId.Equals(x.Title)))
      {
        if (timeoutToken.IsCancellationRequested) return false;
        _ExecuteCommand($"rename --name \"{Adb.DeviceId}\" --title \"{newName}\"", Adb.TimeoutDefault);
        Adb.Delay(500);
      }
      Adb.DeviceId = newName;
      return true;
    }

    public void InstallAppFile(string fileName) 
      => _ExecuteCommand($"installapp --name \"{Adb.DeviceId}\" --filename {fileName}", Adb.TimeoutDefault);
    public void InstallAppPackage(string pakageName) 
      => _ExecuteCommand($"installapp --name \"{Adb.DeviceId}\" --packagename {pakageName}", Adb.TimeoutDefault);
    public void UninstallApp(string pakageName) 
      => _ExecuteCommand($"uninstallapp --name \"{Adb.DeviceId}\" --packagename {pakageName}", Adb.TimeoutDefault);
    public void RunApp(string pakageName) 
      => _ExecuteCommand($"runapp --name \"{Adb.DeviceId}\" --packagename {pakageName}", Adb.TimeoutDefault);
    public void KillApp(string pakageName) 
      => _ExecuteCommand($"killapp --name \"{Adb.DeviceId}\" --packagename {pakageName}", Adb.TimeoutDefault);
    public void Locatte(double lng, double lat) 
      => _ExecuteCommand($"remove --name \"{Adb.DeviceId}\" --LLI {lng},{lat}", Adb.TimeoutDefault);
    public void BackupApp(string pakageName, string pcPath) 
      => _ExecuteCommand($"backupapp --name \"{Adb.DeviceId}\" --packagename {pakageName} --file \"{pcPath}\"", Adb.TimeoutDefault);
    public void RestoreApp(string pakageName, string pcPath) 
      => _ExecuteCommand($"restoreapp --name \"{Adb.DeviceId}\" --packagename {pakageName} --file \"{pcPath}\"", Adb.TimeoutDefault);

  }
}
