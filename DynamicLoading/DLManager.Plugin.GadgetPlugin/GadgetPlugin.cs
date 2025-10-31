namespace DLManager.Plugin.GadgetPlugin;

[DynamicLoading(nameof(GadgetPlugin))]
public class GadgetPlugin : BasePlugin
{
    public override List<PluginViewEntry> Views { get; } =
    [
        CreateEntry(sp => new CountdownTimerView())
    ];

    private static PluginViewEntry CreateEntry<T>(PluginViewFactory<T> factory, string? displayName = null)
        where T : PluginView
        => CreateEntry<T, T>(factory, displayName);

    private static PluginViewEntry CreateEntry<TR, T>(PluginViewFactory<T> factory, string? displayName = null)
        where TR : PluginView
        where T : TR
    {
        var type = typeof(T);
        var attr = type.GetCustomAttribute<DeclareViewAttribute>();
        return attr is null
            ? throw new ArgumentException($"Type '{type}' must have a {nameof(DeclareViewAttribute)} attribute")
            : displayName is null
                ? new PluginViewEntry(attr.ViewName, typeof(TR), attr.LifeCycle, factory)
                : new PluginViewEntry(attr.ViewName, typeof(TR), attr.LifeCycle, factory) { DisplayName = displayName };
    }
}