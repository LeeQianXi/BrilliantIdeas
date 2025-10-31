using System.Reflection;
using DLManager.Plugin.Abstract;

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

        public IServiceCollection RegisterDynamic()
        {
            var pluginPath = Path.Combine(ServiceLocator.ProgramPath, "plugins");
            if (!Path.Exists(pluginPath))
            {
                Directory.CreateDirectory(pluginPath);
                return collection;
            }

            var plugins = Directory.GetFiles(pluginPath, "*.dll");
            foreach (var plugin in plugins)
            {
                try
                {
                    var ass = Assembly.LoadFile(plugin);
                    foreach (var type in ass.GetTypes()
                                 .Where(t => t.GetCustomAttribute<DynamicLoadingAttribute>() is not null)
                                 .Where(t => t.IsAssignableTo(typeof(BasePlugin)))
                                 .Where(t => !t.IsAbstract))
                    {
                        ServiceLocator.Plugins.Add(type.GetCustomAttribute<DynamicLoadingAttribute>()!.PluginId, type);
                        collection.AddSingleton(type);
                    }
                }
                catch
                {
                }
            }

            return collection;
        }
    }
}