using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.AdbDotNet
{
  public static class AdbExtensions
  {
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
  }
}
