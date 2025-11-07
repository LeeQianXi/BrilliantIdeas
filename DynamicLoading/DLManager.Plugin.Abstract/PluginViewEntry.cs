namespace DLManager.Plugin.Abstract;

/// <summary>
///     插件View信息集合
/// </summary>
/// <param name="ViewId">插件ViewID</param>
/// <param name="LifeCycle">插件View生命周期</param>
/// <param name="ViewType">插件View注册类型</param>
/// <param name="Factory">插件View构造器</param>
public record PluginViewEntry(
    string ViewId,
    Type ViewType,
    PluginLifeCycle LifeCycle,
    PluginViewFactory<PluginView> Factory)
{
    [field: AllowNull]
    public string DisplayName
    {
        get => field ?? ViewId;
        set => field = value;
    }
}

public delegate T PluginViewFactory<out T>(IServiceProvider provider) where T : PluginView;