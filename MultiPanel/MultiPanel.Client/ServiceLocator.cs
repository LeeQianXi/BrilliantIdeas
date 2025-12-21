using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiPanel.Client.Orleans;
using NetUtility.Singleton;

namespace MultiPanel.Client;

public class ServiceLocator : StaticSingleton<ServiceLocator>
{
    private static IServiceProvider _serviceProvider = null!;

    public IServiceProvider ServiceProvider
    {
        get => _serviceProvider;
        set => _serviceProvider = value;
    }

    public IClientContext ClientContext => ServiceProvider.GetRequiredService<IClientContext>();

    public ILogger<T> GetLogger<T>()
    {
        return ServiceProvider.GetRequiredService<ILogger<T>>();
    }
}