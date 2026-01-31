using NetUtility.Singleton;

namespace DLManager.Plugin.Preview;

public class ServiceLocator : StaticSingleton<ServiceLocator>
{
    public static IServiceProvider ServiceProvider { get; internal set; } = null!;
}