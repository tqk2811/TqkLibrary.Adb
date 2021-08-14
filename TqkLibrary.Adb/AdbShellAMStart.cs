using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.AdbDotNet
{
  public class AdbShellAMStart
  {
    public string a { get; set; }
    public string n { get; set; }
    public string d { get; set; }

    internal string GetCommand()
    {
      List<string> args = new List<string>();
      if (!string.IsNullOrEmpty(a)) args.Add($"-a {a}");
      if (!string.IsNullOrEmpty(n)) args.Add($"-n {n}");
      if (!string.IsNullOrEmpty(d)) args.Add($"-d {d}");
      if (args.Count == 0) throw new ArgumentNullException("AdbShellAMStart");
      return string.Join(" ", args);
    }
  }
}
