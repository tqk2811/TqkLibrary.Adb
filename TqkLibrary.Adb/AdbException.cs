using System;

namespace TqkLibrary.Adb
{
  public class AdbException : Exception
  {
    public AdbException(string messsage, string StandardOutput) : base(messsage)
    {
      this.StandardOutput = StandardOutput;
    }

    public string StandardOutput { get; }
  }

  public class AdbTimeoutException : Exception
  {
    public AdbTimeoutException()
    {

    }
  }
}