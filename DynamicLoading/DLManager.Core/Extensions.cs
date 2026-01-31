using DIAbstract;
using DLManager.Core.Abstract.Plugin;
using DLManager.Core.Plugin;

namespace DLManager.Core;

public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseAvaloniaCore<TStartUp>()
            where TStartUp : class, IStartupWindow
        {
            return collection
                .AddMultiSingleton<Application, DlManagerApp>()
                .AddMultiSingleton<IStartupWindow, TStartUp>();
        }

        public IServiceCollection UseDlManagerCore()
        {
            return collection
                .AddSingleton<IDlManagerView, DlManagerWindow>(s =>
                    (DlManagerWindow)s.GetRequiredService<IStartupWindow>())
                .AddMultiSingleton<IDlManagerViewModel, DlManagerViewModel>();
        }

        public IServiceCollection AddDlPlugin()
        {
            collection
                .AddMultiSingleton<IPluginContainer, PluginContainer>()
                .AddHostedService<PluginMonitor>();
            return collection;
        }
    }
}