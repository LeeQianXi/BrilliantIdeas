namespace DLManager.Core.Views;

public partial class DlManagerView : ViewModelWindowBase<IDlManagerViewModel>, IStartupWindow, IDlManagerView
{
    public DlManagerView()
    {
        InitializeComponent();
    }

    private LruCache<PluginViewInfo, PluginView> PluginViewCache { get; } = new(10);

    private void PluginTitle_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count < 1) return;
        Container.Content = null;
        var info = (PluginViewInfo)e.AddedItems[0]!;
        Container.Content = PluginViewCache.GetOrAdd(info,
            pvi => (PluginView)ServiceLocator.Instance.ServiceProvider.GetRequiredService(
                ServiceLocator.GetPluginViewType(pvi.PluginId, pvi.ViewId)!));
    }
}