namespace TqkLibrary.Adb.LdPlayer
{
  public class LdList2
  {
    public int Index { get; set; }
    public string Title { get; set; }
    public int TopWindowHandle { get; set; }
    public int BindWindowHandle { get; set; }
    public int AndroidStarted { get; set; }
    public int ProcessId { get; set; }
    public int ProcessIdOfVbox { get; set; }

    public override string ToString()
    {
      return Title;

    }
  }
}
