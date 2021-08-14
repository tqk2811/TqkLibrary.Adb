using System;

namespace TqkLibrary.AdbDotNet
{
  public class AdbException : Exception
  {
    public AdbException(string StandardOutput, string StandardError, string arguments, int exitCode) : base(StandardError)
    {
      this.StandardOutput = StandardOutput;
      this.StandardError = StandardError;
      this.Arguments = arguments;
      this.ExitCode = exitCode;
    }

    public string StandardOutput { get; }
    public string StandardError { get; }
    public string Arguments { get; }
    public int ExitCode { get; }
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
    public LdPlayerException(string StandardOutput, string StandardError, string arguments, int exitCode) : base(StandardError)
    {
      this.StandardOutput = StandardOutput;
      this.StandardError = StandardError;
      this.Arguments = arguments; 
      this.ExitCode = exitCode;
    }

    public string StandardOutput { get; }
    public string StandardError { get; }
    public string Arguments { get; }
    public int ExitCode { get; }
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