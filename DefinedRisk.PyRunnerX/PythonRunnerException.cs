// <copyright file="PythonRunnerException.cs" company="DefinedRisk">
// Copyright (c) DefinedRisk. MIT License.
// </copyright>
// <author>DefinedRisk</author>

namespace DefinedRisk.PyRunnerX
{
    using System;
    using System.Collections.Generic;
    using System.Text;

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
