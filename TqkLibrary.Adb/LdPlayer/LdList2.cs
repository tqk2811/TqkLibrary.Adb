using System;

namespace TqkLibrary.AdbDotNet.LdPlayer
{
  public class LdList2
  {
    public int Index { get; set; }
    public string Title { get; set; }
    public IntPtr TopWindowHandle { get; set; }
    public IntPtr BindWindowHandle { get; set; }
    public bool AndroidStarted { get; set; }
    public int ProcessId { get; set; }
    public int ProcessIdOfVbox { get; set; }

    public override string ToString()
    {
      return Title;

    }

    public bool IsLdClosed
    {
      get { return ProcessId == -1 && ProcessIdOfVbox == -1 && TopWindowHandle == IntPtr.Zero && BindWindowHandle == IntPtr.Zero && !AndroidStarted; }
    }
  }
}
