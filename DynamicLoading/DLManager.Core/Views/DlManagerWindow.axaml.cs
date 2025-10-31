using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaUtility.Views;
using DLManager.Plugin.Abstract.Views;

namespace DLManager.Core.Views;

public partial class DlManagerView : ViewModelWindowBase<IDlManagerViewModel>, IStartupWindow, IDlManagerView
{
    private readonly Dictionary<(string, string), IPluginView> _plugViews = new();

    private void PluginTitle_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Container.Children.Clear();
        if (e.AddedItems.Count < 1) return;
        var title = (string)e.AddedItems[0]!;
        var plugin = ServiceLocator.Instance.GetPlugin(title);
        foreach (var (n, b) in plugin.Views)
        {
            if (!_plugViews.TryGetValue((title, n), out var pluginView))
            {
                _plugViews[(title, n)] = pluginView = b.Invoke();
            }

            Container.Children.Add((Control)pluginView);
        }
    }
}