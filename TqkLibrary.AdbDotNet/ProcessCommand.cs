using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TqkLibrary.AdbDotNet
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessCommand
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executeFile"></param>
        /// <param name="arguments"></param>
        public ProcessCommand(string executeFile, string arguments)
        {
            if (string.IsNullOrWhiteSpace(executeFile)) throw new ArgumentNullException(nameof(executeFile));
            if (string.IsNullOrWhiteSpace(arguments)) throw new ArgumentNullException(nameof(arguments));
            this.ExecuteFile = executeFile;
            this.Arguments = arguments;
        }
        /// <summary>
        /// 
        /// </summary>
        public int? Timeout { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool ThrowIfTimeout { get; set; } = false;
        /// <summary>
        /// 
        /// </summary>
        public string Arguments { get; }
        /// <summary>
        /// 
        /// </summary>
        public string WorkingDirectory { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// 
        /// </summary>
        public string ExecuteFile { get; }

        /// <summary>
        /// 
        /// </summary>

        public event Action<string>? CommandLogEvent;

        private Process BuildProcess()
        {
            ProcessStartInfo info = new ProcessStartInfo(ExecuteFile, Arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = WorkingDirectory
            };
            Process process = new Process();
            process.StartInfo = info;
            return process;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="throwifTimeout"></param>
        /// <returns></returns>
        public ProcessCommand WithTimeout(int? timeout, bool throwifTimeout)
        {
            this.Timeout = Timeout;
            this.ThrowIfTimeout = throwifTimeout;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="throwIfCancel"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<ProcessResult> ExecuteAsync(CancellationToken cancellationToken = default, bool throwIfCancel = false)
        {
            using Process process = this.BuildProcess();

#if !NET5_0_OR_GREATER
            //https://github.com/Tyrrrz/CliWrap/blob/8ff36a648d57b22497a7cb6feae14ef28bbb2be8/CliWrap/Utils/ProcessEx.cs#L41
            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);
#endif
            if (!process.Start())
            {
                throw new InvalidOperationException("Failed to obtain the handle when starting a process. " +
                    "This could mean that the target executable doesn't exist or that execute permission is missing.");
            }
            if (CommandLogEvent != null) ThreadPool.QueueUserWorkItem((o) => CommandLogEvent?.Invoke(Arguments));

            using CancellationTokenSource cancellationTokenSource_timeout = Timeout.HasValue ? new CancellationTokenSource(Timeout.Value) : new CancellationTokenSource();
            using var register_timeout = cancellationTokenSource_timeout.Token.Register(() => { try { process.Kill(); } catch { } });

            using var register = cancellationToken.Register(() => { try { process.Kill(); } catch { } });

            using MemoryStream stdout_memoryStream = new MemoryStream();
            using MemoryStream stderr_memoryStream = new MemoryStream();
            Task task_stdout = process.StandardOutput.BaseStream.CopyToAsync(stdout_memoryStream);//.ConfigureAwait(false);
            Task task_stderr = process.StandardError.BaseStream.CopyToAsync(stderr_memoryStream);//.ConfigureAwait(false);

            Task task_process =
#if NET5_0_OR_GREATER
            process.WaitForExitAsync();
#else
            tcs.Task;//.ConfigureAwait(false);
#endif
            await Task.WhenAll(task_stdout, task_stderr, task_process).ConfigureAwait(false);
            if (throwIfCancel) cancellationToken.ThrowIfCancellationRequested();
            if (cancellationTokenSource_timeout.IsCancellationRequested)
            {
                if (CommandLogEvent != null) ThreadPool.QueueUserWorkItem((o) => CommandLogEvent?.Invoke($"Stuck {Arguments}"));
                if (ThrowIfTimeout) throw new ProcessCommandTimeoutException();
            }
            ProcessResult processResult = new ProcessResult(process.ExitCode, stdout_memoryStream.ToArray(), stderr_memoryStream.ToArray());
            return processResult;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="throwIfCancel"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ProcessResult Execute(CancellationToken cancellationToken = default, bool throwIfCancel = false)
        {
            using Process process = this.BuildProcess();
            if (!process.Start())
            {
                throw new InvalidOperationException("Failed to obtain the handle when starting a process. " +
                    "This could mean that the target executable doesn't exist or that execute permission is missing.");
            }
            if (CommandLogEvent != null) ThreadPool.QueueUserWorkItem((o) => CommandLogEvent?.Invoke(Arguments));
            using var register = cancellationToken.Register(() => { try { process.Kill(); } catch { } });
            using MemoryStream stdout_memoryStream = new MemoryStream();
            using MemoryStream stderr_memoryStream = new MemoryStream();
            process.StandardOutput.BaseStream.CopyTo(stdout_memoryStream);
            process.StandardError.BaseStream.CopyTo(stderr_memoryStream);
            process.WaitForExit();
            if (throwIfCancel) cancellationToken.ThrowIfCancellationRequested();
            ProcessResult processResult = new ProcessResult(process.ExitCode, stdout_memoryStream.ToArray(), stderr_memoryStream.ToArray());
            return processResult;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ProcessCommandTimeoutException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public ProcessCommandTimeoutException()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public ProcessCommandTimeoutException(string message) : base(message)
        {

        }
    }
}
