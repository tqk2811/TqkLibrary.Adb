using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TqkLibrary.AdbSocket
{  
  public class Adb
  {
    readonly int port;
    public Adb(int port = 5037)
    {
      this.port = port;
    }

    static readonly List<string> StartArgs = new List<string>()
    { 
      /* adb_connect() commands */
      "devices",
      "connect",
      "disconnect",
      "emu",
      "hell", "shell",
      "kill-server",
      "remount", "reboot",
      "bugreport",
      /* adb_command() wrapper commands */
      "wait-for-",
      "forward",
      /* do_sync_*() commands */
      "ls",
      "push",
      "pull",
      "install",
      "uninstall",
      "sync",
      /* passthrough commands */
      "get-state", "get-serialno",
      /* other commands */
      "status-window",
      "logcat", "lolcat",
      "ppp",
      "start-server",
      "backup",
      "restore",
      "jdwp"
    };
    public string Command(string command)
    {
      var args = CommandLineParser.SplitCommandLineIntoArguments(command, true).ToList();
      if (args.Count == 0) return string.Empty;
      for(int i = 0; i < StartArgs.Count; i++)
      {
        if (args[0].StartsWith(StartArgs[i]))
        {
          switch (StartArgs[i])
          {
            /* adb_connect() commands */
            case "devices":
              {

                break;
              }
            case "connect":
              {

                break;
              }
            case "disconnect":
              {

                break;
              }
            case "emu":
              {

                break;
              }
            case "hell":
            case "shell":
              {

                break;
              }
            case "kill-server":
              {

                break;
              }

            case "remount":
            case "reboot":
              {

                break;
              }
            case "bugreport":
              {

                break;
              }
            /* adb_command() wrapper commands */
            case "wait-for-":
              {
                break;
              }
            case "forward":
              {
                break;
              }
            /* do_sync_*() commands */
            case "ls":
              {
                break;
              }
            case "push":
              {
                break;
              }
            case "pull":
              {
                break;
              }
            case "install":
              {
                break;
              }
            case "uninstall":
              {
                break;
              }
            case "sync":
              {
                break;
              }
            /* passthrough commands */
            case "get-state":
            case "get-serialno":
              {
                break;
              }
            /* other commands */
            case "status-window":
              {
                break;
              }
            case "logcat":
            case "lolcat":
              {
                break;
              }
            case "ppp":
              {
                break;
              }
            case "start-server":
              {
                break;
              }
            case "backup":
              {
                break;
              }
            case "restore":
              {
                break;
              }
            case "jdwp":
              {
                break;
              }
          }
          return string.Empty;
        }
      }
      return string.Empty;
    }


    byte[] BuildPackage()
    {
      return null;
    }

    void SendPacket(byte[] pack)
    {

    }
  }
}
