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
    /// as well as image output (work in progress), both synchronously and asynchronously.
    /// </summary>
    /// <remarks>
    /// You can think of <see cref="PythonRunner"/> instances as <see cref="Process"/>
    /// instances that were specialized for Python and Python scripts. The python interpreter
    /// (and launcher for windows) can also be called directly without appending scipts.
    /// </remarks>
    /// <seealso cref="Process"/>
    public sealed class PythonRunner : IPyRunnerX
    {
        private const int DEFAULTTIMEOUT = 60000;

        private const int SETUPTIMEOUT = 120000;

        // collects output from process std_output during latest run
        private StringBuilder _outputBuilder;

        // collects error from process std_err during latest run
        private StringBuilder _errorBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonRunner"/> class to be used for creating virtual
        /// environments using default global python installation (either "c:\windows\py.exe" or "/usr/bin/python3"
        /// evaluated from OS (windows or linux) at runtime.
        /// </summary>
        /// <param name="timeout">Optional script timeout in msec. Defaults to <see cref="DEFAULTTIMEOUT"/>.</param>
        /// <exception cref="PythonRunnerException">
        /// Default launcher (interpreter) does not exist (invalid path). Use alternative constructor and specify
        /// launcher (interpreter) directly.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Timeout should be greater than 0.
        /// </exception>
        /// <seealso cref="Timeout"/>
        public PythonRunner(
            int timeout = DEFAULTTIMEOUT)
        {
            if (timeout < 0)
            {
                throw new ArgumentOutOfRangeException($"Timeout should be greater than or equal to zero: {timeout}");
            }

            var launcher = GetOSLauncher;
            if (!File.Exists(launcher))
            {
                throw new PythonRunnerException($"Default launcher (interpreter) does not exist: {launcher}", new FileNotFoundException(launcher));
            }

            Interpreter = launcher;
            Timeout = timeout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonRunner"/> class.
        /// Interpreter (launcher) is fixed for the lifetime of this instance. Arguments can be changed.
        /// </summary>
        /// <param name="interpreter">Path to the Python interpreter (launcher).</param>
        /// <param name="interpreterArgs">Optional interpreter specific arguments.</param>
        /// <param name="timeout">Optional script timeout in msec. Defaults to <see cref="DEFAULTTIMEOUT"/>.</param>
        /// <exception cref="PythonRunnerException">
        /// Argument <paramref name="interpreter"/> is an invalid path.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Timeout should be greater than 0.
        /// </exception>
        /// <seealso cref="Interpreter"/>
        /// <seealso cref="InterpreterArgs"/>
        /// <seealso cref="Timeout"/>
        public PythonRunner(
            string interpreter,
            string[] interpreterArgs = null,
            int timeout = DEFAULTTIMEOUT)
        {
            if (timeout < 0)
            {
                throw new ArgumentOutOfRangeException($"Timeout should be greater than or equal to zero: {timeout}");
            }

            if (!File.Exists(interpreter))
            {
                throw new PythonRunnerException($"Invalid path: {interpreter}", new FileNotFoundException(interpreter));
            }

            Interpreter = interpreter;
            InterpreterArgs = interpreterArgs ?? Array.Empty<string>();
            Timeout = timeout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonRunner"/> class. The optional parameter
        /// <c>launcher</c> (interpreter) is fixed for the lifetime of this instance and
        /// defaults to either <c>c:\windows\py.exe</c> or <c>/usr/bin/python3</c> (evaluated from OS
        /// at runtime). Both sets of arguments can be changed throughout the lifetime of this class.
        /// Note that for windows launcher usage is "<c>py [launcher-args] [python-args] script [script-args]</c>".
        /// When calling this function on Ubuntu Linux the args will effectively be chained together so it would
        /// make more sense to call the constructor with a single set of <c>interpreterArgs</c> in that case.
        /// </summary>
        /// <param name="launcherArgs">Launcher specific arguments.</param>
        /// <param name="interpreterArgs">Optional interpreter specific arguments.</param>
        /// <param name="launcher">Optional full path to the Python launcher (interpreter). Othwerwise
        /// uses the default global python installation (either "c:\windows\py.exe" or "/usr/bin/python3"
        /// evaluated from OS (windows or linux) at runtime.</param>
        /// <param name="timeout">Optional script timeout in msec. Defaults to <see cref="DEFAULTTIMEOUT"/>.</param>
        /// <exception cref="PythonRunnerException">
        /// Launcher (interpreter) <paramref name="launcher"/> is an invalid path.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Timeout should be greater than 0.
        /// </exception>
        /// <seealso cref="LauncherArgs"/>
        /// <seealso cref="InterpreterArgs"/>
        /// <seealso cref="Interpreter"/>
        /// <seealso cref="Timeout"/>
        public PythonRunner(
            string[] launcherArgs,
            string[] interpreterArgs = null,
            string launcher = null,
            int timeout = DEFAULTTIMEOUT)
        {
            if (timeout < 0)
            {
                throw new ArgumentOutOfRangeException($"Timeout should be greater than or equal to zero: {timeout}");
            }

            launcher ??= GetOSLauncher;

            if (!File.Exists(launcher))
            {
                throw new PythonRunnerException($"Default launcher (interpreter) does not exist: {launcher}", new FileNotFoundException(launcher));
            }

            LauncherArgs = launcherArgs;
            InterpreterArgs = interpreterArgs ?? Array.Empty<string>();
            Interpreter = launcher;
            Timeout = timeout;
        }

        public PythonRunner(PythonRunner copy)
        {
            Interpreter = copy.Interpreter;
            InterpreterArgs = copy.InterpreterArgs;
            LauncherArgs = copy.LauncherArgs;
            WorkingDirectory = copy.WorkingDirectory;
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

        public string Interpreter { get; }

        public string[] LauncherArgs { get; set; } = new string[0];

        public string[] InterpreterArgs { get; set; } = new string[0];

        public string WorkingDirectory { get; set; }

        public int Timeout { get; set; }

        // Gets the default operating system launcher (interpreter) full path.
        private static string GetOSLauncher
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    try
                    {
                        return FindExePath("py.exe");
                    }
                    catch (FileNotFoundException)
                    {
                        return string.Empty;
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    try
                    {
                        return FindExePath("python3");
                    }
                    catch (FileNotFoundException)
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        // Gets the path for the virtual environment to be assoicated with the app
        private static string GetVirtualLauncher
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return Path.Combine(EnvPath, "bin", "python3");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return Path.Combine(EnvPath, "Scripts", "python.exe");
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        // Gets the virtual environment folder i.e. <c>./base-directory/.venv</c>.
        private static string EnvPath
        {
            get
            {
                return Path.Combine(AppContext.BaseDirectory, @".venv");
            }
        }

        /// <summary>
        /// Get a PythonRunner with launcher set to virtual environment.
        /// </summary>
        /// <returns>PythonRunner with launcher set to virtual environment or NULL if
        /// environment does not exist.</returns>
        public static PythonRunner GetVirtualEnvironment()
        {
            PythonRunner runner = null;

            if (File.Exists(Path.Combine(EnvPath, "pyvenv.cfg")))
            {
                runner = new PythonRunner(GetVirtualLauncher);
            }

            return runner;
        }

        /// <summary>
        /// Get a PythonRunner with launcher set to global default environment.
        /// </summary>
        /// <returns>PythonRunner with launcher set to default system launcher (interpreter)
        /// or NULL if not found.</returns>
        public static PythonRunner GetSystemEnvironment()
        {
            PythonRunner runner = null;

            if (File.Exists(GetOSLauncher))
            {
                runner = new PythonRunner(GetOSLauncher);
            }

            return runner;
        }

        /// <summary>
        /// Expands environment variables and locates the exe with either fully qualifed or relative directory.
        /// If unqualified then searches the the evironment's path after working directory.
        /// </summary>
        /// <param name="exe">The name of the executable file.</param>
        /// <param name="recursive">Optional recursive seach sub-folders (default is false).</param>
        /// <returns>The fully-qualified path to the file.</returns>
        /// <exception cref="System.IO.FileNotFoundException">Raised when the exe was not found.</exception>
        public static string FindExePath(string exe, bool recursive = false)
        {
            exe = Environment.ExpandEnvironmentVariables(exe);

            var directoryName = Path.GetDirectoryName(exe) ?? string.Empty;
            if (directoryName == string.Empty)
            {
                directoryName = Directory.GetCurrentDirectory();
            }

            var result = Directory.EnumerateFiles(
                directoryName,
                Path.GetFileName(exe),
                new EnumerationOptions { RecurseSubdirectories = recursive });

            if (result.FirstOrDefault() != null)
            {
                return result.FirstOrDefault();
            }
            else
            {
                foreach (string test in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(';'))
                {
                    string path = test.Trim();
                    if (!string.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                    {
                        return Path.GetFullPath(path);
                    }
                }
            }

            throw new FileNotFoundException(new FileNotFoundException().Message, exe);
        }

        public string GetEnvironmentDirectory()
        {
            return new FileInfo(Interpreter).Directory.Parent.FullName;
        }

        public async Task<PythonRunner> CreateVirtualEnvAsync(CancellationToken ct, string requirements = null)
        {
            // Check for existance of venv and create if not
            if (!File.Exists(Path.Combine(EnvPath, "pyvenv.cfg")))
            {
                Timeout = SETUPTIMEOUT;
                LauncherArgs = null;

                InterpreterArgs = new string[] { "-m", "venv", EnvPath };
                await RunAsync(ct).ConfigureAwait(false);

                Timeout = DEFAULTTIMEOUT;
            }

            PythonRunner runner = GetVirtualEnvironment();

            if (requirements != null)
            {
                await runner.InstallRequirementsAsync(ct, requirements).ConfigureAwait(false);
            }

            return runner;
        }

        public async Task InstallRequirementsAsync(CancellationToken ct, string requirements)
        {
            Timeout = SETUPTIMEOUT;
            LauncherArgs = null;

            // ensure no exception due to out-of-date pip
            InterpreterArgs = new string[] { "-m", "pip", "install", "--upgrade", "pip" };
            await RunAsync(ct).ConfigureAwait(false);

            // install requirements.txt
            InterpreterArgs = new string[] { "-m", "pip", "install", "-r", requirements };
            await RunAsync(ct).ConfigureAwait(false);

            Timeout = DEFAULTTIMEOUT;
            InterpreterArgs = null;
        }

        public string Execute(string script, params object[] scriptArguments)
        {
            InternalExecute(script, scriptArguments);

            return !string.IsNullOrWhiteSpace(_errorBuilder.ToString())
                ? throw new PythonScriptException(_errorBuilder.ToString())
                : _outputBuilder.ToString().Trim();
        }

        public async Task<string> ExecuteAsync(CancellationToken ct, string script, params object[] scriptArguments)
        {
            // cancellation token will only prevent execution if not already started
            return await Task.Run<string>(() => Execute(script, scriptArguments), ct).ConfigureAwait(false);
        }

        public string Run()
        {
            ProcessStartInfo startInfo = CreateStartInfo();
            InternalRun(startInfo);

            return !string.IsNullOrWhiteSpace(_errorBuilder.ToString())
                 ? throw new PythonRunnerException(_errorBuilder.ToString())
                 : _outputBuilder.ToString().Trim();
        }

        public async Task<string> RunAsync(CancellationToken ct)
        {
            // cancellation token will only prevent execution if not already started
            return await Task.Run<string>(() => Run(), ct).ConfigureAwait(false);
        }

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

        public void DeleteEnvironment()
        {
            if (File.Exists(Path.Combine(EnvPath, "pyvenv.cfg")))
            {
                Directory.Delete(EnvPath, true);
            }
        }

        // Append python script and script arguments then run the Process component.
        private void InternalExecute(string script, object[] arguments)
        {
            if (script == null)
            {
                throw new PythonRunnerException("Script file is required.", new ArgumentNullException(nameof(script)));
            }

            if (!File.Exists(Path.Combine(WorkingDirectory, script)))
            {
                throw new PythonRunnerException($"Script file not found: {script}", new FileNotFoundException(script));
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
                    _outputBuilder.Append("PYRUNNER TIMEOUT\n");
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
                WorkingDirectory = WorkingDirectory ?? string.Empty
            };

            // Add launcher args first if any.
            if (LauncherArgs != null)
            {
                foreach (var arg in LauncherArgs)
                {
                    startInfo.ArgumentList.Add(arg);
                }
            }

            // Add interpreter args.
            if (InterpreterArgs != null)
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
    }
}
