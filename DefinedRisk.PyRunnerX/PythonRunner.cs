// <copyright file="PythonRunner.cs" company="DefinedRisk">
// Copyright (c) DefinedRisk. MIT License.
// </copyright>
// <author>DefinedRisk</author>

namespace DefinedRisk.PyRunnerX
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using SixLabors.ImageSharp;

    /// <summary>
    /// A specialized runner for python and python scripts. Supports textual output
    /// as well as image output, both synchronously and asynchronously.
    /// </summary>
    /// <remarks>
    /// You can think of <see cref="PythonRunner"/> instances as <see cref="Process"/>
    /// instances that were specialized for Python and Python scripts. The python interpreter
    /// (and launcher for windows) can also be called directly without appending scipts.
    /// </remarks>
    /// <seealso cref="Process"/>
    public sealed class PythonRunner
    {
        // collects output from process std_output during latest run
        private StringBuilder _outputBuilder;

        // collects error from process std_err during latest run
        private StringBuilder _errorBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonRunner"/> class to be used for creating virtual
        /// environments. Launcher (interpreter) is fixed for the lifetime of this instance.
        /// It is either "c:\windows\py.exe" or "/usr/bin/python3" and is evaluated from OS
        /// at runtime.
        /// </summary>
        /// <param name="timeout">Optional script timeout in msec. Defaults to 30000 (30 sec).</param>
        /// <exception cref="PythonRunnerException">
        /// Default launcher (interpreter) does not exist (invalid path). Use alternative constructor and specify
        /// launcher or interpreter directly.
        /// </exception>
        /// <seealso cref="Timeout"/>
        public PythonRunner(
            int timeout = 30000)
        {
            var launcher = GetOSLauncher;
            if (!File.Exists(launcher))
            {
                throw new PythonRunnerException($"Default launcher (interpreter) does not exist: {launcher}", new FileNotFoundException(launcher));
            }

            Interpreter = GetOSLauncher;
            Timeout = timeout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonRunner"/> class.
        /// Launcher (interpreter) is fixed for the lifetime of this instance.
        /// It is either "c:\windows\py.exe" or "/usr/bin/python3" and is evaluated from OS
        /// at runtime. Both sets of arguments can be changed throughout the lifetime of this class.
        /// Note that for windows launcher usage is: "py [launcher-args] [python-args] script [script-args]".
        /// </summary>
        /// <param name="launcherArgs">Launcher specific arguments.</param>
        /// <param name="interpreterArgs">Interpreter specific arguments.</param>
        /// <param name="timeout">Optional script timeout in msec. Defaults to 10000 (10 sec).</param>
        /// <exception cref="PythonRunnerException">
        /// Default launcher (interpreter) does not exist (invalid path). Use alternative constructor and specify
        /// launcher or interpreter directly.
        /// </exception>
        /// <seealso cref="Interpreter"/>
        /// <seealso cref="Timeout"/>
        public PythonRunner(
            string[] launcherArgs,
            string[] interpreterArgs,
            int timeout = 10000)
        {
            var launcher = GetOSLauncher;
            if (!File.Exists(launcher))
            {
                throw new PythonRunnerException($"Default launcher (interpreter) does not exist: {launcher}", new FileNotFoundException(launcher));
            }

            LauncherArgs = launcherArgs;
            InterpreterArgs = interpreterArgs;
            Interpreter = GetOSLauncher;
            Timeout = timeout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonRunner"/> class.
        /// Launcher (interpreter) is fixed for the lifetime of this instance.
        /// It is either "c:\windows\py.exe" or "/usr/bin/python3" and is evaluated from OS
        /// at runtime. Launcher-args are ommitted however both sets of arguments can be changed throughout
        /// the lifetime of this class.
        /// Note that for windows launcher usage is: "py [launcher-args] [python-args] script [script-args]".
        /// </summary>
        /// <param name="interpreterArgs">Interpreter specific arguments.</param>
        /// <param name="timeout">Optional script timeout in msec. Defaults to 10000 (10 sec).</param>
        /// <exception cref="PythonRunnerException">
        /// Default launcher (interpreter) does not exist (invalid path). Use alternative constructor and specify
        /// launcher or interpreter directly.
        /// </exception>
        /// <seealso cref="Interpreter"/>
        /// <seealso cref="Timeout"/>
        public PythonRunner(
            string[] interpreterArgs,
            int timeout = 10000)
        {
            var launcher = GetOSLauncher;
            if (!File.Exists(launcher))
            {
                throw new PythonRunnerException($"Default launcher (interpreter) does not exist: {launcher}", new FileNotFoundException(launcher));
            }

            LauncherArgs = Array.Empty<string>();
            InterpreterArgs = interpreterArgs;
            Interpreter = GetOSLauncher;
            Timeout = timeout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonRunner"/> class.
        /// Launcher is fixed for the lifetime of this instance. Arguments can be changed.
        /// </summary>
        /// <param name="launcherArgs">Launcher specific arguments.</param>
        /// <param name="interpreterArgs">Interpreter specific arguments.</param>
        /// <param name="launcher">Full path to the Python launcher (interpreter).</param>
        /// <param name="timeout">Optional script timeout in msec. Defaults to 10000 (10 sec).</param>
        /// <exception cref="PythonRunnerException">
        /// Argument <paramref name="launcher"/> is an invalid path.
        /// </exception>
        /// <seealso cref="Interpreter"/>
        /// <seealso cref="Timeout"/>
        public PythonRunner(
            string[] launcherArgs,
            string[] interpreterArgs,
            string launcher,
            int timeout = 10000)
        {
            if (!File.Exists(launcher))
            {
                throw new PythonRunnerException($"Invalid launcher (interpreter) path: {launcher}", new FileNotFoundException(launcher));
            }

            LauncherArgs = launcherArgs;
            InterpreterArgs = interpreterArgs;
            Interpreter = launcher;
            Timeout = timeout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonRunner"/> class.
        /// Interpreter is fixed for the lifetime of this instance. Arguments can be changed.
        /// </summary>
        /// <param name="interpreterArgs">Interpreter specific arguments.</param>
        /// <param name="interpreter">Path to the Python interpreter.</param>
        /// <param name="timeout">Optional script timeout in msec. Defaults to 10000 (10 sec).</param>
        /// <exception cref="FileNotFoundException">
        /// Argument <paramref name="interpreter"/> is an invalid path.
        /// </exception>
        /// <seealso cref="Interpreter"/>
        /// <seealso cref="Timeout"/>
        public PythonRunner(
            string[] interpreterArgs,
            string interpreter,
            int timeout = 10000)
        {
            if (!File.Exists(interpreter))
            {
                throw new PythonRunnerException($"Invalid interpreter path: {interpreter}", new FileNotFoundException(interpreter));
            }

            InterpreterArgs = interpreterArgs;
            Interpreter = interpreter;
            Timeout = timeout;
        }

        public PythonRunner(PythonRunner copy)
        {
            Interpreter = copy.Interpreter;
            InterpreterArgs = copy.InterpreterArgs;
            LauncherArgs = copy.LauncherArgs;
            Timeout = copy.Timeout;
        }

        /// <summary>
        /// Occurs when a python process is started.
        /// </summary>
        /// <seealso cref="PyRunnerStartedEventArgs"/>
        public event EventHandler<PyRunnerStartedEventArgs> Started;

        /// <summary>
        /// Occurs when a python process has exited.
        /// </summary>
        /// <seealso cref="PyRunnerExitedEventArgs"/>
        public event EventHandler<PyRunnerExitedEventArgs> Exited;

        /// <summary>
        /// Gets the full path to Python launcher or interpreter (eg. 'py.exe' or 'python3') for use by this instance.
        /// </summary>
        public string Interpreter { get; }

        /// <summary>
        /// Gets or sets the Python launcher args for use by this instance. See 'py --help'
        /// for available launcher args.
        /// </summary>
        public string[] LauncherArgs { get; set; } = new string[0];

        /// <summary>
        /// Gets or sets the Python interpreter args for use by this instance. See 'python3 --help'
        /// for available interpreter args.
        /// </summary>
        public string[] InterpreterArgs { get; set; } = new string[0];

        /// <summary>
        /// Gets or Sets the timeout for the underlying <see cref="Process"/> component in msec.
        /// </summary>
        /// <remarks>
        /// See <see cref="Process.WaitForExit(int)"/> for details about this value.
        /// </remarks>
        public int Timeout { get; set; }

        private static string GetOSLauncher
        {
            get
            {
                if (OperatingSystem.IsWindows())
                {
                    return @"c:\windows\py.exe";
                }
                else if (OperatingSystem.IsLinux())
                {
                    return @"/usr/bin/python3";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        private static string EnvPath
        {
            get
            {
                return Path.Combine(AppContext.BaseDirectory, @".venv");
            }
        }

        /// <summary>
        /// Execute a Python script and return text that would have been printed
        /// to console.
        /// </summary>
        /// <param name="script">Full path to script file.</param>
        /// <param name="scriptArguments">Arguments to pass to script.</param>
        /// <returns>The text output of the script (re-directed from Stdout).</returns>
        /// <exception cref="PythonRunnerException">
        /// Thrown if error text was outputted by the script (this normally happens
        /// if an exception was raised by the script). <br/>
        /// -- or -- <br/>
        /// An unexpected error happened during script execution. In this case, the
        /// <see cref="Exception.InnerException"/> property contains the original
        /// <see cref="Exception"/>.
        /// </exception>
        /// <remarks>
        /// <b>Important:</b> Output to the error stream can also come from warnings, that are frequently
        /// outputted by various python package components. These warnings would result
        /// in an exception, therefore they must be switched off within the script by
        /// including the following statement: <c>warnings.simplefilter("ignore")</c>.
        /// </remarks>
        public string Execute(string script, params object[] scriptArguments)
        {
            InternalExecute(script, scriptArguments);

            return !string.IsNullOrWhiteSpace(_errorBuilder.ToString())
                ? throw new PythonRunnerException(_errorBuilder.ToString())
                : _outputBuilder.ToString().Trim();
        }

        /// <summary>
        /// Runs the <see cref="Execute"/> method asynchronously using a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="script">Full path to script file.</param>
        /// <param name="ct">Task cancellation token.</param>
        /// <param name="scriptArguments">Arguments to pass to script.</param>
        /// <returns>
        /// An awaitable task, with the text output of the script as <see cref="Task{TResult}.Result"/>.
        /// </returns>
        /// <seealso cref="Execute"/>
        public Task<string> ExecuteAsync(string script, CancellationToken ct, params object[] scriptArguments)
        {
            return Task.Run<string>(() => Execute(script, scriptArguments), ct);
        }

        /// <summary>
        /// Run the Launcher or Interpreter with arguments (without Python Script).
        /// Use this to setup environments for example.
        /// </summary>
        /// <returns>The text output (re-directed from Stdout).</returns>
        public string Run()
        {
            ProcessStartInfo startInfo = CreateStartInfo();
            InternalRun(startInfo);

            return !string.IsNullOrWhiteSpace(_errorBuilder.ToString())
                 ? throw new PythonRunnerException(_errorBuilder.ToString())
                 : _outputBuilder.ToString().Trim();
        }

        /// <summary>
        /// Runs the <see cref="Run"/> method asynchronously using a <see cref="CancellationToken"/>.
        /// </summary>
        /// <returns>The text output (re-directed from Stdout).</returns>
        public Task<string> RunAsync(CancellationToken ct)
        {
            return Task.Run<string>(() => Run(), ct);
        }

        /// <summary>
        /// Executes a Python script and returns the resulting image (mostly a chart that was produced
        /// by a Python package like e.g. <see href="https://matplotlib.org/">matplotlib</see>).
        /// </summary>
        /// <param name="script">Full path to the script to execute.</param>
        /// <param name="scriptArguments">Arguments that were passed to the script.</param>
        /// <returns>The <see cref="Image"/> that the script creates.</returns>
        /// <exception cref="PythonRunnerException">
        /// Thrown if error text was outputted by the script (this normally happens
        /// if an exception was raised by the script). <br/>
        /// -- or -- <br/>
        /// An unexpected error happened during script execution. In this case, the
        /// <see cref="Exception.InnerException"/> property contains the original
        /// <see cref="Exception"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Argument <paramref name="script"/> is null.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Argument <paramref name="script"/> is an invalid path.
        /// </exception>
        /// <remarks>
        /// <para>
        /// In a 'normal' case, a Python script that creates a chart would show this chart
        /// with the help of Python's own backend, like this.
        /// <example>
        /// import matplotlib.pyplot as plt
        /// ...
        /// plt.show()
        /// </example>
        /// For the script to be used within the context of this <see cref="PythonRunner"/>,
        /// it should instead convert the image to a base64-encoded string and print this string
        /// to the console. The following code snippet shows a Python method (<c>print_figure</c>)
        /// that does this:
        /// <example>
        /// import io, sys, base64
        ///
        /// def print_figure(fig):
        ///   buf = io.BytesIO()
        ///   fig.savefig(buf, format='png')
        ///   print(base64.b64encode(buf.getbuffer()))
        ///
        /// import matplotlib.pyplot as plt
        /// ...
        /// print_figure(plt.gcf()) # the gcf() method retrieves the current figure
        /// </example>
        /// </para><para>
        /// <b>Important:</b> Output to the error stream can also come from warnings, that are frequently
        /// outputted by various python package components. These warnings would result
        /// in an exception, therefore they must be switched off within the script by
        /// including the following statement: <c>warnings.simplefilter("ignore")</c>.
        /// </para>
        /// </remarks>
        public Image GetImage(string script, params object[] scriptArguments)
        {
            InternalExecute(script, scriptArguments);

            string errorMessage = _errorBuilder.ToString();

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new PythonRunnerException(errorMessage);
            }

            try
            {
                return PythonBase64ImageConverter.FromPythonBase64String(_outputBuilder.ToString().Trim());
            }
            catch (Exception exception)
            {
                throw new PythonRunnerException(
                    "An error occured while trying to create an image from Python script output. " +
                    "See inner exception for details.",
                    exception);
            }
        }

        public PythonRunner CreateVirtualEnv(string requirements)
        {
            // Check for existance of venv and create if not. Note: on Debian/Ubuntu systems, you need to
            // install the python3-venv package using the following command: apt install python3.8-venv
            // You may need to use sudo with that command. After installing the python3-venv package, recreate your virtual environment.
            // On Windows 10 use the default installer provided from python.org.
            if (!File.Exists(Path.Combine(EnvPath, "pyvenv.cfg")))
            {
                // create virtual environment
                var startInfo = new ProcessStartInfo(Interpreter)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };

                var args = new string[] { "-m", "venv", EnvPath };
                foreach (var arg in args)
                {
                    startInfo.ArgumentList.Add(arg);
                }

                InternalRun(startInfo);

                // install packages
                if (OperatingSystem.IsLinux())
                {
                    startInfo.FileName = Path.Combine(EnvPath, "bin", "pip");
                }
                else if (OperatingSystem.IsWindows())
                {
                    startInfo.FileName = Path.Combine(EnvPath, "Scripts", "pip");
                }

                startInfo.ArgumentList.Clear();
                args = new string[] { "install", "-r", requirements };
                foreach (var arg in args)
                {
                    startInfo.ArgumentList.Add(arg);
                }

                InternalRun(startInfo);
            }

            PythonRunner runner = null;

            if (OperatingSystem.IsLinux())
            {
                runner = new PythonRunner(Array.Empty<string>(), Path.Combine(EnvPath, "bin", "python3"));
            }
            else if (OperatingSystem.IsWindows())
            {
                runner = new PythonRunner(Array.Empty<string>(), Path.Combine(EnvPath, "Scripts", "python.exe"));
            }

            return runner;
        }

        // Append python script and script arguments then run the Process component.
        private void InternalExecute(string script, object[] arguments)
        {
            if (script == null)
            {
                throw new PythonRunnerException("Script file is required.", new ArgumentNullException(nameof(script)));
            }

            if (!File.Exists(script))
            {
                throw new PythonRunnerException("Script file not found: {script}", new FileNotFoundException(script));
            }

            ProcessStartInfo startInfo = CreateStartInfo();

            // Add script.py file.
            startInfo.ArgumentList.Add(script);

            // Add script args.
            if (arguments != null)
            {
                foreach (object argument in arguments.Where(arg => arg != null))
                {
                    startInfo.ArgumentList.Add(argument.ToString());
                }
            }

            InternalRun(startInfo);
        }

        // Run the Process component.
        private void InternalRun(ProcessStartInfo startInfo)
        {
            _outputBuilder = new StringBuilder();
            _errorBuilder = new StringBuilder();

            using (var process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            })
            {
                try
                {
                    // asynchronous reading of output and error streams
                    process.OutputDataReceived += OnProcessOutputDataReceived;
                    process.ErrorDataReceived += OnProcessErrorDataReceived;

                    // Start the process.
                    process.Start();
                    OnStarted(process.StartTime);

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (!process.WaitForExit(Timeout))
                    {
                        throw new TimeoutException();
                    }

                    OnExited(process.ExitCode, process.ExitTime);
                }
                catch (TimeoutException)
                {
                    process.Kill();
                    process.WaitForExit();
                    OnExited(process.ExitCode, process.ExitTime);
                }
                catch (Exception exception)
                {
                    throw new PythonRunnerException(
                        "An error occured during process run. See inner exception for details.",
                        exception);
                }
            }
        }

        // Create basic startinfo with Arguments added for Launcher and Interpreter
        private ProcessStartInfo CreateStartInfo()
        {
            var startInfo = new ProcessStartInfo(Interpreter)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            // Add launcher args first if launcher is being used
            if (Interpreter.EndsWith("py.exe"))
            {
                if (LauncherArgs.Any())
                {
                    foreach (var arg in LauncherArgs)
                    {
                        startInfo.ArgumentList.Add(arg);
                    }
                }
            }

            // Add interpreter args.
            if (InterpreterArgs.Any())
            {
                if (InterpreterArgs.Any())
                {
                    foreach (var arg in InterpreterArgs)
                    {
                        startInfo.ArgumentList.Add(arg);
                    }
                }
            }

            return startInfo;
        }

        // Collect output on the go. Otherwise, the internal buffer may run full and script execution
        // may be hanging. This can especially happen when printing images as base64 strings (which
        // produces a very large amount of ASCII text).
        private void OnProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _outputBuilder.AppendLine(e.Data);
        }

        // Collect error output on the go.
        private void OnProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            _errorBuilder.AppendLine(e.Data);
        }

        // Raise the Started event (see above).
        private void OnStarted(DateTime startTime) =>
        Started?.Invoke(this, new PyRunnerStartedEventArgs(startTime));

        // Raise the Exited event (see above).
        private void OnExited(int exitCode, DateTime exitTime) =>
            Exited?.Invoke(this, new PyRunnerExitedEventArgs(exitCode, exitTime));

        public static class OperatingSystem
        {
            public static bool IsWindows() =>
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            ////public static bool IsMacOS() =>
            ////    RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

            public static bool IsLinux() =>
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        }
    }
}
