using System.Collections.ObjectModel;
using Avalonia.Controls;
using DLManager.Plugin.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace DLManager.Core.Abstract.Plugin;

public interface IPluginContainer
{
    ReadOnlyObservableCollection<PluginInfo> Plugins { get; }
    ReadOnlyObservableCollection<string> PluginPaths { get; }
    ReadOnlyObservableCollection<PluginViewInfo> PluginViewInfos { get; }
    void AddPlugin(PluginInfo pluginInfo);
    void RemovePlugin(string pluginPath);
    void RenamePlugin(string oldPath, string newPath);
}

public record PluginInfo(string Path, string Name, BasePlugin Instance);

public record PluginViewInfo(
    string ViewId,
    ServiceLifetime LifeCycle,
    Func<Control> Factory,
    Type ViewType,
    string? DisplayName = null)
{
    public string DisplayName
    {
        get => field ?? ViewId;
        set => field = value;
    } = DisplayName;
}