// ************************************************************************************************
// <copyright file="PythonRunner.cs">
//   Copyright (c) 2019 Thomas Weller
// </copyright>
// <authors>
//   <author>Thomas Weller</author>
// </authors>
// <summary/>
// ************************************************************************************************

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyRunner
{
	/// <summary>
	/// A specialized runner for python scripts. Supports textual output
	/// as well as image output, both synchronously and asynchronously.
	/// </summary>
	/// <remarks>
	/// You can think of <see cref="PythonRunner"/> instances <see cref="Process"/>
	/// instances that were specialized for Python scripts.
	/// </remarks>
	/// <seealso cref="Process"/>
	public class PythonRunner
	{
		#region Fields

		// for collecting output/error output
		private StringBuilder _outputBuilder;
		private StringBuilder _errorBuilder;

		#endregion //  Fields

		#region Construction

		/// <summary>
		/// Instantiates a new <see cref="PythonRunner"/> instance.
		/// </summary>
		/// <param name="interpreter">
		/// Full path to the Python interpreter ('python.exe').
		/// </param>
		/// <param name="timeout">
		/// The script timeout in msec. Defaults to 10000 (10 sec).
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Argument <paramref name="interpreter"/> is null.
		/// </exception>
		/// <exception cref="FileNotFoundException">
		/// Argument <paramref name="interpreter"/> is an invalid path.
		/// </exception>
		/// <seealso cref="Interpreter"/>
		/// <seealso cref="Timeout"/>
		public PythonRunner(string interpreter, int timeout = 10000)
		{
			if (interpreter == null)
			{
				throw new ArgumentNullException(nameof(interpreter));
			}

			if (!File.Exists(interpreter))
			{
				throw new FileNotFoundException(interpreter);
			}

			Interpreter = interpreter;
			Timeout = timeout;
		}

		#endregion //  Construction

		#region Events

		/// <summary>
		/// Occurs when a python process is started.
		/// </summary>
		/// <seealso cref="PyRunnerStartedEventArgs"/>
		// ReSharper disable once EventNeverSubscribedTo.Global
		public event EventHandler<PyRunnerStartedEventArgs> Started;

		/// <summary>
		/// Occurs when a python process has exited.
		/// </summary>
		/// <seealso cref="PyRunnerExitedEventArgs"/>
		// ReSharper disable once EventNeverSubscribedTo.Global
		public event EventHandler<PyRunnerExitedEventArgs> Exited;

		#endregion //  Events

		#region Properties

		/// <summary>
		/// The Python interpreter ('python.exe') that is used by this instance.
		/// </summary>
		public string Interpreter { get; }

		/// <summary>
		/// The timeout for the underlying <see cref="Process"/> component in msec.
		/// </summary>
		/// <remarks>
		/// See <see cref="Process.WaitForExit(int)"/> for details about this value.
		/// </remarks>
		// ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
		public int Timeout { get; set; }

		#endregion //  Properties

		#region Operations

		/// <summary>
		/// Executes a Python script and returns the text that it prints to the console.
		/// </summary>
		/// <param name="script">Full path to the script to execute.</param>
		/// <param name="arguments">Arguments that were passed to the script.</param>
		/// <returns>The text output of the script.</returns>
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
		/// Output to the error stream can also come from warnings, that are frequently
		/// outputted by various python package components. These warnings would result
		/// in an exception, therefore they must be switched off within the script by
		/// including the following statement: <c>warnings.simplefilter("ignore")</c>.
		/// </remarks>
		public string Execute(string script, params object[] arguments)
		{
			InternalExecute(script, arguments);

			string errorMessage = _errorBuilder.ToString();

			return !string.IsNullOrWhiteSpace(errorMessage)
				? throw new PythonRunnerException(errorMessage)
				: _outputBuilder.ToString().Trim();
		}

		/// <summary>
		/// Runs the <see cref="Execute"/> method asynchronously. 
		/// </summary>
		/// <returns>
		/// An awaitable task, with the text output of the script as <see cref="Task{TResult}.Result"/>.
		/// </returns>
		/// <seealso cref="Execute"/>
		public Task<string> ExecuteAsync(string script, params object[] arguments)
		{
			var task = new Task<string>(() => Execute(script, arguments));
			task.Start();
			return task;
		}

		/// <summary>
		/// Executes a Python script and returns the resulting image (mostly a chart that was produced
		/// by a Python package like e.g. <see href="https://matplotlib.org/">matplotlib</see> or
		/// <see href="https://seaborn.pydata.org/">seaborn</see>).
		/// </summary>
		/// <param name="script">Full path to the script to execute.</param>
		/// <param name="arguments">Arguments that were passed to the script.</param>
		/// <returns>The <see cref="Bitmap"/> that the script creates.</returns>
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
		/// 	buf = io.BytesIO()
		/// 	fig.savefig(buf, format='png')
		/// 	print(base64.b64encode(buf.getbuffer()))
		///
		/// import matplotlib.pyplot as plt
		/// ...
		/// print_figure(plt.gcf()) # the gcf() method retrieves the current figure
		/// </example>
		/// </para><para>
		/// Output to the error stream can also come from warnings, that are frequently
		/// outputted by various python package components. These warnings would result
		/// in an exception, therefore they must be switched off within the script by
		/// including the following statement: <c>warnings.simplefilter("ignore")</c>.
		/// </para>
		/// </remarks>
		public Bitmap GetImage(string script, params object[] arguments)
		{
			InternalExecute(script, arguments);

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

		/// <summary>
		/// Runs the <see cref="GetImage"/> method asynchronously. 
		/// </summary>
		/// <returns>
		/// An awaitable task, with the <see cref="Bitmap"/> that the script
		/// creates as <see cref="Task{TResult}.Result"/>.
		/// </returns>
		/// <seealso cref="GetImage"/>
		public Task<Bitmap> GetImageAsync(string script, params object[] arguments)
		{
			var task = new Task<Bitmap>(() => GetImage(script, arguments));
			task.Start();
			return task;
		}

		#endregion //  Operations

		#region Implementation

		// Helper function. Formats the arguments for the Process component.
		internal static string BuildArgumentString(string scriptPath, object[] arguments)
		{
			var stringBuilder = new StringBuilder("\"" + scriptPath.Trim() + "\"");

			if (arguments != null)
			{
				foreach (object argument in arguments.Where(arg => arg != null))
				{
					stringBuilder.Append(" \"" + argument.ToString().Trim() + "\"");
				}
			}

			return stringBuilder.ToString();
		}

		// Runs the Process component.
		private void InternalExecute(string script, object[] arguments)
		{
			if (script == null)
			{
				throw new ArgumentNullException(nameof(script));
			}

			if (!File.Exists(script))
			{
				throw new FileNotFoundException(script);
			}

			_outputBuilder = new StringBuilder();
			_errorBuilder = new StringBuilder();

			var startInfo = new ProcessStartInfo(Interpreter)
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				Arguments = BuildArgumentString(script, arguments)
			};

			using (var process = new Process
			{
				StartInfo = startInfo,
				EnableRaisingEvents = true
			})
			{
				try
				{
					process.ErrorDataReceived += OnProcessErrorDataReceived;
					process.OutputDataReceived += OnProcessOutputDataReceived;

					// Start the process.
					process.Start();
					OnStarted(process.StartTime);

					process.BeginOutputReadLine();
					process.BeginErrorReadLine();

					if (!process.WaitForExit(Timeout))
					{
						throw new TimeoutException($"The timeout of {Timeout}msec expired.");
					}

					OnExited(process.ExitCode, process.ExitTime);
				}
				catch (Exception exception)
				{
					throw new PythonRunnerException(
						"An error occured during script execution. See inner exception for details.",
						exception);
				}
			}
		}

		// Collect output on the go. Otherwise, the internal buffer may run full and script execution
		// may be hanging. This can especially happen when printing images as base64 strings (which
		// produces a very large amount of ASCII text).
		private void OnProcessOutputDataReceived(object sender, DataReceivedEventArgs e) => 
			_outputBuilder.AppendLine(e.Data);

		// Collect error output on the go.
		private void OnProcessErrorDataReceived(object sender, DataReceivedEventArgs e) => 
			_errorBuilder.AppendLine(e.Data);

		// Raise the Exited event (see above).
		private void OnExited(int exitCode, DateTime exitTime) =>
			Exited?.Invoke(this, new PyRunnerExitedEventArgs(exitCode, exitTime));

		// Raise the Started event (see above).
		private void OnStarted(DateTime startTime) =>
			Started?.Invoke(this, new PyRunnerStartedEventArgs(startTime));

		#endregion //  Implementation

	}
}