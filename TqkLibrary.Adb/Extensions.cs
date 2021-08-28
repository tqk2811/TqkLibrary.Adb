using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.AdbDotNet
{
  public static class Extensions
  {
    public static Bitmap ToBitMap(this byte[] buffer)
    {
      MemoryStream memoryStream = new MemoryStream(buffer);
      return (Bitmap)Bitmap.FromStream(memoryStream);
    }
    public static void SwipeSpeed(this Adb adb, int x1, int y1, int x2, int y2, int pixelPerSec = 1600)
    {
      int x = x2 - x1;
      int y = y2 - y1;
      double range = Math.Pow((double)(x * x + y * y), 0.5);
      double duration = 1000 * range / pixelPerSec;

      adb.Swipe(x1, y1, x2, y2, (int)duration);
    }

    public static void SwipeSpeed(this Adb adb, Point from, Point to, int pixelPerSec = 1600)
      => adb.SwipeSpeed(from.X, from.Y, to.X, to.Y, pixelPerSec);

   
    public static string AdbCharEscape(this string input)
    {
      return input
        .Replace(" ", "%s")
        .Replace("\\", "\\\\")
        .Replace("&", "\\&")
        .Replace("\"", "\\\\\"")
        .Replace("<", "\\<")
        .Replace(">", "\\>")
        .Replace("?", "\\?")
        .Replace("!", "\\!")
        .Replace(":", "\\:")
        .Replace("{", "\\{")
        .Replace("}", "\\}")
        .Replace("[", "\\[")
        .Replace("]", "\\]")
        .Replace("|", "\\|");
    }

    /// <summary>
    /// https://stackoverflow.com/a/16018942/5034139
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string WindowCharEscape(this string input)
    {
      return input
        .Replace("\"", "\"\"");
        //.Replace("<", "\\<")
        //.Replace(">", "\\>")
        //.Replace("?", "\\?")
        //.Replace("!", "\\!")
        ////.Replace(":", "\\:")
        //.Replace("{", "\\{")
        //.Replace("}", "\\}")
        //.Replace("[", "\\[")
        //.Replace("]", "\\]")
        //.Replace("|", "\\|");
    }
  }
}
