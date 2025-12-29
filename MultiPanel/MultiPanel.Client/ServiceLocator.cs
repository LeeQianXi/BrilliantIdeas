using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiPanel.Client.Abstract.ViewModels;
using MultiPanel.Client.Abstract.Views;
using MultiPanel.Client.Orleans;
using NetUtility.Singleton;

namespace MultiPanel.Client;

public class ServiceLocator : StaticSingleton<ServiceLocator>
{
    private static IClientContext _clientContext = null!;

    public static string ApplicationDataFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MultiPanel");

    public IServiceProvider ServiceProvider => _clientContext.ServiceProvider;

    public IClientContext ClientContext
    {
        get => _clientContext;
        set => _clientContext = value;
    }

    public ILoginInViewModel LoginInViewModel => ServiceProvider.GetRequiredService<ILoginInViewModel>();
    public IMainMenuView MainMenuView => ServiceProvider.GetRequiredService<IMainMenuView>();
    public IMainMenuViewModel MainMenuViewModel => ServiceProvider.GetRequiredService<IMainMenuViewModel>();

    public ILogger<T> GetLogger<T>()
    {
        return ServiceProvider.GetRequiredService<ILogger<T>>();
    }
}