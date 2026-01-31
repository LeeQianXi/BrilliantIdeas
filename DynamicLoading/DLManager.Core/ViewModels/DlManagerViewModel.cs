using System.Collections.ObjectModel;
using DLManager.Core.Abstract.Plugin;

namespace DLManager.Core.ViewModels;

public class DlManagerViewModel : ViewModelBase, IDlManagerViewModel
{
    public DlManagerViewModel(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Logger = serviceProvider.GetRequiredService<ILogger<DlManagerViewModel>>();
        var pluginContainer = serviceProvider.GetRequiredService<IPluginContainer>();
        PluginViewInfos = pluginContainer.PluginViewInfos;
    }

    public override IServiceProvider ServiceProvider { get; }
    public override ILogger Logger { get; }
    public ReadOnlyObservableCollection<PluginViewInfo> PluginViewInfos { get; }
}