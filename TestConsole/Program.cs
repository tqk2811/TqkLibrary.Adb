using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Adb.LdPlayer;

namespace TestConsole
{
  class Program
  {
    static void Main(string[] args)
    {
      LdPlayer.LdConsolePath = @"D:\Program Files\LDPlayer\LDPlayer4.0\ldconsole.exe";
      //var list = LdPlayer.List().ToList();
      //var list2 = LdPlayer.List2().ToList();

      LdPlayer ldPlayer = new LdPlayer("tiktok 2aa");
      ldPlayer.Adb.ScreenShot();
    }
  }
}
