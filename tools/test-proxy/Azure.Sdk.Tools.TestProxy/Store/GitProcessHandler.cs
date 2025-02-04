using System;
using System.Collections.Generic;
using System.Diagnostics;
using Azure.Sdk.Tools.TestProxy.Common;
using Azure.Sdk.Tools.TestProxy.Common.Exceptions;

namespace Azure.Sdk.Tools.TestProxy.Store
{

    public class CommandResult
    {
        public int ExitCode;
        public string StdErr;
        public string StdOut;
        public string Arguments;
        public Exception CommandException;
    }

    /// <summary>
    /// This class offers an easy wrapper abstraction for shelling out to git.
    /// </summary>
    public class GitProcessHandler
    {
        public virtual ProcessStartInfo CreateGitProcessInfo(GitAssetsConfiguration config)
        {
            var startInfo = new ProcessStartInfo("git")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = config.AssetsRepoLocation,
            };

            startInfo.EnvironmentVariables["PATH"] = Environment.GetEnvironmentVariable("PATH");

            return startInfo;
        }

        /// <summary>
        /// Invokes a git command. If it fails in any way, throws GitProcessException. Otherwise returns the result of the git invocation.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="inputCommands"></param>
        /// <returns>A list of command results. One can assume success in all of them unless an exception has been thrown</returns>
        /// <exception cref="GitProcessException">Throws GitProcessException on returnCode != 0 OR if an unexpected exception is thrown during invocation.</exception>
        public virtual IEnumerable<CommandResult> Run(GitAssetsConfiguration config, params string[] inputCommands)
        {
            List<CommandResult> results = new List<CommandResult>();

            foreach (var inputCommand in inputCommands)
            {
                results.Add(Run(inputCommand, config));
            }

            return results;
        }

        /// <summary>
        /// Invokes a git command. If it fails in any way, throws GitProcessException. Otherwise returns the result of the git invocation.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        /// <exception cref="GitProcessException">Throws GitProcessException on returnCode != 0 OR if an unexpected exception is thrown during invocation.</exception>
        public virtual CommandResult Run(string arguments, GitAssetsConfiguration config)
        {
            ProcessStartInfo processStartInfo = CreateGitProcessInfo(config);
            processStartInfo.Arguments = arguments;

            CommandResult result = new CommandResult()
            {
                Arguments = arguments
            };

            try
            {
                DebugLogger.LogInformation($"git {arguments}");
                var process = Process.Start(processStartInfo);
                string stdOut = process.StandardOutput.ReadToEnd();
                string stdErr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                int returnCode = process.ExitCode;

                DebugLogger.LogDebug($"StdOut: {stdOut}");
                DebugLogger.LogDebug($"StdErr: {stdErr}");
                DebugLogger.LogDebug($"ExitCode: {process.ExitCode}");


                result.ExitCode = process.ExitCode;
                result.StdErr = stdErr;
                result.StdOut = stdOut;


                if (result.ExitCode == 0){
                    return result;
                }
                else
                {
                    throw new GitProcessException(result);
                }
            }
            catch (Exception e)
            {
                DebugLogger.LogDebug(e.Message);

                result.ExitCode = -1;
                result.CommandException = e;

                throw new GitProcessException(result);
            }
        }

        /// <summary>
        /// Invokes git binary against a GitAssetsConfiguration.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="arguments"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public virtual bool TryRun(string arguments, GitAssetsConfiguration config, out CommandResult result)
        {
            ProcessStartInfo processStartInfo = CreateGitProcessInfo(config);
            processStartInfo.Arguments = arguments;

            try
            {
                DebugLogger.LogInformation($"git {arguments}");
                var process = Process.Start(processStartInfo);
                string stdOut = process.StandardOutput.ReadToEnd();
                string stdErr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                int returnCode = process.ExitCode;

                DebugLogger.LogDebug($"StdOut: {stdOut}");
                DebugLogger.LogDebug($"StdErr: {stdErr}");
                DebugLogger.LogDebug($"ExitCode: {process.ExitCode}");

                result = new CommandResult()
                {
                    ExitCode = process.ExitCode,
                    StdErr = stdOut,
                    StdOut = stdErr,
                    Arguments = arguments
                };
            }
            catch (Exception e)
            {
                DebugLogger.LogDebug(e.Message);

                result = new CommandResult()
                {
                    ExitCode = -1,
                    CommandException = e
                };
            }

            if (result.ExitCode != 0)
            {
                return false;
            }

            return true;
        }
    }
}
