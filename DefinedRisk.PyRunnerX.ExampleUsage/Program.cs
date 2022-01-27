// <copyright file="Program.cs" company="DefinedRisk">
// Copyright (c) DefinedRisk. MIT License.
// </copyright>
// <author>DefinedRisk</author>

namespace DefinedRisk.PyRunnerX.ExampleUsage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    public class Program
  {
        public static void Main(string[] args)
        {
            // See also the ./Worker/PyRunnerHostedService.cs example

            // Cancellation by token does not currently propagate to tasks once started
            // but would prevent tasks starting if token is already in cancelled state
            // when tasks are created
            var cts = new CancellationTokenSource();

            // Show OS type
            Console.WriteLine($"OSDescription: {RuntimeInformation.OSDescription}");
            Console.WriteLine($"RuntimeIdentifier: {RuntimeInformation.RuntimeIdentifier}");

            // Create a default python runner will use global environment
            PythonRunner runnerGlobal = new PythonRunner();

            // Use to create a virtual environment in local directory (or just return if already exists)#
            // Blocking call with .Result (use await)
            PythonRunner runnerVirtual = runnerGlobal
                .CreateVirtualEnvAsync(cts.Token, Path.Combine(AppContext.BaseDirectory, "Python", "requirements.txt"))
                .Result;

            runnerVirtual.WorkingDirectory = Path.Combine(AppContext.BaseDirectory, "Python");

            // These three seperate runners will all use this same virtual environment
            var runner1 = new PythonRunner(runnerVirtual);
            var runner2 = new PythonRunner(runnerVirtual);
            var runner3 = new PythonRunner(runnerVirtual);

            // Setup optional launcher arguments and timeout dependent upon OS and runner as needed
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ////runner1.LauncherArgs = new string[] { "--arg1", "--arg2", "arg3", "etc" };
                runner1.Timeout = 10000; // 10 seconds
                runner2.Timeout = 20000; // 20 seconds
                runner3.Timeout = 30000; // 30 seconds
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                runner1.Timeout = 10000;
                runner2.Timeout = 20000;
                runner3.Timeout = 30000;
            }

            // In this example the SAME script will be called with different arguments
            // Note that this path is relative to Runner.WorkingDirectory if set or
            // the working directory of the calling process if not.
            string script = Path.Combine("ExampleScript.py");

            // Confirm and setup string arrays with script arguments
            // https://docs.microsoft.com/en-gb/archive/blogs/twistylittlepassagesallalike/everyone-quotes-command-line-arguments-the-wrong-way
            // https://docs.microsoft.com/en-us/previous-versions//17w5ykft(v=vs.85)?redirectedfrom=MSDN
            string[] script1args = { args[0], args[1], args[2], args[3] };
            string[] script2args = { "Title Two", "15.5", @"./output/script2", "Second \"quoted\" example" };
            string[] script3args = { "Title Three", "25.5", @"./output/script3", "Third \"quoted\" example" };

            var pythonScriptTasks = new List<Task<string>>() { runner1.ExecuteAsync(cts.Token, script, script1args) };
            pythonScriptTasks.Add(runner2.ExecuteAsync(cts.Token, script, script2args));
            pythonScriptTasks.Add(runner3.ExecuteAsync(cts.Token, script, script3args));

            int count = 0;

            // Wait for completion or timeout - remove from list on completion
            while (pythonScriptTasks.Any())
            {
                // This task completes when any of the supplied tasks completes
                var task = Task.WhenAny(pythonScriptTasks);

                // Blocking loop to do something else (use await)
                while (!task.IsCompleted)
                {
                    // output count in seconds on screen then sleep for a second
                    Console.WriteLine(count++.ToString());
                    Thread.Sleep(1000);
                }

                // Display output of script and remove from collection
                Console.WriteLine(task.Result.Result);
                pythonScriptTasks.Remove(task.Result);
            }

            // Delete the virtual environment when application finishes
            runnerVirtual.DeleteEnvironment();
        }
    }
}
