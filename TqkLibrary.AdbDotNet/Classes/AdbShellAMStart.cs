using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.AdbDotNet.Classes
{
    /// <summary>
    /// 
    /// </summary>
    public class AdbShellAMStart
    {
        /// <summary>
        /// 
        /// </summary>
        public string? A { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? N { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? D { get; set; }

        internal string GetCommand()
        {
            List<string> args = new List<string>();
            if (!string.IsNullOrEmpty(A)) args.Add($"-a {A}");
            if (!string.IsNullOrEmpty(N)) args.Add($"-n {N}");
            if (!string.IsNullOrEmpty(D)) args.Add($"-d {D}");
            if (args.Count == 0) throw new ArgumentNullException("AdbShellAMStart");
            return string.Join(" ", args);
        }
    }
}
