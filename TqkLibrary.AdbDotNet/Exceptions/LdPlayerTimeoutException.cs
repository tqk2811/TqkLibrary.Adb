using System;

namespace TqkLibrary.AdbDotNet
{
    /// <summary>
    /// 
    /// </summary>
    public class LdPlayerTimeoutException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        public LdPlayerTimeoutException(string arguments)
        {
            this.Arguments = arguments;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Arguments { get; }
    }
}