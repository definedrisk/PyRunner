﻿// <copyright file="Program.cs" company="DefinedRisk">
// Copyright (c) DefinedRisk. MIT License.
// </copyright>
// <author>DefinedRisk</author>

namespace DefinedRisk.PyRunnerX.ExampleUsage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class Program
  {
        public static void Main(string[] args)
        {
            // show OS type
            Console.WriteLine("IsWindows(): " + PythonRunner.OperatingSystem.IsWindows());
            Console.WriteLine("IsLinux(): " + PythonRunner.OperatingSystem.IsLinux());

            // create a default (global) python runner
            PythonRunner runnerGlobal = new PythonRunner();

            // use it to create a virtual environment in local directory (or just return if one already exists)
            PythonRunner runnerVirtual = runnerGlobal.CreateVirtualEnv(Path.Combine(AppContext.BaseDirectory, "Python", "requirements.txt"));

            // these copies of runner will use this same virtual environment
            var runner1 = new PythonRunner(runnerVirtual);
            var runner2 = new PythonRunner(runnerVirtual);
            var runner3 = new PythonRunner(runnerVirtual);

            // setup optional launcher arguments and timeout
            if (PythonRunner.OperatingSystem.IsWindows())
            {
                // set launcher-args
                runner1.LauncherArgs = new string[] { "-3.7-64" };
                runner1.Timeout = 10000; // 10 seconds

                runner2.LauncherArgs = new string[] { "-3.7-64" };
                runner2.Timeout = 20000; // 20 seconds

                runner3.LauncherArgs = new string[] { "-3.7-64" };
                runner3.Timeout = 5000; // 4 seconds!!
            }
            else if (PythonRunner.OperatingSystem.IsLinux())
            {
                runner1.Timeout = 10000;
                runner2.Timeout = 20000;
                runner3.Timeout = 4000;
            }

            // here the same script will be called - but with different arguments
            string script = @"./Python/ExampleScript.py";

            // confirm and setup string arrays with script arguments
            Console.WriteLine($"Program args 0 and 1: Title= \"{args[0]}\", Delay= \"{args[1]}\".");
            Console.WriteLine($"Program args 2 and 3: Title= \"{args[2]}\", Delay= \"{args[3]}\".");

            string[] script1args = { args[0], args[1], @"test 1 with spaces", @"{""json1a"":""value1a"", ""json1b"":""value1b""}" };

            string[] script2args = { args[2], args[3], @"test 2 with spaces", @"{""json2a"":""value2a"", ""json2b"":""value2b""}" };

            // this one will cause the Timeout to expire for this runner
            string[] script3args = { "Third Script", args[3], @"test 3 with spaces", @"{""json3a"":""value3a"", ""json3b"":""value3b""}" };

            // starts scripts immediately - asynchronously
            // cancellation by token does not currently propagate to tasks once started
            // but would prevent tasks starting if token is already in cancelled state
            // when tasks are created
            var cts = new CancellationTokenSource();

            var pythonScriptTasks = new List<Task<string>>() { runner1.ExecuteAsync(script, cts.Token, script1args) };

            pythonScriptTasks.Add(runner2.ExecuteAsync(script, cts.Token, script2args));
            pythonScriptTasks.Add(runner3.ExecuteAsync(script, cts.Token, script3args));

            // If using same runner again BOTH tasks will excute on seperate process
            // BUT they will overwrite the output builders so DO NOT DO THIS!
            // Do not excute a script on a runner while it is still running, if the output
            // is required.
            ////pythonScriptTasks.Add(runner2.ExecuteAsync(script, cts.Token, script2args));

            int count = 0;

            // wait for completion or timeout (check every second) - remove from list on completion
            while (pythonScriptTasks.Any())
            {
                // this task completes when any of the supplied tasks completes
                var task = Task.WhenAny(pythonScriptTasks);

                // do something else to replicate await task - otherwise this console app will complete!
                while (!task.IsCompleted)
                {
                    // output count in seconds on screen
                    Console.WriteLine(count++.ToString());
                    Thread.Sleep(1000);
                }

                // display output of script and remove from collection
                Console.WriteLine(task.Result.Result);
                pythonScriptTasks.Remove(task.Result);
            }
        }
    }
}