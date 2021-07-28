using System;

namespace TqkLibrary.Adb
{
  public class AdbException : Exception
  {
    public AdbException(string StandardOutput, string StandardError, string arguments) : base(StandardError)
    {
      this.StandardOutput = StandardOutput;
      this.StandardError = StandardError;
      this.Arguments = arguments;
    }

    public string StandardOutput { get; }
    public string StandardError { get; }
    public string Arguments { get; }
  }

  public class AdbTimeoutException : Exception
  {
    public AdbTimeoutException(string arguments)
    {
      this.Arguments = arguments;
    }
    public string Arguments { get; }
  }





  public class LdPlayerException : Exception
  {
    public LdPlayerException(string StandardOutput, string StandardError, string arguments) : base(StandardError)
    {
      this.StandardOutput = StandardOutput;
      this.StandardError = StandardError;
      this.Arguments = arguments;
    }

    public string StandardOutput { get; }
    public string StandardError { get; }
    public string Arguments { get; }
  }

  public class LdPlayerTimeoutException : Exception
  {
    public LdPlayerTimeoutException(string arguments)
    {
      this.Arguments = arguments;
    }
    public string Arguments { get; }
  }
}