using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiPanel.Client.Orleans;
using NetUtility.Singleton;

namespace MultiPanel.Client;

public class ServiceLocator : StaticSingleton<ServiceLocator>
{
    private static IClientContext _clientContext = null!;

    public IServiceProvider ServiceProvider => _clientContext.ServiceProvider;

    public IClientContext ClientContext
    {
        get => _clientContext;
        set => _clientContext = value;
    }

    public ILogger<T> GetLogger<T>()
    {
        return ServiceProvider.GetRequiredService<ILogger<T>>();
    }
}