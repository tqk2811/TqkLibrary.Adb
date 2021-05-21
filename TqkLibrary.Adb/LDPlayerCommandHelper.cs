using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.Adb
{
  public static class LDPlayerCommandHelper
  {
    internal static string LdConsolePath = "ldconsole.exe";
    public static void LdConsoleInit(string path)
    {
      if (File.Exists(path)) LdConsolePath = path;
      else throw new FileNotFoundException(path);
    }

    public static void Quit(int timeout)
    {
      using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
      Quit(cancellationTokenSource.Token);
    }
    public static void Quit(CancellationToken cancellationToken = default) => ExecuteCommand("quit", cancellationToken);


    public static void QuitAll(int timeout)
    {
      using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
      QuitAll(cancellationTokenSource.Token);
    }
    public static void QuitAll(CancellationToken cancellationToken = default) => ExecuteCommand("quitall", cancellationToken);


    public static void Launch(string name, int timeout)
    {
      using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
      Launch(name, cancellationTokenSource.Token);
    }
    public static void Launch(string name, CancellationToken cancellationToken = default)
    {
      ExecuteCommand($"launch --name {name}", cancellationToken);
    }


    public static void Reboot(string name, int timeout)
    {
      using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
      Reboot(name, cancellationTokenSource.Token);
    }
    public static void Reboot(string name, CancellationToken cancellationToken = default)
    {
      ExecuteCommand($"reboot --name {name}", cancellationToken);
    }


    public static IEnumerable<string> List(int timeout)
    {
      using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
      return List(cancellationTokenSource.Token);
    }
    public static IEnumerable<string> List(CancellationToken cancellationToken = default)
    {
      string result = ExecuteCommand("list", cancellationToken);
      return result.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
    }


    public static IEnumerable<string> RunningList(int timeout)
    {
      using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
      return RunningList(cancellationTokenSource.Token);
    }
    public static IEnumerable<string> RunningList(CancellationToken cancellationToken = default)
    {
      string result = ExecuteCommand("runninglist", cancellationToken);
      return result.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
    }


    public static bool IsRunning(string name, int timeout)
    {
      using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
      return IsRunning(name, cancellationTokenSource.Token);
    }
    public static bool IsRunning(string name, CancellationToken cancellationToken = default)
    {
      string result = ExecuteCommand($"isrunning --name {name}", cancellationToken);
      return result.Contains("running");
    }


    public static IEnumerable<string> List2(int timeout)
    {
      using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
      return List2(cancellationTokenSource.Token);
    }
    public static IEnumerable<string> List2(CancellationToken cancellationToken = default)
    {
      string result = ExecuteCommand("list2", cancellationToken);
      return result.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
    }


    //add


    public static void Copy(string from, string name, int timeout)
    {
      using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
      Copy(from, name, cancellationTokenSource.Token);
    }
    public static void Copy(string from,string name, CancellationToken cancellationToken = default)
    {
      ExecuteCommand($"copy --name {name} --from {from}", cancellationToken);
    }


    public static void Remove(string name, int timeout)
    {
      using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
      Remove(name, cancellationTokenSource.Token);
    }
    public static void Remove(string name, CancellationToken cancellationToken = default)
    {
      ExecuteCommand($"remove --name {name}", cancellationToken);
    }


    /// <summary>
    /// Ld still bad when deviceId random change
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Adb InitAdb(string name)
    {
      Adb baseAdb = new Adb(name);
      return baseAdb;
    }


    internal static string Adb(string name,string command, CancellationToken cancellationToken)
    {
      return ExecuteCommand($"adb --name {name} --command {command}", cancellationToken);
    }



    static string ExecuteCommand(string command, CancellationToken cancellationToken, string ldConsolePath = null)
    {
      using Process process = new Process();
      process.StartInfo.FileName = string.IsNullOrEmpty(ldConsolePath) ? LdConsolePath : ldConsolePath;
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
      Console.WriteLine($"Command: {command}; Result: {result}");
      string err = process.StandardError.ReadToEnd();
      if (!string.IsNullOrEmpty(err))
      {
        throw new AdbException(result, err, command);
      }
      return result;
    }
  }
}
