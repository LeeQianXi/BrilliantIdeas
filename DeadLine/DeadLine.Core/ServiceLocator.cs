namespace DeadLine.Core;

public class ServiceLocator : StaticSingleton<ServiceLocator>
{
    private static IServiceProvider _serviceProvider = null!;

    public IServiceProvider ServiceProvider
    {
        get => _serviceProvider;
        set => _serviceProvider = value;
    }

    public IDeadLineViewModel DeadLineViewModel => ServiceProvider.GetRequiredService<IDeadLineViewModel>();
}