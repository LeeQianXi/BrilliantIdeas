using DLManager.Core.Abstract.Plugin;

namespace DLManager.Core.Views;

public partial class DlManagerWindow : ViewModelWindowBase<IDlManagerViewModel>, IStartupWindow, IDlManagerView
{
    public DlManagerWindow()
    {
        InitializeComponent();
    }

    private LruCache<PluginViewInfo, Control> PluginViewCache { get; } = new(10);

    private void PluginTitle_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count < 1) return;
        Container.Content = null;
        var info = (PluginViewInfo)e.AddedItems[0]!;
        Container.Content = PluginViewCache.GetOrAdd(info,
            pvi => info.Factory.Invoke());
    }
}