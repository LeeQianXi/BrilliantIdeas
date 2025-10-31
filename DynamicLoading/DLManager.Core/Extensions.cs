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
                        var pluginId = type.GetCustomAttribute<DynamicLoadingAttribute>()!.PluginId;
                        var pluginInstance = (BasePlugin)Activator.CreateInstance(type)!;
                        foreach (var entry in pluginInstance.Views)
                        {
                            switch (entry.LifeCycle)
                            {
                                case PluginLifeCycle.Singleton:
                                    collection.AddSingleton(entry.ViewType, sp => entry.Factory.Invoke(sp));
                                    break;
                                case PluginLifeCycle.Scoped:
                                    collection.AddScoped(entry.ViewType, sp => entry.Factory.Invoke(sp));
                                    break;
                                case PluginLifeCycle.Transient:
                                    collection.AddTransient(entry.ViewType, sp => entry.Factory.Invoke(sp));
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            if (!ServiceLocator.Plugins.TryGetValue(pluginId, out var viewDic))
                                ServiceLocator.Plugins[pluginId] = viewDic = new Dictionary<string, (string, Type)>();
                            viewDic[entry.ViewId] = (entry.DisplayName, entry.ViewType);
                        }
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