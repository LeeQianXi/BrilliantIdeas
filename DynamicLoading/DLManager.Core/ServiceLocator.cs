namespace DLManager.Core;

public class ServiceLocator : StaticSingleton<ServiceLocator>
{
    private static IServiceProvider _serviceProvider = null!;

    public IServiceProvider ServiceProvider
    {
        get => _serviceProvider;
        set => _serviceProvider = value;
    }

    public IDlManagerView DlManagerView => ServiceProvider.GetRequiredService<IDlManagerView>();
    public IDlManagerViewModel DlManagerViewModel => ServiceProvider.GetRequiredService<IDlManagerViewModel>();
}