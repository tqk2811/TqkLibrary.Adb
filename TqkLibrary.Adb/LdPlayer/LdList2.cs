namespace TqkLibrary.AdbDotNet.LdPlayer
{
  public class LdList2
  {
    public int Index { get; set; }
    public string Title { get; set; }
    public int TopWindowHandle { get; set; }
    public int BindWindowHandle { get; set; }
    public bool AndroidStarted { get; set; }
    public int ProcessId { get; set; }
    public int ProcessIdOfVbox { get; set; }

    public override string ToString()
    {
      return Title;

    }

    public bool IsLdClosed
    {
      get { return ProcessId == -1 && ProcessIdOfVbox == -1 && TopWindowHandle == 0 && BindWindowHandle == 0 && !AndroidStarted; }
    }
  }
}
