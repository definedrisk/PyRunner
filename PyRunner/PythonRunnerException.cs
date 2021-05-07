// ************************************************************************************************
// <copyright file="PythonRunnerException.cs">
//   Copyright (c) 2019 Thomas Weller
// </copyright>
// <authors>
//   <author>Thomas Weller</author>
// </authors>
// <summary/>
// ************************************************************************************************

using System;

namespace PyRunner
{
	/// <summary>
	/// The <see cref="Exception"/> type that is thrown by <see cref="PythonRunner"/> instances
	/// when an error occured during execution of a script.
	/// </summary>
	public class PythonRunnerException : Exception
	{
		public PythonRunnerException()
		{
		}

		public PythonRunnerException(string message)
			: base(message)
		{
		}

		public PythonRunnerException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}