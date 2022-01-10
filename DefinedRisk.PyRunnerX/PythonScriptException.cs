// <copyright file="PythonScriptException.cs" company="DefinedRisk">
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
    /// when ProcessErrorData is received ie. errors occuring during exceution of the python script.
    /// </summary>
    public class PythonScriptException : PythonRunnerException
    {
        public PythonScriptException()
        {
        }

        public PythonScriptException(string message)
            : base(message)
        {
        }

        public PythonScriptException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
