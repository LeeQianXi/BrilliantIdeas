namespace DLManager.Core;

public class ServiceLocator : StaticSingleton<ServiceLocator>
{
    private static IServiceProvider _serviceProvider = null!;
    public static string ProgramPath { get; } = Environment.CurrentDirectory;
    public static Dictionary<string, Dictionary<string, (string, Type)>> Plugins { get; } = new();

    public IServiceProvider ServiceProvider
    {
        get => _serviceProvider;
        set => _serviceProvider = value;
    }

    public IDlManagerView DlManagerView => ServiceProvider.GetRequiredService<IDlManagerView>();
    public IDlManagerViewModel DlManagerViewModel => ServiceProvider.GetRequiredService<IDlManagerViewModel>();

    public static IEnumerable<string> GetPluginViews(string pluginId)
    {
        return Plugins.TryGetValue(pluginId, out var views) ? views.Keys : Enumerable.Empty<string>();
    }

    public static Type? GetPluginViewType(string pluginId, string viewName)
    {
        if (!Plugins.TryGetValue(pluginId, out var views)) return null;
        return views.TryGetValue(viewName, out var pair) ? pair.Item2 : null;
    }

    public static Type? GetPluginViewType(string pluginId, string viewName, out string displayName)
    {
        displayName = null!;
        if (!Plugins.TryGetValue(pluginId, out var views)) return null;
        if (!views.TryGetValue(viewName, out var pair)) return null;
        displayName = pair.Item1;
        return pair.Item2;
    }
}