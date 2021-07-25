// <copyright file="PyRunnerExitedEventArgs.cs" company="DefinedRisk">
// Copyright (c) DefinedRisk. MIT License.
// </copyright>
// <author>DefinedRisk</author>

namespace DefinedRisk.PyRunnerX
{
    using System;

    public struct PyRunnerExitedEventArgs
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
