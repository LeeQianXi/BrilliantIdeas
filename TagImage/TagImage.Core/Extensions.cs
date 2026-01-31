using DIAbstract;

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
                .AddMultiSingleton<Application, TagImageApp>()
                .AddMultiSingleton<IStartupWindow, TStartUp>();
        }

        public IServiceCollection UseTagImageCore()
        {
            return collection
                .AddMultiSingleton<ISplashView, SplashView>()
                .AddMultiSingleton<ISplashViewModel, SplashViewModel>();
        }
    }
}