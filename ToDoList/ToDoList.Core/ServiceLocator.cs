using ToDoList.Core.Abstract.ViewModels;
using ToDoList.Core.Abstract.Views;

namespace ToDoListCore;

public class ServiceLocator
{
    public static IServiceProvider ServiceProvider { get; set; }
    public static ISplashViewModel SplashViewModel => ServiceProvider.GetRequiredService<ISplashViewModel>();
    public static IMainMenuView MainMenuView => ServiceProvider.GetRequiredService<IMainMenuView>();
    public static IMainMenuViewModel MainMenuViewModel => ServiceProvider.GetRequiredService<IMainMenuViewModel>();
}