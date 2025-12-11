using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetUtility.Singleton;

namespace MultiPanel.Client;

public class ServiceLocator : StaticSingleton<ServiceLocator>
{
    private static IHost _clientHost = null!;

    public IHost ClientHost
    {
        get => _clientHost;
        set => _clientHost = value;
    }

    public IClusterClient ClusterClient => _clientHost.Services.GetRequiredService<IClusterClient>();
    public IServiceProvider ServiceProvider => _clientHost.Services;
}