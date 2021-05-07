// ************************************************************************************************
// <copyright file="PyRunnerStartedEventArgs.cs">
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
	public class PyRunnerStartedEventArgs
	{
		public PyRunnerStartedEventArgs(DateTime startTime)
		{
			StartTime = startTime;
		}

		public DateTime StartTime { get; }
	}
}