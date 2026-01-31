namespace DLManager.Core;

public class ServiceLocator : StaticSingleton<ServiceLocator>
{
    public static IServiceProvider ServiceProvider { get; internal set; } = null!;

    public IDlManagerView DlManagerView => ServiceProvider.GetRequiredService<IDlManagerView>();
    public IDlManagerViewModel DlManagerViewModel => ServiceProvider.GetRequiredService<IDlManagerViewModel>();
}