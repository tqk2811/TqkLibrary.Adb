using System;

namespace TqkLibrary.Adb
{
  public class AdbException : Exception
  {
    public AdbException(string StandardOutput, string StandardError, string StandardIn) : base(StandardError)
    {
      this.StandardOutput = StandardOutput;
      this.StandardError = StandardError;
      this.StandardIn = StandardIn;
    }

    public string StandardOutput { get; }
    public string StandardError { get; }
    public string StandardIn { get; }
  }

  public class AdbTimeoutException : Exception
  {
    public AdbTimeoutException()
    {

    }
  }
}