// <copyright file="PyRunnerStartedEventArgs.cs" company="DefinedRisk">
// Copyright (c) DefinedRisk. MIT License.
// </copyright>
// <author>DefinedRisk</author>

namespace DefinedRisk.PyRunnerX
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public struct PyRunnerStartedEventArgs
    {
        public PyRunnerStartedEventArgs(DateTime startTime)
        {
            StartTime = startTime;
        }

        public DateTime StartTime { get; }
    }
}
