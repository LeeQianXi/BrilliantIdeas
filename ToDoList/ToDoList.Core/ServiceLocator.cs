namespace ToDoList.Core;

public class ServiceLocator : StaticSingleton<ServiceLocator>
{
    private static IServiceProvider _serviceProvider = null!;

    public IServiceProvider ServiceProvider
    {
        get => _serviceProvider;
        set => _serviceProvider = value;
    }

    public ISplashViewModel SplashViewModel => ServiceProvider.GetRequiredService<ISplashViewModel>();
    public IMainMenuView MainMenuView => ServiceProvider.GetRequiredService<IMainMenuView>();
    public IMainMenuViewModel MainMenuViewModel => ServiceProvider.GetRequiredService<IMainMenuViewModel>();
}