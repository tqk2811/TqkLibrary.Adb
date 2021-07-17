using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.Adb
{

  public delegate void AdbLog(string log);

  public class Adb : IDisposable
  {
    private static string _AdbPath = "adb.exe";

    public static string AdbPath
    {
      get { return _AdbPath; }
      set
      {
        if (File.Exists(value)) _AdbPath = value;
        else throw new FileNotFoundException(value);
      }
    }

    public bool LogDelay { get; set; } = false;
    public int TimeoutDefault { get; set; } = 30000;
    private readonly string adbPath;
    private readonly Random rd = new Random();
    protected CancellationTokenSource TokenSource;

    public CancellationToken CancellationToken { get { return TokenSource.Token; } }

    public readonly string DeviceId;

    public event AdbLog LogCommand;


    public bool IsLd { get; private set; } = false;
    internal Adb(string ldName)
    {
      IsLd = true;
      DeviceId = ldName; 
      TokenSource = new CancellationTokenSource();
    }

    public Adb(string deviceId = null, string adbPath = null)
    {
      this.DeviceId = deviceId;
      this.adbPath = adbPath;
      if (File.Exists(adbPath)) _AdbPath = adbPath;
      TokenSource = new CancellationTokenSource();
    }

    #region Static

    public static void KillServer(int timeout = 30000) => ExecuteCommand("adb kill-server", timeout);

    public static void StartServer(int timeout = 30000) => ExecuteCommand("adb start-server", timeout);

    public static List<string> Devices(DeviceState state = DeviceState.All)
    {
      string input = ExecuteCommand("devices");
      var lines = Regex.Split(input, "\r\n").Skip(1).Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
      if(state != DeviceState.All)
      {
        var states = state.ToString().ToLower().Split(',').Select(x => x.Trim()).ToList();
        return lines.Where(x => states.Any(y => x.EndsWith(y))).Select(x => x.Split('\t').First().Trim()).ToList();
      }
      else return lines;
    }

    public static string ExecuteCommand(string command, int timeout = 30000, string adbPath = null)
    {
      using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
      return ExecuteCommand(command, cancellationTokenSource.Token, adbPath);
    }

    public static string ExecuteCommand(string command, CancellationToken cancellationToken, string adbPath = null)
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

      using (cancellationToken.Register(() => process.Kill())) process.WaitForExit();
      cancellationToken.ThrowIfCancellationRequested();

      string result = process.StandardOutput.ReadToEnd();
      string err = process.StandardError.ReadToEnd().Trim();
      if (!string.IsNullOrEmpty(err))
      {
        if (err.StartsWith("Error:") || err.StartsWith("Fatal:") || err.StartsWith("Silent:")) throw new AdbException(result, err, command);
        else
        {
          Console.WriteLine($"AdbCommand:\t" + command);
          Console.WriteLine($"StandardOutput:\t" + result);
          Console.WriteLine($"StandardError:\t" + err);
        }
      }
      return result;
    }

    public static string ExecuteCommandCmd(string command, int timeout = 30000, string adbPath = null)
    {
      using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
      return ExecuteCommandCmd(command, cancellationTokenSource.Token, adbPath);
    }

    public static string ExecuteCommandCmd(string command, CancellationToken cancellationToken, string adbPath = null)
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
      using (cancellationToken.Register(() => process.Kill())) process.WaitForExit();

      string result = process.StandardOutput.ReadToEnd();
      string err = process.StandardError.ReadToEnd().Trim();
      if (!string.IsNullOrEmpty(err))
      {
        if (err.StartsWith("Error:") || err.StartsWith("Fatal:") || err.StartsWith("Silent:")) throw new AdbException(result, err, command);
        else
        {
          Console.WriteLine($"AdbCommand:\t" + command);
          Console.WriteLine($"StandardOutput:\t" + result);
          Console.WriteLine($"StandardError:\t" + err);
        }
      }
      return result;
    }

    #endregion

    #region mainMethod
    public void Stop() => TokenSource.Cancel();

    public void Delay(int value)
    {
      if(LogDelay) LogCommand?.Invoke($"Delay {value}ms");
      Task.Delay(value, CancellationToken).Wait();
    }

    public void Delay(int min, int max) => Delay(rd.Next(min, max));

    public void Dispose() => TokenSource.Dispose();


    public string AdbCommand(string command)
      => AdbCommand(command, TimeoutDefault);

    public string AdbCommand(string command, int timeout)
    {
      CancellationToken.ThrowIfCancellationRequested();
      if(IsLd)
      {
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
        using (CancellationToken.Register(() => cancellationTokenSource.Cancel()))
        {
          return LDPlayerCommandHelper.Adb(DeviceId, command, cancellationTokenSource.Token);
        }
      }
      else
      {
        string adbLocation = string.IsNullOrEmpty(adbPath) ? AdbPath : adbPath;
        LogCommand?.Invoke("adb " + command);
        string commands = string.IsNullOrEmpty(DeviceId) ? command : $"-s {DeviceId} {command}";
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
        using (CancellationToken.Register(() => cancellationTokenSource.Cancel()))
        {
          return ExecuteCommand(commands, cancellationTokenSource.Token, adbLocation);
        }
      }
    }


    public string AdbCommandCmd(string command)
      => AdbCommandCmd(command, TimeoutDefault);

    public string AdbCommandCmd(string command, int timeout)
    {
      CancellationToken.ThrowIfCancellationRequested();
      if (IsLd)
      {
        throw new NotSupportedException(command);
      }
      else
      {
        string adbLocation = string.IsNullOrEmpty(adbPath) ? AdbPath : adbPath;
        string commands = string.IsNullOrEmpty(DeviceId) ? command : $"-s {DeviceId} {command}";
        LogCommand?.Invoke(commands);
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(timeout);
        using (CancellationToken.Register(() => cancellationTokenSource.Cancel()))
        {
          return ExecuteCommandCmd(commands, cancellationTokenSource.Token, adbLocation);
        }
      }
    }

    #endregion


    public void Root() => AdbCommand("root");

    public void UnRoot() => AdbCommand("unroot");

    public void WaitFor(WaitForType type, int timeout = 120000) => AdbCommand($"wait-for-{type.ToString().ToLower()}", timeout);

    public void Shutdown() => AdbCommand("shell reboot -p");

    public void Reboot() => AdbCommand("shell reboot");

    public void RebootRecovery() => AdbCommand("shell reboot-recovery");

    public void RebootBootLoader() => AdbCommand("shell reboot-bootloader");

    public void FastBoot() => AdbCommand("shell fastboot");

    public void PushFile(string pcPath, string androidPath) => AdbCommand($"push \"{pcPath}\" \"{androidPath}\"");

    public void PushFile(string pcPath, string androidPath, int timeout) => AdbCommand($"push \"{pcPath}\" \"{androidPath}\"", timeout);

    public void PullFile(string androidPath, string pcPath) => AdbCommand($"pull \"{androidPath}\" \"{pcPath}\"");

    public void PullFile(string androidPath, string pcPath, int timeout) => AdbCommand($"pull \"{androidPath}\" \"{pcPath}\"", timeout);

    public void DeleteFile(string androidPath) => AdbCommand($"shell rm \"{androidPath}\"");

    public void InstallApk(string pcPath) => AdbCommand($"install \"{pcPath}\"");

    public void InstallApk(string pcPath, int timeout = 30000) => AdbCommand($"install \"{pcPath}\"", timeout);

    public void UpdateApk(string pcPath) => AdbCommand($"install -r \"{pcPath}\"");

    public void UpdateApk(string pcPath, int timeout = 30000) => AdbCommand($"install -r \"{pcPath}\"", timeout);

    /// am [start|instrument]<br/>
    /// am start[-a <action>] [-d <data_uri>][-t <mime_type>] [-c <category> [-c <category>] ...][-e <extra_key> <extra_value>[-e <extra_key> <extra_value> ...][-n <component>][-D][<uri>]
    /// am instrument[-e <arg_name> <arg_value>] [-p <prof_file>] [-w] <component>
    //https://developer.android.com/studio/command-line/adb#IntentSpec
    /// <summary>
    /// Example: com.google.android.gms/.accountsettings.mg.ui.main.MainActivity<br/>
    /// OpenApk("com.google.android.gms/.accountsettings.mg.ui.main.MainActivity");<br/>
    /// </summary>
    public void OpenApk(string n) => AdbCommand($"shell am start -n {n}");

    public void OpenApk(string n, string d) => AdbCommand($"shell am start -d {d} -n {n}");

    public void OpenApk(string n, string d, string a) => AdbCommand($"shell am start -d {d} -n {n} -a {a}");

    public void OpenApk(AdbShellAMStart adbShellAMStart) => AdbCommand($"shell am start {adbShellAMStart.GetCommand()}");

    public void DisableApk(string packageName) => AdbCommand($"shell pm disable {packageName}");

    public void EnableApk(string packageName) => AdbCommand($"shell pm enable {packageName}");

    public void UnInstallApk(string packageName) => AdbCommand($"uninstall {packageName}");

    public void ForceStopApk(string packageName) => AdbCommand($"shell am force-stop {packageName}");

    public void ClearApk(string packageName) => AdbCommand($"shell pm clear {packageName}");

    public void SetProxy(string proxy, int timeout) => AdbCommand($"shell settings put global http_proxy {proxy}", timeout);

    public void SetProxy(string proxy) => AdbCommand($"shell settings put global http_proxy {proxy}", TimeoutDefault);

    public void ClearProxy() => AdbCommand("shell settings put global http_proxy :0");

    public Bitmap ScreenShot(string FilePath = null, bool deleteInAndroid = true)
    {
      bool IsDelete = false;
      if (string.IsNullOrEmpty(FilePath))
      {
        FilePath = (string.IsNullOrEmpty(DeviceId) ? Guid.NewGuid().ToString() : DeviceId.Replace(":", "_")) + ".png";
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

    Point? point = null;
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
        throw new AdbException(result, "shell dumpsys display failed", "shell dumpsys display | Find \"mCurrentDisplayRect\"");
      }
      else return point.Value;
    }

    public IEnumerable<string> ListActivities(string packageName)
    {
      string result = AdbCommandCmd($"shell dumpsys package | Find \"{packageName}/\" | Find \"Activity\"");//shell dumpsys package | Find "com.km.karaoke/" | Find "Activity"
      var lines = result.Split('\r');
      return lines.Select(x => x.Trim());
    }



    public void Tap(int x, int y, int count = 1)
    {
      for (int i = 0; i < count; i++) AdbCommand($"shell input tap {x} {y}");
    }

    public void TapByPercent(double x, double y, int count = 1)
    {
      var resolution = GetScreenResolution();
      int X = (int)(x * resolution.X);
      int Y = (int)(y * resolution.Y);
      Tap(X, Y, count);
    }

    public void Swipe(int x1, int y1, int x2, int y2, int duration = 100) => AdbCommand($"shell input swipe {x1} {y1} {x2} {y2} {duration}");

    public void SwipeByPercent(double x1, double y1, double x2, double y2, int duration = 100)
    {
      var resolution = GetScreenResolution();

      int X1 = (int)(x1 * resolution.X);
      int Y1 = (int)(y1 * resolution.Y);
      int X2 = (int)(x2 * resolution.X);
      int Y2 = (int)(y2 * resolution.Y);

      Swipe(X1, Y1, X2, Y2, duration);
    }

    public void LongPress(int x, int y, int duration = 100) => AdbCommand($"shell input swipe {x} {y} {x} {y} {duration}");

    public void Key(ADBKeyEvent key) => AdbCommand($"shell input keyevent {(int)key}");

    public void Key(int keyCode) => AdbCommand($"shell input keyevent {keyCode}");

    public void InputText(string text)
    {
      string text_fix = text
        .Replace(" ", "%s")
        .Replace("&", "\\&")
        .Replace("<", "\\<")
        .Replace(">", "\\>")
        .Replace("?", "\\?")
        .Replace(":", "\\:")
        .Replace("{", "\\{")
        .Replace("}", "\\}")
        .Replace("[", "\\[")
        .Replace("]", "\\]")
        .Replace("|", "\\|");
      AdbCommand($"shell input text \"{text_fix}\"");
    }

    public void PlanModeON()
    {
      AdbCommand("settings put global airplane_mode_on 1");
      AdbCommand("am broadcast -a android.intent.action.AIRPLANE_MODE");
    }

    public void PlanModeOFF()
    {
      AdbCommand("settings put global airplane_mode_on 0");
      AdbCommand("am broadcast -a android.intent.action.AIRPLANE_MODE");
    }
  }
}