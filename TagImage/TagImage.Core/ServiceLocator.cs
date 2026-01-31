namespace TagImage.Core;

public class ServiceLocator : StaticSingleton<ServiceLocator>
{
    public static IServiceProvider ServiceProvider { get; internal set; } = null!;

    public ISplashView SplashView => ServiceProvider.GetRequiredService<ISplashView>();
    public ISplashViewModel SplashViewModel => ServiceProvider.GetRequiredService<ISplashViewModel>();
}