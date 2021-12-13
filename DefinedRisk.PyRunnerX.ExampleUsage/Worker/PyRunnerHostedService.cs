// <copyright file="PyRunnerHostedService.cs" company="DefinedRisk">
// Copyright (c) DefinedRisk. MIT License.
// </copyright>
// <author>DefinedRisk</author>

namespace DefinedRisk.PyRunnerX.ExampleUsage.Worker
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    // Code to be run just before receiving requests should be placed in the StartAsync method.
    // The StopAsync method can be ignored for this use case.
    // Based on example https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-3/
    public class PyRunnerHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        private IPyRunnerX _vRunner;

        public PyRunnerHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                // create a virtual environment
                var service = scope.ServiceProvider.GetRequiredService<IPyRunnerX>();
                try
                {
                    _vRunner = await service.CreateVirtualEnvAsync(cancellationToken, Path.Join(AppContext.BaseDirectory, "Python", "requirements.txt"));
                }
                catch (PythonRunnerException ex)
                {
                    // \TODO Implement something here instead
                    throw new NotImplementedException("\\TODO Implement something here instead", ex);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _vRunner.DeleteEnvironment();
            return Task.CompletedTask;
        }
    }
}
