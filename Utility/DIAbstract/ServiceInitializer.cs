using System.Collections.Frozen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DIAbstract;

internal class ServiceInitializer(ILogger<ServiceInitializer> logger, IServiceProvider serviceProvider)
    : IHostedService
{
    private readonly ICollection<IAsyncLifecycle>
        _lifecycles = serviceProvider.GetServices<IAsyncLifecycle>().ToFrozenSet();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var count = 0;
        logger.LogInformation("Starting Initializer BrilliantServices");
        foreach (var asyncLifecycle in _lifecycles)
            try
            {
                await asyncLifecycle.InitializeAsync();
                count++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An Error occured while initializing the service");
            }

        logger.LogInformation("{Count} of {LifecyclesCount} BrilliantServices started", count, _lifecycles.Count);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping BrilliantServices");
        foreach (var asyncLifecycle in _lifecycles)
            try
            {
                await asyncLifecycle.DisposeAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An Error occured while stopping the service");
            }

        logger.LogInformation("Total {LifecyclesCount} of BrilliantServices stopped", _lifecycles.Count);
    }
}