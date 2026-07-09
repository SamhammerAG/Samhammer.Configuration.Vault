using System;
using System.Diagnostics;
using System.Text;

namespace Samhammer.Configuration.Vault.Sag.Services
{
    public class ProcessExecutionService
    {
        public static string RunCliProcess(string executable, string arguments)
        {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                },
            };

            process.OutputDataReceived += (sender, e) => outputBuilder.Append(e.Data);
            process.ErrorDataReceived += (sender, e) => errorBuilder.Append(e.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
            process.CancelOutputRead();
            process.CancelErrorRead();

            if (process.ExitCode == 0)
            {
                return outputBuilder.ToString();
            }

            throw new Exception($"'{process.StartInfo.FileName}' with args '{process.StartInfo.Arguments}' failed with error '{outputBuilder}{errorBuilder}'");
        }
    }
}
