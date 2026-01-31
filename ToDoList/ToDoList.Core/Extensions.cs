using DIAbstract;

namespace ToDoList.Core;

[SuppressMessage("Performance", "CA1822:将成员标记为 static")]
public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseAvaloniaCore<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            TStartUp>()
            where TStartUp : class, IStartupWindow
        {
            return collection
                .AddMultiSingleton<Application, ToDoListApp>()
                .AddMultiSingleton<IStartupWindow, TStartUp>();
        }

        public IServiceCollection UseToDoListCore()
        {
            return collection
                .AddMultiSingleton<ISplashViewModel, SplashViewModel>()
                .AddMultiSingleton<IMainMenuViewModel, MainMenuViewModel>()
                .AddMultiSingleton<IMainMenuView, MainMenuView>();
        }
    }
}