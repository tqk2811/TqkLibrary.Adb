using System;

namespace TqkLibrary.AdbDotNet
{
    /// <summary>
    /// 
    /// </summary>
    public class AdbTimeoutException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        public AdbTimeoutException(string arguments)
        {
            this.Arguments = arguments;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Arguments { get; }
    }
}