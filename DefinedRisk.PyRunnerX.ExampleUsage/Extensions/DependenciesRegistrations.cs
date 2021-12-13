// <copyright file="DependenciesRegistrations.cs" company="DefinedRisk">
// Copyright (c) DefinedRisk. MIT License.
// </copyright>
// <author>DefinedRisk</author>

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using DefinedRisk.PyRunnerX;
    using DefinedRisk.PyRunnerX.ExampleUsage.Worker;

    public static class DependenciesRegistrations
    {
        public static IServiceCollection AddPythonRunner(this IServiceCollection services)
        {
            return services
                .AddScoped<IPyRunnerX, PythonRunner>()
                .AddHostedService<PyRunnerHostedService>();
        }
    }
}
