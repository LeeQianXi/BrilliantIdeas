namespace ToDoList.Core;

public class ServiceLocator : StaticSingleton<ServiceLocator>
{
    public static IServiceProvider ServiceProvider { get; internal set; } = null!;

    public ISplashViewModel SplashViewModel => ServiceProvider.GetRequiredService<ISplashViewModel>();
    public IMainMenuView MainMenuView => ServiceProvider.GetRequiredService<IMainMenuView>();
    public IMainMenuViewModel MainMenuViewModel => ServiceProvider.GetRequiredService<IMainMenuViewModel>();
}