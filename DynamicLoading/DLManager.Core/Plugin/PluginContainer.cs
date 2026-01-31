using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DLManager.Core.Abstract.Plugin;
using DynamicData;
using DynamicData.Alias;
using ReactiveUI;

namespace DLManager.Core.Plugin;

public class PluginContainer : IPluginContainer
{
    private readonly ReadOnlyObservableCollection<PluginInfo> _pluginInfos;
    private readonly ReadOnlyObservableCollection<string> _pluginPaths;
    private readonly ReadOnlyObservableCollection<PluginViewInfo> _pluginViewInfos;

    public PluginContainer(IServiceProvider serviceProvider)
    {
        PluginCache.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _pluginInfos)
            .Subscribe();
        PluginCache.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(info => info.Path)
            .Bind(out _pluginPaths)
            .Subscribe();
        PluginCache.Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .SelectMany(info => info.Instance.Views, view => view.ViewId)
            .Select(entry => new PluginViewInfo(
                entry.ViewId,
                entry.LifeCycle,
                ParseEntry(serviceProvider, entry),
                entry.ViewType,
                entry.DisplayName))
            .Bind(out _pluginViewInfos)
            .Subscribe();
    }

    private SourceCache<PluginInfo, string> PluginCache { get; } = new(i => i.Path);
    public ReadOnlyObservableCollection<PluginInfo> Plugins => _pluginInfos;
    public ReadOnlyObservableCollection<string> PluginPaths => _pluginPaths;
    public ReadOnlyObservableCollection<PluginViewInfo> PluginViewInfos => _pluginViewInfos;

    public void AddPlugin(PluginInfo pluginInfo)
    {
        PluginCache.AddOrUpdate(pluginInfo);
    }

    public void RemovePlugin(string pluginPath)
    {
        if (_pluginPaths.Contains(pluginPath))
            PluginCache.Remove(pluginPath);
    }

    public void RenamePlugin(string oldPath, string newPath)
    {
        if (_pluginPaths.Contains(oldPath))
            PluginCache.Edit(p =>
            {
                var item = p.Items.First(i => i.Path == oldPath);
                //p.Remove(item);
                p.AddOrUpdate(item with { Path = newPath });
            });
    }

    private Func<Control> ParseEntry(IServiceProvider serviceProvider, PluginViewEntry entry)
    {
        var propInfo = entry.GetType()
            .GetProperty(nameof(PluginViewEntry<>.Factory))!;
        dynamic factory = propInfo.GetValue(entry)!;
        switch (entry.LifeCycle)
        {
            case ServiceLifetime.Singleton:
                var sing = factory(serviceProvider);
                return () => sing;
            case ServiceLifetime.Transient:
                return () => factory(serviceProvider);
            default:
                throw new ArgumentOutOfRangeException(nameof(entry.LifeCycle));
        }
    }
}