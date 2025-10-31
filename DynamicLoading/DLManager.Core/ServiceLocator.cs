using DLManager.Plugin.Abstract;

namespace DLManager.Core;

public class ServiceLocator : StaticSingleton<ServiceLocator>
{
    private static IServiceProvider _serviceProvider = null!;
    public static string ProgramPath { get; } = Environment.CurrentDirectory;
    public static Dictionary<string, Type> Plugins { get; } = [];

    public IServiceProvider ServiceProvider
    {
        get => _serviceProvider;
        set => _serviceProvider = value;
    }

    public BasePlugin GetPlugin(string pluginId)
    {
        return (BasePlugin)ServiceProvider.GetRequiredService(Plugins[pluginId]);
    }

    public IDlManagerView DlManagerView => ServiceProvider.GetRequiredService<IDlManagerView>();
    public IDlManagerViewModel DlManagerViewModel => ServiceProvider.GetRequiredService<IDlManagerViewModel>();
}