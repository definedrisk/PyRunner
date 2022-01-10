// <copyright file="IPyRunnerX.cs" company="DefinedRisk">
// Copyright (c) DefinedRisk. MIT License.
// </copyright>
// <author>DefinedRisk</author>

namespace DefinedRisk.PyRunnerX
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using SixLabors.ImageSharp;

    /// <summary>
    /// Intrface for the specialized runner of python and python scripts. Supports textual output
    /// as well as image output (work in progress), both synchronously and asynchronously.
    /// </summary>
    public interface IPyRunnerX
    {
        /// <summary>
        /// Gets the full path to the underlying Python launcher or interpreter (eg. 'py.exe' or 'python3')
        /// for use by this instance (set during construction and cannot be changed).
        /// </summary>
        public string Interpreter { get; }

        /// <summary>
        /// Gets or sets the Python launcher args for use by this instance. See 'py --help'
        /// for available launcher args. Not necessary for Ubuntu linux.
        /// </summary>
        public string[] LauncherArgs { get; set; }

        /// <summary>
        /// Gets or sets the Python interpreter args for use by this instance. See 'python3 --help'
        /// for available interpreter args.
        /// </summary>
        public string[] InterpreterArgs { get; set; }

        /// <summary>
        /// Gets or Sets the optional working directory used by the process. This is often necessary
        /// when subscripts and modules are being called.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or Sets the timeout for the underlying process in msec.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// Use the exisiting interpreter (launcher) to create a virtual environment in the base-directory if
        /// it does not already exist. Then return a new <see cref="PythonRunner"/> with interpreter setup
        /// to use this virtual environment.
        /// Note: on Debian/Ubuntu systems, you need to install the python3-venv package before this can
        /// be used (sudo apt install python3.8-venv). On Windows 10 use the default installer provided
        /// from python.org to ensure the venv module is available for use by this function.
        /// </summary>
        /// <seealso aref="https://docs.python.org/3/tutorial/venv.html"/>
        /// <param name="ct">CancellationToken.</param>
        /// <param name="requirements">Optional path to <c>requirements.txt</c> file.</param>
        /// <returns>A python runner with interpreter setup to use the virtual environment.</returns>
        public Task<PythonRunner> CreateVirtualEnvAsync(CancellationToken ct, string requirements = null);

        /// <summary>
        /// Gets the directory of the interpreter.
        /// </summary>
        /// <returns>Directory name.</returns>
        public string GetEnvironmentDirectory();

        /// <summary>
        /// Install requirements.txt into this environment.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <param name="requirements">Path to requirements.txt.</param>
        /// <returns><see cref="Task"/>.</returns>
        public Task InstallRequirementsAsync(CancellationToken ct, string requirements);

        /// <summary>
        /// Delete the virtual environment if it exists.
        /// </summary>
        public void DeleteEnvironment();

        /// <summary>
        /// Execute a Python script and return text that would have been printed
        /// to console.
        /// </summary>
        /// <param name="script">Full path to script file.</param>
        /// <param name="scriptArguments">Arguments to pass to script.</param>
        /// <returns>The text output of the script (re-directed from StdOut).</returns>
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
        /// <b>Important:</b> Output to the error stream can also come from warnings, that are frequently
        /// outputted by various python package components. These warnings would result
        /// in an exception, therefore they should be switched off within the script by
        /// including the following statement: <c>warnings.simplefilter("ignore")</c>.
        /// </remarks>
        public string Execute(string script, params object[] scriptArguments);

        /// <summary>
        /// Runs the <see cref="Execute"/> method asynchronously using a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="ct">Task cancellation token.</param>
        /// <param name="script">Full path to script file.</param>
        /// <param name="scriptArguments">Arguments to pass to script.</param>
        /// <exception cref="ArgumentNullException">
        /// Argument <paramref name="script"/> is null.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Argument <paramref name="script"/> is an invalid path.
        /// </exception>
        /// <returns>
        /// An awaitable task, with the text output of the script as <see cref="Task{TResult}.Result"/>.
        /// </returns>
        /// <seealso cref="Execute"/>
        public Task<string> ExecuteAsync(CancellationToken ct, string script, params object[] scriptArguments);

        /// <summary>
        /// Run the Launcher or Interpreter with arguments (without Python Script).
        /// Use this to setup environments for example.
        /// </summary>
        /// <returns>The text output (re-directed from StdOut).</returns>
        public string Run();

        /// <summary>
        /// Runs the <see cref="Run"/> method asynchronously using a <see cref="CancellationToken"/>.
        /// Cancellation can only be triggered before the process is started.
        /// After that time the cancellation token has no effect.
        /// </summary>
        /// <param name="ct">Cancellationtoken.</param>
        /// <returns>The text output (re-directed from Stdout).</returns>
        public Task<string> RunAsync(CancellationToken ct);

        /// <summary>
        /// Executes a Python script and returns the resulting image (mostly a chart that was produced
        /// by a Python package like e.g. <see href="https://matplotlib.org/">matplotlib</see>).
        /// \TEST Image conversion functionality.
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
        public Image GetImage(string script, params object[] scriptArguments);
    }
}
