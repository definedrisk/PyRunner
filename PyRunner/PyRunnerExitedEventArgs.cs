// ************************************************************************************************
// <copyright file="PyRunnerExitedEventArgs.cs">
//   Copyright (c) 2019 Thomas Weller
// </copyright>
// <authors>
//   <author>Thomas Weller</author>
// </authors>
// <summary/>
// ************************************************************************************************
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System;

namespace PyRunner
{
	public class PyRunnerExitedEventArgs
	{
		public PyRunnerExitedEventArgs(int exitCode, DateTime exitTime)
		{
			ExitCode = exitCode;
			ExitTime = exitTime;
		}

		public int ExitCode { get; }
		public DateTime ExitTime { get; }
	}
}