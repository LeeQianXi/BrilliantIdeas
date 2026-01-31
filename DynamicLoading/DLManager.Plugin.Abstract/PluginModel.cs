using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DLManager.Plugin.Abstract;

public abstract class BasePlugin(IServiceProvider serviceProvider)
{
    public IServiceProvider ServiceProvider { get; set; } = serviceProvider;
    public abstract IEnumerable<PluginViewEntry> Views { get; }

    protected static PluginViewEntry CreateEntry<T>(PluginViewFactory<T> factory, string? displayName = null)
        where T : IPluginView<T>
    {
        return CreateEntry<T, T>(factory, displayName);
    }

    protected static PluginViewEntry CreateEntry<TR, T>(PluginViewFactory<T> factory, string? displayName = null)
        where TR : IPluginView<T>
        where T : TR
    {
        var type = typeof(T);
        var attr = type.GetCustomAttribute<DeclareViewAttribute>();
        return attr is null
            ? throw new ArgumentException($"Type '{type}' must have a {nameof(DeclareViewAttribute)} attribute")
            : displayName is null
                ? new PluginViewEntry<T>(attr.ViewName, typeof(TR), attr.LifeCycle, factory)
                : new PluginViewEntry<T>(attr.ViewName, typeof(TR), attr.LifeCycle, factory)
                    { DisplayName = displayName };
    }
}

//无需显示调用InitializeComponent
public interface IPluginView<T> where T : IPluginView<T>;

public abstract record PluginViewEntry(
    string ViewId,
    Type ViewType,
    ServiceLifetime LifeCycle)
{
    [field: AllowNull]
    public string DisplayName
    {
        get => field ?? ViewId;
        init;
    }
}

/// <summary>
///     插件View信息集合
/// </summary>
/// <param name="ViewId">插件ViewID</param>
/// <param name="LifeCycle">插件View生命周期,Scoped是非法的</param>
/// <param name="ViewType">插件View注册类型</param>
/// <param name="Factory">插件View构造器</param>
public record PluginViewEntry<T>(
    string ViewId,
    Type ViewType,
    ServiceLifetime LifeCycle,
    PluginViewFactory<T> Factory) : PluginViewEntry(ViewId, ViewType, LifeCycle) where T : IPluginView<T>
{
}

public delegate T PluginViewFactory<out T>(IServiceProvider provider) where T : IPluginView<T>;