namespace DLManager.Core;

public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseAvaloniaCore<TStartUp>()
            where TStartUp : class, IStartupWindow
        {
            return collection
                .AddSingleton<DlManagerApp>()
                .AddSingleton<IStartupWindow, TStartUp>();
        }

        public IServiceCollection UseDlManagerCore()
        {
            return collection
                .AddSingleton<IDlManagerView, DlManagerView>()
                .AddSingleton<IDlManagerViewModel, DlManagerViewModel>();
        }
    }
}