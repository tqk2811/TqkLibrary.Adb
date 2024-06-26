﻿using System;

namespace TqkLibrary.AdbDotNet
{
    /// <summary>
    /// 
    /// </summary>
    public class AdbException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="StandardOutput"></param>
        /// <param name="StandardError"></param>
        /// <param name="arguments"></param>
        /// <param name="exitCode"></param>
        public AdbException(string StandardOutput, string StandardError, string arguments, int exitCode) : base(StandardError)
        {
            this.StandardOutput = StandardOutput;
            this.StandardError = StandardError;
            this.Arguments = arguments;
            this.ExitCode = exitCode;
        }
        /// <summary>
        /// 
        /// </summary>
        public string StandardOutput { get; }
        /// <summary>
        /// 
        /// </summary>
        public string StandardError { get; }
        /// <summary>
        /// 
        /// </summary>
        public string Arguments { get; }
        /// <summary>
        /// 
        /// </summary>
        public int ExitCode { get; }
    }
}