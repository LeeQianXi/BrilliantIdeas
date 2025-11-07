namespace DeadLine.Core;

public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseAvaloniaCore<TStartUp>()
            where TStartUp : class, IStartupWindow
        {
            return collection
                .AddSingleton<DeadLineApp>()
                .AddSingleton<IStartupWindow, TStartUp>();
        }

        public IServiceCollection UseDeadLineCore()
        {
            return collection
                .AddSingleton<IDeadLineView, DeadLineWindow>()
                .AddSingleton<IDeadLineViewModel, DeadLineViewModel>();
        }
    }
}