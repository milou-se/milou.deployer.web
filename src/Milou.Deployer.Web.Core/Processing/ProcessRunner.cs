using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Milou.Deployer.Web.Core.Extensions;
using Serilog;

namespace Milou.Deployer.Web.Core.Processing
{
    public static class ProcessRunner
    {
        private const string NoProcessIsAssociatedWithThisObject = "No process is associated with this object";

        public static Task<ExitCode> ExecuteAsync(
            string executePath,
            ILogger logger,
            IEnumerable<string> arguments = null,
            IEnumerable<KeyValuePair<string, string>> environmentVariables = null,
            CancellationToken cancellationToken = default)
        {
            ILogger usedLogger = logger;

            return ExecuteAsync(executePath,
                cancellationToken,
                arguments,
                (m, _) => usedLogger.Information("{Message}", m),
                (m, _) => usedLogger.Error("{Message}", m),
                verboseAction: (m, _) => usedLogger.Verbose("{Message}", m),
                toolAction: (m, _) => usedLogger.Information("{Message}", m),
                environmentVariables: environmentVariables,
                debugAction: (m, _) => usedLogger.Debug("{Message}", m));
        }

        public static async Task<ExitCode> ExecuteAsync(
            string executePath,
            CancellationToken cancellationToken = default,
            IEnumerable<string> arguments = null,
            Action<string, string> standardOutLog = null,
            Action<string, string> standardErrorAction = null,
            Action<string, string> toolAction = null,
            Action<string, string> verboseAction = null,
            IEnumerable<KeyValuePair<string, string>> environmentVariables = null,
            Action<string, string> debugAction = null)
        {
            if (string.IsNullOrWhiteSpace(executePath))
            {
                throw new ArgumentNullException(nameof(executePath));
            }

            if (!File.Exists(executePath))
            {
                throw new ArgumentException(
                    $"The executable file '{executePath}' does not exist",
                    nameof(executePath));
            }

            IEnumerable<string> usedArguments = arguments ?? Enumerable.Empty<string>();

            string formattedArguments = string.Join(" ", usedArguments.Select(arg => $"\"{arg}\""));

            Task<ExitCode> task = RunProcessAsync(executePath,
                formattedArguments,
                standardErrorAction,
                standardOutLog,
                cancellationToken,
                toolAction,
                verboseAction,
                environmentVariables,
                debugAction);

            ExitCode exitCode = await task;

            return exitCode;
        }

        private static async Task<ExitCode> RunProcessAsync(
            string executePath,
            string formattedArguments,
            Action<string, string> standardErrorAction,
            Action<string, string> standardOutputLog,
            CancellationToken cancellationToken,
            Action<string, string> toolAction,
            Action<string, string> verboseAction = null,
            IEnumerable<KeyValuePair<string, string>> environmentVariables = null,
            Action<string, string> debugAction = null)
        {
            Process process = null;
            int processId = -1;

            bool isKilledOrExited = false;

            try
            {

                toolAction = toolAction ?? ((message, prefix) => { });
                Action<string, string> standardAction = standardOutputLog ?? ((message, prefix) => { });
                Action<string, string> errorAction = standardErrorAction ?? ((message, prefix) => { });
                Action<string, string> verbose = verboseAction ?? ((message, prefix) => { });
                Action<string, string> debug = debugAction ?? ((message, prefix) => { });

                var taskCompletionSource = new TaskCompletionSource<ExitCode>();

                string processWithArgs = $"\"{executePath}\" {formattedArguments}".Trim();

                toolAction($"[{typeof(ProcessRunner).Name}] Executing: {processWithArgs}", null);

                bool useShellExecute = standardErrorAction == null && standardOutputLog == null;

                bool redirectStandardError = standardErrorAction != null;

                bool redirectStandardOutput = standardOutputLog != null;

                var processStartInfo = new ProcessStartInfo(executePath)
                {
                    Arguments = formattedArguments,
                    RedirectStandardError = redirectStandardError,
                    RedirectStandardOutput = redirectStandardOutput,
                    UseShellExecute = useShellExecute
                };

                if (environmentVariables != null)
                {
                    foreach (KeyValuePair<string, string> environmentVariable in environmentVariables)
                    {
                        processStartInfo.EnvironmentVariables.Add(environmentVariable.Key, environmentVariable.Value);
                    }
                }

                var exitCode = new ExitCode(-1);

                process = new Process
                {
                    StartInfo = processStartInfo,
                    EnableRaisingEvents = true
                };

                process.Disposed += (sender, args) =>
                {
                    if (!taskCompletionSource.Task.IsCompleted)
                    {
                        verbose("Task was not completed, but process was disposed", null);
                        taskCompletionSource.TrySetResult(ExitCode.Failure);
                    }

                    verbose($"Disposed process '{processWithArgs}'", null);
                };

                if (redirectStandardError)
                {
                    process.ErrorDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            errorAction(args.Data, null);
                        }
                    };
                }

                if (redirectStandardOutput)
                {
                    process.OutputDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            standardAction(args.Data, null);
                        }
                    };
                }

                process.Exited += (sender, args) =>
                {
                    var proc = (Process)sender;
                    toolAction(
                        $"Process '{processWithArgs}' exited with code {new ExitCode(proc.ExitCode)}",
                        null);
                    isKilledOrExited = true;
                    taskCompletionSource.SetResult(new ExitCode(proc.ExitCode));
                };


                try
                {
                    bool started = process.Start();

                    if (!started)
                    {
                        errorAction($"Process '{processWithArgs}' could not be started", null);
                        return ExitCode.Failure;
                    }

                    if (redirectStandardError)
                    {
                        process.BeginErrorReadLine();
                    }

                    if (redirectStandardOutput)
                    {
                        process.BeginOutputReadLine();
                    }

                    int bits = process.IsWin64() ? 64 : 32;

                    try
                    {
                        processId = process.Id;
                    }
                    catch (InvalidOperationException ex)
                    {
                        debug($"Could not get process id for process '{processWithArgs}'. {ex}", null);
                    }

                    string temp = process.HasExited ? "was" : "is";

                    verbose(
                        $"The process '{processWithArgs}' {temp} running in {bits}-bit mode",
                        null);
                }
                catch (Exception ex)
                {
                    if (ex.IsFatal())
                    {
                        throw;
                    }

                    errorAction($"An error occured while running process {processWithArgs}: {ex}", null);
                    taskCompletionSource.SetException(ex);
                }

                bool done = false;

                try
                {
                    while (IsAlive(process,
                        taskCompletionSource.Task,
                        cancellationToken,
                        done,
                        processWithArgs,
                        toolAction,
                        standardAction,
                        errorAction,
                        verbose))
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        try
                        {
                            Task delay = Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);

                            await delay;
                        }
                        catch (TaskCanceledException)
                        {
                            if (!taskCompletionSource.Task.IsCompleted && !taskCompletionSource.Task.IsCanceled &&
                                !taskCompletionSource.Task.IsCompletedSuccessfully &&
                                !taskCompletionSource.Task.IsFaulted)
                            {
                                taskCompletionSource.SetCanceled();
                            }
                        }

                        if (taskCompletionSource.Task.IsCompleted)
                        {
                            done = true;
                            exitCode = await taskCompletionSource.Task;
                        }
                        else if (taskCompletionSource.Task.IsCanceled)
                        {
                            exitCode = ExitCode.Failure;
                        }
                        else if (taskCompletionSource.Task.IsFaulted)
                        {
                            exitCode = ExitCode.Failure;
                        }
                    }
                }
                finally
                {
                    if (!exitCode.IsSuccess)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            if (process != null && !isKilledOrExited && !process.HasExited)
                            {
                                try
                                {
                                    toolAction($"Cancellation is requested, trying to kill process {processWithArgs}",
                                        null);

                                    if (processId > 0)
                                    {
                                        string args = $"/PID {processId}";

                                        string killProcessPath =
                                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                                                "taskkill.exe");

                                        toolAction($"Running {killProcessPath} {args}", null);

                                        using (Process.Start(killProcessPath, args))
                                        {
                                        }

                                        isKilledOrExited = true;

                                        errorAction(
                                            $"Killed process {processWithArgs} because cancellation was requested",
                                            null);
                                    }
                                    else
                                    {
                                        debugAction(
                                            $"Could not kill process '{processWithArgs}', missing process id",
                                            null);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (ex.IsFatal())
                                    {
                                        throw;
                                    }

                                    errorAction(
                                        $"ProcessRunner could not kill process {processWithArgs} when cancellation was requested",
                                        null);
                                    errorAction(
                                        $"Could not kill process {processWithArgs} when cancellation was requested",
                                        null);
                                    errorAction(ex.ToString(), null);
                                }
                            }
                        }
                    }

                    using (process)
                    {
                        verbose(
                            $"Task status: {taskCompletionSource.Task.Status}, {taskCompletionSource.Task.IsCompleted}",
                            null);
                        verbose($"Disposing process {processWithArgs}", null);
                    }
                }

                verbose($"Process runner exit code {exitCode} for process {processWithArgs}", null);

                try
                {
                    if (processId > 0)
                    {
                        Process stillRunningProcess = Process.GetProcesses().SingleOrDefault(p => p.Id == processId);

                        if (stillRunningProcess != null && !isKilledOrExited)
                        {
                            if (!stillRunningProcess.HasExited)
                            {
                                errorAction(
                                    $"The process with ID {processId.ToString(CultureInfo.InvariantCulture)} '{processWithArgs}' is still running",
                                    null);

                                return ExitCode.Failure;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.IsFatal())
                    {
                        throw;
                    }

                    debugAction($"Could not check processes. {ex}", null);
                }

                return exitCode;
            }
            finally
            {
                if (process != null)
                {
                    try
                    {
                        if (!isKilledOrExited && !process.HasExited)
                        {
                            try
                            {
                                process.Kill();
                            }
                            catch (InvalidOperationException ex) when (ex.Message.Contains(NoProcessIsAssociatedWithThisObject))
                            {
                                // ignore
                            }
                            catch (Exception ex)
                            {
                                standardErrorAction?.Invoke($"Could not kill process {processId}, {ex}", null);
                            }
                        }
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains(NoProcessIsAssociatedWithThisObject))
                    {
                        // ignore
                    }
                    catch (Exception ex)
                    {
                        standardErrorAction?.Invoke($"Could not determine if process {processId} has exited, {ex}", null);
                    }

                    try
                    {
                        process.Dispose();
                    }
                    catch (Exception ex)
                    {
                        standardErrorAction?.Invoke($"Could not dispose process {processId}, {ex}", null);
                    }
                }
            }
        }

        private static bool IsAlive(
            Process process,
            Task<ExitCode> task,
            CancellationToken cancellationToken,
            bool done,
            string processWithArgs,
            Action<string, string> toolAction,
            Action<string, string> standardAction,
            Action<string, string> errorAction,
            Action<string, string> verbose)
        {
            if (process == null)
            {
                verbose($"Process {processWithArgs} does no longer exist", null);
                return false;
            }

            if (task.IsCompleted || task.IsFaulted || task.IsCanceled)
            {
                TaskStatus status = task.Status;
                verbose($"Task status for process {processWithArgs} is {status}", null);
                return false;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                verbose($"Cancellation is requested for process {processWithArgs}", null);
                return false;
            }

            if (done)
            {
                verbose($"Process {processWithArgs} is flagged as done", null);
                return false;
            }

            return true;
        }
    }
}