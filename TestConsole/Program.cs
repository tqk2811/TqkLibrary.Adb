using Cysharp.Diagnostics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.AdbDotNet;
using TqkLibrary.AdbDotNet.LdPlayer;

namespace TestConsole
{
  class Program
  {
    static void Main(string[] args)
    {
      TestScreenShot2();

      //LdPlayer.LdConsolePath = @"D:\Program Files\LDPlayer\LDPlayer4.0\ldconsole.exe";
      ////var list = LdPlayer.List().ToList();
      ////var list2 = LdPlayer.List2().ToList();

      //LdPlayer ldPlayer = new LdPlayer("cookingbet1021@gmail.com");
      //ldPlayer.Adb.LogCommand += Adb_LogCommand;
      ////var list = LdPlayer.List().ToList();
      //Bitmap bitmap = ldPlayer.Adb.ScreenShot();
    }

    private static void Adb_LogCommand(string log)
    {
      Console.WriteLine(log);
    }



    static void TestScreenShot2()
    {
      Adb adb = new Adb("a29bc285");
      adb.LogCommand += Adb_LogCommand;
      adb.ScreenShot2().Save(@"D:\temp\test.png");
    }
  }
}
