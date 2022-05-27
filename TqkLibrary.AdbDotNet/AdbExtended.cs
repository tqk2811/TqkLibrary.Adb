using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.AdbDotNet
{
    /// <summary>
    /// 
    /// </summary>
    public static class AdbExtended
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adb"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Bitmap ScreenShotExecOut(this Adb adb, CancellationToken cancellationToken = default)
            => (Bitmap)Bitmap.FromStream(adb.BuildAdbDeviceCommand($"exec-out screencap -p").Execute(cancellationToken, true).StdoutStream());
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adb"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<Bitmap> ScreenShotExecOutAsync(this Adb adb, CancellationToken cancellationToken = default)
            => (Bitmap)Bitmap.FromStream(await adb.BuildAdbDeviceCommand($"exec-out screencap -p").ExecuteAsync(cancellationToken, true).StdoutStreamAsync().ConfigureAwait(false));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adb"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Bitmap ScreenShotPull(this Adb adb, CancellationToken cancellationToken = default)
        {
            string androidPath = $"/sdcard/{Guid.NewGuid()}.img_file";
            string pcPath = Path.Combine(Directory.GetCurrentDirectory(), $"{Guid.NewGuid()}.png");
            try
            {
                if (!adb.Shell.BuildShellCommand($"screencap -p {androidPath}").Execute(cancellationToken, true).EnsureSucess())
                    return null;
                if (!adb.PullFile(androidPath, pcPath, cancellationToken).EnsureSucess())
                    return null;
            }
            finally
            {
                adb.Shell.DeleteFile(androidPath);
            }

            if (File.Exists(pcPath))
            {
                try
                {
                    byte[] buff = File.ReadAllBytes(pcPath);
                    MemoryStream memoryStream = new MemoryStream(buff);
                    return (Bitmap)Bitmap.FromStream(memoryStream);
                }
                finally
                {
                    File.Delete(pcPath);
                }
            }
            else return null;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adb"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<Bitmap> ScreenShotPullAsync(this Adb adb, CancellationToken cancellationToken = default)
        {
            string androidPath = $"/sdcard/{Guid.NewGuid()}.screencap_file";
            string pcPath = Path.Combine(Directory.GetCurrentDirectory(), $"{Guid.NewGuid()}.png");
            try
            {
                if (!await adb.Shell.BuildShellCommand($"screencap -p {androidPath}").ExecuteAsync(cancellationToken, true).EnsureSucessAsync())
                    return null;
                if (!await adb.PullFileAsync(androidPath, pcPath, cancellationToken).EnsureSucessAsync())
                    return null;
            }
            finally
            {
                _ = adb.Shell.DeleteFileAsync(androidPath);
            }

            if (File.Exists(pcPath))
            {
                try
                {
#if NET5_0_OR_GREATER
                    byte[] buff = await File.ReadAllBytesAsync(pcPath, cancellationToken);
#else
                    byte[] buff = File.ReadAllBytes(pcPath);                    
#endif
                    MemoryStream memoryStream = new MemoryStream(buff);
                    return (Bitmap)Bitmap.FromStream(memoryStream);
                }
                finally
                {
                    File.Delete(pcPath);
                }
            }
            else return null;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adb"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AdbException"></exception>
        public static Point GetScreenResolution(this Adb adb, CancellationToken cancellationToken = default)
        {
            Regex regex = new Regex("(?<=mCurrentDisplayRect=Rect\\().*?(?=\\))", RegexOptions.Multiline);
            string result = adb.Shell.BuildShellCommand("dumpsys display | Find \"mCurrentDisplayRect\"").Execute(cancellationToken, true).Stdout();//
            Match match = regex.Match(result);
            if (match.Success)
            {
                result = match.Value;

                result = result.Substring(result.IndexOf("- ") + 2);
                string[] temp = result.Split(',');

                int x = Convert.ToInt32(temp[0].Trim());
                int y = Convert.ToInt32(temp[1].Trim());
                Point point = new Point(x, y);
                return point;
            }
            throw new AdbException(result, "shell dumpsys display failed", "shell dumpsys display | Find \"mCurrentDisplayRect\"", 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adb"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AdbException"></exception>
        public static async Task<Point> GetScreenResolutionAsync(this Adb adb, CancellationToken cancellationToken = default)
        {
            Regex regex = new Regex("(?<=mCurrentDisplayRect=Rect\\().*?(?=\\))", RegexOptions.Multiline);
            string result = await adb.Shell.BuildShellCommand("dumpsys display | Find \"mCurrentDisplayRect\"").ExecuteAsync(cancellationToken, true).StdoutAsync();//
            Match match = regex.Match(result);
            if (match.Success)
            {
                result = match.Value;

                result = result.Substring(result.IndexOf("- ") + 2);
                string[] temp = result.Split(',');

                int x = Convert.ToInt32(temp[0].Trim());
                int y = Convert.ToInt32(temp[1].Trim());
                Point point = new Point(x, y);
                return point;
            }
            throw new AdbException(result, "shell dumpsys display failed", "shell dumpsys display | Find \"mCurrentDisplayRect\"", 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adb"></param>
        /// <param name="isOn"></param>
        /// <param name="cancellationToken"></param>
        public static void PlanMode(this Adb adb, bool isOn, CancellationToken cancellationToken = default)
        {
            adb.BuildAdbDeviceCommand($"settings put global airplane_mode_on {(isOn ? "1" : "0")}").Execute(cancellationToken, true);
            adb.BuildAdbDeviceCommand("am broadcast -a android.intent.action.AIRPLANE_MODE").Execute(cancellationToken, true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adb"></param>
        /// <param name="isOn"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task PlanModeAsync(this Adb adb, bool isOn, CancellationToken cancellationToken = default)
        {
            await adb.BuildAdbDeviceCommand($"settings put global airplane_mode_on {(isOn ? "1" : "0")}").ExecuteAsync(cancellationToken, true).ConfigureAwait(false);
            await adb.BuildAdbDeviceCommand("am broadcast -a android.intent.action.AIRPLANE_MODE").ExecuteAsync(cancellationToken, true).ConfigureAwait(false);
        }
    }
}
