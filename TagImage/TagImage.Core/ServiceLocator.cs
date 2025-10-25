using NetUtility.Singleton;

namespace TagImage.Core;

public class ServiceLocator : StaticSingleton<ServiceLocator>
{
    private static IServiceProvider _serviceProvider = null!;

    public IServiceProvider ServiceProvider
    {
        get => _serviceProvider;
        set => _serviceProvider = value;
    }

    public ISplashView SplashView => ServiceProvider.GetRequiredService<ISplashView>();
    public ISplashViewModel SplashViewModel => ServiceProvider.GetRequiredService<ISplashViewModel>();
}