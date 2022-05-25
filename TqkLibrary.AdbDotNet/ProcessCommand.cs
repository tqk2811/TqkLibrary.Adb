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
        public string Arguments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string WorkingDirectory { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// 
        /// </summary>
        public string ExecuteFile { get; set; }

        /// <summary>
        /// 
        /// </summary>

        public event Action<string> CommandLogEvent;

        private Process BuildProcess()
        {
            ProcessStartInfo info = new ProcessStartInfo(ExecuteFile, Arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                WorkingDirectory = WorkingDirectory
            };
            Process process = new Process();
            process.StartInfo = info;
            return process;
        }


        internal async Task<ProcessResult> ExecuteAsync(CancellationToken cancellationToken = default, bool throwIfCancel = false)
        {
            ProcessResult processResult = new ProcessResult();
            using Process process = this.BuildProcess();

#if !NET5_0_OR_GREATER
            //https://github.com/Tyrrrz/CliWrap/blob/8ff36a648d57b22497a7cb6feae14ef28bbb2be8/CliWrap/Utils/ProcessEx.cs#L41
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);
#endif
            if (!process.Start())
            {
                throw new InvalidOperationException("Failed to obtain the handle when starting a process. " +
                    "This could mean that the target executable doesn't exist or that execute permission is missing.");
            }
            if (CommandLogEvent != null) ThreadPool.QueueUserWorkItem((o) => CommandLogEvent?.Invoke($"adb {Arguments}"));

            using var register = cancellationToken.Register(() => { try { process.Kill(); } catch { } });
            using MemoryStream memoryStream = new MemoryStream();
            await process.StandardOutput.BaseStream.CopyToAsync(memoryStream);
#if NET5_0_OR_GREATER
            await process.WaitForExitAsync();
#else
            await tcs.Task.ConfigureAwait(false);
#endif
            if (throwIfCancel) cancellationToken.ThrowIfCancellationRequested();
            processResult._stdout = memoryStream.ToArray();
            processResult.ExitCode = process.ExitCode;
            return processResult;
        }

        internal ProcessResult Execute(CancellationToken cancellationToken = default, bool throwIfCancel = false)
        {
            ProcessResult processResult = new ProcessResult();
            using Process process = this.BuildProcess();
            if (!process.Start())
            {
                throw new InvalidOperationException("Failed to obtain the handle when starting a process. " +
                    "This could mean that the target executable doesn't exist or that execute permission is missing.");
            }
            if (CommandLogEvent != null) ThreadPool.QueueUserWorkItem((o) => CommandLogEvent?.Invoke($"adb {Arguments}"));
            using var register = cancellationToken.Register(() => { try { process.Kill(); } catch { } });
            using MemoryStream memoryStream = new MemoryStream();
            process.StandardOutput.BaseStream.CopyTo(memoryStream);
            process.WaitForExit();
            if (throwIfCancel) cancellationToken.ThrowIfCancellationRequested();
            processResult._stdout = memoryStream.ToArray();
            processResult.ExitCode = process.ExitCode;
            return processResult;
        }
    }
}
