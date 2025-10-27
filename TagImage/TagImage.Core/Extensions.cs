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