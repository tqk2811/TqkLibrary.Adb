using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.AdbDotNet
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessResult
    {
        /// <summary>
        /// 
        /// </summary>
        public int ExitCode { get; internal set; }
        internal byte[] _stdout;

        /// <summary>
        /// 
        /// </summary>
        public string Stdout() => Encoding.UTF8.GetString(_stdout);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string Stdout(Encoding encoding) => encoding.GetString(_stdout);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MemoryStream StdoutStream() => new MemoryStream(_stdout);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool EnsureSucess() => ExitCode == 0;
    }
    /// <summary>
    /// 
    /// </summary>
    public static class AdbResultExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adbResult"></param>
        /// <returns></returns>
        public static async Task<string> StdoutAsync(this Task<ProcessResult> adbResult)
        {
            ProcessResult result = await adbResult.ConfigureAwait(false);
            return result.Stdout();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="adbResult"></param>
        /// <returns></returns>
        public static async Task<bool> EnsureSucessAsync(this Task<ProcessResult> adbResult)
        {
            ProcessResult result = await adbResult.ConfigureAwait(false);
            return result.EnsureSucess();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="adbResult"></param>
        /// <returns></returns>
        public static async Task<MemoryStream> StdoutStreamAsync(this Task<ProcessResult> adbResult)
        {
            ProcessResult result = await adbResult.ConfigureAwait(false);
            return result.StdoutStream();
        }
    }
}
