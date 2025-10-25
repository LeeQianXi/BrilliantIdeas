using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using AvaloniaUtility.Services;
using TagImage.Core.Abstract.Views;
using TagImage.Core.ViewModels;
using TagImage.Core.Views;

namespace TagImage.Core;

[SuppressMessage("Performance", "CA1822:将成员标记为 static")]
public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseAvaloniaCore<TStartUp>()
            where TStartUp : class, IStartupWindow
        {
            return collection
                .AddSingleton<TagImageApp>()
                .AddSingleton<IStartupWindow, TStartUp>();
        }

        public IServiceCollection UseTagImageCore()
        {
            return collection
                .AddSingleton<ISplashView, SplashView>()
                .AddSingleton<ISplashViewModel, SplashViewModel>();
        }
    }
}