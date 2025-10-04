using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using ToDoList.Core.Abstract.ViewModels;
using ToDoList.Core.Abstract.Views;
using ToDoList.Core.Views;
using ToDoListCore.Abstract.Services;
using ToDoListCore.ViewModels;

namespace ToDoListCore;

[SuppressMessage("Performance", "CA1822:将成员标记为 static")]
public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseAvaloniaCore<TStartUp>()
            where TStartUp : class, IStartupWindow
        {
            return collection
                .AddSingleton<ToDoListApp>()
                .AddSingleton<IStartupWindow, TStartUp>();
        }

        public IServiceCollection UseToDoListCore()
        {
            return collection
                .AddTransient<ISplashViewModel, SplashViewModel>()
                .AddScoped<IMainMenuViewModel, MainMenuViewModel>()
                .AddScoped<IMainMenuView, MainMenuView>();
        }
    }
}