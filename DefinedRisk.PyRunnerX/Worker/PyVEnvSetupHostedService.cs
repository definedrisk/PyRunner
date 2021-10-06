// <copyright file="PyVEnvSetupHostedService.cs" company="DefinedRisk">
// Copyright (c) DefinedRisk. MIT License.
// </copyright>
// <author>DefinedRisk</author>

namespace DefinedRisk.PyRunnerX.Worker
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
    public class PyVEnvSetupHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        private IPyRunnerX _vRunner;

        public PyVEnvSetupHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                // create a virtual environment
                var service = scope.ServiceProvider.GetRequiredService<IPyRunnerX>();
                _vRunner = await service.CreateVirtualEnvAsync(cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _vRunner.DeleteEnvironment();
            return Task.CompletedTask;
        }
    }
}
