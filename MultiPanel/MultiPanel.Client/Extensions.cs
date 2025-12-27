using AvaloniaUtility.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MultiPanel.Client;

public static class Extensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection UseAvaloniaCore<TStartUp>()
            where TStartUp : class, IStartupWindow
        {
            return collection
                .AddSingleton<MultiPanelApp>()
                .AddSingleton<IStartupWindow, TStartUp>();
        }

        public IServiceCollection UseDeadLineCore()
        {
            return collection;
        }
    }
}